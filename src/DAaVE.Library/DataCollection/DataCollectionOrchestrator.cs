// <copyright file="DataCollectionOrchestrator.cs" company="David Nicholson">
//     Copyright (c) David Nicholson. All rights reserved.
// </copyright>
// <summary>See class header.</summary>

namespace DAaVE.Library.DataCollection
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Threading.Tasks;

    using DAaVE.Library.ErrorHandling;
    using DAaVE.Library.Storage;

    /// <summary>
    /// Continually polls provided data collectors, placing their results in storage ready for
    /// aggregation.
    /// </summary>
    /// <typeparam name="TDataPointTypeEnum">Enumeration of all possible data point types.</typeparam>
    public sealed class DataCollectionOrchestrator<TDataPointTypeEnum> : IDisposable
        where TDataPointTypeEnum : struct, IComparable, IFormattable
    {
        /// <summary>
        /// All threads polling data collectors.
        /// </summary>
        private IDictionary<Type, DataCollectorPollerBackgroundWorker<TDataPointTypeEnum>> pollerThreads =
            new Dictionary<Type, DataCollectorPollerBackgroundWorker<TDataPointTypeEnum>>();

        /// <summary>
        /// Whether a shut down is currently in progress. Can immediately be considered valid in any thread that has
        /// a lock on <see cref="pollerThreads"/>.
        /// </summary>
        private volatile bool shuttingDown = false;

        /// <summary>
        /// Instantiates an instance of any classes in the provided assembly that are annotated with the
        /// <see cref="DataCollectorAttribute"/> attribute using default constructors (that must exist).
        /// These newly instantiated data collectors will immediately begin being polled for data.
        /// If a data collector class is encountered that is already being polled, the existing instance 
        /// will be shutdown.
        /// </summary>
        /// <param name="assembly">
        /// The assembly to discover <see cref="IDataCollector{TDataPointTypeEnum}"/> implementations within.
        /// </param>
        /// <param name="dataPointFireHose">Data points will be submitted here.</param>
        /// <param name="errorSink">Exceptional circumstances will be reported here.</param>
        public void StartCollectors(
            Assembly assembly, 
            IDataPointFireHose<TDataPointTypeEnum> dataPointFireHose, 
            IErrorSink errorSink)
        {
            if (assembly == null)
            {
                throw new ArgumentNullException("assembly");
            }

            IEnumerable<IDataCollector<TDataPointTypeEnum>> dataCollectors = InstantiateCollectors(assembly);

            lock (this.pollerThreads)
            {
                if (this.shuttingDown)
                {
                    throw new ObjectDisposedException("DataCollectionOrchestrator");
                }

                TaskFactory taskFactory = new TaskFactory();

                foreach (IDataCollector<TDataPointTypeEnum> newDataCollector in dataCollectors)
                {
                    DataCollectorAttribute dataCollectorAttribute =
                        newDataCollector.GetType().GetCustomAttribute<DataCollectorAttribute>();

                    DataCollectorPollerBackgroundWorker<TDataPointTypeEnum> existingPollerThread;
                    Type dataCollectorType = newDataCollector.GetType();
                    if (this.pollerThreads.TryGetValue(dataCollectorType, out existingPollerThread))
                    {
                        existingPollerThread.Dispose();
                    }

                    var newPollerThread = new DataCollectorPollerBackgroundWorker<TDataPointTypeEnum>(
                        taskFactory,
                        newDataCollector,
                        resultProcessor: results => dataPointFireHose.StoreRawData(results),
                        pollEvery: dataCollectorAttribute.PollInterval,
                        pollResultsMustBeProducedWithin: DataCollectionOrchestrator.MaximumPollDuration,
                        errorSink: errorSink);

                    this.pollerThreads.Add(dataCollectorType, newPollerThread);
                }
            }
        }

        /// <summary>
        /// Shuts down all collectors.
        /// </summary>
        public void Dispose()
        {
            this.shuttingDown = true;

            lock (this.pollerThreads)
            {
                foreach (DataCollectorPollerBackgroundWorker<TDataPointTypeEnum> pollingThread in this.pollerThreads.Select(_ => _.Value))
                {
                    pollingThread.Dispose();
                }
            }
        }

        /// <summary>
        /// Instantiates an instance of any classes in the provided assembly that are annotated with the
        /// <see cref="DataCollectorAttribute"/> attribute using default constructors (that must exist).
        /// </summary>
        /// <param name="assembly">
        /// The assembly to discover <see cref="IDataCollector{TDataPointTypeEnum}"/> implementations within.
        /// </param>
        /// <returns>The instantiated objects.</returns>
        private static IEnumerable<IDataCollector<TDataPointTypeEnum>> InstantiateCollectors(Assembly assembly)
        {
            return assembly.GetTypes()
                .Where(t => t.CustomAttributes.Any(a => a.AttributeType.Equals(typeof(DataCollectorAttribute))))
                .Select(t => Activator.CreateInstance(t))
                .Select(o => o as IDataCollector<TDataPointTypeEnum>)
                .Where(c => c != null);
        }
    }
}
