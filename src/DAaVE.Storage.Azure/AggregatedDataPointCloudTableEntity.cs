// <copyright file="AggregatedDataPointCloudTableEntity.cs" company="David Nicholson">
//     Copyright (c) David Nicholson. All rights reserved.
// </copyright>

using DAaVE.Library.DataAggregation;
using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Globalization;

namespace DAaVE.Storage.Azure
{
    internal class AggregatedDataPointCloudTableEntity<TDataPointTypeEnum> : TableEntity
        where TDataPointTypeEnum : struct, IComparable, IFormattable
    {
        private const int RuntimeVersion = 2;

        public AggregatedDataPointCloudTableEntity()
        {
        }

        public AggregatedDataPointCloudTableEntity(TDataPointTypeEnum key, AggregatedDataPoint value, string rawDataFireHosePartitionKey)
        {
            this.Version = RuntimeVersion;
            this.TimestampUtc = value.UtcTimestamp;
            this.Type = key;
            this.AggregatedValue = value.AggregatedValue;
            this.Aggregator = Environment.MachineName;

            // Row-key consistent based on data that was aggregated (duplicate aggregation should not persist duplicate data)
            this.RowKey = rawDataFireHosePartitionKey + "." + this.Version + "." + this.TimestampUtc.Ticks;

            this.PartitionKey = GetPartition(key, this.TimestampUtc);
        }

        public const int MinutesOfAggregatedDataPerPage = 60 * 6;

        public string Aggregator { get; set; }

        public TDataPointTypeEnum Type { get; set; }

        public DateTime TimestampUtc { get; set; }

        public int Version { get; set; }

        public double AggregatedValue { get; set; }

        public static string GetPartition(TDataPointTypeEnum type, DateTime utcTime)
        {
            // Querying aggregated data is conducted in parrelel 'MinutesOfAggregatedDataPerPage' minute batches
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

        public static IEnumerable<Tuple<string, DateTime>> GetPartitionsInRange(TDataPointTypeEnum type, DateTime startTimeUtc, DateTime endTimeUtc)
        {
            do
            {
                yield return new Tuple<string, DateTime>(
                    GetPartition(type, startTimeUtc),
                    new DateTime(startTimeUtc.Year, startTimeUtc.Month, startTimeUtc.Day, 23, 59, 59));

                startTimeUtc = startTimeUtc.AddMinutes(MinutesOfAggregatedDataPerPage);
            }
            while (startTimeUtc <= endTimeUtc);
        }
    }
}
