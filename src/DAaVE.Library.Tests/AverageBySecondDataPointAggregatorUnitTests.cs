// <copyright file="AverageBySecondDataPointAggregatorUnitTests.cs" company="David Nicholson">
//     Copyright (c) David Nicholson. All rights reserved.
// </copyright>
// <summary>See class header.</summary>

namespace DAaVE.Library.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using DAaVE.Library.DataAggregation;
    using DAaVE.Library.DataAggregation.Aggregators;
    using DAaVE.Library.Storage;
    using DAaVE.Samples;

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

        /// <summary>
        /// Verify that <see cref="AverageBySecondDataPointAggregator.Aggregate(Storage.ConsecutiveDataPointObservationsCollection)"/>
        /// accepts empty observation sequences.
        /// </summary>
        [TestMethod]
        public void AggregateEmptyObservationsTest()
        {
            AverageBySecondDataPointAggregator target = new AverageBySecondDataPointAggregator();
            IEnumerable<AggregatedDataPoint> result = target.Aggregate(ConsecutiveDataPointObservationsCollection.Empty);
            Assert.IsFalse(result.Any());
        }

        /// <summary>
        /// Verify that <see cref="AverageBySecondDataPointAggregator.Aggregate(Storage.ConsecutiveDataPointObservationsCollection)"/>
        /// correctly aggregates a sample data point observation sequence.
        /// </summary>
        [TestMethod]
        public void AggregateTest()
        {
            const double Group1Sentinel = 0.0;
            const double Group2Sentinel = 100.0;
            const double Group3Sentinel = 10000.0;

            IEnumerable<AggregatedDataPoint> aggregation = DummyAggregation(
                new DataPointObservation(new DateTime(2016, 3, 10, 17, 44, 10, 999, DateTimeKind.Utc), Group1Sentinel),
                new DataPointObservation(new DateTime(2016, 3, 10, 17, 44, 11, 0, DateTimeKind.Utc), Group2Sentinel),
                new DataPointObservation(new DateTime(2016, 3, 10, 17, 44, 11, 1, DateTimeKind.Utc), Group2Sentinel),
                new DataPointObservation(new DateTime(2016, 3, 10, 17, 44, 11, 999, DateTimeKind.Utc), Group2Sentinel),
                new DataPointObservation(new DateTime(2016, 3, 10, 17, 44, 12, 0, DateTimeKind.Utc), Group3Sentinel),
                new DataPointObservation(new DateTime(2016, 3, 10, 17, 44, 12, 1, DateTimeKind.Utc), Group3Sentinel));

            AggregatedDataPoint[] result = aggregation.ToArray();

            Assert.AreEqual(3, result.Length);

            Assert.AreEqual(new DateTime(2016, 3, 10, 17, 44, 10, 0, DateTimeKind.Utc), result[0].UtcTimestamp);
            Assert.AreEqual(new DateTime(2016, 3, 10, 17, 44, 11, 0, DateTimeKind.Utc), result[1].UtcTimestamp);
            Assert.AreEqual(new DateTime(2016, 3, 10, 17, 44, 12, 0, DateTimeKind.Utc), result[2].UtcTimestamp);

            Assert.AreEqual(Group1Sentinel, result[0].AggregatedValue);
            Assert.AreEqual(Group2Sentinel, result[1].AggregatedValue);
            Assert.AreEqual(Group3Sentinel, result[2].AggregatedValue);
        }

        /// <summary>
        /// Creates a new instance of <see cref="AverageBySecondDataPointAggregator"/> and uses it to aggregate
        /// some mock data.
        /// </summary>
        /// <param name="observations">
        /// The (UTC) dates to use for the mock observations. The amount of mock observations generated will be
        /// equal to the length of this array.
        /// </param>
        /// <returns>The results of the aggregation.</returns>
        private static IEnumerable<AggregatedDataPoint> DummyAggregation(params DataPointObservation[] observations)
        {
            IOrderedEnumerable<DataPointObservation> dummyObservations = observations.OrderBy(o => o.UtcTimestamp);

            const bool DoesntMatter = true;

            ConsecutiveDataPointObservationsCollection aggregationInput =
                new SampleConsecutiveDataPointObservationsCollection(dummyObservations, isPartial: DoesntMatter);

            AverageBySecondDataPointAggregator target = new AverageBySecondDataPointAggregator();

            return target.Aggregate(aggregationInput);
        }
    }
}
