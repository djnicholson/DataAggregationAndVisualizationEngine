// <copyright file="ConsecutiveDataPointObservationsCollection.cs" company="David Nicholson">
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
    /// Represents a contiguous segment of raw data points that are expected to be aggregated
    /// by some implementation of <see cref="IDataPointAggregator"/>.
    /// </summary>
    public abstract class ConsecutiveDataPointObservationsCollection : IOrderedEnumerable<DataPointObservation>
    {
        /// <summary>
        /// All raw data points within this time segment, in ascending time order.
        /// </summary>
        private IOrderedEnumerable<DataPointObservation> rawDataPoints;

        /// <summary>
        /// Initializes a new instance of the ConsecutiveDataPointObservationsCollection class.
        /// </summary>
        /// <param name="rawDataPoints">All raw data points within this time segment, in ascending time order.</param>
        protected ConsecutiveDataPointObservationsCollection(IOrderedEnumerable<DataPointObservation> rawDataPoints)
        {
            this.rawDataPoints = rawDataPoints;
        }

        /// <summary>
        /// Gets a value representing a zero-length segment of raw data points.
        /// </summary>
        /// <value>A zero-length segment of raw data points that ignores all aggregation responses.</value>
        public static ConsecutiveDataPointObservationsCollection Empty
        {
            get
            {
                return EmptyDataPointObservationsCollection.Instance;
            }
        }

        /// <summary>
        /// Gets a value indicating whether the page of data that these observations originated from 
        /// is considered 'incomplete'; that is to say that the page will be re-aggregated at a later 
        /// time, possibly in a more complete form.
        /// </summary>
        /// <value>Whether these values are considered an incomplete page.</value>
        public abstract bool IsPartial
        {
            get;
        }

        /// <summary>
        /// Facilitates enumeration of all data points in ascending order by observation time.
        /// </summary>
        /// <returns>An enumerator of raw data points.</returns>
        public IEnumerator<DataPointObservation> GetEnumerator()
        {
            return this.rawDataPoints.GetEnumerator();
        }

        /// <summary>
        /// Defers to see <see cref="GetEnumerator"/>.
        /// </summary>
        /// <returns>See <see cref="GetEnumerator"/>.</returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return (this as ConsecutiveDataPointObservationsCollection).GetEnumerator();
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
        public abstract Task ProvideCorrespondingAggregatedData(IEnumerable<AggregatedDataPoint> aggregatedDataPoints);

        /// <inheritdoc/>
        public IOrderedEnumerable<DataPointObservation> CreateOrderedEnumerable<TKey>(
            Func<DataPointObservation, TKey> keySelector, 
            IComparer<TKey> comparer, 
            bool descending)
        {
            return descending ?
                this.rawDataPoints.OrderByDescending(keySelector, comparer) :
                this.rawDataPoints.OrderBy(keySelector, comparer);
        }
    }
}
