// <copyright file="DataAggregationBackgroundWorkerFunctionalTests.tests.cs" company="David Nicholson">
//     Copyright (c) David Nicholson. All rights reserved.
// </copyright>
// <summary>See class header.</summary>

namespace DAaVE.Library.Tests
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    using DAaVE.Library.DataAggregation;
    using DAaVE.Library.ErrorHandling.ErrorSinks;
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
                DataPointObservation[] sampleData = new[]
                {
                    new DataPointObservation(new DateTime(2016, 3, 18, 18, 19, 20, DateTimeKind.Utc), 1.4),
                    new DataPointObservation(new DateTime(2016, 3, 18, 18, 19, 21, DateTimeKind.Utc), 1.5),
                };

                AggregatedDataPoint[] sampleDataAggregated = new[] { new AggregatedDataPoint() };
                sampleDataAggregated[0].UtcTimestamp = new DateTime(2016, 3, 18, 18, 0, 0, DateTimeKind.Utc);
                sampleDataAggregated[0].AggregatedValue = 1.45;

                ManualResetEventSlim aggregationResultReceived = new ManualResetEventSlim(false);

                SampleConsecutiveDataPointObservationsCollection dataFromPager = new SampleConsecutiveDataPointObservationsCollection(
                    sampleData.OrderBy(d => d.UtcTimestamp),
                    aggregationResult => 
                    {
                        Assert.IsTrue(
                            aggregationResult.SequenceEqual(sampleDataAggregated),
                            "Entire pager output should be passed verbatim to the aggregator");
                        aggregationResultReceived.Set();
                    },
                    isPartial: false);

                this.ExpectPagerRequest(dataFromPager);

                ConsecutiveDataPointObservationsCollection dataProvidedToAggregator = this.ExpectAggregationRequestResponse(sampleDataAggregated);

                Assert.IsTrue(
                    dataProvidedToAggregator.SequenceEqual(sampleData), 
                    "Entire pager output should be passed verbatim to the aggregator");

                aggregationResultReceived.Wait(Timeout);
                Assert.IsTrue(aggregationResultReceived.IsSet, "Aggregator results not provided to originating pager data object");
            }
        }
    }
}
