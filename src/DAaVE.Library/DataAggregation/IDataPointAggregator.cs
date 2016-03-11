// <copyright file="IDataPointAggregator.cs" company="David Nicholson">
//     Copyright (c) David Nicholson. All rights reserved.
// </copyright>
// <summary>See class header.</summary>

namespace DAaVE.Library.DataAggregation
{
    using System.Collections.Generic;

    using DAaVE.Library.Storage;

    /// <summary>
    /// Given a contiguous segment of raw collected data points, produces a stream of corresponding
    /// aggregated points.
    /// </summary>
    public interface IDataPointAggregator
    {
        /// <summary>
        /// Aggregates the provided raw data points in a repeatable way.
        /// </summary>
        /// <param name="continuousObservations">
        /// A sequence of observations of a specific data point that may or may not be considered a 'complete
        /// page' of observations. It is always guaranteed that the sequence is in ascending observation time
        /// order; for complete pages, it is also guaranteed that there are no missing intermediary points in
        /// the sequence. For incomplete pages, it is guaranteed that the same page (possibly with missing 
        /// points added) will be provided for aggregation again at a later time.
        /// </param>
        /// <returns>
        /// A stream of aggregated points in arbitrary order. The mapping from observation time to aggregation 
        /// timestamp must be fixed, This facilitates: 
        /// - Continual creation of increasingly accurate aggregation of a recent time window as more raw
        ///   data observations become available.
        /// - Safe reprocessing of raw data from past dates if there is uncertainty about whether they were
        ///   aggregated, or whether the aggregation was correct.
        /// - Failover between aggregator instances without quorum (at the expense of redundant re-computation
        ///   of some aggregations during the failover period).
        /// </returns>
        IEnumerable<AggregatedDataPoint> Aggregate(
            ConsecutiveDataPointObservationsCollection continuousObservations);
    }
}
