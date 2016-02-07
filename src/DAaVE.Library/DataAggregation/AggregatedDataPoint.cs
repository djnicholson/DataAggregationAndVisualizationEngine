// <copyright file="AggregatedDataPoint.cs" company="David Nicholson">
//     Copyright (c) David Nicholson. All rights reserved.
// </copyright>

namespace DAaVE.Library.DataAggregation
{
    using System;

    /// <summary>
    /// An illustrative value of a specific data point at a certain time created by
    /// aggregating one or more actual data points.
    /// </summary>
    public sealed class AggregatedDataPoint
    {
        /// <summary>
        /// Gets or sets the time being represented (in UTC).
        /// </summary>
        public DateTime UtcTimestamp { get; set; }

        /// <summary>
        /// Gets or sets expected data point value at this time (based on aggregating nearby actual
        /// data point collections).
        /// </summary>
        public double AggregatedValue { get; set; }
    }
}
