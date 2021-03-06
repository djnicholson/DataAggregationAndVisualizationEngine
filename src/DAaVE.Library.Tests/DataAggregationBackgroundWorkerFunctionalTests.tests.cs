﻿// <copyright file="DataAggregationBackgroundWorkerFunctionalTests.tests.cs" company="David Nicholson">
//     Copyright (c) David Nicholson. All rights reserved.
// </copyright>
// <summary>See class header.</summary>

namespace DAaVE.Library.Tests
{
    using System;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    using DAaVE.Library.DataAggregation;
    using DAaVE.Library.Storage;
    using DAaVE.Samples;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    /// <summary>
    /// Functional (make and depend on assumptions about the shared thread pool and availability of CPU for test
    /// code execution) tests for the <see cref="DataAggregationBackgroundWorker{TDataPointTypeEnum}"/> class.
    /// </summary>
    [TestClass]
    public partial class DataAggregationBackgroundWorkerFunctionalTests
    {
        /// <summary>
        /// The maximum amount of time that each individual expectation must be realized within before
        /// the current test is marked as failed.
        /// </summary>
        private static readonly TimeSpan Timeout = TimeSpan.FromSeconds(15.0);

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
        /// Confirms that empty pages being returned does not cause any failures (but does cause a delay
        /// of approximately <see cref="DataAggregationOrchestrator.SleepDurationOnDataExhaustion"/> before
        /// a re-query).
        /// </summary>
        [SuppressMessage(
            "Microsoft.Globalization",
            "CA1303:Do not pass literals as localized parameters",
            MessageId = "DAaVE.Library.Tests.DataAggregationBackgroundWorkerFunctionalTests.AssertTimeSpanBetween(System.TimeSpan,System.TimeSpan,System.TimeSpan,System.String,System.Object[])",
            Justification = "Proxied to Assert.IsTrue")]
        [TestMethod]
        public void EmptyPagesProvidedByPager()
        {
            using (DataAggregationBackgroundWorker<SampleDataPointType> target = this.NewTarget())
            {
                this.AssertSingleIteration(seed: 03211954);

                this.AssertSingleIteration(seed: 03211955, noRawData: true);

                Stopwatch stopwatch = Stopwatch.StartNew();

                this.AssertSingleIteration(seed: 03211956);

                AssertTimeSpanBetween(
                    TimeSpan.FromSeconds(3.0),
                    stopwatch.Elapsed,
                    TimeSpan.FromSeconds(7.5),
                    "Empty page did not cause expected delay. Expected: ~5 seconds; Actual: {0}",
                    stopwatch.Elapsed);
            }
        }

        /// <summary>
        /// Confirms that empty pages being returned does not cause a delay that is capable of blocking
        /// disposal of the entire <see cref="DataAggregationBackgroundWorker{TDataPointTypeEnum}"/>
        /// object.
        /// </summary>
        [SuppressMessage(
            "Microsoft.Globalization",
            "CA1303:Do not pass literals as localized parameters",
            MessageId = "DAaVE.Library.Tests.DataAggregationBackgroundWorkerFunctionalTests.AssertTimeSpanBetween(System.TimeSpan,System.TimeSpan,System.TimeSpan,System.String,System.Object[])",
            Justification = "Proxied to Assert.IsTrue")]
        [TestMethod]
        public void EmptyPagesProvidedByPagerDoNotInterruptShutdown()
        {
            Stopwatch stopwatch = new Stopwatch();

            using (DataAggregationBackgroundWorker<SampleDataPointType> target = this.NewTarget())
            {
                this.AssertSingleIteration(seed: 03220849);

                stopwatch.Start();

                this.AssertSingleIteration(seed: 03220850, noRawData: true);    
            }

            AssertTimeSpanBetween(
                TimeSpan.FromSeconds(0.0),
                stopwatch.Elapsed,
                TimeSpan.FromSeconds(0.5),
                "Empty page delay should not block shutdown. There was {0} in between the beginning of the empty iteration and the end of disposal.",
                stopwatch.Elapsed);
        }

