// <copyright file="DataAggregationBackgroundWorkerFunctionalTests.cs" company="David Nicholson">
//     Copyright (c) David Nicholson. All rights reserved.
// </copyright>
// <summary>See class header.</summary>

namespace DAaVE.Library.Tests
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Diagnostics;
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
    public sealed partial class DataAggregationBackgroundWorkerFunctionalTests : IDisposable
    {
        /// <summary>
        /// An arbitrary data point type used throughout these tests.
        /// </summary>
        private const SampleDataPointType ArbitraryDataPointType = SampleDataPointType.PriceOfBitcoin;

        /// <summary>
        /// Pager used throughout tests.
        /// </summary>
        private readonly SampleDataPointPager<SampleDataPointType> pager = new SampleDataPointPager<SampleDataPointType>();

        /// <summary>
        /// Aggregator used throughout tests.
        /// </summary>
        private readonly SampleDataPointAggregator aggregator = new SampleDataPointAggregator();

        /// <summary>
        /// All incoming aggregation requests that are not yet expected.
        /// </summary>
        private readonly ConcurrentBag<AggregationRequestEventArgs> aggregationRequests = 
            new ConcurrentBag<AggregationRequestEventArgs>();

        /// <summary>
        /// Error sink used throughout tests.
        /// </summary>
        private CallbackErrorSink errorSink = new CallbackErrorSink((message, hasException, exception) =>
        {
            // TODO...
            Assert.Fail("TODO");
        });

        /// <summary>
        /// Initializes a new instance of the DataAggregationBackgroundWorkerFunctionalTests class.
        /// </summary>
        public DataAggregationBackgroundWorkerFunctionalTests()
        {
            this.aggregator.OnAggregate += (_, aggregationRequest) =>
            {
                aggregationRequests.Add(aggregationRequest);
            };
        }

        /// <summary>
        /// Disposes the pager created during construction.
        /// </summary>
        public void Dispose()
        {
            this.pager.Dispose();
        }

        /// <summary>
        /// Runs some code and blocks the calling thread until that code terminates. If the
        /// code is still running after the <see cref="Timeout"/> has elapsed a call is made 
        /// to <see cref="Microsoft.VisualStudio.TestTools.UnitTesting.Assert.Fail(string)"/>.
        /// </summary>
        /// <param name="action">The code to run.</param>
        /// <param name="context">
        /// Arbitrary context to provide in the call to 
        /// <see cref="Microsoft.VisualStudio.TestTools.UnitTesting.Assert.Fail(string)"/>.
        /// Must not be null but can be empty. Providing objects that have a suitable
        /// <see cref="object.ToString"/> overload is recommended.
        /// </param>
        private static void RunWithTimeout(Action action, params object[] context)
        {
            Debug.WriteLine("Enter: DataAggregationBackgroundWorkerFunctionalTests.RunWithTimeout");

            string contextAsString = string.Join(string.Empty, context);
            Debug.WriteLine("Context: " + contextAsString);

            using (Task worker = Task.Run(action))
            {
                bool actionCompletedWithinTimeout = worker.Wait(Timeout);
                if (!actionCompletedWithinTimeout)
                {
                    Assert.Fail(
                        "An expectation within this test is not yet realized. The condition was first expected about " + Timeout +
                        " ago though, so the test has been marked as 'failed' to ease identification of the specific expectation that may need" +
                        " investigation. The stack trace of this failure will identify the location within the test-code that has the expectation" +
                        " . The following context may be useful in further debugging: " + contextAsString);
                }
            }

            Debug.WriteLine("Success: DataAggregationBackgroundWorkerFunctionalTests.RunWithTimeout");
        }

        /// <summary>
        /// Initializes a new object to test.
        /// </summary>
        /// <returns>The new object.</returns>
        private DataAggregationBackgroundWorker<SampleDataPointType> NewTarget()
        {
            return new DataAggregationBackgroundWorker<SampleDataPointType>(
                ArbitraryDataPointType, 
                this.aggregator, 
                this.pager, 
                this.errorSink);
        }

        /// <summary>
        /// Block until a request has been made by one of the created targets to the pager
        /// for a page of data. Fails the current test if more the <see cref="Timeout"/>
        /// elapses before this happens.
        /// </summary>
        /// <param name="dataToReturn">Data to return to the target.</param>
        private void ExpectPagerRequest(ConsecutiveDataPointObservationsCollection dataToReturn)
        {
            RunWithTimeout(
                () =>
                {
                    ManualResetEventSlim success = new ManualResetEventSlim(initialState: false);

                    this.pager.QueueObservation(
                        ArbitraryDataPointType, 
                        () =>
                        {
                            success.Set();
                            return dataToReturn;
                        });

                    success.Wait();
                },
                "Awaiting invocation of the pager; planning to return: [",
                dataToReturn,
                "]");
        }

        /// <summary>
        /// Block until a request has been made by one of the created targets to the aggregator
        /// to perform an aggregation. Fails the current test if more the <see cref="Timeout"/>
        /// elapses before this happens.
        /// </summary>
        /// <param name="unaggregatedData">
        /// Un-aggregated data that is expected. Any requests to aggregate data that does not exactly 
        /// match this parameter will be ignored.
        /// </param>
        /// <param name="dataToReturn">Data to return to the target.</param>
        private void ExpectAggregationRequest(
            ConsecutiveDataPointObservationsCollection unaggregatedData, 
            IEnumerable<AggregatedDataPoint> dataToReturn)
        {
            RunWithTimeout(
                () => 
                {
                    // TODO...
                    Assert.Fail("TODO");
                },
                "Awaiting invocation of the aggregator for [",
                unaggregatedData,
                "]; planning to return: [",
                dataToReturn,
                "]");
        }
    }
}
