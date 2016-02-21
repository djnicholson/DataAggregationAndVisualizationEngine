// <copyright file="IDataPointPager.cs" company="David Nicholson">
//     Copyright (c) David Nicholson. All rights reserved.
// </copyright>
// <summary>See class header.</summary>

namespace DAaVE.Library.Storage
{
    using System;
    using System.Threading.Tasks;

    /// <summary>
    /// Exposes a contiguous stream of raw collected data points as a sequence of pages.
    /// </summary>
    /// <typeparam name="TDataPointTypeEnum">An enumeration of all possible types of data point.</typeparam>
    public interface IDataPointPager<TDataPointTypeEnum>
        where TDataPointTypeEnum : struct, IComparable, IFormattable
    {
        /// <summary>
        /// Get the next page of raw values collected for a specific type of data point.
        /// Implementations should return as expediently as possible (even if that means 
        /// returning an empty data set).
        /// </summary>
        /// <param name="type">The type of data point to query.</param>
        /// <returns>
        /// A running task that upon successful completion will provide a set of raw data
        /// points for aggregation (always non-null, but possibly empty).
        /// </returns>
        Task<ConsecutiveDataPointObservationsCollection> GetPageOfRawData(TDataPointTypeEnum type);
    }
}
