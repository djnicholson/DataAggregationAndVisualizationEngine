// <copyright file="Observation.cs" company="David Nicholson">
//     Copyright (c) David Nicholson. All rights reserved.
// </copyright>
// <summary>See class header.</summary>

namespace DAaVE.Library.DataCollection
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Represents an observation the current values of zero or more types of data points
    /// (simultaneously).
    /// </summary>
    /// <typeparam name="TDataPointTypeEnum">Enumeration of all possible data point types.</typeparam>
    public sealed class Observation<TDataPointTypeEnum>
    {
        /// <summary>
        /// Initializes a new instance of the Observation class that represents a single
        /// value that was just observed.
        /// </summary>
        /// <param name="type">The type of data point observed.</param>
        /// <param name="value">The data points current value.</param>
        public Observation(TDataPointTypeEnum type, double value) : this()
        {
            this.Data[type] = value;
        }

        /// <summary>
        /// Initializes a new instance of the Observation class that represents a set of values
        /// of varying types that were simultaneously observed.
        /// </summary>
        /// <param name="valuesObserved">The values (and their types).</param>
        public Observation(IDictionary<TDataPointTypeEnum, double> valuesObserved) : this()
        {
            if (valuesObserved == null)
            {
                throw new ArgumentNullException("valuesObserved");
            }

            foreach (KeyValuePair<TDataPointTypeEnum, double> valueObserved in valuesObserved)
            {
                this.Data.Add(valueObserved);
            }
        }

        /// <summary>
        /// Prevents a default instance of the Observation class from being created.
        /// Initializes state for the current time and date with an initially empty 
        /// set of observed data values.
        /// </summary>
        private Observation()
        {
            this.DateTimeUtc = DateTime.UtcNow;
            this.Data = new Dictionary<TDataPointTypeEnum, double>();
        }

        /// <summary>
        /// Gets the date and time that this data was observed.
        /// </summary>
        internal DateTime DateTimeUtc { get; private set; }

        /// <summary>
        /// Gets the (possibly empty) set of data points observed. 
        /// </summary>
        internal IDictionary<TDataPointTypeEnum, double> Data { get; private set; }
    }
}