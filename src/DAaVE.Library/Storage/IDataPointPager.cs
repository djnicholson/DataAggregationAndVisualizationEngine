// <copyright file="IDataPointPager.cs" company="David Nicholson">
//     Copyright (c) David Nicholson. All rights reserved.
// </copyright>

namespace DAaVE.Library.Storage
{
    using System;

    /// <summary>
    /// Exposes a continuous stream of raw collected data points as a sequence of pages.
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
        /// A set of raw data points for aggregation. Non-null, but possibly empty.
        /// </returns>
        ContinuousRawDataPointCollection GetPageOfRawData(TDataPointTypeEnum type);
    }
}
