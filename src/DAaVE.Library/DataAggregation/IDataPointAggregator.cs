// <copyright file="IDataPointAggregator.cs" company="David Nicholson">
//     Copyright (c) David Nicholson. All rights reserved.
// </copyright>
// <summary>See class header.</summary>

namespace DAaVE.Library.DataAggregation
{
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// Given a contiguous segment of raw collected data points, produces a stream of corresponding
    /// aggregated points.
    /// </summary>
    public interface IDataPointAggregator
    {
        /// <summary>
        /// Aggregates the provided raw data points. The mapping from observation time to aggregation 
        /// timestamp must be fixed, This facilitates: 
        /// - Continual creation of increasingly accurate aggregation of a recent time window as more raw
        ///   data observations become available.
        /// - Safe reprocessing of raw data from past dates if there is uncertainty about whether they were
        ///   aggregated, or whether the aggregation was correct.
        /// - Failover between aggregator instances without quorum (at the expense of redundant re-computation
        ///   of some aggregations during the failover period).
        /// </summary>
        /// <param name="contiguousDataSegment">
        /// A contiguous segment of raw observed data point values (in ascending order by collection time).
        /// </param>
        /// <returns>A stream of aggregated points in arbitrary order.</returns>
        IEnumerable<AggregatedDataPoint> Aggregate(IOrderedEnumerable<DataPoint> contiguousDataSegment);
    }
}
