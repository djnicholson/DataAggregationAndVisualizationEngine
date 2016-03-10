// <copyright file="DataAggregationOrchestrator.cs" company="David Nicholson">
//     Copyright (c) David Nicholson. All rights reserved.
// </copyright>
// <summary>See class header.</summary>

namespace DAaVE.Library.DataAggregation
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;

    using DAaVE.Library.ErrorHandling;
    using DAaVE.Library.Storage;
    
    /// <summary>
    /// Continually reads blocks (contiguous segments) of raw observed data point values for each possible data point 
    /// type and passes these blocks into an aggregator; the output from the aggregator is stored.
    /// </summary>
    /// <typeparam name="TDataPointTypeEnum">An enumeration of all possible data point types.</typeparam>
    public sealed class DataAggregationOrchestrator<TDataPointTypeEnum> : IDisposable
           where TDataPointTypeEnum : struct, IComparable, IFormattable
    {
        /// <summary>
        /// All aggregation threads being orchestrated.
        /// </summary>
        private IDictionary<TDataPointTypeEnum, DataAggregationBackgroundWorker<TDataPointTypeEnum>> aggregationThreads =
            new Dictionary<TDataPointTypeEnum, DataAggregationBackgroundWorker<TDataPointTypeEnum>>();

        /// <summary>
        /// Initializes a new instance of the <see cref="DataAggregationOrchestrator{TDataPointTypeEnum}"/> class.
        /// </summary>
        /// <param name="aggregator">An instance of some concrete aggregation implementation.</param>
        /// <param name="pager">Provides access to raw data points.</param>
        /// <param name="errorSink">
        /// Will receive reports of all exceptional circumstances encountered during aggregation.
        /// </param>
        public DataAggregationOrchestrator(
            IDataPointAggregator aggregator,
            IDataPointPager<TDataPointTypeEnum> pager,
            IErrorSink errorSink)
        {
            if (aggregator == null)
            {
                throw new ArgumentNullException("aggregator");
            }

            if (pager == null)
            {
                throw new ArgumentNullException("pager");
            }

            if (errorSink == null)
            {
                throw new ArgumentNullException("errorSink");
            }

            Type aggregatorType = aggregator.GetType();

            IEnumerable<MemberInfo> dataPointTypeMembersToAggregate = typeof(TDataPointTypeEnum).GetMembers().Where(
                m => m.GetCustomAttributes<AggregateWithAttribute>().Any(aggregateWith => aggregateWith.AggregatorType == aggregatorType));

            foreach (MemberInfo member in dataPointTypeMembersToAggregate)
            {
                TDataPointTypeEnum individualDataType;
                if (Enum.TryParse<TDataPointTypeEnum>(member.Name, out individualDataType))
                {
                    var newThread = new DataAggregationBackgroundWorker<TDataPointTypeEnum>(
                        individualDataType,
                        aggregator,
                        pager,
                        errorSink);

                    this.aggregationThreads.Add(individualDataType, newThread);
                }
            }
        }

        /// <summary>
        /// Shuts down the aggregation.
        /// </summary>
        public void Dispose()
        {
            foreach (DataAggregationBackgroundWorker<TDataPointTypeEnum> thread in this.aggregationThreads.Values)
            {
                thread.Dispose();
            }
        }
    }
}
