// <copyright file="DataPointCloudTableEntity.cs" company="David Nicholson">
//     Copyright (c) David Nicholson. All rights reserved.
// </copyright>
// <summary>See class header.</summary>

namespace DAaVE.Storage.Azure
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;

    using DAaVE.Library;

    using Microsoft.WindowsAzure.Storage.Table;

    /// <summary>
    /// Represents a row of data in Azure Table Storage that contains information about the
    /// exact value of some data point type at some time as observed by a single data collector.
    /// </summary>
    /// <typeparam name="TDataPointTypeEnum">Possible types of data.</typeparam>
    internal sealed class DataPointCloudTableEntity<TDataPointTypeEnum> : TableEntity
        where TDataPointTypeEnum : struct, IComparable, IFormattable
    {
        /// <summary>
        /// The schema version currently in use (increment whenever making changes to the public surface area of this class).
        /// </summary>
        private const int RuntimeVersion = 1;

        /// <summary>
        /// For resolving properties of the various data point types being stored.
        /// </summary>
        private static readonly DataPointTypeAttributes<TDataPointTypeEnum> DataPointTypeAttributes =
            new DataPointTypeAttributes<TDataPointTypeEnum>();

        /// <summary>
        /// Initializes a new instance of the <see cref="DataPointCloudTableEntity{TDataPointTypeEnum}"/> class.
        /// For use in deserialization only, not to be called explicitly.
        /// </summary>
        public DataPointCloudTableEntity()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DataPointCloudTableEntity{TDataPointTypeEnum}"/> class.
        /// </summary>
        /// <param name="key">The type of data point being collected.</param>
        /// <param name="value">The current value of the data point type.</param>
        public DataPointCloudTableEntity(TDataPointTypeEnum key, DataPoint value)
        {
            this.PersistedVersion = RuntimeVersion;
            this.CollectionTimeUtc = value.UtcTimestamp;
            this.Type = key;
            this.Value = value.Value;
            this.Collector = Environment.MachineName;

            this.RowKey = Guid.NewGuid().ToString();

            this.PartitionKey = GeneratePartitionKey(this.Type, this.CollectionTimeUtc);
        }

        /// <summary>
        /// Gets or sets the name of the machine that observed this data value.
        /// Setter is for use in deserialization only and not to be called explicitly.
        /// </summary>
        public string Collector { get; set; }

        /// <summary>
        /// Gets or sets the type of data point observed.
        /// Setter is for use in deserialization only and not to be called explicitly.
        /// </summary>
        public TDataPointTypeEnum Type { get; set; }

        /// <summary>
        /// Gets or sets the time at which this value was observed.
        /// Setter is for use in deserialization only and not to be called explicitly.
        /// </summary>
        public DateTime CollectionTimeUtc { get; set; }

        /// <summary>
        /// Gets or sets the schema version in use at the time the value was stored.
        /// Setter is for use in deserialization only and not to be called explicitly.
        /// </summary>
        public int PersistedVersion { get; set; }

        /// <summary>
        /// Gets or sets the value observed.
        /// Setter is for use in deserialization only and not to be called explicitly.
        /// </summary>
        public double Value { get; set; }

        /// <summary>
        /// Produces an enumeration of all partition keys that were in use within a time window.
        /// </summary>
        /// <param name="type">The type of data point.</param>
        /// <param name="startUtc">Start of the time window (in UTC).</param>
        /// <param name="endUtc">End of the time window (in UTC).</param>
        /// <returns>An enumeration of partition keys.</returns>
        public static IEnumerable<string> GetPartitions(TDataPointTypeEnum type, DateTime startUtc, DateTime endUtc)
        {
            DateTime current = new DateTime(startUtc.Year, startUtc.Month, startUtc.Day, startUtc.Hour, startUtc.Minute, second: 0);
            while (current < endUtc)
            {
                yield return GeneratePartitionKey(type, current);
                current = current.AddMinutes(GetMinutesOfRawDataPerFireHosePage(type));
            }
        }

        /// <summary>
        /// Determines the partition key that is used for recording the value of a specific data point type
        /// at a specific time.
        /// </summary>
        /// <param name="type">The type of data point.</param>
        /// <param name="utcTimestamp">The time at which the data point was observed.</param>
        /// <returns>A partition key.</returns>
        private static string GeneratePartitionKey(TDataPointTypeEnum type, DateTime utcTimestamp)
        {
            // When providing "pages" of data to aggregators, we return 'MinutesOfRawDataPerFireHosePage' 
            // minutes of data at a time. Each page shares a partition in storage for efficient retrieval
            // of entire pages.
            return string.Format(
                CultureInfo.InvariantCulture,
                "{0}@y{1}m{2}d{3}h{4}/{5}-v{6}",
                type,
                utcTimestamp.Year,
                utcTimestamp.Month,
                utcTimestamp.Day,
                utcTimestamp.Hour,
                utcTimestamp.Minute / GetMinutesOfRawDataPerFireHosePage(type),
                RuntimeVersion);
        }

        /// <summary>
        /// Determine how many minutes worth of raw data points should be placed in a single Azure Table Storage
        /// partition for a certain type of data.
        /// </summary>
        /// <param name="type">The type of data.</param>
        /// <returns>The amount of data points to place in each page (1-minute resolution).</returns>
        private static double GetMinutesOfRawDataPerFireHosePage(TDataPointTypeEnum type)
        {
            return DataPointTypeAttributes.GetAggregationInputWindowSizeInMinutes(type);
        }
    }
}
