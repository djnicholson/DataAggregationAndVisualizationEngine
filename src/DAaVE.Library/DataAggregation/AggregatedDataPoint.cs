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
    public struct AggregatedDataPoint
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

        /// <summary>
        /// Determines if the timestamps are identical.
        /// Assumes consistency of re-aggregation so does not compare <see cref="AggregatedValue"/>.
        /// </summary>
        /// <param name="leftHandSide">Left hand side operand.</param>
        /// <param name="rightHandSide">Right hand side operand.</param>
        /// <returns>True if the two parameters are to be considered exactly equal; false otherwise.</returns>
        public static bool operator ==(AggregatedDataPoint leftHandSide, AggregatedDataPoint rightHandSide)
        {
            return leftHandSide.UtcTimestamp == rightHandSide.UtcTimestamp;
        }

        /// <summary>
        /// The negation of <see cref="operator ==(AggregatedDataPoint, AggregatedDataPoint)"/>.
        /// </summary>
        /// <param name="leftHandSide">Left hand side operand.</param>
        /// <param name="rightHandSide">Right hand side operand.</param>
        /// <returns>True if the two parameters are not exactly identical; false otherwise.</returns>
        public static bool operator !=(AggregatedDataPoint leftHandSide, AggregatedDataPoint rightHandSide)
        {
            return !(leftHandSide == rightHandSide);
        }

        /// <summary>
        /// Determines equality with arbitrary objects.
        /// </summary>
        /// <param name="obj">Candidate for comparison.</param>
        /// <returns>False for non-DataPoint parameters; uses <see cref="operator ==(AggregatedDataPoint, AggregatedDataPoint)"/> otherwise.</returns>
        public override bool Equals(object obj)
        {
            if (obj is AggregatedDataPoint)
            {
                return ((AggregatedDataPoint)obj) == this;
            }

            return false;
        }

        /// <summary>
        /// Defers to <see cref="DateTime.GetHashCode"/> on <see cref="UtcTimestamp"/>.
        /// </summary>
        /// <returns>See <see cref="DateTime.GetHashCode"/>.</returns>
        public override int GetHashCode()
        {
            return this.UtcTimestamp.GetHashCode();
        }
    }
}
