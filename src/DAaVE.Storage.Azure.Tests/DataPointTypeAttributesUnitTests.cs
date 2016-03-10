// <copyright file="DataPointTypeAttributesUnitTests.cs" company="David Nicholson">
//     Copyright (c) David Nicholson. All rights reserved.
// </copyright>
// <summary>See class header.</summary>

namespace DAaVE.Storage.Azure.Tests
{
    using System;

    using DAaVE.Samples;

    using DAaVE.Storage.Azure;

    using Microsoft.VisualStudio.TestTools.UnitTesting;
    
    /// <summary>
    /// Unit tests for the <see cref="DataPointTypeAttributes{TDataPointTypeEnum}"/> class.
    /// </summary>
    [TestClass]
    public class DataPointTypeAttributesUnitTests
    {
        /// <summary>
        /// Verify that <see cref="DataPointTypeAttributes{TDataPointTypeEnum}.GetAggregationInputWindowSizeInMinutes(TDataPointTypeEnum)"/> 
        /// aims to store about 1000 observations per partition (based on the observation rate supplied by data point type attribution).
        /// </summary>
        [TestMethod]
        public void GetAggregationInputWindowSizeInMinutesCustomization()
        {
            DataPointTypeAttributes<SampleDataPointType> target = new DataPointTypeAttributes<SampleDataPointType>();

            Assert.AreEqual<double>(
                1.024,
                target.GetAggregationInputWindowSizeInMinutes(SampleDataPointType.PriceOfBitcoin));

            Assert.AreEqual<double>(
                1474560.0,
                Math.Round(target.GetAggregationInputWindowSizeInMinutes(SampleDataPointType.MinutesOfDaylight)));

            Assert.AreEqual<double>(
                85.0,
                Math.Round(target.GetAggregationInputWindowSizeInMinutes(SampleDataPointType.HttpRequests)));
        }
    }
}
