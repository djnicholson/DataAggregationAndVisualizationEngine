// <copyright file="CloudTableDataPointStorage.cs" company="David Nicholson">
//     Copyright (c) David Nicholson. All rights reserved.
// </copyright>
// <summary>See class header.</summary>

[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage(
    "Microsoft.Design",
    "CA1031:DoNotCatchGeneralExceptionTypes",
    Scope = "member",
    Target = "DAaVE.Storage.Azure.CloudTableDataPointStorage`1+<GetPageOfRawData>d__6.#MoveNext()",
    Justification = "The IL generated when using the await operator does re-throw exceptions from the task, but is too complex for FXCop to be able to determine this.")]

[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage(
    "Microsoft.Design",
    "CA1031:DoNotCatchGeneralExceptionTypes",
    Scope = "member",
    Target = "DAaVE.Storage.Azure.CloudTableDataPointStorage`1+<RebuildRawDataPageBuffersForType>d__9.#MoveNext()",
    Justification = "The IL generated when using the await operator does re-throw exceptions from the task, but is too complex for FXCop to be able to determine this.")]

namespace DAaVE.Storage.Azure
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    using DAaVE.Library;
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
        private readonly ConcurrentDictionary<TDataPointTypeEnum, ConcurrentDictionary<string, IOrderedEnumerable<DataPointObservation>>> bufferedRawDataPartitionsByType =
            new ConcurrentDictionary<TDataPointTypeEnum, ConcurrentDictionary<string, IOrderedEnumerable<DataPointObservation>>>();

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
        public Task StoreRawData(IDictionary<TDataPointTypeEnum, DataPointObservation> rawDataSample)
        {
            DateTime postMarkedOnUtc = DateTime.UtcNow;

            IEnumerable<ObservationCloudTableEntity<TDataPointTypeEnum>> recentDataPoints = rawDataSample
                .Where(d => (postMarkedOnUtc - d.Value.UtcTimestamp) < CloudTableDataPointStorage.MaximumFireHoseRecentDataPointAge)
                .Select(d => new ObservationCloudTableEntity<TDataPointTypeEnum>(d.Key, d.Value));

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
        public async Task<ConsecutiveDataPointObservationsCollection> GetPageOfRawData(TDataPointTypeEnum type)
        {
            this.bufferedRawDataPartitionsByType.AddOrUpdate(
                key: type,
                addValueFactory: _ => new ConcurrentDictionary<string, IOrderedEnumerable<DataPointObservation>>(),
                updateValueFactory: (_, existing) => existing);

            ConcurrentDictionary<string, IOrderedEnumerable<DataPointObservation>> bufferedPartitions = null;
            if (!this.bufferedRawDataPartitionsByType.TryGetValue(type, out bufferedPartitions) || !bufferedPartitions.Any())
            {
                await this.RebuildRawDataPageBuffersForType(type);

                if (!this.bufferedRawDataPartitionsByType.TryGetValue(type, out bufferedPartitions) || !bufferedPartitions.Any())
                {
                    // Either empty, or very nearly empty (due to possible racing of concurrent calls to this method)
                    return ConsecutiveDataPointObservationsCollection.Empty;
                }
            }

            KeyValuePair<string, IOrderedEnumerable<DataPointObservation>> nextPartitionData = bufferedPartitions.FirstOrDefault();
            if (nextPartitionData.Equals(default(KeyValuePair<string, DataPointObservation[]>)))
            {
                // Definite racing of concurrent calls to this method. The buffer is nearly empty anyway so no work for this caller:
                return ConsecutiveDataPointObservationsCollection.Empty;
            }

            var firehosePartition = nextPartitionData.Key;
            return new ObservationsSinglePartitionCollection<TDataPointTypeEnum>(
                type,
                firehosePartition, 
                this.aggregationTable, 
                onAggregationSuccess: () => 
                {
                    IOrderedEnumerable<DataPointObservation> _;
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
            IEnumerable<ObservationCloudTableEntity<TDataPointTypeEnum>> recentDataPoints)
        {
            IDictionary<string, TableBatchOperation> batches = new Dictionary<string, TableBatchOperation>();
            foreach (ObservationCloudTableEntity<TDataPointTypeEnum> entity in recentDataPoints)
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
        /// Creates a query that will retrieve all points in the supplied partition that were observed recently
        /// enough to be aggregated.
        /// </summary>
        /// <param name="partition">The partition to query.</param>
        /// <returns>A table query that can be used to execute a query on a table.</returns>
        private static TableQuery<ObservationCloudTableEntity<TDataPointTypeEnum>> CreateTableQueuery(string partition)
        {
            string partitionFilter = TableQuery.GenerateFilterCondition(
                "PartitionKey",
                QueryComparisons.Equal,
                partition);

            string recencyFilter = TableQuery.GenerateFilterConditionForDate(
                "CollectionTimeUtc",
                QueryComparisons.GreaterThan,
                DateTimeOffset.UtcNow.Subtract(CloudTableDataPointStorage.ProcessingDelay));

            TableQuery<ObservationCloudTableEntity<TDataPointTypeEnum>> tableQuery =
                (new TableQuery<ObservationCloudTableEntity<TDataPointTypeEnum>>())
                .Where(partitionFilter)
                .Where(recencyFilter);
            return tableQuery;
        }

        /// <summary>
        /// Queries for raw data currently eligible for aggregation.
        /// </summary>
        /// <param name="type">The type of data point to query.</param>
        /// <returns>
        /// A running task that upon successful completion guarantees that if any raw data is
        /// available for aggregation, it will have been placed in 
        /// <see cref="bufferedRawDataPartitionsByType"/>.
        /// </returns>
        private async Task RebuildRawDataPageBuffersForType(TDataPointTypeEnum type)
        {
            DateTime utcNow = DateTime.UtcNow;
            IEnumerable<string> partitions = ObservationCloudTableEntity<TDataPointTypeEnum>.GetPartitions(
                type,
                utcNow - CloudTableDataPointStorage.MaximumFireHoseRecentDataPointAge,
                utcNow - CloudTableDataPointStorage.ProcessingDelay);

            foreach (string partition in partitions)
            {
                TableQuery<ObservationCloudTableEntity<TDataPointTypeEnum>> tableQuery = 
                    CreateTableQueuery(partition);

                IList<IEnumerable<DataPointObservation>> allSegments = new List<IEnumerable<DataPointObservation>>();
                TableContinuationToken continuationToken = null;
                do
                {
                    TableQuerySegment<ObservationCloudTableEntity<TDataPointTypeEnum>> segment =
                        await this.firehoseTable.ExecuteQuerySegmentedAsync(tableQuery, continuationToken);

                    allSegments.Add(segment.Results.Select(e => new DataPointObservation() { UtcTimestamp = e.CollectionTimeUtc, Value = e.Value }));

                    continuationToken = segment.ContinuationToken;
                }
                while (continuationToken != null);

                if (allSegments.Any())
                {
                    IEnumerable<DataPointObservation> allPoints = allSegments.Aggregate((a, b) => a.Concat(b));
                    this.bufferedRawDataPartitionsByType[type][partition] = allPoints.OrderBy(dp => dp.UtcTimestamp);
                }
            }
        }
    }
}
