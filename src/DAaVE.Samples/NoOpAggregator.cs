// <copyright file="NoOpAggregator.cs" company="David Nicholson">
//     Copyright (c) David Nicholson. All rights reserved.
// </copyright>
// <summary>See class header.</summary>

namespace DAaVE.Samples
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;

    using DAaVE.Library;
    using DAaVE.Library.DataAggregation;
    using DAaVE.Library.Storage;
    
    /// <summary>
    /// A trivial implementation of <see cref="IDataPointAggregator"/>.
    /// </summary>
    public sealed class NoOpAggregator : IDataPointAggregator
    {
        /// <summary>
        /// Outputs debug information about observations.
        /// </summary>
        /// <param name="continuousObservations">
        /// Each item will be passed to <see cref="Debug.WriteLine(object)"/>.
        /// </param>
        /// <returns>An empty set of aggregated data points.</returns>
        public IEnumerable<AggregatedDataPoint> Aggregate(
            ConsecutiveDataPointObservationsCollection continuousObservations)
        {
            Debug.WriteLine("Enter: NoOpAggregator.Aggregate");

            if (continuousObservations == null)
            {
                throw new ArgumentNullException("continuousObservations");
            }

            foreach (DataPointObservation dataPoint in continuousObservations)
            {
                Debug.WriteLine(dataPoint);
            }

            Debug.WriteLine("Success: NoOpAggregator.Aggregate");

            return new AggregatedDataPoint[0];
        }
    }
}
