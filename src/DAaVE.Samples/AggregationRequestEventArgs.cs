// <copyright file="AggregationRequestEventArgs.cs" company="David Nicholson">
//     Copyright (c) David Nicholson. All rights reserved.
// </copyright>
// <summary>See class header.</summary>

namespace DAaVE.Samples
{
    using System;

    using DAaVE.Library.Storage;

    /// <summary>
    /// Information about a request made to an instance of <see cref="SampleDataPointAggregator"/>.
    /// </summary>
    public sealed class AggregationRequestEventArgs : EventArgs
    {
        /// <summary>
        /// Initializes a new instance of the AggregationRequestEventArgs class.
        /// </summary>
        /// <param name="input">Data that is about to be aggregated.</param>
        public AggregationRequestEventArgs(ConsecutiveDataPointObservationsCollection input)
        {
            this.Input = input;
        }

        /// <summary>
        /// Gets the (presumably, un-aggregated) data that the aggregator was asked to aggregate.
        /// </summary>
        /// <value>Data supplied to the aggregator.</value>
        public ConsecutiveDataPointObservationsCollection Input
        {
            get;
            private set;
        }
    }
}