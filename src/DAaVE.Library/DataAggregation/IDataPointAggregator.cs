// <copyright file="IDataPointAggregator.cs" company="David Nicholson">
//     Copyright (c) David Nicholson. All rights reserved.
// </copyright>

namespace DAaVE.Library.DataAggregation
{
    using System.Collections.Generic;

    /// <summary>
    /// Given a continuous segment of raw collected data points, produces a stream of corresponding
    /// aggregated points.
    /// </summary>
    public interface IDataPointAggregator
    {
        /// <summary>
        /// Aggregates the provided raw data points.
        /// Implementations must be deterministic (to support stateless aggregation failover).
        /// </summary>
        /// <param name="continuousDataSegment">A continuous segment of raw collected data points.</param>
        /// <returns>A stream of aggregated points.</returns>
        IEnumerable<AggregatedDataPoint> Aggregate(IEnumerable<DataPoint> continuousDataSegment);
    }
}