        /// <summary>
        /// Confirms that individual exceptions from the pager do not cause any failures (but do cause a delay
        /// of approximately <see cref="DataAggregationOrchestrator.SleepDurationOnError"/> before a re-query).
        /// </summary>
        [SuppressMessage(
            "Microsoft.Globalization",
            "CA1303:Do not pass literals as localized parameters",
            MessageId = "DAaVE.Library.Tests.DataAggregationBackgroundWorkerFunctionalTests.AssertTimeSpanBetween(System.TimeSpan,System.TimeSpan,System.TimeSpan,System.String,System.Object[])",
            Justification = "Proxied to Assert.IsTrue")]
        [TestMethod]
        public void ErrorFromPager()
        {
            using (DataAggregationBackgroundWorker<SampleDataPointType> target = this.NewTarget())
            {
                this.AssertSingleIteration(seed: 03221825);

                this.AssertSingleIteration(seed: 03221826, expectPagerToFail: true);

                Stopwatch stopwatch = Stopwatch.StartNew();

                this.AssertSingleIteration(seed: 03221827);

                AssertTimeSpanBetween(
                    TimeSpan.FromSeconds(7.5),
                    stopwatch.Elapsed,
                    TimeSpan.FromSeconds(12.5),
                    "Exception did not cause expected delay. Expected: ~10 seconds; Actual: {0}",
                    stopwatch.Elapsed);
            }
        }

        /// <summary>
        /// Confirms that exceptions thrown by the pager do not cause a delay that is capable of blocking
        /// disposal of the entire <see cref="DataAggregationBackgroundWorker{TDataPointTypeEnum}"/>
        /// object.
        /// </summary>
        [SuppressMessage(
            "Microsoft.Globalization",
            "CA1303:Do not pass literals as localized parameters",
            MessageId = "DAaVE.Library.Tests.DataAggregationBackgroundWorkerFunctionalTests.AssertTimeSpanBetween(System.TimeSpan,System.TimeSpan,System.TimeSpan,System.String,System.Object[])",
            Justification = "Proxied to Assert.IsTrue")]
        [TestMethod]
        public void ErrorFromPagerDoesNotInterruptShutdown()
        {
            Stopwatch stopwatch = Stopwatch.StartNew();

            using (DataAggregationBackgroundWorker<SampleDataPointType> target = this.NewTarget())
            {
                this.AssertSingleIteration(seed: 03220849);

                this.AssertSingleIteration(seed: 03220850, expectPagerToFail: true);
            }

            AssertTimeSpanBetween(
                TimeSpan.FromSeconds(0.0),
                stopwatch.Elapsed,
                TimeSpan.FromSeconds(2.0),
                "Pager exceptions should not block shutdown. This test took {0} which appears to have an unexpected delay",
                stopwatch.Elapsed);
        }

        /// <summary>
        /// Confirms that empty aggregation results are acceptable (when there is a non-empty amount of raw data
        /// point observations).
        /// </summary>
        [TestMethod]
        public void EmptyAggregationResult()
        {
            using (DataAggregationBackgroundWorker<SampleDataPointType> target = this.NewTarget())
            {
                this.AssertSingleIteration(seed: 03220858);

                this.AssertSingleIteration(seed: 03220859, noAggregatedData: true);

                this.AssertSingleIteration(seed: 03220900);
            }
        }

