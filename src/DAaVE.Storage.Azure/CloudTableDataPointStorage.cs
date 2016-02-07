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
    /// 
    /// </summary>
    /// <typeparam name="TDataPointTypeEnum"></typeparam>
    public sealed class CloudTableDataPointStorage<TDataPointTypeEnum> 
        : IDataPointFireHose<TDataPointTypeEnum>, IDataPointPager<TDataPointTypeEnum>
        where TDataPointTypeEnum : struct, IComparable, IFormattable
    {
        private static readonly TableRequestOptions LongDrawnOutRetry =
            new TableRequestOptions()
            {
                MaximumExecutionTime = CloudTableDataPointStorage.MaximumFireHoseRecentDataPointAge,
                RetryPolicy = new ExponentialRetry(TimeSpan.FromSeconds(1.0), int.MaxValue),
            };

        private readonly ConcurrentDictionary<TDataPointTypeEnum, IDictionary<string, DataPoint[]>> rawDataPageBuffers =
            new ConcurrentDictionary<TDataPointTypeEnum, IDictionary<string, DataPoint[]>>();

        private readonly CloudTable firehoseTable;

        private readonly CloudTable aggregationTable;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="fireHoseTableUri"></param>
        /// /// <param name="aggregationTableUri"></param>
        /// <param name="storageAccount"></param>
        /// <param name="storageKey"></param>
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
                        this.firehoseTable.ExecuteBatchAsync(batch.Value, requestOptions: LongDrawnOutRetry, operationContext: new OperationContext()));
                }
            }

            return Task.WhenAll(allTasks);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="type"></param>
        /// <param name="continuationToken"></param>
        /// <returns></returns>
        public IEnumerable<DataPoint> ReadPageOfRawData(TDataPointTypeEnum type, ref object continuationToken)
        {
            this.rawDataPageBuffers.AddOrUpdate(
                key: type, 
                addValueFactory: _ => new Dictionary<string, DataPoint[]>(), 
                updateValueFactory: (_, existing) => existing);

            string lastPartition = continuationToken as string;
            if (lastPartition != null)
            {
                this.rawDataPageBuffers[type].Remove(lastPartition);
            }

            IDictionary<string, DataPoint[]> nextPage = null;
            if (!this.rawDataPageBuffers.TryGetValue(type, out nextPage) || (nextPage.Count == 0))
            {
                this.RebuildRawDataPageBuffersForType(type);
                if (!this.rawDataPageBuffers.TryGetValue(type, out nextPage) || (nextPage.Count == 0))
                {
                    continuationToken = null;
                    return new DataPoint[0];
                }
            }

            string nextPartition = nextPage.Keys.First();

            continuationToken = nextPartition;
            return nextPage[nextPartition];
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="type"></param>
        /// <param name="aggregatedDataPoints"></param>
        /// <param name="continuationToken"></param>
        public Task StoreAggregatedData(
            TDataPointTypeEnum type,
            IEnumerable<AggregatedDataPoint> aggregatedDataPoints,
            object continuationToken)
        {
            IEnumerable<IGrouping<string, AggregatedDataPointCloudTableEntity<TDataPointTypeEnum>>> aggregatedDataPointEntitiesByPartition = aggregatedDataPoints
                .Select(p => new AggregatedDataPointCloudTableEntity<TDataPointTypeEnum>(type, p, GetPartitionKey(continuationToken)))
                .GroupBy(pe => pe.PartitionKey);

            List<Task> allTasks = new List<Task>(aggregatedDataPointEntitiesByPartition.Count());

            foreach (IGrouping<string, AggregatedDataPointCloudTableEntity<TDataPointTypeEnum>> aggregatedDataPointsInSamePartition in aggregatedDataPointEntitiesByPartition)
            {
                TableBatchOperation batch = new TableBatchOperation();

                foreach (AggregatedDataPointCloudTableEntity<TDataPointTypeEnum> aggregatedDataPoint in aggregatedDataPointsInSamePartition.ToArray())
                {
                    batch.Add(TableOperation.InsertOrReplace(aggregatedDataPoint));
                }

                if (batch.Any())
                {
                    allTasks.Add(
                        this.aggregationTable.ExecuteBatchAsync(batch, requestOptions: LongDrawnOutRetry, operationContext: new OperationContext()));
                }
            }

            return Task.WhenAll(allTasks);
        }

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

        private static string GetPartitionKey(object pagerContext)
        {
            return pagerContext as string;
        }

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

                IEnumerable<DataPoint> pointsInPartition = this.firehoseTable
                    .ExecuteQuery(tableQuery.Where(TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, partition)))
                    .Where(ce => (ce.CollectionTimeUtc + CloudTableDataPointStorage.ProcessingDelay) < utcNow)
                    .Select(ce => new DataPoint() { UtcTimestamp = ce.CollectionTimeUtc, Value = ce.Value });

                if (pointsInPartition.Count() > 0)
                {
                    lock (this.rawDataPageBuffers)
                    {
                        this.rawDataPageBuffers[type][partition] = pointsInPartition.ToArray();
                    }
                }
            }
        }
    }
}
