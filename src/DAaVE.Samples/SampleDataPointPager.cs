// <copyright file="SampleDataPointPager.cs" company="David Nicholson">
//     Copyright (c) David Nicholson. All rights reserved.
// </copyright>
// <summary>See class header.</summary>

namespace DAaVE.Samples
{
    using System;
    using System.Collections.Concurrent;
    using System.Diagnostics.CodeAnalysis;
    using System.Threading;
    using System.Threading.Tasks;

    using DAaVE.Library.Storage;
    
    /// <summary>
    /// A trivial implementation of <see cref="IDataPointPager{TDataPointTypeEnum}"/>. Allows arbitrary code to be
    /// registered for execution whenever data of a particular type is requested.
    /// </summary>
    /// <typeparam name="TDataPointTypeEnum">An enumeration of all possible types of data point.</typeparam>
    public sealed class SampleDataPointPager<TDataPointTypeEnum> : IDataPointPager<TDataPointTypeEnum>, IDisposable
        where TDataPointTypeEnum : struct, IComparable, IFormattable
    {
        /// <summary>
        /// A dictionary to store a queue (and a corresponding semaphore used to signal new items) for each data
        /// point type that is used.
        /// </summary>
        private ConcurrentDictionary<TDataPointTypeEnum, Tuple<ConcurrentQueue<Func<ConsecutiveDataPointObservationsCollection>>, SemaphoreSlim>> pendingObservations;

        /// <summary>
        /// All semaphores created by this object.
        /// </summary>
        private ConcurrentQueue<SemaphoreSlim> semaphoresCreated;

        /// <summary>
        /// Initializes a new instance of the SampleDataPointPager class. Initially no data is available.
        /// </summary>
        public SampleDataPointPager()
        {
            this.pendingObservations = 
                new ConcurrentDictionary<TDataPointTypeEnum, Tuple<ConcurrentQueue<Func<ConsecutiveDataPointObservationsCollection>>, SemaphoreSlim>>();

            this.semaphoresCreated = new ConcurrentQueue<SemaphoreSlim>();
        }

        /// <summary>
        /// Register some code that could be executed as a result of a subsequent call to <see cref="GetPageOfObservations(TDataPointTypeEnum)"/>.
        /// (the code returns a <see cref="ConsecutiveDataPointObservationsCollection"/>).
        /// </summary>
        /// <param name="type">The type of data point that <paramref name="observation"/> generates observations for.</param>
        /// <param name="observation">
        /// Code that may run at a later time to retrieve a page of observations. Placed at the back of a queue of any other
        /// currently unused observation retrievers (from previous calls to this method).
        /// </param>
        public void QueueObservation(TDataPointTypeEnum type, Func<ConsecutiveDataPointObservationsCollection> observation)
        {
            Tuple<ConcurrentQueue<Func<ConsecutiveDataPointObservationsCollection>>, SemaphoreSlim> queue =
                this.pendingObservations.GetOrAdd(type, _ => this.NewDataTypeQueue());

            queue.Item1.Enqueue(observation);
            queue.Item2.Release();
        }

        /// <inheritdoc/>
        public Task<ConsecutiveDataPointObservationsCollection> GetPageOfObservations(TDataPointTypeEnum type)
        {
            Tuple<ConcurrentQueue<Func<ConsecutiveDataPointObservationsCollection>>, SemaphoreSlim> queue =
                this.pendingObservations.GetOrAdd(type, _ => this.NewDataTypeQueue());

            Func<ConsecutiveDataPointObservationsCollection> result;
            while (!queue.Item1.TryDequeue(out result))
            {
                queue.Item2.Wait();
            }

            return Task.Run(result);
        }

        /// <summary>
        /// Disposes any semaphores that were created by this object.
        /// </summary>
        public void Dispose()
        {
            foreach (SemaphoreSlim semaphore in this.semaphoresCreated)
            {
                semaphore.Dispose();
            }
        }

        /// <summary>
        /// Construct a new (queue, semaphore) tuple to store observation generation functions for a specific
        /// data point type.
        /// </summary>
        /// <returns>A new (queue, semaphore) tuple.</returns>
        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "Semaphore owned by class.")]
        private Tuple<ConcurrentQueue<Func<ConsecutiveDataPointObservationsCollection>>, SemaphoreSlim> NewDataTypeQueue()
        {
            SemaphoreSlim semaphore = new SemaphoreSlim(0);
            this.semaphoresCreated.Enqueue(semaphore);

            return new Tuple<ConcurrentQueue<Func<ConsecutiveDataPointObservationsCollection>>, SemaphoreSlim>(
                new ConcurrentQueue<Func<ConsecutiveDataPointObservationsCollection>>(),
                semaphore);
        }
    }
}