        /// <summary>
        /// Asserts the correct concurrency model for aggregations (assumed to be CPU heavy) and
        /// storage of aggregation results (assumed to be IO heavy). At most one aggregation is
        /// allowed to happen concurrently, and at most one result storage operation can happen
        /// concurrently, but it is acceptable for a single aggregation computation to happen 
        /// simultaneously with the upload of the previous result.
        /// </summary>
        [SuppressMessage(
            "Microsoft.Globalization",
            "CA1303:Do not pass literals as localized parameters",
            MessageId = "DAaVE.Library.Tests.DataAggregationBackgroundWorkerFunctionalTests.AssertTimeSpanBetween(System.TimeSpan,System.TimeSpan,System.TimeSpan,System.String,System.Object[])",
            Justification = "Proxied to Assert.IsTrue")]
        [TestMethod]
        public void AggregationUploadConcurrency()
        {
            Stopwatch timer = Stopwatch.StartNew();
            using (DataAggregationBackgroundWorker<SampleDataPointType> target = this.NewTarget())
            {
                DataPointObservation[] sampleRawData;
                AggregatedDataPoint[] sampleAggregatedData;
                bool isPartial = GenerateSampleData(
                    seed: 03291812,
                    sampleRawDataMaximumLength: 1,
                    sampleAggregatedDataMaximumLength: 1,
                    sampleRawData: out sampleRawData,
                    sampleAggregatedData: out sampleAggregatedData);

                ManualResetEventSlim aggregation1UploadUnblocked = new ManualResetEventSlim(initialState: false);
                SampleConsecutiveDataPointObservationsCollection dataObject1 = new SampleConsecutiveDataPointObservationsCollection(
                    sampleRawData.OrderBy(d => d.UtcTimestamp),
                    aggregationResult => aggregation1UploadUnblocked.Wait(),
                    isPartial);

                ManualResetEventSlim aggregation2UploadStarted = new ManualResetEventSlim(initialState: false);
                SampleConsecutiveDataPointObservationsCollection dataObject2 = new SampleConsecutiveDataPointObservationsCollection(
                    sampleRawData.OrderBy(d => d.UtcTimestamp),
                    aggregationResult => aggregation2UploadStarted.Set(),
                    isPartial);

                // Iteration 1:
                this.ExpectPagerRequest(dataToReturn: dataObject1);
                this.ExpectAggregationRequestResponse(response: sampleAggregatedData);

                // Iteration 2:
                this.ExpectPagerRequest(dataToReturn: dataObject2);
                this.ExpectAggregationRequestResponse(response: sampleAggregatedData);
                Assert.IsFalse(aggregation2UploadStarted.Wait(TimeSpan.FromSeconds(10.0)), "Only one upload should be allowed at a time.");

                aggregation1UploadUnblocked.Set();

                Assert.IsTrue(aggregation2UploadStarted.Wait(TimeSpan.FromSeconds(10.0)), "Second aggregation did not get unblocked when expected.");
            }

            TimeSpan elapsed = timer.Elapsed;
            AssertTimeSpanBetween(
                TimeSpan.FromSeconds(5.0),
                elapsed,
                TimeSpan.FromSeconds(15.0),
                "Test should be blocked for about 10 seconds, but took {0} to complete",
                elapsed);
        }

        /// <summary>
        /// Asserts the disposing the worker while an upload is still in progress blocks until the upload is
        /// complete.
        /// </summary>
        [TestMethod]
        public void DisposalDuringUpload()
        {
            DataPointObservation[] sampleRawData;
            AggregatedDataPoint[] sampleAggregatedData;
            bool isPartial = GenerateSampleData(
                seed: 03291952,
                sampleRawDataMaximumLength: 1,
                sampleAggregatedDataMaximumLength: 1,
                sampleRawData: out sampleRawData,
                sampleAggregatedData: out sampleAggregatedData);

            DataAggregationBackgroundWorker<SampleDataPointType> target = this.NewTarget();

            ManualResetEventSlim targetDisposalUnblocked = new ManualResetEventSlim(initialState: false);
            ManualResetEventSlim aggregationUpload1Started = new ManualResetEventSlim(initialState: false);
            ManualResetEventSlim aggregationUpload2Started = new ManualResetEventSlim(initialState: false);
            ManualResetEventSlim aggregationUpload2Unblocked = new ManualResetEventSlim(initialState: false);

            SampleConsecutiveDataPointObservationsCollection dataObject1 = new SampleConsecutiveDataPointObservationsCollection(
                sampleRawData.OrderBy(d => d.UtcTimestamp),
                aggregationResult =>
                {
                    aggregationUpload1Started.Set();
                },
                isPartial);

            SampleConsecutiveDataPointObservationsCollection dataObject2 = new SampleConsecutiveDataPointObservationsCollection(
                sampleRawData.OrderBy(d => d.UtcTimestamp),
                aggregationResult =>
                {
                    aggregationUpload2Started.Set();
                    aggregationUpload2Unblocked.Wait();
                    targetDisposalUnblocked.Set();
                },
                isPartial);

            this.ExpectPagerRequest(dataToReturn: dataObject1);
            this.ExpectAggregationRequestResponse(response: sampleAggregatedData);
            this.ExpectPagerRequest(dataToReturn: dataObject2);
            this.ExpectAggregationRequestResponse(response: sampleAggregatedData);
            Assert.IsTrue(aggregationUpload1Started.Wait(TimeSpan.FromSeconds(5.0)), "Upload 1 should have begun.");
            Assert.IsTrue(aggregationUpload2Started.Wait(TimeSpan.FromSeconds(5.0)), "Upload 2 should have begun.");

            using (Task disposalTask = Task.Run(() => target.Dispose()))
            {
                Assert.IsFalse(disposalTask.Wait(TimeSpan.FromSeconds(5.0)), "Dispose should be blocked.");

                aggregationUpload2Unblocked.Set();

                Assert.IsTrue(targetDisposalUnblocked.Wait(TimeSpan.FromSeconds(5.0)), "Dispose should be unblocked.");
                Assert.IsTrue(disposalTask.Wait(TimeSpan.FromSeconds(5.0)), "Dispose should terminate.");
            }
        }

