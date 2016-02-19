// <copyright file="ExpectedObservationRateAttribute.cs" company="David Nicholson">
//     Copyright (c) David Nicholson. All rights reserved.
// </copyright>
// <summary>See class header.</summary>

namespace DAaVE.Library.DataCollection
{
    using System;

    /// <summary>
    /// Can be applied to individual data point types to indicate the approximate frequency 
    /// at which they are expected to be observed.
    /// </summary>
    [AttributeUsage(AttributeTargets.All, AllowMultiple = false)]
    public sealed class ExpectedObservationRateAttribute : Attribute
    {
        /// <summary>
        /// Initializes a new instance of the ExpectedObservationRateAttribute class that is
        /// an undefined state until after the setter for one of <see cref="InSeconds"/>, 
        /// <see cref="InMinutes"/> or <see cref="InHours"/> has been successfully invoked.
        /// </summary>
        public ExpectedObservationRateAttribute()
        {
        }

        /// <summary>
        /// Gets or sets the value of <see cref="Interval"/> (a <see cref="TimeSpan"/>) via
        /// a conversion to the amount of seconds in that time-span (represented as a 
        /// <see cref="double"/>.
        /// </summary>
        /// <value>Total amount of seconds.</value>
        public double InSeconds
        {
            get
            {
                return this.Interval.TotalSeconds;
            }

            set
            {
                this.Interval = TimeSpan.FromSeconds(value);
            }
        }

        /// <summary>
        /// Gets or sets the value of <see cref="Interval"/> (a <see cref="TimeSpan"/>) via
        /// a conversion to the amount of minutes in that time-span (represented as a 
        /// <see cref="double"/>.
        /// </summary>
        /// <value>Total amount of minutes.</value>
        public double InMinutes
        {
            get
            {
                return this.Interval.TotalMinutes;
            }

            set
            {
                this.Interval = TimeSpan.FromMinutes(value);
            }
        }

        /// <summary>
        /// Gets or sets the value of <see cref="Interval"/> (a <see cref="TimeSpan"/>) via
        /// a conversion to the amount of hours in that time-span (represented as a 
        /// <see cref="double"/>.
        /// </summary>
        /// <value>Total amount of hours.</value>
        public double InHours
        {
            get
            {
                return this.Interval.TotalHours;
            }

            set
            {
                this.Interval = TimeSpan.FromHours(value);
            }
        }

        /// <summary>
        /// Gets the expected time-span between observations.
        /// </summary>
        /// <value>The expected time-span between observations.</value>
        public TimeSpan Interval
        {
            get;
            private set;
        }
    }
}
