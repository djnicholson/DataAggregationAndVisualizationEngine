﻿// <copyright file="DataPoint.cs" company="David Nicholson">
//     Copyright (c) David Nicholson. All rights reserved.
// </copyright>
// <summary>See class header.</summary>

namespace DAaVE.Library
{
    using System;

    /// <summary>
    /// The raw value collected for a specific data point, at a specific time.
    /// </summary>
    public struct DataPoint
    {
        /// <summary>
        /// Initializes a new instance of the DataPoint struct for a specific value observed
        /// at a specific time.
        /// </summary>
        /// <param name="utcTimestamp">Time stamp of the observation (must be <see cref="DateTimeKind.Utc"/>).</param>
        /// <param name="value">The data value observed.</param>
        internal DataPoint(DateTime utcTimestamp, double value)
        {
            this.UtcTimestamp = utcTimestamp;
            this.Value = value;
        }

        /// <summary>
        /// Gets or sets the time of the observation..
        /// </summary>
        /// <value>The time of the observation..</value>
        public DateTime UtcTimestamp { get; set; }

        /// <summary>
        /// Gets or sets the raw value observed at this time.
        /// </summary>
        /// <value>The raw value observed at this time.</value>
        public double Value { get; set; }

        /// <summary>
        /// Determines if the timestamp and <emphasis>floating point</emphasis> value are identical.
        /// </summary>
        /// <param name="leftHandSide">Left hand side operand.</param>
        /// <param name="rightHandSide">Right hand side operand.</param>
        /// <returns>True if the two parameters are to be considered exactly equal; false otherwise.</returns>
        public static bool operator ==(DataPoint leftHandSide, DataPoint rightHandSide)
        {
            return (leftHandSide.UtcTimestamp == rightHandSide.UtcTimestamp) &&
                (leftHandSide.Value == rightHandSide.Value);
        }

        /// <summary>
        /// The negation of <see cref="operator ==(DataPoint, DataPoint)"/>.
        /// </summary>
        /// <param name="leftHandSide">Left hand side operand.</param>
        /// <param name="rightHandSide">Right hand side operand.</param>
        /// <returns>True if the two parameters are not exactly identical; false otherwise.</returns>
        public static bool operator !=(DataPoint leftHandSide, DataPoint rightHandSide)
        {
            return !(leftHandSide == rightHandSide);
        }

        /// <summary>
        /// Determines equality with arbitrary objects.
        /// </summary>
        /// <param name="obj">Candidate for comparison.</param>
        /// <returns>False for non-DataPoint parameters; uses <see cref="operator ==(DataPoint, DataPoint)"/> otherwise.</returns>
        public override bool Equals(object obj)
        {
            if (obj is DataPoint)
            {
                return ((DataPoint)obj) == this;
            }

            return false;
        }

        /// <summary>
        /// A naive combination of the timestamp and value hash-codes.
        /// </summary>
        /// <returns>A (possibly not well distributed) 32-bit hash-code.</returns>
        public override int GetHashCode()
        {
            unchecked
            {
                return this.Value.GetHashCode() + this.UtcTimestamp.GetHashCode();
            }
        }
    }
}
