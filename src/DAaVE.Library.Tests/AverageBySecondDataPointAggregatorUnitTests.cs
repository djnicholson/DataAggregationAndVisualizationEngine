// <copyright file="AverageBySecondDataPointAggregatorUnitTests.cs" company="David Nicholson">
//     Copyright (c) David Nicholson. All rights reserved.
// </copyright>
// <summary>See class header.</summary>

namespace DAaVE.Library.Tests
{
    using System;

    using DAaVE.Library.DataAggregation.Aggregators;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    /// <summary>
    /// Unit tests for the <see cref="AverageBySecondDataPointAggregator"/> class.
    /// </summary>
    [TestClass]
    public class AverageBySecondDataPointAggregatorUnitTests
    {
        /// <summary>
        /// Verify that <see cref="AverageBySecondDataPointAggregator.TruncateToSecondsUtc(DateTime)"/>
        /// works correctly.
        /// </summary>
        [TestMethod]
        public void TruncateToSecondsUtcTest()
        {
            int[] millisecondValues = { 27, 0 };
            DateTimeKind[] kinds = { DateTimeKind.Local, DateTimeKind.Unspecified, DateTimeKind.Utc };

            foreach (int milliseconds in millisecondValues)
            {
                foreach (DateTimeKind kind in kinds)
                {
                    DateTime input = new DateTime(2016, 3, 9, 17, 52, 27, milliseconds, kind);

                    DateTime output = AverageBySecondDataPointAggregator.TruncateToSecondsUtc(input);

                    Assert.AreEqual(input.Year, output.Year, "Input: {0}, Output {1}", input, output);
                    Assert.AreEqual(input.Month, output.Month, "Input: {0}, Output {1}", input, output);
                    Assert.AreEqual(input.Day, output.Day, "Input: {0}, Output {1}", input, output);
                    Assert.AreEqual(input.Hour, output.Hour, "Input: {0}, Output {1}", input, output);
                    Assert.AreEqual(input.Minute, output.Minute, "Input: {0}, Output {1}", input, output);
                    Assert.AreEqual(input.Second, output.Second, "Input: {0}, Output {1}", input, output);
                    Assert.AreEqual(input.Kind, output.Kind, "Input: {0}, Output {1}", input, output);

                    Assert.AreEqual(0, output.Millisecond, "Input: {0}, Output {1}", input, output);
                }
            }
        }
    }
}
