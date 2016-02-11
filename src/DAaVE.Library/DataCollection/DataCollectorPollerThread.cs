// <copyright file="DataCollectorPollerThread.cs" company="David Nicholson">
//     Copyright (c) David Nicholson. All rights reserved.
// </copyright>
// <summary>See class header.</summary>

namespace DAaVE.Library.DataCollection
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;

    using DAaVE.Library.ErrorHandling;

    /// <summary>
    /// Continually polls a single data collector.
    /// </summary>
    /// <typeparam name="DataPointType">The type of data point produced by the data collector.</typeparam>
    internal sealed class DataCollectorPollerThread<DataPointType> : IDisposable
            where DataPointType : struct, IComparable, IFormattable
    {
        /// <summary>
        /// Factory used to create tasks to invoke <see cref="dataCollector"/> within.
        /// </summary>
        private readonly TaskFactory taskFactory;

        /// <summary>
        /// An instance of an implementation that can poll for the current value of a data point type.
        /// </summary>
        private readonly IDataCollector<DataPointType> dataCollector;

        /// <summary>
        /// Callback to be invoked when new and valid data is available.
        /// </summary>
        private readonly Action<IDictionary<DataPointType, DataPoint>> resultProcessor;

        /// <summary>
        /// The amount of time to wait during any single invocation of <see cref="dataCollector"/> before
        /// giving up.
        /// </summary>
        private readonly TimeSpan pollResultsMustBeProducedWithin;

        /// <summary>
        /// Any exceptional circumstances will be reported here.
        /// </summary>
        private readonly IErrorSink errorSink;

        /// <summary>
        /// New invocations of <see cref="dataCollector"/> are issued cancellation tokens from here.
        /// </summary>
        private readonly CancellationTokenSource pollLoopCancellationTokenSource;

        /// <summary>
        /// Timer for scheduling continued invocations.
        /// </summary>
        private readonly Timer pollingLoop;

        /// <summary>
        /// Signals full termination of all outstanding invocations (gates return of <see cref="Dispose"/>).
        /// </summary>
        private readonly ManualResetEventSlim pollingLoopTerminated;

        /// <summary>
        /// Initializes a new instance of the <see cref="DataCollectorPollerThread{DataPointType}"/> class.
        /// </summary>
        /// <param name="taskFactory">Factory used to create tasks to invoke <see cref="dataCollector"/> within.</param>
        /// <param name="dataCollector">An instance of an implementation that can poll for the current value of a data point type.</param>
        /// <param name="resultProcessor">Callback to be invoked when new and valid data is available.</param>
        /// <param name="pollEvery">The frequency at which <paramref name="dataCollector"/> is to be polled.</param>
        /// <param name="pollResultsMustBeProducedWithin">
        /// The <paramref name="resultProcessor"/> is not to be invoked with data older than this, even if such data becomes
        /// available.
        /// </param>
        /// <param name="errorSink">Exceptional circumstances will be reported here.</param>
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

        /// <summary>
        /// Code invoked periodically (and potentially concurrently depending on speed of return)
        /// that makes an individual poll for data.
        /// </summary>
        /// <param name="state">This parameter is not used.</param>
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

        /// <summary>
        /// Makes a single poll for data and waits up to <see cref="pollResultsMustBeProducedWithin"/> for the results.
        /// If results are available within that time, they will be supplied to <see cref="resultProcessor"/>.
        /// </summary>
        private void IndividualPoll()
        {
            using (Task<IDictionary<DataPointType, DataPoint>> newDataPointsTask = this.InvokePoll())
            {
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
        }

        /// <summary>
        /// Requests data from <see cref="dataCollector"/>, reporting any exceptions to <see cref="errorSink"/> before
        /// re-throwing them.
        /// </summary>
        /// <returns>The task within which the collection is taking place.</returns>
        private Task<IDictionary<DataPointType, DataPoint>> InvokePoll()
        {
            try
            {
                return this.dataCollector.Poll();
            }
            catch (Exception e)
            {
                this.errorSink.OnError("Exception when polling " + this.dataCollector + ": " + e.Message, e);
                throw;
            }
        }
    }
}
