﻿// <copyright file="AverageBySecondDataPointAggregator.cs" company="David Nicholson">
//     Copyright (c) David Nicholson. All rights reserved.
// </copyright>
// <summary>See class header.</summary>

namespace DAaVE.Library.DataAggregation.Aggregators
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// Generates aggregate data points spaced at exact 1-second intervals. Each point is generated by taking the average
    /// (mean) of all actual data points collected in the following second.
    /// </summary>
    public sealed class AverageBySecondDataPointAggregator : IDataPointAggregator
    {
        /// <inheritdoc />
        public IEnumerable<AggregatedDataPoint> Aggregate(IOrderedEnumerable<DataPoint> contiguousDataSegment)
        {
            IEnumerable<DataPoint> remainingPoints = contiguousDataSegment;
            while (remainingPoints.Any())
            {
                DateTime aggregateUtcTime = TruncateToSecondsUtc(remainingPoints.First().UtcTimestamp);

                Func<DataPoint, bool> inAggregationWindow =
                    p => TruncateToSecondsUtc(p.UtcTimestamp).Ticks == aggregateUtcTime.Ticks;

                IEnumerable<DataPoint> pointsUnderConsideration = remainingPoints.TakeWhile(inAggregationWindow);
                remainingPoints = remainingPoints.SkipWhile(p => !inAggregationWindow(p));

                yield return new AggregatedDataPoint()
                {
                    UtcTimestamp = aggregateUtcTime,
                    AggregatedValue = pointsUnderConsideration.Average(p => p.Value),
                };
            }

            yield break;
        }

        /// <summary>
        /// Truncates a time-stamp to its equivalent value if observed at a resolution of 1 second (with
        /// observations happening on the 0'th millisecond of each second).
        /// </summary>
        /// <param name="input">The time-stamp to truncate.</param>
        /// <returns>The input time-stamp with any components more accurate than seconds set to 0.</returns>
        private static DateTime TruncateToSecondsUtc(DateTime input)
        {
            return new DateTime(input.Year, input.Month, input.Day, input.Hour, input.Minute, input.Second, input.Kind);
        }
    }
}
