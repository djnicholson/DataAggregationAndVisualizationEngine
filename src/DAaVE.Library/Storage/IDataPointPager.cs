// <copyright file="IDataPointPager.cs" company="David Nicholson">
//     Copyright (c) David Nicholson. All rights reserved.
// </copyright>

namespace DAaVE.Library.Storage
{
    using System;
    using System.Collections.Generic;

    using DAaVE.Library.DataAggregation;

    /// <summary>
    /// Exposes a continuous stream of raw collected data points as a sequence of pages.
    /// </summary>
    /// <typeparam name="TDataPointTypeEnum">An enumeration of all possible types of data point.</typeparam>
    public interface IDataPointPager<TDataPointTypeEnum>
        where TDataPointTypeEnum : struct, IComparable, IFormattable
    {
        /// <summary>
        /// Get the next page of raw values collected for a specific type of data point.
        /// </summary>
        /// <param name="type">The type of data point to return.</param>
        /// <param name="continuationToken">
        /// An object that can be used to maintain state between a series of invocations of NextPage.
        /// Providing a non-null token is interpreted as a signal that the page corresponding to that
        /// token has now been successfully, irrevocably aggregated.
        /// </param>
        /// <returns>A set of raw data points suitable for aggregation</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1045:DoNotPassTypesByReference", MessageId = "1#")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1007:UseGenericsWhereAppropriate")]
        IEnumerable<DataPoint> ReadPageOfRawData(TDataPointTypeEnum type, ref object continuationToken);

        /// <summary>
        /// Store a stream of aggregated data points of a specific type.
        /// </summary>
        /// <param name="type">The type of data point that was aggregated.</param>
        /// <param name="aggregatedDataPoints">The aggregated values.</param>
        /// <param name="continuationToken">
        /// The continuation token that was returned from <see cref="ReadPageOfRawData"/> when the raw
        /// data was received.
        /// </param>
        void StoreAggregatedData(
            TDataPointTypeEnum type,
            IEnumerable<AggregatedDataPoint> aggregatedDataPoints,
            object continuationToken);
    }
}
