// <copyright file="DataCollectorObserver.cs" company="David Nicholson">
//     Copyright (c) David Nicholson. All rights reserved.
// </copyright>
// <summary>See class header.</summary>

namespace DAaVE.Library.DataCollection
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    
    /// <summary>
    /// Watch a single data collector and processes its observations, until disposed.
    /// </summary>
    /// <typeparam name="TDataPointType">The type of data point produced by the data collector.</typeparam>
    internal sealed class DataCollectorObserver<TDataPointType> : IDisposable
            where TDataPointType : struct, IComparable, IFormattable
    {
        /// <summary>
        /// Allows the processing loop to be shutdown.
        /// </summary>
        private readonly CancellationTokenSource cancellationTokenSource;

        /// <summary>
        /// Processes observations as they become available.
        /// </summary>
        private readonly Task processingLoop;

        /// <summary>
        /// Initializes a new instance of the <see cref="DataCollectorObserver{TDataPointType}"/> class.
        /// </summary>
        /// <param name="dataCollector">An instance of an implementation that can poll for the current value of a data point type.</param>
        /// <param name="resultProcessor">Callback to be invoked when new and valid data is available.</param>
        internal DataCollectorObserver(
            DataCollector<TDataPointType> dataCollector,
            Action<Observation<TDataPointType>> resultProcessor)
        {
            this.cancellationTokenSource = new CancellationTokenSource();

            this.processingLoop = Task.Run(
                () => 
                {
                    Observation<TDataPointType> observation = null;
                    while (!dataCollector.Observations.TryDequeue(out observation))
                    {
                        Task.WaitAny(
                            new[] { dataCollector.Wait() },
                            this.cancellationTokenSource.Token);
                    }

                    if ((observation != null) && 
                        (observation.Data != null))
                    {
                        resultProcessor(observation);
                    }
                }, 
                this.cancellationTokenSource.Token);
        }

        /// <summary>
        /// Trigger a shutdown after any currently active polls return.
        /// </summary>
        public void Dispose()
        {
            this.cancellationTokenSource.Cancel();
            this.processingLoop.Wait();
        }
    }
}
