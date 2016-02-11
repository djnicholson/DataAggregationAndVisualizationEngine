// <copyright file="SinglePartitionRawDataCollection.cs" company="David Nicholson">
//     Copyright (c) David Nicholson. All rights reserved.
// </copyright>
// <summary>See class header.</summary>

namespace DAaVE.Storage.Azure
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    using DAaVE.Library;
    using DAaVE.Library.DataAggregation;
    using DAaVE.Library.Storage;

    using Microsoft.WindowsAzure.Storage;
    using Microsoft.WindowsAzure.Storage.RetryPolicies;
    using Microsoft.WindowsAzure.Storage.Table;

    /// <summary>
    /// Provides access to all raw data points in a single raw data partition.
    /// </summary>
    /// <typeparam name="TDataPointTypeEnum">An enumeration of all possible data point types.</typeparam>
    internal class SinglePartitionRawDataCollection<TDataPointTypeEnum> : ContinuousRawDataPointCollection
        where TDataPointTypeEnum : struct, IComparable, IFormattable
    {
        /// <summary>
        /// Retries (at an exponentially decaying rate) for as long as the raw data being stored may still
        /// be eligible for inclusion in the current or previous aggregation partition.
        /// </summary>
        private static readonly TableRequestOptions AggregatedDataStorageLongDrawnOutRetry =
            new TableRequestOptions()
            {
                MaximumExecutionTime =
                    TimeSpan.FromMinutes(AggregatedDataPointCloudTableEntity<TDataPointTypeEnum>.MinutesOfAggregatedDataPerPage),
                RetryPolicy = new ExponentialRetry(TimeSpan.FromSeconds(1.0), int.MaxValue),
            };

        /// <summary>
        /// The type of data being queried.
        /// </summary>
        private readonly TDataPointTypeEnum seriesDataPointType;

        /// <summary>
        /// The key for the partition in the raw data table that these points were stored in.
        /// </summary>
        private readonly string firehosePartitionKey;

        /// <summary>
        /// The table where aggregated data points are stored.
        /// </summary>
        private readonly CloudTable aggregationTable;

        /// <summary>
        /// Code that can be invoked whenever a successful aggregation has happened, and the results have been committed to
        /// storage. Can be invoked multiple times for the same parameters, must be invoked at least once per partition that
        /// is successfully aggregated.
        /// </summary>
        private readonly Action onAggregationSuccess;

        /// <summary>
        /// Initializes a new instance of the SinglePartitionRawDataCollection class.
        /// </summary>
        /// <param name="seriesDataPointType">The type of data being queried.</param>
        /// <param name="firehosePartitionKey">The key for the partition in the raw data table that these points were stored in.</param>
        /// <param name="aggregationTable">The table where corresponding aggregated data should be stored.</param>
        /// <param name="onAggregationSuccess">
        /// Code that will be invoked whenever a successful aggregation has happened, and the results have been committed to
        /// storage. May be invoked multiple times for the same partition.
        /// </param>
        /// <param name="rawDataPoints">
        /// All raw data points of type <paramref name="seriesDataPointType"/> in the <paramref name="firehosePartitionKey"/> partition of 
        /// the fire hose table. In ascending time order.
        /// </param>
        public SinglePartitionRawDataCollection(
            TDataPointTypeEnum seriesDataPointType,
            string firehosePartitionKey,
            CloudTable aggregationTable,
            Action onAggregationSuccess,
            IOrderedEnumerable<DataPoint> rawDataPoints) : base(rawDataPoints)
        {
            this.seriesDataPointType = seriesDataPointType;
            this.firehosePartitionKey = firehosePartitionKey;
            this.aggregationTable = aggregationTable;
            this.onAggregationSuccess = onAggregationSuccess;
        }

        /// <inheritdoc/>
        public override Task ProvideAggregatedData(IEnumerable<AggregatedDataPoint> aggregatedDataPoints)
        {
            IEnumerable<IGrouping<string, AggregatedDataPointCloudTableEntity<TDataPointTypeEnum>>> aggregatedDataPointEntitiesByPartition = aggregatedDataPoints
                .Select(p => new AggregatedDataPointCloudTableEntity<TDataPointTypeEnum>(this.seriesDataPointType, p, this.firehosePartitionKey))
                .GroupBy(pe => pe.PartitionKey);

            List<Task> allBatchInsertOperations = new List<Task>(aggregatedDataPointEntitiesByPartition.Count());

            foreach (IGrouping<string, AggregatedDataPointCloudTableEntity<TDataPointTypeEnum>> aggregatedDataPointsInSamePartition in aggregatedDataPointEntitiesByPartition)
            {
                TableBatchOperation batch = new TableBatchOperation();

                foreach (AggregatedDataPointCloudTableEntity<TDataPointTypeEnum> aggregatedDataPoint in aggregatedDataPointsInSamePartition.ToArray())
                {
                    batch.Add(TableOperation.InsertOrReplace(aggregatedDataPoint));
                }

                if (batch.Any())
                {
                    allBatchInsertOperations.Add(
                        this.aggregationTable.ExecuteBatchAsync(batch, requestOptions: AggregatedDataStorageLongDrawnOutRetry, operationContext: new OperationContext()));
                }
            }

            return Task.WhenAll(
                Task.WhenAll(allBatchInsertOperations),
                Task.Run(this.onAggregationSuccess));
        }
    }
}
