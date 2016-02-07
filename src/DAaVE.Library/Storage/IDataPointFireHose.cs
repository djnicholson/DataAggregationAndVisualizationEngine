// <copyright file="IDataPointFireHose.cs" company="David Nicholson">
//     Copyright (c) David Nicholson. All rights reserved.
// </copyright>

namespace DAaVE.Library.Storage
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    /// <summary>
    /// Capable of receiving and persisting high volumes of incoming raw data points
    /// at low caller-facing latency.
    /// </summary>
    /// <typeparam name="TDataPointTypeEnum">An enumeration of all possible types of data point.</typeparam>
    public interface IDataPointFireHose<TDataPointTypeEnum>
        where TDataPointTypeEnum : struct, IComparable, IFormattable 
    {
        /// <summary>
        /// Store a sample of raw data points just received from a collector.
        /// </summary>
        /// <param name="rawDataSample">The raw data points.</param>
        Task StoreRawData(
            IDictionary<TDataPointTypeEnum, DataPoint> rawDataSample);
    }
}
