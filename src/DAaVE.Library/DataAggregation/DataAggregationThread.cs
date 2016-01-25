using DAaVE.Library.ErrorHandling;
using DAaVE.Library.Storage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace DAaVE.Library.DataAggregation
{
    internal class DataAggregationThread<TDataPointType> : IDisposable
        where TDataPointType : struct, IComparable, IFormattable
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "TDataPointTypeEnum")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1065:DoNotRaiseExceptionsInUnexpectedLocations")]
        static DataAggregationThread()
        {
            if (!typeof(TDataPointType).IsEnum)
            {
                throw new NotSupportedException("TDataPointTypeEnum parameter must be an enum");
            }
        }

        private ManualResetEventSlim shutdownStart = new ManualResetEventSlim(false);

        internal DataAggregationThread(
            TDataPointType type,
            IDataPointAggregator aggregator,
            IDataPointPager<TDataPointType> pager,
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
                        object continuationTokenNext = continuationTokenCurrent;

                        IEnumerable<DataPoint> pageOfUnaggregatedData;
                        do
                        {
                            pageOfUnaggregatedData = pager.ReadPageOfRawData(type, ref continuationTokenNext);

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

                        pager.StoreAggregatedData(type, aggregatedData, continuationTokenNext);

                        continuationTokenCurrent = continuationTokenNext;
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
            shutdownStart.Set();
        }
    }
}