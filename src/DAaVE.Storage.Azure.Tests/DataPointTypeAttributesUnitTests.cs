// <copyright file="DataPointTypeAttributesUnitTests.cs" company="David Nicholson">
//     Copyright (c) David Nicholson. All rights reserved.
// </copyright>
// <summary>See class header.</summary>

namespace DAaVE.Storage.Azure.Tests
{
    using System;

    using DAaVE.Library.DataCollection;
    using DAaVE.Storage.Azure;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    /// <summary>
    /// Unit tests for the <see cref="DataPointTypeAttributes{TDataPointTypeEnum}"/> class.
    /// </summary>
    [TestClass]
    public class DataPointTypeAttributesUnitTests
    {
        /// <summary>
        /// Some example data types that illustrate potentially differing observation
        /// rates.
        /// </summary>
        private enum TestDataPointTypes
        {
            /// <summary>
            /// How many minutes of day light were there during this calendar day?
            /// Observed once per day.
            /// </summary>
            [ExpectedObservationRate(InHours = 24.0)]
            MinutesOfDaylight = 0,

            /// <summary>
            /// Amount of web requests seen in some fixed interval.
            /// </summary>
            [ExpectedObservationRate(InSeconds = 5.0)]
            HttpRequests = 1,

            /// <summary>
            /// What is the current 'ask' for BTC? Observed very frequently.
            /// </summary>
            [ExpectedObservationRate(InMinutes = 0.001)]
            PriceOfBitcoin = 2,
        }

        /// <summary>
        /// Verify that <see cref="DataPointTypeAttributes{TDataPointTypeEnum}.GetAggregationInputWindowSizeInMinutes(TDataPointTypeEnum)"/> 
        /// aims to store about 1000 observations per partition (based on the observation rate supplied by data point type attribution).
        /// </summary>
        [TestMethod]
        public void GetAggregationInputWindowSizeInMinutesCustomization()
        {
            DataPointTypeAttributes<TestDataPointTypes> target = new DataPointTypeAttributes<TestDataPointTypes>();

            Assert.AreEqual<double>(
                1.024,
                target.GetAggregationInputWindowSizeInMinutes(TestDataPointTypes.PriceOfBitcoin));

            Assert.AreEqual<double>(
                1474560.0,
                Math.Round(target.GetAggregationInputWindowSizeInMinutes(TestDataPointTypes.MinutesOfDaylight)));

            Assert.AreEqual<double>(
                85.0,
                Math.Round(target.GetAggregationInputWindowSizeInMinutes(TestDataPointTypes.HttpRequests)));
        }
    }
}
