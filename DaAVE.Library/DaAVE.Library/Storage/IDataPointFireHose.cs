using DAaVE.Library.DataAggregation;
using System;
using System.Collections.Generic;

namespace DAaVE.Library.Storage
{
    /// <summary>
    /// Capable of receiving and persisting high volumes of incoming raw and aggregated data points
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
        void ProcessRawData(
            IDictionary<TDataPointTypeEnum, DataPoint> rawDataSample);

        /// <summary>
        /// Store a stream of aggregated data points of a specific type.
        /// </summary>
        /// <param name="type">The type of data point that was aggregated.</param>
        /// <param name="aggregatedDataPoints">The aggregated values.</param>
        /// <param name="pager">The pager that was used to retrieve the raw data points used in aggregation.</param>
        /// <param name="pagerContext">The context within which <paramref name="pager"/> should continue retrieving raw data points.</param>
        void ProcessAggregatedData(
            TDataPointTypeEnum type, 
            IEnumerable<AggregatedDataPoint> aggregatedDataPoints, 
            IDataPointPager<TDataPointTypeEnum> pager, 
            object pagerContext);
    }
}
