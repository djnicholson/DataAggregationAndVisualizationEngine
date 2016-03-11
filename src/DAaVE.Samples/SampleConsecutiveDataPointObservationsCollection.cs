// <copyright file="SampleConsecutiveDataPointObservationsCollection.cs" company="David Nicholson">
//     Copyright (c) David Nicholson. All rights reserved.
// </copyright>
// <summary>See class header.</summary>

namespace DAaVE.Samples
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    using DAaVE.Library;
    using DAaVE.Library.DataAggregation;
    using DAaVE.Library.Storage;
    
    /// <summary>
    /// A trivial implementation of <see cref="ConsecutiveDataPointObservationsCollection"/>.
    /// </summary>
    public sealed class SampleConsecutiveDataPointObservationsCollection : ConsecutiveDataPointObservationsCollection
    {
        /// <summary>
        /// Backing store for <see cref="IsPartial"/>.
        /// </summary>
        private bool isPartial;

        /// <summary>
        /// Initializes a new instance of the SampleConsecutiveDataPointObservationsCollection class.
        /// </summary>
        /// <param name="observations">The set of data point observations to represent.</param>
        /// <param name="isPartial">Whether <paramref name="observations"/> is considered to be partial.</param>
        public SampleConsecutiveDataPointObservationsCollection(
            IOrderedEnumerable<DataPointObservation> observations,
            bool isPartial)
            : base(observations)
        {
            this.isPartial = isPartial;
        }

        /// <inheritdoc />
        public override bool IsPartial
        {
            get
            {
                return this.isPartial;
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
