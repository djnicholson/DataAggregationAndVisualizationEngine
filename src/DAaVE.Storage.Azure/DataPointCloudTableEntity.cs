// <copyright file="DataPointCloudTableEntity.cs" company="David Nicholson">
//     Copyright (c) David Nicholson. All rights reserved.
// </copyright>

namespace DAaVE.Storage.Azure
{
    using DAaVE.Library;
    using Microsoft.WindowsAzure.Storage.Table;
    using System;
    using System.Collections.Generic;
    using System.Globalization;

    internal sealed class DataPointCloudTableEntity<TDataPointTypeEnum> : TableEntity
        where TDataPointTypeEnum : struct, IComparable, IFormattable
    {
        public const int MinutesOfRawDataPerFireHosePage = 5;

        private const int RuntimeVersion = 1;

        public DataPointCloudTableEntity()
        {
        }

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

        public string Collector { get; set; }

        public TDataPointTypeEnum Type { get; set; }

        public DateTime CollectionTimeUtc { get; set; }

        public int PersistedVersion { get; set; }

        public double Value { get; set; }

        public static string GeneratePartitionKey(TDataPointTypeEnum type, DateTime utcTimestamp)
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
                utcTimestamp.Minute / MinutesOfRawDataPerFireHosePage,
                RuntimeVersion);
        }

        public static IEnumerable<string> GetPartitions(TDataPointTypeEnum type, DateTime start, DateTime end)
        {
            DateTime current = new DateTime(start.Year, start.Month, start.Day, start.Hour, start.Minute, second: 0);
            while (current < end)
            {
                yield return GeneratePartitionKey(type, current);
                current = current.AddMinutes(MinutesOfRawDataPerFireHosePage);
            }
        }
    }
}
