// <copyright file="DataCollectionOrchestrator.cs" company="David Nicholson">
//     Copyright (c) David Nicholson. All rights reserved.
// </copyright>

namespace DAaVE.Library.DataCollection
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Reflection;
    using System.Threading.Tasks;

    using DAaVE.Library.ErrorHandling;
    using DAaVE.Library.Storage;

    /// <summary>
    /// Constants used during the orchestration of various collectors.
    /// </summary>
    public static class DataCollectionOrchestrator
    {
        /// <summary>
        /// Collectors are only given a short amount of time to successfully return a result when being
        /// polled. This facilitates easy creation of naive pollers that make a synchronous blocking
        /// network request); more advanced pollers will typically return synchronously and do actual
        /// processing in their own background thread.
        /// </summary>
        public static readonly TimeSpan MaximumPollDuration = TimeSpan.FromSeconds(30.0);
    }

    /// <summary>
    /// Continually polls provided data collectors, placing their results in storage ready for
    /// aggregation.
    /// </summary>
    /// <typeparam name="TDataPointTypeEnum">Enumeration of all possible data point types.</typeparam>
    public sealed class DataCollectionOrchestrator<TDataPointTypeEnum> : IDisposable
        where TDataPointTypeEnum : struct, IComparable, IFormattable
    {
        private IDictionary<Type, DataCollectorPollerThread<TDataPointTypeEnum>> pollerThreads =
            new Dictionary<Type, DataCollectorPollerThread<TDataPointTypeEnum>>();

        private volatile bool shuttingDown = false;

        [SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "TDataPointTypeEnum")]
        [SuppressMessage("Microsoft.Design", "CA1065:DoNotRaiseExceptionsInUnexpectedLocations")]
        static DataCollectionOrchestrator()
        {
            if (!typeof(TDataPointTypeEnum).IsEnum)
            {
                throw new NotSupportedException("TDataPointTypeEnum parameter must be an enum");
            }
        }

        /// <summary>
        /// Instantiates an instance of any classes in the provided assembly that are annotated with the
        /// <see cref="DataCollectorAttribute"/> attribute using default constructors (that must exist).
        /// These newly instantiated data collectors will immeadietely being being polled for data.
        /// If a data collector class is encountered that is already being polled, the existing instance 
        /// will be shutdown.
        /// </summary>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
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

                    DataCollectorPollerThread<TDataPointTypeEnum> existingPollerThread;
                    Type dataCollectorType = newDataCollector.GetType();
                    if (this.pollerThreads.TryGetValue(dataCollectorType, out existingPollerThread))
                    {
                        existingPollerThread.Dispose();
                    }

                    var newPollerThread = new DataCollectorPollerThread<TDataPointTypeEnum>(
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
        /// Shuts down all collectors
        /// </summary>
        public void Dispose()
        {
            this.shuttingDown = true;

            lock (this.pollerThreads)
            {
                foreach (DataCollectorPollerThread<TDataPointTypeEnum> pollingThread in this.pollerThreads.Select(_ => _.Value))
                {
                    pollingThread.Dispose();
                }
            }
        }

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
