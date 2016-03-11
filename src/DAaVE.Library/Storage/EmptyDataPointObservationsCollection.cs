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
    /// Ignores any data provided to <see cref="ProvideCorrespondingAggregatedData(IEnumerable{AggregatedDataPoint})"/>.
    /// </summary>
    internal sealed class EmptyDataPointObservationsCollection : ConsecutiveDataPointObservationsCollection
    {
        /// <summary>
        /// A concrete implementation of an empty <see cref="IOrderedEnumerable{TElement}"/>.
        /// </summary>
        private static readonly IOrderedEnumerable<DataPointObservation> EmptyArrayAsOrderedEnumerable =
            (new DataPointObservation[0]).OrderBy(_ => 0);

        /// <summary>
        /// A singleton instance.
        /// </summary>
        private static readonly EmptyDataPointObservationsCollection InstanceInternal = new EmptyDataPointObservationsCollection();

        /// <summary>
        /// Initializes a new instance of the EmptyDataPointObservationsCollection class.
        /// Objects of this class are immutable and a static instance is available in <see cref="Instance"/>.
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
        /// Empty pages are always considered incomplete.
        /// </summary>
        public override bool IsPartial
        {
            get
            {
                return true;
            }
        }

        /// <summary>
        /// Does nothing.
        /// </summary>
        /// <param name="aggregatedDataPoints">This parameter is not used.</param>
        /// <returns>A running (or already completed) no-op task.</returns>
        public override Task ProvideCorrespondingAggregatedData(IEnumerable<AggregatedDataPoint> aggregatedDataPoints)
        {
            return Task.Run(() => { });
        }
    }
}
