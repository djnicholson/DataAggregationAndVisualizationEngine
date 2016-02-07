// <copyright file="AggregatedDataPointCloudTableEntity.cs" company="David Nicholson">
//     Copyright (c) David Nicholson. All rights reserved.
// </copyright>

namespace DAaVE.Storage.Azure
{
    using System;
    using System.Globalization;

    using DAaVE.Library.DataAggregation;

    using Microsoft.WindowsAzure.Storage.Table;

    /// <summary>
    /// Represents a row of data in Azure Table Storage that contains information about the
    /// aggregated value of some data point type at some time.
    /// </summary>
    /// <typeparam name="TDataPointTypeEnum">Possible types of data.</typeparam>
    internal class AggregatedDataPointCloudTableEntity<TDataPointTypeEnum> : TableEntity
        where TDataPointTypeEnum : struct, IComparable, IFormattable
    {
        /// <summary>
        /// The amount of minutes worth of aggregated data in each partition of the Azure Table. Queries of aggregated data 
        /// are most efficient when split into chunks of this size executed in parallel.
        /// TODO: Allow customization by collector/aggregator implementers.
        /// </summary>
        public const int MinutesOfAggregatedDataPerPage = 60 * 6;

        /// <summary>
        /// The schema version currently in use (increment whenever making changes to the public surface area of this class).
        /// </summary>
        private const int RuntimeVersion = 2;

        /// <summary>
        /// Initializes a new instance of the <see cref="AggregatedDataPointCloudTableEntity{TDataPointTypeEnum}"/> class.
        /// For use in deserialization only, not to be called explicitly.
        /// </summary>
        public AggregatedDataPointCloudTableEntity()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AggregatedDataPointCloudTableEntity{TDataPointTypeEnum}"/> class.
        /// </summary>
        /// <param name="key">The type of data point being stored.</param>
        /// <param name="value">The current value of the data point.</param>
        /// <param name="rawDataFireHosePartitionKey">
        /// The partition key for each of the <see cref="DataPointCloudTableEntity{TDataPointTypeEnum}"/> objects that were 
        /// used during this aggregation.
        /// </param>
        public AggregatedDataPointCloudTableEntity(
            TDataPointTypeEnum key, 
            AggregatedDataPoint value, 
            string rawDataFireHosePartitionKey)
        {
            this.PersistedVersion = RuntimeVersion;
            this.TimestampUtc = value.UtcTimestamp;
            this.Type = key;
            this.AggregatedValue = value.AggregatedValue;
            this.Aggregator = Environment.MachineName;

            // Row-key consistent based on data that was aggregated (duplicate aggregation should not persist duplicate data)
            this.RowKey = rawDataFireHosePartitionKey + "." + this.PersistedVersion + "." + this.TimestampUtc.Ticks;

            this.PartitionKey = GetPartition(key, this.TimestampUtc);
        }

        /// <summary>
        /// Gets or sets the name of the machine that performed the aggregation.
        /// Setter is for use in deserialization only and not to be called explicitly.
        /// </summary>
        public string Aggregator { get; set; }

        /// <summary>
        /// Gets or sets the type of data point.
        /// Setter is for use in deserialization only and not to be called explicitly.
        /// </summary>
        public TDataPointTypeEnum Type { get; set; }

        /// <summary>
        /// Gets or sets the time-stamp being represented.
        /// Setter is for use in deserialization only and not to be called explicitly.
        /// </summary>
        public DateTime TimestampUtc { get; set; }

        /// <summary>
        /// Gets or sets the schema version in use at the time the value was stored.
        /// Setter is for use in deserialization only and not to be called explicitly.
        /// </summary>
        public int PersistedVersion { get; set; }

        /// <summary>
        /// Gets or sets an illustration of the value at a specific time according to some aggregation of raw data points.
        /// Setter is for use in deserialization only and not to be called explicitly.
        /// </summary>
        public double AggregatedValue { get; set; }

        /// <summary>
        /// Determines the partition key that is used for recording the aggregated value of a specific data point type
        /// at a specific time.
        /// </summary>
        /// <param name="type">The data point type.</param>
        /// <param name="utcTime">The time representing the aggregation window.</param>
        /// <returns>A partition key.</returns>
        private static string GetPartition(TDataPointTypeEnum type, DateTime utcTime)
        {
            // Querying aggregated data is conducted in parallel 'MinutesOfAggregatedDataPerPage' minute batches
            return string.Format(
                CultureInfo.InvariantCulture,
                "{0}@y{1}m{2}d{3}/{4}-v{5}",
                type,
                utcTime.Year,
                utcTime.Month,
                utcTime.Day,
                (utcTime.Hour * 60) / MinutesOfAggregatedDataPerPage,
                RuntimeVersion);
        }
    }
}
