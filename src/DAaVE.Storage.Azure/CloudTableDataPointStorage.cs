// <copyright file="CloudTableDataPointStorage.cs" company="David Nicholson">
//     Copyright (c) David Nicholson. All rights reserved.
// </copyright>

namespace DAaVE.Storage.Azure
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    using DAaVE.Library;
    using DAaVE.Library.DataAggregation;
    using DAaVE.Library.Storage;

    using Microsoft.WindowsAzure.Storage;
    using Microsoft.WindowsAzure.Storage.Auth;
    using Microsoft.WindowsAzure.Storage.RetryPolicies;
    using Microsoft.WindowsAzure.Storage.Table;

    /// <summary>
    /// Performs all operations required for storing and reading both raw and aggregated data points
    /// using Azure Table Storage.
    /// </summary>
    /// <typeparam name="TDataPointTypeEnum">
    /// An enumeration of all possible data point types. Values must not change or be reassigned after
    /// use and should not be removed.
    /// </typeparam>
    public sealed class CloudTableDataPointStorage<TDataPointTypeEnum> 
        : IDataPointFireHose<TDataPointTypeEnum>, IDataPointPager<TDataPointTypeEnum>
        where TDataPointTypeEnum : struct, IComparable, IFormattable
    {
        /// <summary>
        /// Retries (at an exponentially decaying rate) for as long as the raw data being stored may still
        /// be eligible for aggregation.
        /// </summary>
        private static readonly TableRequestOptions RawDataStorageLongDrawnOutRetry =
            new TableRequestOptions()
            {
                MaximumExecutionTime = CloudTableDataPointStorage.MaximumFireHoseRecentDataPointAge,
                RetryPolicy = new ExponentialRetry(TimeSpan.FromSeconds(1.0), int.MaxValue),
            };

        /// <summary>
        /// Data retrieved from storage but that could be issued to clients upon request.
        /// </summary>
        private readonly ConcurrentDictionary<TDataPointTypeEnum, ConcurrentDictionary<string, IOrderedEnumerable<DataPoint>>> bufferedRawDataPartitionsByType =
            new ConcurrentDictionary<TDataPointTypeEnum, ConcurrentDictionary<string, IOrderedEnumerable<DataPoint>>>();

        /// <summary>
        /// The table where raw data points are stored.
        /// </summary>
        private readonly CloudTable firehoseTable;

        /// <summary>
        /// The table where aggregated data points are stored.
        /// </summary>
        private readonly CloudTable aggregationTable;

        /// <summary>
        /// Initializes a new instance of the <see cref="CloudTableDataPointStorage{TDataPointTypeEnum}"/> class.
        /// </summary>
        /// <param name="fireHoseTableUri">URL to the Azure Table Storage table used to persist raw collected data.</param>
        /// <param name="aggregationTableUri">URL to the Azure Table Storage table used to persist aggregated data.</param>
        /// <param name="storageAccount">The name of the Azure Storage account.</param>
        /// <param name="storageKey">The key for the Azure Storage account.</param>
        public CloudTableDataPointStorage(Uri fireHoseTableUri, Uri aggregationTableUri, string storageAccount, string storageKey)
        {
            StorageCredentials credentials = new StorageCredentials(storageAccount, storageKey);

            this.firehoseTable = new CloudTable(fireHoseTableUri, credentials);
            this.aggregationTable = new CloudTable(aggregationTableUri, credentials);

            Task.WaitAll(
                this.firehoseTable.CreateIfNotExistsAsync(),
                this.aggregationTable.CreateIfNotExistsAsync());
        }

        /// <summary>
        /// Could be called concurrently on multiple different threads. Can take as long as needed to terminate
        /// (but should try and play nice -- it is taking up a thread on the shared 
        /// <see cref="System.Threading.ThreadPool"/>.
        /// </summary>
        /// <param name="rawDataSample">Data points produced by a collector. These may not be recent.</param>
        /// <returns>The task within which the storage is taking place.</returns>
        public Task StoreRawData(IDictionary<TDataPointTypeEnum, DataPoint> rawDataSample)
        {
            DateTime postMarkedOnUtc = DateTime.UtcNow;

            IEnumerable<DataPointCloudTableEntity<TDataPointTypeEnum>> recentDataPoints = rawDataSample
                .Where(d => (postMarkedOnUtc - d.Value.UtcTimestamp) < CloudTableDataPointStorage.MaximumFireHoseRecentDataPointAge)
                .Select(d => new DataPointCloudTableEntity<TDataPointTypeEnum>(d.Key, d.Value));

            IDictionary<string, TableBatchOperation> batches = CreateInsertOperations(recentDataPoints);

            List<Task> allTasks = new List<Task>(batches.Count);
            foreach (KeyValuePair<string, TableBatchOperation> batch in batches)
            {
                if (batch.Value.Any())
                {
                    allTasks.Add(
                        this.firehoseTable.ExecuteBatchAsync(batch.Value, requestOptions: RawDataStorageLongDrawnOutRetry, operationContext: new OperationContext()));
                }
            }

            return Task.WhenAll(allTasks);
        }

        /// <inheritdoc/>
        public ContinuousRawDataPointCollection GetPageOfRawData(TDataPointTypeEnum type)
        {
            this.bufferedRawDataPartitionsByType.AddOrUpdate(
                key: type,
                addValueFactory: _ => new ConcurrentDictionary<string, IOrderedEnumerable<DataPoint>>(),
                updateValueFactory: (_, existing) => existing);

            ConcurrentDictionary<string, IOrderedEnumerable<DataPoint>> bufferedPartitions = null;
            if (!this.bufferedRawDataPartitionsByType.TryGetValue(type, out bufferedPartitions) || !bufferedPartitions.Any())
            {
                this.RebuildRawDataPageBuffersForType(type);

                if (!this.bufferedRawDataPartitionsByType.TryGetValue(type, out bufferedPartitions) || !bufferedPartitions.Any())
                {
                    // Either empty, or very nearly empty (due to possible racing of concurrent calls to this method)
                    return ContinuousRawDataPointCollection.Empty;
                }
            }

            KeyValuePair<string, IOrderedEnumerable<DataPoint>> nextPartitionData = bufferedPartitions.FirstOrDefault();
            if (nextPartitionData.Equals(default(KeyValuePair<string, DataPoint[]>)))
            {
                // Definite racing of concurrent calls to this method. The buffer is nearly empty anyway so no work for this caller:
                return ContinuousRawDataPointCollection.Empty;
            }

            var firehosePartition = nextPartitionData.Key;
            return new SinglePartition(
                type,
                firehosePartition, 
                this.aggregationTable, 
                onAggregationSuccess: () => 
                {
                    IOrderedEnumerable<DataPoint> _;
                    this.bufferedRawDataPartitionsByType[type].TryRemove(firehosePartition, out _);
                }, 
                rawDataPoints: nextPartitionData.Value);
        }

        /// <summary>
        /// Splits data based on partition key and creates a batch insert operation for each partition.
        /// </summary>
        /// <param name="recentDataPoints">The data to be inserted.</param>
        /// <returns>A set of operations to perform.</returns>
        private static IDictionary<string, TableBatchOperation> CreateInsertOperations(
            IEnumerable<DataPointCloudTableEntity<TDataPointTypeEnum>> recentDataPoints)
        {
            IDictionary<string, TableBatchOperation> batches = new Dictionary<string, TableBatchOperation>();
            foreach (DataPointCloudTableEntity<TDataPointTypeEnum> entity in recentDataPoints)
            {
                if (!batches.ContainsKey(entity.PartitionKey))
                {
                    batches[entity.PartitionKey] = new TableBatchOperation();
                }

                batches[entity.PartitionKey].Add(TableOperation.Insert(entity));
            }

            return batches;
        }

        /// <summary>
        /// Converts context stored by user back into a string.
        /// </summary>
        /// <param name="pagerContext">Context from user.</param>
        /// <returns>Context as a string (containing a partition key).</returns>
        private static string GetPartitionKey(object pagerContext)
        {
            return pagerContext as string;
        }

        /// <summary>
        /// Queries for raw data currently eligible for aggregation.
        /// </summary>
        /// <param name="type">The type of data point to query.</param>
        private void RebuildRawDataPageBuffersForType(TDataPointTypeEnum type)
        {
            DateTime utcNow = DateTime.UtcNow;
            IEnumerable<string> partitions = DataPointCloudTableEntity<TDataPointTypeEnum>.GetPartitions(
                type,
                utcNow - CloudTableDataPointStorage.MaximumFireHoseRecentDataPointAge,
                utcNow - CloudTableDataPointStorage.ProcessingDelay);

            foreach (string partition in partitions)
            {
                var tableQuery = new TableQuery<DataPointCloudTableEntity<TDataPointTypeEnum>>();

                //// TODO: Async IO for Azure HTTP requests here.

                IEnumerable<DataPoint> pointsInPartition = this.firehoseTable
                    .ExecuteQuery(tableQuery.Where(TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, partition)))
                    .Where(ce => (ce.CollectionTimeUtc + CloudTableDataPointStorage.ProcessingDelay) < utcNow)
                    .Select(ce => new DataPoint() { UtcTimestamp = ce.CollectionTimeUtc, Value = ce.Value });

                if (pointsInPartition.Any())
                {
                    this.bufferedRawDataPartitionsByType[type][partition] = pointsInPartition.OrderBy(dp => dp.UtcTimestamp);
                }
            }
        }

        /// <summary>
        /// Provides access to all raw data points in a single raw data partition.
        /// </summary>
        private class SinglePartition : ContinuousRawDataPointCollection
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
            /// Initializes a new instance of the SinglePartition class.
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
            public SinglePartition(
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
}
