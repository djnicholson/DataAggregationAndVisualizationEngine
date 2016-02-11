﻿// <copyright file="DataAggregationBackgroundWorker.cs" company="David Nicholson">
//     Copyright (c) David Nicholson. All rights reserved.
// </copyright>
// <summary>See class header.</summary>

namespace DAaVE.Library.DataAggregation
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    using DAaVE.Library.ErrorHandling;
    using DAaVE.Library.Storage;

    /// <summary>
    /// Continually performs aggregation over contiguous segments of observed values for a specific
    /// data point type, until disposed.
    /// </summary>
    /// <typeparam name="TDataPointTypeEnum">An enumeration of all possible data point types.</typeparam>
    internal sealed class DataAggregationBackgroundWorker<TDataPointTypeEnum> : IDisposable
        where TDataPointTypeEnum : struct, IComparable, IFormattable
    {
        /// <summary>
        /// Indicates that aggregation should now cease.
        /// </summary>
        private ManualResetEventSlim shutdownStart = new ManualResetEventSlim(false);

        /// <summary>
        /// Initializes a new instance of the DataAggregationBackgroundWorker class.
        /// </summary>
        /// <param name="type">The type of data point to aggregate in the this worker.</param>
        /// <param name="aggregator">The implementation of a specific aggregation technique.</param>
        /// <param name="pager">Provides access to raw data points.</param>
        /// <param name="errorSink">Exceptional circumstances during aggregation will be reported here.</param>
        internal DataAggregationBackgroundWorker(
            TDataPointTypeEnum type,
            IDataPointAggregator aggregator,
            IDataPointPager<TDataPointTypeEnum> pager,
            IErrorSink errorSink)
        {
            object continuationTokenCurrent = null;

            int consecutiveErrorCount = 0;

            Task.Run(() =>
            {
                try
                {
                    while (true)
                    {
                        ContiguousRawDataPointCollection pageOfUnaggregatedData;
                        do
                        {
                            pageOfUnaggregatedData = pager.GetPageOfRawData(type);

                            if (pageOfUnaggregatedData.Count() == 0)
                            {
                                if (shutdownStart.Wait(DataAggregationOrchestrator.SleepDurationOnDataExhaustion))
                                {
                                    return;
                                }
                            }
                        }
                        while (pageOfUnaggregatedData.Count() == 0);

                        IEnumerable<AggregatedDataPoint> aggregatedData = aggregator.Aggregate(pageOfUnaggregatedData);

                        // TODO: Let this go async (it will be mostly network IO to Azure) and get started on the (probably CPU-heavy)
                        // work of doing the next aggregation
                        pageOfUnaggregatedData.ProvideAggregatedData(aggregatedData).Wait();

                        consecutiveErrorCount = 0;
                    }
                }
                catch (Exception e)
                {
                    errorSink.OnError("Exception when aggregating page of data of type: " + type + "@" + continuationTokenCurrent, e);

                    consecutiveErrorCount++;
                    if (consecutiveErrorCount > 20)
                    {
                        errorSink.OnError("Too many consecutive errors; re-throwing", e);
                        throw;
                    }

                    shutdownStart.Wait(DataAggregationOrchestrator.SleepDurationOnError);
                }
            });
        }

        /// <summary>
        /// Trigger a shutdown after any currently active polls return.
        /// </summary>
        public void Dispose()
        {
            this.shutdownStart.Set();
        }
    }
}