        /// <summary>
        /// At most 20 consecutive exceptions are acceptable during aggregation (each one will
        /// be reported to the error sink and cause a deliberate delay, but not stop the worker).
        /// </summary>
        [TestMethod]
        [Timeout(1000000)] // TODO: Add hooks to hit this scenario quicker.
        public void ErrorHandling()
        {
            bool success = false;
            bool rethrew = false;
            try
            {
                using (DataAggregationBackgroundWorker<SampleDataPointType> target = this.NewTarget())
                {
                    this.AssertSingleIteration(seed: 03293000, expectPagerToFail: true);
                    this.AssertSingleIteration(seed: 03293001, expectAggregationToFail: true);
                    this.AssertSingleIteration(seed: 03293002, expectPagerToFail: true);

                    // Reset consecutive error count to zero:
                    this.AssertSingleIteration(seed: 03294000);

                    // TODO: Not currently covering expectFailureStoringResults (as the way this test framework is hooked up is 
                    // leading to undesired behavior using expectFailureStoringResults adjacent to other error conditions (as 
                    // throwing of the exception is delayed, possibly until disposal.
                    this.AssertSingleIteration(seed: 03292000, expectAggregationToFail: true);
                    this.AssertSingleIteration(seed: 03292001, expectPagerToFail: true);
                    this.AssertSingleIteration(seed: 03292002, expectAggregationToFail: true);
                    this.AssertSingleIteration(seed: 03292003, expectPagerToFail: true);
                    this.AssertSingleIteration(seed: 03292004, expectAggregationToFail: true);
                    this.AssertSingleIteration(seed: 03292005, expectPagerToFail: true);
                    this.AssertSingleIteration(seed: 03292006, expectAggregationToFail: true);
                    this.AssertSingleIteration(seed: 03292007, expectPagerToFail: true);
                    this.AssertSingleIteration(seed: 03292008, expectAggregationToFail: true);
                    this.AssertSingleIteration(seed: 03292009, expectPagerToFail: true);
                    this.AssertSingleIteration(seed: 03292010, expectAggregationToFail: true);
                    this.AssertSingleIteration(seed: 03292011, expectPagerToFail: true);
                    this.AssertSingleIteration(seed: 03292012, expectAggregationToFail: true);
                    this.AssertSingleIteration(seed: 03292013, expectPagerToFail: true);
                    this.AssertSingleIteration(seed: 03292014, expectAggregationToFail: true);
                    this.AssertSingleIteration(seed: 03292015, expectPagerToFail: true);
                    this.AssertSingleIteration(seed: 03292016, expectAggregationToFail: true);
                    this.AssertSingleIteration(seed: 03292017, expectPagerToFail: true);
                    this.AssertSingleIteration(seed: 03292018, expectAggregationToFail: true);
                    this.AssertSingleIteration(seed: 03292019, expectPagerToFail: true);

                    this.PushError("Too many consecutive errors during aggregation of PriceOfBitcoin data from PagerWrapper.FixedToStringValue using DAaVE.Samples.SampleDataPointAggregator; re-throwing", typeof(FormatException));
                    success = true;
                }
            }
            catch (AggregateException)
            {
                Assert.IsTrue(success, "19 consecutive exceptions were absorbed");
                rethrew = true;
            }
            finally
            {
                Assert.IsTrue(rethrew, "20th consecutive exception was rethrown");
            }
        }

