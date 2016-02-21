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
        private IDictionary<Type, DataCollectorObserver<TDataPointTypeEnum>> pollerThreads =
            new Dictionary<Type, DataCollectorObserver<TDataPointTypeEnum>>();

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
        /// The assembly to discover <see cref="DataCollector{TDataPointTypeEnum}"/> implementations within.
        /// </param>
        /// <param name="dataPointFireHose">Data points will be submitted here.</param>
        public void StartCollectors(
            Assembly assembly, 
            IDataPointFireHose<TDataPointTypeEnum> dataPointFireHose)
        {
            if (assembly == null)
            {
                throw new ArgumentNullException("assembly");
            }

            IEnumerable<DataCollector<TDataPointTypeEnum>> dataCollectors = InstantiateCollectors(assembly);

            lock (this.pollerThreads)
            {
                if (this.shuttingDown)
                {
                    throw new ObjectDisposedException("DataCollectionOrchestrator");
                }

                foreach (DataCollector<TDataPointTypeEnum> newDataCollector in dataCollectors)
                {
                    DataCollectorObserver<TDataPointTypeEnum> existingPollerThread;
                    Type dataCollectorType = newDataCollector.GetType();
                    if (this.pollerThreads.TryGetValue(dataCollectorType, out existingPollerThread))
                    {
                        existingPollerThread.Dispose();
                    }

                    Action<Observation<TDataPointTypeEnum>> resultProcessor = observation =>
                    {
                        dataPointFireHose.StoreRawData(
                            observation.Data.ToDictionary(datum => datum.Key, datum => new DataPointObservation(observation.DateTimeUtc, datum.Value)));
                    };

                    var newPollerThread = new DataCollectorObserver<TDataPointTypeEnum>(newDataCollector, resultProcessor);

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
                foreach (DataCollectorObserver<TDataPointTypeEnum> pollingThread in this.pollerThreads.Select(_ => _.Value))
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
        /// The assembly to discover <see cref="DataCollector{TDataPointTypeEnum}"/> implementations within.
        /// </param>
        /// <returns>The instantiated objects.</returns>
        private static IEnumerable<DataCollector<TDataPointTypeEnum>> InstantiateCollectors(Assembly assembly)
        {
            return assembly.GetTypes()
                .Where(t => t.CustomAttributes.Any(a => a.AttributeType.Equals(typeof(DataCollectorAttribute))))
                .Select(t => Activator.CreateInstance(t))
                .Select(o => o as DataCollector<TDataPointTypeEnum>)
                .Where(c => c != null);
        }
    }
}
