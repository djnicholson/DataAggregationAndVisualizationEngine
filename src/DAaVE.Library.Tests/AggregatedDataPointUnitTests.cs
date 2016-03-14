// <copyright file="AggregatedDataPointUnitTests.cs" company="David Nicholson">
//     Copyright (c) David Nicholson. All rights reserved.
// </copyright>
// <summary>See class header.</summary>

namespace DAaVE.Library.Tests
{
    using System;

    using DAaVE.Library.DataAggregation;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    /// <summary>
    /// Unit tests for the <see cref="AggregatedDataPoint"/> class.
    /// </summary>
    [TestClass]
    public class AggregatedDataPointUnitTests
    {
        /// <summary>
        /// Tests that the property setters and getters function correctly.
        /// </summary>
        [TestMethod]
        public void AccessorTests()
        {
            AggregatedDataPoint target = new AggregatedDataPoint();

            target.UtcTimestamp = new DateTime(2016, 3, 13, 17, 28, 59, DateTimeKind.Utc);
            Assert.AreEqual(new DateTime(2016, 3, 13, 17, 28, 59, DateTimeKind.Utc), target.UtcTimestamp);

            target.AggregatedValue = 0.6;
            Assert.AreEqual(0.6, target.AggregatedValue);
        }

        /// <summary>
        /// Tests that equality is correctly implemented in terms of <see cref="AggregatedDataPoint.UtcTimestamp"/>.
        /// </summary>
        [TestMethod]
        public void EqualityTests()
        {
            AggregatedDataPoint point1 = new AggregatedDataPoint();
            point1.UtcTimestamp = new DateTime(2016, 3, 13, 17, 28, 2, DateTimeKind.Utc);
            point1.AggregatedValue = 0.3;

            AggregatedDataPoint point2 = new AggregatedDataPoint();
            point2.UtcTimestamp = new DateTime(2016, 3, 13, 17, 28, 2, DateTimeKind.Utc);
            point2.AggregatedValue = 0.4; // Invalid (aggregation must be consistent) but overlooked in this context

            AggregatedDataPoint point3 = new AggregatedDataPoint();
            point3.UtcTimestamp = new DateTime(2016, 3, 13, 17, 28, 3, DateTimeKind.Utc);
            point3.AggregatedValue = 0.5;

            Assert.IsTrue(point1 == point2);
            Assert.IsFalse(point1 == point3);
            Assert.IsTrue(point2 == point1);
            Assert.IsFalse(point2 == point3);
            Assert.IsFalse(point3 == point1);
            Assert.IsFalse(point3 == point2);

            Assert.IsTrue(point1.GetHashCode() == point2.GetHashCode());
            Assert.IsFalse(point1.GetHashCode() == point3.GetHashCode());
            Assert.IsTrue(point2.GetHashCode() == point1.GetHashCode());
            Assert.IsFalse(point2.GetHashCode() == point3.GetHashCode());
            Assert.IsFalse(point3.GetHashCode() == point1.GetHashCode());
            Assert.IsFalse(point3.GetHashCode() == point2.GetHashCode());

            Assert.IsFalse(point1 != point2);
            Assert.IsTrue(point1 != point3);
            Assert.IsFalse(point2 != point1);
            Assert.IsTrue(point2 != point3);
            Assert.IsTrue(point3 != point1);
            Assert.IsTrue(point3 != point2);

            Assert.IsTrue(point1.Equals(point2));
            Assert.IsFalse(point1.Equals(point3));
            Assert.IsTrue(point2.Equals(point1));
            Assert.IsFalse(point2.Equals(point3));
            Assert.IsFalse(point3.Equals(point1));
            Assert.IsFalse(point3.Equals(point2));

            Assert.IsFalse(point1.Equals("A String object"));
        }
    }
}