        /// <summary>
        /// Asserts that three <see cref="TimeSpan"/> values are strictly increasing.
        /// </summary>
        /// <param name="lowerBound">The lowest of the three values.</param>
        /// <param name="mid">The middle value.</param>
        /// <param name="upperBound">The highest of the three values.</param>
        /// <param name="message">Message to use to describe a failure.</param>
        /// <param name="parameters">String-format parameters for <paramref name="message"/>.</param>
        private static void AssertTimeSpanBetween(
            TimeSpan lowerBound,
            TimeSpan mid,
            TimeSpan upperBound,
            string message,
            params object[] parameters)
        {
            Assert.IsTrue(lowerBound < mid, message + " (timespan too short; expecting longer than " + lowerBound + ")", parameters);
            Assert.IsTrue(mid < upperBound, message + " (timespan too long; expecting shorter than " + upperBound + ")", parameters);
        }

        /// <summary>
        /// Generate predictable (based on test build) but seemingly random sample data for use in tests.
        /// </summary>
        /// <param name="seed">
        /// Seed. A constant seed will produce consistent results. A small change in the seed used should produce massively 
        /// different results.</param>
        /// <param name="sampleRawDataMaximumLength">Amount of data point observations. Can be zero.</param>
        /// <param name="sampleAggregatedDataMaximumLength">Amount of aggregated data points. Can be zero.</param>
        /// <param name="sampleRawData">Will be populated with sample raw data point observations.</param>
        /// <param name="sampleAggregatedData">Will be populated with aggregated data points.</param>
        /// <returns>Whether or not to consider the sample raw data a partial page.</returns>
        private static bool GenerateSampleData(
            int seed,
            int sampleRawDataMaximumLength,
            int sampleAggregatedDataMaximumLength,
            out DataPointObservation[] sampleRawData,
            out AggregatedDataPoint[] sampleAggregatedData)
        {
            Random psuedoRandomNumberGenerator = new Random(seed);

            int sampleRawDataLength = sampleRawDataMaximumLength == 0 ? 0 : psuedoRandomNumberGenerator.Next(1, sampleRawDataMaximumLength + 1);
            sampleRawData = new DataPointObservation[sampleRawDataLength];
            for (int i = 0; i < sampleRawData.Length; i++)
            {
                sampleRawData[i] = new DataPointObservation(NewRandomDateTimeUtc(psuedoRandomNumberGenerator), value: psuedoRandomNumberGenerator.NextDouble());
            }

            int sampleAggregatedDataLength = sampleAggregatedDataMaximumLength == 0 ? 0 : psuedoRandomNumberGenerator.Next(1, sampleAggregatedDataMaximumLength + 1);
            sampleAggregatedData = new AggregatedDataPoint[sampleAggregatedDataLength];
            for (int i = 0; i < sampleAggregatedData.Length; i++)
            {
                sampleAggregatedData[i] = new AggregatedDataPoint();
                sampleAggregatedData[i].UtcTimestamp = NewRandomDateTimeUtc(psuedoRandomNumberGenerator);
                sampleAggregatedData[i].AggregatedValue = psuedoRandomNumberGenerator.NextDouble();
            }

            return psuedoRandomNumberGenerator.NextDouble() < 0.5;
        }

        /// <summary>
        /// Generates a random date using the provided random number generator.
        /// </summary>
        /// <param name="r">Random number to generate.</param>
        /// <returns>A new random <see cref="DateTime"/> of type <see cref="DateTimeKind.Utc"/>.</returns>
        private static DateTime NewRandomDateTimeUtc(Random r)
        {
            return DateTime.SpecifyKind(
                value: DateTime.FromOADate(r.NextDouble() * DateTime.MaxValue.ToOADate()),
                kind: DateTimeKind.Utc);
        }

