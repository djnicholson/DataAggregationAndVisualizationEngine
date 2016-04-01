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
        private BlockingCollection<ConsecutiveDataPointObservationsCollection> aggregationRequests;

        /// <summary>
        /// Generators (that may throw) of responses that can be given to aggregation requests.
        /// </summary>
        private BlockingCollection<Func<IEnumerable<AggregatedDataPoint>>> aggregationResponseGenerators;

        /// <summary>
        /// Expected errors.
        /// </summary>
        private BlockingCollection<Tuple<string, Type>> expectedErrors;

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
                this.aggregationRequests.Add(aggregationRequest);
                Func<IEnumerable<AggregatedDataPoint>> aggregationGenerator = this.aggregationResponseGenerators.Take();
                return aggregationGenerator();
            });

            this.errorSink = new CallbackErrorSink((message, hasException, exception) =>
            {
                Debug.WriteLine(
                    "ERROR (possibly expected by test): '" + message + "'" + (hasException ? "; exception: " + exception : string.Empty));

                Tuple<string, Type> expectedError = this.expectedErrors.Take();

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
            this.aggregationRequests = new BlockingCollection<ConsecutiveDataPointObservationsCollection>(new ConcurrentQueue<ConsecutiveDataPointObservationsCollection>());

            this.aggregationResponseGenerators = new BlockingCollection<Func<IEnumerable<AggregatedDataPoint>>>(new ConcurrentQueue<Func<IEnumerable<AggregatedDataPoint>>>());

            this.expectedErrors = new BlockingCollection<Tuple<string, Type>>(new ConcurrentQueue<Tuple<string, Type>>());

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
            Assert.AreEqual(0, this.aggregationResponseGenerators.Count, "An aggregation request was not responded to as requested by the test");
            Assert.AreEqual(0, this.expectedErrors.Count, "An expected error did not happen");
        }

        /// <summary>
        /// Disposes the pager created during construction.
        /// </summary>
        public void Dispose()
        {
            this.aggregationRequests.Dispose();
            this.aggregationResponseGenerators.Dispose();
            this.expectedErrors.Dispose();
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
        private static void RunWithDebuggingContext(Action action, params object[] context)
        {
            Debug.WriteLine("// ~~ RunWithDebuggingContext [[[[");

            try
            {
                Assert.IsNotNull(action);
                Assert.IsNotNull(context);

                string contextAsString = string.Join(string.Empty, context);
                Debug.WriteLine("// Context: " + contextAsString);

                action();
            }
            finally
            {
                Debug.WriteLine("// ~~ RunWithDebuggingContext ]]]]");
            }
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

            RunWithDebuggingContext(
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

            RunWithDebuggingContext(
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

            RunWithDebuggingContext(
                () => 
                {
                    this.aggregationResponseGenerators.Add(() => response);
                    unaggregatedData = this.aggregationRequests.Take();
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

            RunWithDebuggingContext(
                () =>
                {
                    unaggregatedData = this.aggregationRequests.Take();
                    this.aggregationResponseGenerators.Add(() => { throw exceptionToThrow; });
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

            this.expectedErrors.Add(new Tuple<string, Type>(errorMessage, exceptionType));
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
