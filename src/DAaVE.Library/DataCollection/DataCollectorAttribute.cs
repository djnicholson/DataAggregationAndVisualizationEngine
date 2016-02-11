// <copyright file="DataCollectorAttribute.cs" company="David Nicholson">
//     Copyright (c) David Nicholson. All rights reserved.
// </copyright>
// <summary>See class header.</summary>

namespace DAaVE.Library.DataCollection
{
    using System;

    /// <summary>
    /// Denotes that a class (implementing <see cref="IDataCollector{DataPointType}"/>) should be created and
    /// polled by instances of <see cref="DataCollectionOrchestrator{DataPointType}"/>.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
    public sealed class DataCollectorAttribute : Attribute
    {
        /// <summary>
        /// The frequency at which collectors are polled unless they explicitly specify otherwise.
        /// </summary>
        public static readonly TimeSpan DefaultPollInterval = TimeSpan.FromSeconds(0.5);

        /// <summary>
        /// Initializes a new instance of the <see cref="DataCollectorAttribute"/> class.
        /// </summary>
        public DataCollectorAttribute()
        {
            this.PollInterval = DefaultPollInterval;
        }

        /// <summary>
        /// Gets or sets the interval at which the collector should be polled.
        /// </summary>
        /// <value>The interval at which the collector should be polled.</value>
        public TimeSpan PollInterval { get; set; }

        /// <summary>
        /// Gets or sets the interval at which the collector should be polled (in seconds).
        /// </summary>
        /// <value>The interval at which the collector should be polled.</value>
        public double PollIntervalInSeconds
        {
            get
            {
                return this.PollInterval.TotalSeconds;
            }

            set
            {
                this.PollInterval = TimeSpan.FromSeconds(value);
            }
        }
    }
}
