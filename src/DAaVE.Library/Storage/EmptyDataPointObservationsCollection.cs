// <copyright file="EmptyDataPointObservationsCollection.cs" company="David Nicholson">
//     Copyright (c) David Nicholson. All rights reserved.
// </copyright>
// <summary>See class header.</summary>

namespace DAaVE.Library.Storage
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    using DAaVE.Library.DataAggregation;

    /// <summary>
    /// Represents an empty (trivially consecutive) collection of raw data points.
    /// Ignores any data provided to <see cref="ProvideAggregatedData(IEnumerable{AggregatedDataPoint})"/>.
    /// </summary>
    internal sealed class EmptyDataPointObservationsCollection : ConsecutiveDataPointObservationsCollection
    {
        /// <summary>
        /// A singleton instance.
        /// </summary>
        private static readonly EmptyDataPointObservationsCollection InstanceInternal = new EmptyDataPointObservationsCollection();

        /// <summary>
        /// A concrete implementation of an empty <see cref="IOrderedEnumerable{TElement}"/>.
        /// </summary>
        private static readonly IOrderedEnumerable<DataPointObservation> EmptyArrayAsOrderedEnumerable =
            (new DataPointObservation[0]).OrderBy(_ => 0);

        /// <summary>
        /// Initializes a new instance of the EmptyDataPointObservationsCollection class.
        /// </summary>
        public EmptyDataPointObservationsCollection() : base(EmptyArrayAsOrderedEnumerable)
        {
        }

        /// <summary>
        /// Gets a reference to the singleton instance of EmptyRawDataPointCollection.
        /// </summary>
        public static EmptyDataPointObservationsCollection Instance
        {
            get
            {
                return InstanceInternal;
            }
        }

        /// <summary>
        /// Does nothing.
        /// </summary>
        /// <param name="aggregatedDataPoints">This parameter is not used.</param>
        /// <returns>A running (or already completed) no-op task.</returns>
        public override Task ProvideAggregatedData(IEnumerable<AggregatedDataPoint> aggregatedDataPoints)
        {
            return Task.Run(() => { });
        }
    }
}
