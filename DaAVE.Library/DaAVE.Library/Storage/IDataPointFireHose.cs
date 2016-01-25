using DAaVE.Library.DataAggregation;
using System;
using System.Collections.Generic;

namespace DAaVE.Library.Storage
{
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
        void StoreRawData(
            IDictionary<TDataPointTypeEnum, DataPoint> rawDataSample);
    }
}
