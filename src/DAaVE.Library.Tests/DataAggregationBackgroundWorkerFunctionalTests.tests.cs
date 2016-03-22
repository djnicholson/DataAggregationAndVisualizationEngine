// <copyright file="DataAggregationBackgroundWorkerFunctionalTests.tests.cs" company="David Nicholson">
//     Copyright (c) David Nicholson. All rights reserved.
// </copyright>
// <summary>See class header.</summary>

namespace DAaVE.Library.Tests
{
    using System;
    using System.Diagnostics;
    using System.Linq;
    using System.Threading;

    using DAaVE.Library.DataAggregation;
    using DAaVE.Library.Storage;
    using DAaVE.Samples;

    using Microsoft.VisualStudio.TestTools.UnitTesting;
    
    /// <summary>
    /// Functional tests (make and depend on assumptions about the shared thread pool) for the 
    /// <see cref="DataAggregationBackgroundWorker{TDataPointTypeEnum}"/> class.
    /// </summary>
    [TestClass]
    public partial class DataAggregationBackgroundWorkerFunctionalTests
    {
        /// <summary>
        /// The maximum amount of time that each individual expectation must be realized within before
        /// the current test is marked as failed.
        /// </summary>
        private static readonly TimeSpan Timeout = TimeSpan.FromSeconds(5.0);

        /// <summary>
        /// Basic sanity test that an instance of <see cref="DataAggregationBackgroundWorker{TDataPointTypeEnum}"/> can successfully
        /// be initialized and disposed (without any actual verification of what it does).
        /// </summary>
        [TestMethod]
        public void SmokeTest()
        {
            using (DataAggregationBackgroundWorker<SampleDataPointType> target = this.NewTarget())
            {
            }
        }

        /// <summary>
        /// Confirms correct behavior for a single iteration of the 'happy path' through the main loop.
        /// </summary>
        [TestMethod]
        public void SingleIterationHappyPath()
        {
            using (DataAggregationBackgroundWorker<SampleDataPointType> target = this.NewTarget())
            {
                this.AssertSingleIteration();
            }
        }

        /// <summary>
        /// Confirms correct behavior over a sequence of consecutive iterations of the main loop.
        /// </summary>
        [TestMethod]
        public void MultipleIterationsHappyPath()
        {
            using (DataAggregationBackgroundWorker<SampleDataPointType> target = this.NewTarget())
            {
                Debug.WriteLine("Commencing verification of iteration 1...");
                this.AssertSingleIteration(seed: 03211902);

                Debug.WriteLine("Commencing verification of iteration 2...");
                this.AssertSingleIteration(seed: 03211903);
            }
        }

        /// <summary>
        /// Generates a random date using the provided random number generator.
        /// </summary>
        /// <param name="r">Random number to generate.</param>
        /// <returns>A new random <see cref="DateTime"/> of type <see cref="DateTimeKind.Utc"/>.</returns>
        private static DateTime NewRandomDateTimeUtc(Random r)
        {
            return DateTime.SpecifyKind(DateTime.FromOADate(r.NextDouble()), kind: DateTimeKind.Utc);
        }

        /// <summary>
        /// Asserts all expectations for a single iteration of the control logic within the target
        /// <see cref="DataAggregationBackgroundWorker{TDataPointTypeEnum}"/>.  Returns when all
        /// expectations have been met.
        /// </summary>
        /// <param name="seed">Seed to use for pseudo-random generation of sample data.</param>
        private void AssertSingleIteration(int seed = 0)
        {
            Random r = new Random(seed);

            // Use a non-empty sample data set with up to 50 raw data point observations:
            DataPointObservation[] sampleRawData = new DataPointObservation[r.Next(1, 51)];
            for (int i = 0; i < sampleRawData.Length; i++)
            {
                sampleRawData[i] = new DataPointObservation(NewRandomDateTimeUtc(r), value: r.NextDouble());
            }

            // Use a non-empty set with up to 50 points to represent the aggregation of the above data:
            AggregatedDataPoint[] sampleAggregatedData = new AggregatedDataPoint[r.Next(1, 51)];
            for (int i = 0; i < sampleAggregatedData.Length; i++)
            {
                sampleAggregatedData[i] = new AggregatedDataPoint();
                sampleAggregatedData[i].UtcTimestamp = NewRandomDateTimeUtc(r);
                sampleAggregatedData[i].AggregatedValue = r.NextDouble();
            }

            ManualResetEventSlim aggregationResultReceived = new ManualResetEventSlim(false);

            SampleConsecutiveDataPointObservationsCollection dataFromPager = new SampleConsecutiveDataPointObservationsCollection(
                sampleRawData.OrderBy(d => d.UtcTimestamp),
                aggregationResult =>
                {
                    Assert.IsTrue(
                        aggregationResult.SequenceEqual(sampleAggregatedData),
                        "Entire aggregation result should be sent verbatim to the original pager-supplied data set");
                    aggregationResultReceived.Set();
                },
                isPartial: false);

            this.ExpectPagerRequest(dataFromPager);

            ConsecutiveDataPointObservationsCollection dataProvidedToAggregator = this.ExpectAggregationRequestResponse(sampleAggregatedData);

            Assert.IsTrue(
                dataProvidedToAggregator.SequenceEqual(sampleRawData.OrderBy(d => d.UtcTimestamp)),
                "Entire pager output should be passed verbatim as a single data-set to the aggregator for aggregation");

            aggregationResultReceived.Wait(Timeout);
            Assert.IsTrue(aggregationResultReceived.IsSet, "Aggregator results not provided to originating pager data object");
        }
    }
}
