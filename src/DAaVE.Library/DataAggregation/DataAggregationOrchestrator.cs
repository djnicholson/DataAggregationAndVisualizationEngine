// <copyright file="DataAggregationOrchestrator.cs" company="David Nicholson">
//     Copyright (c) David Nicholson. All rights reserved.
// </copyright>

namespace DAaVE.Library.DataAggregation
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using DAaVE.Library.ErrorHandling;
    using DAaVE.Library.Storage;

    /// <summary>
    /// Continually reads raw collected data for each data point type and passes it into an aggregator; the
    /// output from the aggregator is stored.
    /// </summary>
    /// <typeparam name="TDataPointTypeEnum">The type of data point being aggregated.</typeparam>
    public sealed class DataAggregationOrchestrator<TDataPointTypeEnum> : IDisposable
           where TDataPointTypeEnum : struct, IComparable, IFormattable
    {
        /// <summary>
        /// All aggregation threads being orchestrated.
        /// </summary>
        private IDictionary<TDataPointTypeEnum, DataAggregationThread<TDataPointTypeEnum>> aggregationThreads =
            new Dictionary<TDataPointTypeEnum, DataAggregationThread<TDataPointTypeEnum>>();

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
            foreach (TDataPointTypeEnum dataType in Enum.GetValues(typeof(TDataPointTypeEnum)).Cast<TDataPointTypeEnum>())
            {
                var newThread = new DataAggregationThread<TDataPointTypeEnum>(
                    dataType,
                    aggregator,
                    pager,
                    errorSink);

                this.aggregationThreads.Add(dataType, newThread);
            }
        }

        /// <summary>
        /// Shuts down the aggregation.
        /// </summary>
        public void Dispose()
        {
            foreach (DataAggregationThread<TDataPointTypeEnum> thread in this.aggregationThreads.Values)
            {
                thread.Dispose();
            }
        }
    }
}
