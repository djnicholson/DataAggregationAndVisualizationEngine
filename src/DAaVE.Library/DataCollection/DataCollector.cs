// <copyright file="DataCollector.cs" company="David Nicholson">
//     Copyright (c) David Nicholson. All rights reserved.
// </copyright>
// <summary>See class header.</summary>

namespace DAaVE.Library.DataCollection
{
    using System;
    using System.Collections.Concurrent;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Capable of providing recent raw values for one or more data point types.
    /// </summary>
    /// <typeparam name="TDataPointTypeEnum">An enumeration of all possible data point types.</typeparam>
    public abstract class DataCollector<TDataPointTypeEnum> : IDisposable
        where TDataPointTypeEnum : struct, IComparable, IFormattable
    {
        /// <summary>
        /// Whether <see cref="Dispose(bool)"/> has been invoked (in any mode).
        /// </summary>
        private bool isDisposed;

        /// <summary>
        /// Used to signal that a new observation has been added to <see cref="Observations"/>.
        /// </summary>
        private ManualResetEventSlim observationAdded;

        /// <summary>
        /// Initializes a new instance of the DataCollector class with an empty observation queue.
        /// </summary>
        protected DataCollector()
        {
            this.observationAdded = new ManualResetEventSlim();
            this.Observations = new ConcurrentQueue<Observation<TDataPointTypeEnum>>();
            this.isDisposed = false;
        }

        /// <summary>
        /// Finalizes an instance of the DataCollector class.
        /// Delegates to <see cref="Dispose(bool)"/> in finalization mode.
        /// </summary>
        ~DataCollector()
        {
            this.Dispose(isDisposing: false);
        }

        /// <summary>
        /// Gets a queue of (possibly empty) collection of data point values that were simultaneously observed.
        /// An empty collection is an indication that successful observation activity took place but there
        /// were no data points to report. A empty queue indicates that no new successful observations are
        /// available.
        /// </summary>
        internal ConcurrentQueue<Observation<TDataPointTypeEnum>> Observations
        {
            get;
            private set;
        }

        /// <summary>
        /// Delegates to <see cref="Dispose(bool)"/> in disposing mode.
        /// </summary>
        public void Dispose()
        {
            this.Dispose(isDisposing: true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Wait for a new observation to be added to <see cref="Observations"/>.
        /// </summary>
        /// <returns>
        /// A running task whose completion is an indication that an observation was recently 
        /// added to <see cref="Observations"/>
        /// </returns>
        internal Task Wait()
        {
            this.observationAdded.Reset();

            return Task.Run(() =>
            {
                this.observationAdded.Wait();
            });
        }

        /// <summary>
        /// Provide a (possibly empty) set of data points that were just observed. This data will most likely
        /// be ignored if received after <see cref="CeaseObservations"/> has been invoked.
        /// </summary>
        /// <param name="observation">
        /// Zero or more simultaneously observed data points during an observation activity that just completed.
        /// </param>
        protected void Observe(Observation<TDataPointTypeEnum> observation)
        {
            if (observation == null)
            {
                throw new ArgumentNullException("observation");
            }

            if (!this.isDisposed)
            {
                this.Observations.Enqueue(observation);
                this.observationAdded.Set();
            }
        }

        /// <summary>
        /// After this method is invoked, further observations passed to <see cref="Observe(Observation{TDataPointTypeEnum})"/>
        /// will (most likely) be ignored.
        /// </summary>
        protected abstract void CeaseObservations();

        /// <summary>
        /// Delegates to <see cref="CeaseObservations"/> (but only when not invoked as a result of finalization).
        /// </summary>
        /// <param name="isDisposing">
        /// Whether this invocation from within a subclass Dispose implementation (i.e. not during finalization).</param>
        protected virtual void Dispose(bool isDisposing)
        {
            this.isDisposed = true;

            if (isDisposing)
            {
                this.CeaseObservations();
                this.observationAdded.Dispose();
            }
        }
    }
}
