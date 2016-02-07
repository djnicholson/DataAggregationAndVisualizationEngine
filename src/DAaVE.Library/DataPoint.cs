// <copyright file="DataPoint.cs" company="David Nicholson">
//     Copyright (c) David Nicholson. All rights reserved.
// </copyright>

namespace DAaVE.Library
{
    using System;

    /// <summary>
    /// The raw value collected for a specific data point, at a specific time.
    /// </summary>
    public sealed class DataPoint
    {
        /// <summary>
        /// Gets or sets the time of collection.
        /// </summary>
        public DateTime UtcTimestamp { get; set; }

        /// <summary>
        /// Gets or sets the value.
        /// </summary>
        public double Value { get; set; }
    }
}
