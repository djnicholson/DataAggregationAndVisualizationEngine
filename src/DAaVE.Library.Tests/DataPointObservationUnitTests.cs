// <copyright file="DataPointObservationUnitTests.cs" company="David Nicholson">
//     Copyright (c) David Nicholson. All rights reserved.
// </copyright>
// <summary>See class header.</summary>

namespace DAaVE.Library.Tests
{
    using System;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    /// <summary>
    /// Unit tests for the <see cref="DataPointObservation"/> class.
    /// </summary>
    [TestClass]
    public class DataPointObservationUnitTests
    {
        /// <summary>
        /// Tests that the property setters and getters function correctly.
        /// </summary>
        [TestMethod]
        public void AccessorTests()
        {
            DataPointObservation target = new DataPointObservation();

            target.UtcTimestamp = new DateTime(2016, 6, 23, 19, 40, 59, DateTimeKind.Utc);
            Assert.AreEqual(new DateTime(2016, 6, 23, 19, 40, 59, DateTimeKind.Utc), target.UtcTimestamp);

            target.Value = 0.6;
            Assert.AreEqual(0.6, target.Value);
        }

        /// <summary>
        /// Tests that equality is correctly implemented in terms of both 
        /// <see cref="DataPointObservation.UtcTimestamp"/> and
        /// <see cref="DataPointObservation.Value"/>.
        /// </summary>
        [TestMethod]
        public void EqualityTests()
        {
            DateTime dateTimeA = new DateTime(2016, 6, 23, 19, 45, 23);
            DateTime dateTimeB = new DateTime(2016, 6, 23, 19, 45, 24);
            double valueX = 0.3;
            double valueY = 0.4;

            DataPointObservation point1 = new DataPointObservation();
            point1.UtcTimestamp = dateTimeA;
            point1.Value = valueX;

            DataPointObservation point2 = new DataPointObservation();
            point2.UtcTimestamp = dateTimeA;
            point2.Value = valueX;

            DataPointObservation point3 = new DataPointObservation();
            point3.UtcTimestamp = dateTimeA;
            point3.Value = valueY;

            DataPointObservation point4 = new DataPointObservation();
            point4.UtcTimestamp = dateTimeB;
            point4.Value = valueX;

            Assert.IsTrue(point1 == point2);
            Assert.IsFalse(point1 == point3);
            Assert.IsFalse(point1 == point4);
            
            Assert.IsTrue(point1.GetHashCode() == point2.GetHashCode());
            Assert.IsFalse(point1.GetHashCode() == point3.GetHashCode());
            Assert.IsFalse(point1.GetHashCode() == point4.GetHashCode());
            
            Assert.IsFalse(point1 != point2);
            Assert.IsTrue(point1 != point3);
            Assert.IsTrue(point1 != point4);

            Assert.IsTrue(point1.Equals(point2));
            Assert.IsFalse(point1.Equals(point3));
            Assert.IsFalse(point1.Equals(point4));

            Assert.IsFalse(point1.Equals("A String object"));
        }
    }
}
