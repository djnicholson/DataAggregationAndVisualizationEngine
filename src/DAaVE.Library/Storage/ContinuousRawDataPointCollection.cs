// <copyright file="ContinuousRawDataPointCollection.cs" company="David Nicholson">
//     Copyright (c) David Nicholson. All rights reserved.
// </copyright>
// <summary>See class header.</summary>

namespace DAaVE.Library.Storage
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    using DAaVE.Library.DataAggregation;
    
    /// <summary>
    /// Represents a continuous segment of raw data points that are expected to be aggregated
    /// by some implementation of <see cref="IDataPointAggregator"/>.
    /// </summary>
    public abstract class ContinuousRawDataPointCollection : IOrderedEnumerable<DataPoint>
    {
        /// <summary>
        /// All raw data points within this time segment, in ascending time order.
        /// </summary>
        private IOrderedEnumerable<DataPoint> rawDataPoints;

        /// <summary>
        /// Initializes a new instance of the ContinuousRawDataPointCollection class.
        /// </summary>
        /// <param name="rawDataPoints">All raw data points within this time segment, in ascending time order.</param>
        protected ContinuousRawDataPointCollection(IOrderedEnumerable<DataPoint> rawDataPoints)
        {
            this.rawDataPoints = rawDataPoints;
        }

        /// <summary>
        /// Gets a value representing a zero-length segment of raw data points.
        /// </summary>
        /// <value>A zero-length segment of raw data points that ignores all aggregation responses.</value>
        public static ContinuousRawDataPointCollection Empty
        {
            get
            {
                return EmptyRawDataPointCollection.Instance;
            }
        }

        /// <summary>
        /// Facilitates enumeration of all data points in ascending order by observation time.
        /// </summary>
        /// <returns>An enumerator of raw data points.</returns>
        public IEnumerator<DataPoint> GetEnumerator()
        {
            return this.rawDataPoints.GetEnumerator();
        }

        /// <summary>
        /// Defers to see <see cref="GetEnumerator"/>.
        /// </summary>
        /// <returns>See <see cref="GetEnumerator"/>.</returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return (this as ContinuousRawDataPointCollection).GetEnumerator();
        }

        /// <summary>
        /// Provide an aggregation of all of the raw data points in this segment. Aggregated
        /// data points are stored according to individual subclass implementations.
        /// </summary>
        /// <param name="aggregatedDataPoints">
        /// A (possibly empty, arbitrary order) set of aggregated data points computed by considering all 
        /// raw data points in this segment.
        /// </param>
        /// <returns>
        /// A running task that can optionally be awaited if the caller wants to know that the aggregation
        /// results were successfully persisted.
        /// </returns>
        public abstract Task ProvideAggregatedData(IEnumerable<AggregatedDataPoint> aggregatedDataPoints);

        /// <inheritdoc/>
        public IOrderedEnumerable<DataPoint> CreateOrderedEnumerable<TKey>(Func<DataPoint, TKey> keySelector, IComparer<TKey> comparer, bool descending)
        {
            return descending ?
                this.rawDataPoints.OrderByDescending(keySelector, comparer) :
                this.rawDataPoints.OrderBy(keySelector, comparer);
        }
    }
}
