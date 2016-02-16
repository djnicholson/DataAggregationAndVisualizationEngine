// <copyright file="DataPointTypeAttributesUnitTests.cs" company="David Nicholson">
//     Copyright (c) David Nicholson. All rights reserved.
// </copyright>
// <summary>See class header.</summary>

namespace DAaVE.Storage.Azure.Tests
{
    using System;

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
            ////[ExpectedObservationRate(TimeSpan.FromDays(1.0))]
            MinutesOfDaylight = 0,

            /// <summary>
            /// What is the current 'ask' for BTC? Observed a minimum of once per
            /// second.
            /// </summary>
            ////[ExpectedObservationRate(TimeSpan.FromSeconds(0.5))]
            PriceOfBitcoin = 1,
        }

        /// <summary>
        /// A sample test.
        /// </summary>
        [TestMethod]
        public void GetAggregationInputWindowSizeInMinutesCustomization()
        {
            DataPointTypeAttributes<TestDataPointTypes> target = new DataPointTypeAttributes<TestDataPointTypes>();
            Assert.AreEqual<uint>(5, target.GetAggregationInputWindowSizeInMinutes(TestDataPointTypes.MinutesOfDaylight));
        }
    }
}
