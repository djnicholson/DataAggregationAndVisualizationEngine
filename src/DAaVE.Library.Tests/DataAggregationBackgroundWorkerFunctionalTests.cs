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
    /// TODO: Don't use partial classes here; move the helper logic into a helper class to better hide it.
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
        private readonly PagerWrapper pager;

        /// <summary>
        /// Aggregator used throughout tests.
        /// </summary>
        private readonly SampleDataPointAggregator aggregator;

        /// <summary>
        /// All incoming aggregation requests that are not yet reconciled with an expected request.
        /// </summary>
        private ConcurrentQueue<ConsecutiveDataPointObservationsCollection> aggregationRequests;

        /// <summary>
        /// Responses that can be given to aggregation requests.
        /// </summary>
        private ConcurrentQueue<IEnumerable<AggregatedDataPoint>> aggregationResponses;

        /// <summary>
        /// Expected errors.
        /// </summary>
        private ConcurrentQueue<Tuple<string, Type>> expectedErrors;

        /// <summary>
        /// Verifications to perform (on the main test thread) after the test completes.
        /// </summary>
        private ConcurrentQueue<Action> postTestVerifications;

        /// <summary>
        /// Error sink used throughout tests.
        /// </summary>
        private CallbackErrorSink errorSink;

        /// <summary>
        /// Initializes a new instance of the DataAggregationBackgroundWorkerFunctionalTests class.
        /// </summary>
        public DataAggregationBackgroundWorkerFunctionalTests()
        {
            this.pager = new PagerWrapper();

            this.aggregator = new SampleDataPointAggregator(aggregationRequest =>
            {
                this.aggregationRequests.Enqueue(aggregationRequest);

                return WaitDequeue(this.aggregationResponses);
            });

            this.errorSink = new CallbackErrorSink((message, hasException, exception) =>
            {
                Debug.Write(
                    "ERROR (possibly expected by test): '" + message + "'" + (hasException ? "; exception: " + exception : string.Empty));

                Tuple<string, Type> expectedError = WaitDequeue(this.expectedErrors);

                postTestVerifications.Enqueue(() => 
                { 
                    Assert.AreEqual(expectedError.Item1, message);
                    Assert.AreEqual(expectedError.Item2 != null, hasException);
                    if (hasException)
                    {
                        Assert.AreEqual(expectedError.Item2, exception.GetType());
                    }
                });
            });
        }

        /// <summary>
        /// Start out with no expectations.
        /// </summary>
        [TestInitialize]
        public void BeforeIndividualTest()
        {
            this.aggregationRequests = new ConcurrentQueue<ConsecutiveDataPointObservationsCollection>();

            this.aggregationResponses = new ConcurrentQueue<IEnumerable<AggregatedDataPoint>>();

            this.expectedErrors = new ConcurrentQueue<Tuple<string, Type>>();

            this.postTestVerifications = new ConcurrentQueue<Action>();
        }

        /// <summary>
        /// At the end of each test, all expectations must have been fulfilled.
        /// </summary>
        [TestCleanup]
        public void AfterIndividualTest()
        {
            Action postTestVerification;
            while (this.postTestVerifications.TryDequeue(out postTestVerification))
            {
                postTestVerification();
            }

            Assert.AreEqual(0, this.aggregationRequests.Count, "An expected aggregation request did not happen");
            Assert.AreEqual(0, this.aggregationResponses.Count, "An aggregation request was not responded to as requested by the test");
            Assert.AreEqual(0, this.expectedErrors.Count, "An expected error did not happen");
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

            Assert.IsNotNull(action);
            Assert.IsNotNull(context);

            string contextAsString = string.Join(string.Empty, context);
            Debug.WriteLine("Context: " + contextAsString);

            using (Task worker = Task.Run(action))
            {
                try
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
                finally
                {
                    worker.Wait();
                }
            }

            Debug.WriteLine("Success: DataAggregationBackgroundWorkerFunctionalTests.RunWithTimeout");
        }

        /// <summary>
        /// Blocks until the provided queue is non-empty then returns the first item
        /// in the queue.
        /// </summary>
        /// <typeparam name="T">Type of item in the queue.</typeparam>
        /// <param name="queue">The queue.</param>
        /// <returns>The first item in the queue.</returns>
        private static T WaitDequeue<T>(ConcurrentQueue<T> queue)
        {
            T result;
            while (!queue.TryDequeue(out result))
            {
                // TODO: Replace Thread.Sleep with a synchronization primitive.
                Thread.Sleep(TimeSpan.FromMilliseconds(100.0));
            }

            return result;
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
        /// Block until a request has been made to the pager for a page of data. Fails the current test 
        /// if more the <see cref="Timeout"/> elapses before this happens.
        /// </summary>
        /// <param name="dataToReturn">Data to return to the target.</param>
        private void ExpectPagerRequest(SampleConsecutiveDataPointObservationsCollection dataToReturn)
        {
            Assert.IsNotNull(dataToReturn);

            RunWithTimeout(
                () =>
                {
                    ManualResetEventSlim success = new ManualResetEventSlim(initialState: false);

                    this.pager.ExpectRequestForPageOfObservations(() =>
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
        /// Block until a request has been made to the pager for a page of data. Fails the current test 
        /// if more the <see cref="Timeout"/> elapses before this happens.
        /// </summary>
        /// <param name="exceptionToThrow">Exception to throw when the page is requested.</param>
        private void ExpectPagerRequest(Exception exceptionToThrow)
        {
            Assert.IsNotNull(exceptionToThrow);

            RunWithTimeout(
                () =>
                {
                    ManualResetEventSlim invoked = new ManualResetEventSlim(initialState: false);

                    this.pager.ExpectRequestForPageOfObservations(() =>
                    {
                        invoked.Set();
                        throw exceptionToThrow;
                    });

                    invoked.Wait();
                },
                "Awaiting invocation of the pager; planning to throw: [",
                exceptionToThrow,
                "]");
        }

        /// <summary>
        /// Block until a request has been made by one of the created targets to the aggregator
        /// to perform an aggregation. Fails the current test if more the <see cref="Timeout"/>
        /// elapses before this happens.
        /// </summary>
        /// <param name="response">Data to return to the target.</param>
        /// <returns>
        /// Un-aggregated data that was provided to the aggregator. Caller should validate that this
        /// was as expected.
        /// </returns>
        private ConsecutiveDataPointObservationsCollection ExpectAggregationRequestResponse(
            IEnumerable<AggregatedDataPoint> response)
        {
            Assert.IsNotNull(response);

            ConsecutiveDataPointObservationsCollection unaggregatedData = null;

            RunWithTimeout(
                () => 
                {
                    unaggregatedData = WaitDequeue(this.aggregationRequests);
                    this.aggregationResponses.Enqueue(response);
                },
                "Awaiting invocation of the aggregator; planning to return: [",
                response,
                "]");

            Assert.IsNotNull(unaggregatedData);

            return unaggregatedData;
        }

        /// <summary>
        /// Block until a request has been made by one of the created targets to the aggregator
        /// to perform an aggregation. Fails the current test if more the <see cref="Timeout"/>
        /// elapses before this happens.
        /// </summary>
        /// <param name="exceptionToThrow">Exception to throw when aggregation is requested.</param>
        /// <returns>
        /// Un-aggregated data that was provided to the aggregator. Caller should validate that this
        /// was as expected.
        /// </returns>
        private ConsecutiveDataPointObservationsCollection ExpectAggregationRequestResponse(Exception exceptionToThrow)
        {
            Assert.IsNotNull(exceptionToThrow);

            ConsecutiveDataPointObservationsCollection unaggregatedData = null;

            RunWithTimeout(
                () =>
                {
                    unaggregatedData = WaitDequeue(this.aggregationRequests);
                    throw exceptionToThrow;
                },
                "Awaiting invocation of the aggregator; planning to throw: [",
                exceptionToThrow,
                "]");

            Assert.IsNotNull(unaggregatedData);

            return unaggregatedData;
        }

        /// <summary>
        /// Registers an expected upcoming error condition (and verifies the contents of the error
        /// when it happens). Fails the current test if the error was not observed at the end of the test.
        /// </summary>
        /// <param name="errorMessage">The expected error message.</param>
        /// <param name="exceptionType">
        /// The type of exception expected (or null if the error should not have an associated exception).
        /// </param>
        private void PushError(string errorMessage, Type exceptionType)
        {
            Assert.IsNotNull(errorMessage);

            this.expectedErrors.Enqueue(new Tuple<string, Type>(errorMessage, exceptionType));
        }

        /// <summary>
        /// Wraps a <see cref="SampleDataPointPager{TDataPointTypeEnum}"/> but guarantees a fixed value
        /// be returned from <see cref="ToString"/>.
        /// </summary>
        private sealed class PagerWrapper : IDataPointPager<SampleDataPointType>, IDisposable
        {
            /// <summary>
            /// A fixed string used by <see cref="ToString"/>.
            /// </summary>
            public const string FixedToStringValue = "PagerWrapper.FixedToStringValue";

            /// <summary>
            /// The pager being wrapped.
            /// </summary>
            private readonly SampleDataPointPager<SampleDataPointType> pager = new SampleDataPointPager<SampleDataPointType>();

            /// <inheritdoc/>
            public void Dispose()
            {
                this.pager.Dispose();
            }

            /// <summary>
            /// Register an expectation for a request and provide code to handle that request when it arrives.
            /// </summary>
            /// <param name="handler">Code to handle the expected request.</param>
            public void ExpectRequestForPageOfObservations(Func<ConsecutiveDataPointObservationsCollection> handler)
            {
                this.pager.QueueObservation(ArbitraryDataPointType, handler);
            }

            /// <inheritdoc/>
            public Task<ConsecutiveDataPointObservationsCollection> GetPageOfObservations(SampleDataPointType type)
            {
                return this.pager.GetPageOfObservations(type);
            }

            /// <summary>
            /// Always returns <see cref="FixedToStringValue"/>.
            /// </summary>
            /// <returns>The value of <see cref="FixedToStringValue"/>.</returns>
            public override string ToString()
            {
                return FixedToStringValue;
            }
        }
    }
}
