// <copyright file="EmptyRawDataPointCollection.cs" company="David Nicholson">
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
    /// Represents an empty (trivially sorted) collection of raw data points.
    /// Ignores any data provided to <see cref="ProvideAggregatedData(IEnumerable{AggregatedDataPoint})"/>.
    /// </summary>
    internal sealed class EmptyRawDataPointCollection : ContinuousRawDataPointCollection
    {
        /// <summary>
        /// A singleton instance.
        /// </summary>
        private static readonly EmptyRawDataPointCollection InstanceInternal = new EmptyRawDataPointCollection();

        /// <summary>
        /// A concrete implementation of an empty <see cref="IOrderedEnumerable{TElement}"/>.
        /// </summary>
        private static readonly IOrderedEnumerable<DataPoint> EmptyArrayAsOrderedEnumerable =
            (new DataPoint[0]).OrderBy(_ => 0);

        /// <summary>
        /// Initializes a new instance of the EmptyRawDataPointCollection class.
        /// </summary>
        public EmptyRawDataPointCollection() : base(EmptyArrayAsOrderedEnumerable)
        {
        }

        /// <summary>
        /// Gets a reference to the singleton instance of EmptyRawDataPointCollection.
        /// </summary>
        public static EmptyRawDataPointCollection Instance
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