        /// <summary>
        /// Asserts all expectations for a single iteration of the control logic within the target
        /// <see cref="DataAggregationBackgroundWorker{TDataPointTypeEnum}"/>.  Returns when all
        /// expectations have been met.
        /// </summary>
        /// <param name="noRawData">
        /// Whether to simulate a situation where the pager is returning empty pages.
        /// </param>
        /// <param name="noAggregatedData">
        /// Whether to simulate a situation where the aggregation of the data provided by the
        /// pager consists of no aggregated data points.
        /// </param>
        /// <param name="expectPagerToFail">
        /// Whether to throw an exception from within the pager when asked to retrieve raw data point observations.
        /// </param>
        /// <param name="expectAggregationToFail">
        /// Whether to throw an exception from within the aggregator when asked to aggregate raw data point observations.
        /// </param>
        /// <param name="expectFailureStoringResults">
        /// Whether to throw an exception when asked to persist aggregation results.
        /// </param>
        /// <param name="seed">
        /// Seed to use for pseudo-random generation of sample data.
        /// </param>
        private void AssertSingleIteration(
            bool noRawData = false,
            bool noAggregatedData = false,
            bool expectPagerToFail = false,
            bool expectAggregationToFail = false,
            bool expectFailureStoringResults = false,
            int seed = 0)
        {
            string errorMessage = 
                "Exception during aggregation of " + ArbitraryDataPointType + " data from PagerWrapper.FixedToStringValue" + 
                " using DAaVE.Samples.SampleDataPointAggregator";

            if (expectPagerToFail)
            {
                this.PushError(errorMessage, typeof(FormatException));
                Assert.IsFalse(expectAggregationToFail, "Aggregation won't happen, so cannot fail.");
                Assert.IsFalse(expectFailureStoringResults, "Result storage won't happen, so cannot fail.");
            }
            else if (expectAggregationToFail)
            {
                this.PushError(errorMessage, typeof(DivideByZeroException));
                Assert.IsFalse(expectFailureStoringResults, "Result storage won't happen, so cannot fail.");
            }
            else if (expectFailureStoringResults)
            {
                this.PushError(errorMessage, typeof(MissingMemberException));
            }

            DataPointObservation[] sampleRawData;
            AggregatedDataPoint[] sampleAggregatedData;
            bool isPartial = GenerateSampleData(
                seed: seed,
                sampleRawDataMaximumLength: noRawData ? 0 : 50,
                sampleAggregatedDataMaximumLength: noAggregatedData ? 0 : 50,
                sampleRawData: out sampleRawData,
                sampleAggregatedData: out sampleAggregatedData);

            ManualResetEventSlim aggregationResultReceivedByPagerDataObject = new ManualResetEventSlim(false);
            SampleConsecutiveDataPointObservationsCollection dataObjectFromPager = new SampleConsecutiveDataPointObservationsCollection(
                sampleRawData.OrderBy(d => d.UtcTimestamp),
                aggregationResult =>
                {
                    Assert.IsTrue(
                        aggregationResult.SequenceEqual(sampleAggregatedData),
                        "Entire aggregation result should be sent verbatim to the original pager-supplied data set");
                    
                    if (expectFailureStoringResults)
                    {
                        throw new MissingMemberException();
                    }

                    aggregationResultReceivedByPagerDataObject.Set();
                },
                isPartial);

            if (expectPagerToFail)
            {
                this.ExpectPagerRequest(exceptionToThrow: new FormatException());
            }
            else
            {
                this.ExpectPagerRequest(dataToReturn: dataObjectFromPager);
            }

            if (noRawData || expectPagerToFail)
            {
                // The aggregator should not be called; this iteration is now complete.
                return;
            }

            ConsecutiveDataPointObservationsCollection dataProvidedToAggregator = 
                expectAggregationToFail ?
                    this.ExpectAggregationRequestResponse(exceptionToThrow: new DivideByZeroException()) :
                    this.ExpectAggregationRequestResponse(response: sampleAggregatedData);

            Assert.IsTrue(
                dataProvidedToAggregator.SequenceEqual(sampleRawData.OrderBy(d => d.UtcTimestamp)),
                "Entire pager output should be passed verbatim as a single data-set to the aggregator for aggregation");

            if (noAggregatedData || expectAggregationToFail) 
            {
                // No aggregation results to report.
                this.postTestVerifications.Enqueue(() => 
                {
                    Assert.IsFalse(aggregationResultReceivedByPagerDataObject.IsSet, "Unexpected aggregator results provided to pager data object");
                });
                return;
            }

            if (!expectFailureStoringResults)
            {
                Assert.IsTrue(aggregationResultReceivedByPagerDataObject.Wait(Timeout), "Aggregator results not provided to originating pager data object");
            }
        }
    }
}
