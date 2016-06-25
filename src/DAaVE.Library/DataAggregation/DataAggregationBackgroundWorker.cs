// <copyright file="DataAggregationBackgroundWorker.cs" company="David Nicholson">
//     Copyright (c) David Nicholson. All rights reserved.
// </copyright>
// <summary>See class header.</summary>

[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage(
    "Microsoft.Design",
    "CA1031:DoNotCatchGeneralExceptionTypes",
    Scope = "member",
    Target = "DAaVE.Library.DataAggregation.DataAggregationBackgroundWorker`1+<>c__DisplayClass2_1+<<-ctor>b__0>d.#MoveNext()",
    Justification = "After a contiguous sequence of exceptional aggregations, an exception will be re-thrown.")]

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
        private readonly CancellationTokenSource disposeCancellationSource;

        /// <summary>
        /// Task performing continuous aggregations.
        /// </summary>
        private Task worker;

        /// <summary>
        /// Task uploading the results of an aggregation (possibly null).
        /// </summary>
        private Task uploadInProgress;

        /// <summary>
        /// Amount of consecutive invocations of the main loop within <see cref="worker"/> that have resulted in exception.
        /// </summary>
        private int consecutiveErrorCount;

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
            this.consecutiveErrorCount = 0;

            this.disposeCancellationSource = new CancellationTokenSource();

            this.worker = Task.Run(() =>
            {
                while (true)
                {
                    try
                    {
                        ConsecutiveDataPointObservationsCollection pageOfUnaggregatedData;
                        do
                        {
                            Task<ConsecutiveDataPointObservationsCollection> observationRetriever = pager.GetPageOfObservations(type);
                            if (!this.WaitForTaskCompletionOrWorkerDisposal(observationRetriever))
                            {
                                return;
                            }
                            
                            pageOfUnaggregatedData = observationRetriever.Result;
                            if (pageOfUnaggregatedData.Count() == 0)
                            {
                                if (!this.WaitForTaskCompletionOrWorkerDisposal(Task.Delay(DataAggregationOrchestrator.SleepDurationOnDataExhaustion)))
                                {
                                    return;
                                }
                            }
                        }
                        while (pageOfUnaggregatedData.Count() == 0);

                        IEnumerable<AggregatedDataPoint> aggregatedData = aggregator.Aggregate(pageOfUnaggregatedData);

                        if (aggregatedData.Any())
                        {
                            if (this.uploadInProgress != null)
                            {
                                // Aggregation (CPU heavy) and upload (IO heavy) are allowed to happen in parallel, but only one
                                // of each at a time.
                                this.uploadInProgress.Wait();
                                this.uploadInProgress.Dispose();
                            }

                            this.uploadInProgress = pageOfUnaggregatedData.ProvideCorrespondingAggregatedData(aggregatedData);
                        }

                        this.consecutiveErrorCount = 0;
                    }
                    catch (Exception e)
                    {
                        string activityDescription = "aggregation of " + type + " data from " + pager + " using " + aggregator;
                        if (!HandleException(activityDescription, e, errorSink))
                        {
                            throw;
                        }
                    }
                }
            });
        }

        /// <summary>
        /// Trigger a shutdown after any in-progress aggregation computations and/or aggregation uploads complete.
        /// </summary>
        public void Dispose()
        {
            this.disposeCancellationSource.Cancel();

            if (this.worker != null)
            {
                this.worker.Wait();
                this.worker.Dispose();
                this.worker = null;
            }

            if (this.uploadInProgress != null)
            {
                this.uploadInProgress.Wait();
                this.uploadInProgress.Dispose();
                this.uploadInProgress = null;
            }
        }

        /// <summary>
        /// Waits for a task to complete, or disposal to be initiated (whichever happens first).
        /// </summary>
        /// <param name="task">An already running (or completed) task.</param>
        /// <returns>False if disposal was initiated while the task was running, true otherwise.</returns>
        private bool WaitForTaskCompletionOrWorkerDisposal(Task task)
        {
            bool completed = false;

            try
            {
                task.Wait(this.disposeCancellationSource.Token);
                completed = true;
            }
            catch (OperationCanceledException)
            {
            }

            if (task.IsFaulted)
            {
                throw task.Exception;
            }

            return completed;
        }

        /// <summary>
        /// Attempts to gracefully handle exceptions encountered during aggregation.
        /// </summary>
        /// <param name="activityDescription">Description of the activity in progress when the exception occurred.</param>
        /// <param name="exception">An exception encountered during aggregation.</param>
        /// <param name="errorSink">Used to log error messages.</param>
        /// <returns>True if the exception was handled; false if not (the caller should re-throw).</returns>
        private bool HandleException(string activityDescription, Exception exception, IErrorSink errorSink)
        {
            AggregateException aggregateException = exception as AggregateException;
            if (aggregateException != null)
            {
                bool handled = true;
                foreach (Exception innerExcpetion in aggregateException.InnerExceptions)
                {
                    handled = handled && this.HandleException(activityDescription, innerExcpetion, errorSink);
                }

                return handled;
            }

            errorSink.OnError("Exception during " + activityDescription, exception);

            this.consecutiveErrorCount++;
            if (this.consecutiveErrorCount == 20)
            {
                errorSink.OnError("Too many consecutive errors during " + activityDescription + "; re-throwing", exception);
                return false;
            }
            else
            {
                this.WaitForTaskCompletionOrWorkerDisposal(Task.Delay(DataAggregationOrchestrator.SleepDurationOnError));
                return true;
            }
        }
    }
}