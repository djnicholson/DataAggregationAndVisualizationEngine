// <copyright file="DataAggregationBackgroundWorkerFunctionalTests.cs" company="David Nicholson">
//     Copyright (c) David Nicholson. All rights reserved.
// </copyright>
// <summary>See class header.</summary>

namespace DAaVE.Library.Tests
{
    using System;

    using DAaVE.Library.DataAggregation;
    using DAaVE.Library.ErrorHandling.ErrorSinks;
    using DAaVE.Samples;

    using Microsoft.VisualStudio.TestTools.UnitTesting;
    
    /// <summary>
    /// Functional tests (make and depend on assumptions about the shared thread pool) for the 
    /// <see cref="DataAggregationBackgroundWorker{TDataPointTypeEnum}"/> class.
    /// </summary>
    [TestClass]
    public class DataAggregationBackgroundWorkerFunctionalTests
    {
        /// <summary>
        /// Basic sanity test that an instance of <see cref="DataAggregationBackgroundWorker{TDataPointTypeEnum}"/> can successfully
        /// be initialized and disposed (without any actual verification of what it does).
        /// </summary>
        [TestMethod]
        public void SmokeTest()
        {
            var type = SampleDataPointType.PriceOfBitcoin;
            var aggregator = new NoOpAggregator();
            var errorSink = new CallbackErrorSink((message, hasException, exception) => { });
            using (var pager = new SampleDataPointPager<SampleDataPointType>())
            using (var target = new DataAggregationBackgroundWorker<SampleDataPointType>(type, aggregator, pager, errorSink))
            {
            }
        }
    }
}
