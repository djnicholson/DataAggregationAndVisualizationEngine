// <copyright file="DataPointTypeAttributes.cs" company="David Nicholson">
//     Copyright (c) David Nicholson. All rights reserved.
// </copyright>
// <summary>See class header.</summary>

namespace DAaVE.Storage.Azure
{
    using System;
    using System.Diagnostics.CodeAnalysis;

    /// <summary>
    /// Allows properties of the various data point types to be retrieved.
    /// </summary>
    /// <typeparam name="TDataPointTypeEnum">
    /// An enumeration of all possible data point types.
    /// </typeparam>
    internal class DataPointTypeAttributes<TDataPointTypeEnum>
        where TDataPointTypeEnum : struct, IComparable, IFormattable
    {
        /// <summary>
        /// Gets the minimum size of the input sent into an aggregator when requesting a complete
        /// aggregation (less points may be offered, but the aggregator wont be obligated to generate
        /// output).  A higher value provides flexibility to aggregate over larger windows of time 
        /// (providing an efficient way to present a data series over a very long time) but can lead
        /// to inefficient aggregation for data types that are observed frequently (as many raw 
        /// observations will be crammed into a single Azure Table Storage partition).
        /// </summary>
        /// <param name="dataPointType">The type of data point.</param>
        /// <returns>Minimum amount of raw data to provide as input to an aggregator.</returns>
        [SuppressMessage(
            "Microsoft.Performance", 
            "CA1822:MarkMembersAsStatic", 
            Justification = "TODO: Eventually a constructor will build a lookup table by reflecting over attributes, and this method will use it.")]
        [SuppressMessage(
            "Microsoft.Usage", 
            "CA1801:ReviewUnusedParameters", 
            MessageId = "dataPointType",
            Justification = "As above.")]
        public uint GetAggregationInputWindowSizeInMinutes(TDataPointTypeEnum dataPointType)
        {
            return 5;
        }
    }
}
