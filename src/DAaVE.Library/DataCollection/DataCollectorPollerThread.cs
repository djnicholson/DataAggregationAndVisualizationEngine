// <copyright file="DataCollectorPollerThread.cs" company="David Nicholson">
//     Copyright (c) David Nicholson. All rights reserved.
// </copyright>

namespace DAaVE.Library.DataCollection
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;

    using DAaVE.Library.ErrorHandling;

    internal sealed class DataCollectorPollerThread<DataPointType> : IDisposable
            where DataPointType : struct, IComparable, IFormattable
    {
        private readonly TaskFactory taskFactory;
        private readonly IDataCollector<DataPointType> dataCollector;
        private readonly Action<IDictionary<DataPointType, DataPoint>> resultProcessor;
        private readonly TimeSpan pollResultsMustBeProducedWithin;
        private readonly IErrorSink errorSink;
        private readonly CancellationTokenSource pollLoopCancellationTokenSource;
        private readonly Timer pollingLoop;
        private readonly ManualResetEventSlim pollingLoopTerminated;

        public DataCollectorPollerThread(
            TaskFactory taskFactory,
            IDataCollector<DataPointType> dataCollector,
            Action<IDictionary<DataPointType, DataPoint>> resultProcessor,
            TimeSpan pollEvery,
            TimeSpan pollResultsMustBeProducedWithin,
            IErrorSink errorSink)
        {
            this.taskFactory = taskFactory;
            this.dataCollector = dataCollector;
            this.resultProcessor = resultProcessor;
            this.pollResultsMustBeProducedWithin = pollResultsMustBeProducedWithin;
            this.errorSink = errorSink;
            this.pollLoopCancellationTokenSource = new CancellationTokenSource();
            this.pollingLoopTerminated = new ManualResetEventSlim(false);

            this.pollingLoop = new Timer(this.PollLoop, state: null, dueTime: -1, period: -1);
            this.pollingLoop.Change(dueTime: TimeSpan.FromTicks(0L), period: pollEvery);
        }

        /// <summary>
        /// Trigger a shutdown after any currently active polls return.
        /// </summary>
        public void Dispose()
        {
            this.pollLoopCancellationTokenSource.Cancel();
            this.pollingLoopTerminated.Wait();
        }

        private void PollLoop(object state)
        {
            if (this.pollLoopCancellationTokenSource.IsCancellationRequested)
            {
                this.pollingLoop.Dispose();

                try
                {
                    this.dataCollector.OnPollingComplete();
                }
                catch (Exception e)
                {
                    this.errorSink.OnError("Exception when notifying " + this.dataCollector + " about polling completion: " + e.Message, e);
                    throw;
                }
                finally
                {
                    this.pollingLoopTerminated.Set();
                }
            }
            else
            {
                this.taskFactory.StartNew(this.IndividualPoll);
            }
        }

        private void IndividualPoll()
        {
            Task<IDictionary<DataPointType, DataPoint>> newDataPointsTask = this.TryInvokePoll();

            bool succeededWithinTimeLimit = newDataPointsTask.Wait(this.pollResultsMustBeProducedWithin);

            if (!succeededWithinTimeLimit)
            {
                this.errorSink.OnError(
                    "A poll of " + this.dataCollector + " is taking too long its results (if any) will be ignored; the polling code may still be consuming resources");
            }
            else
            {
                IDictionary<DataPointType, DataPoint> newDataPoints = newDataPointsTask.Result;

                if (newDataPoints != null)
                {
                    this.resultProcessor(newDataPointsTask.Result);
                }
            }
        }

        private async Task<IDictionary<DataPointType, DataPoint>> TryInvokePoll()
        {
            try
            {
                return await this.dataCollector.Poll();
            }
            catch (Exception e)
            {
                this.errorSink.OnError("Exception when polling " + this.dataCollector + ": " + e.Message, e);
                throw;
            }
        }
    }
}
