// <copyright file="SampleDataPointAggregator.cs" company="David Nicholson">
//     Copyright (c) David Nicholson. All rights reserved.
// </copyright>
// <summary>See class header.</summary>

namespace DAaVE.Samples
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;

    using DAaVE.Library;
    using DAaVE.Library.DataAggregation;
    using DAaVE.Library.Storage;
    
    /// <summary>
    /// A trivial implementation of <see cref="IDataPointAggregator"/>.
    /// </summary>
    public sealed class SampleDataPointAggregator : IDataPointAggregator
    {
        /// <summary>
        /// A callback that can be invoked whenever an aggregation is requested to supply
        /// appropriate aggregated data points.
        /// </summary>
        private Func<ConsecutiveDataPointObservationsCollection, IEnumerable<AggregatedDataPoint>> callback;

        /// <summary>
        /// Initializes a new instance of the SampleDataPointAggregator class.
        /// </summary>
        /// <param name="callback">
        /// A callback that can be invoked whenever an aggregation is requested to supply
        /// appropriate aggregated data points.
        /// </param>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "TODO")]
        public SampleDataPointAggregator(
            Func<ConsecutiveDataPointObservationsCollection, IEnumerable<AggregatedDataPoint>> callback)
        {
            if (callback == null)
            {
                throw new ArgumentNullException("callback");
            }

            this.callback = callback;
        }

        /// <summary>
        /// Outputs debug information about observations and calls any registered event
        /// handlers.
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

            IEnumerable<AggregatedDataPoint> result = this.callback(continuousObservations);

            Debug.WriteLine("Success: NoOpAggregator.Aggregate");

            return result;
        }
    }
}
