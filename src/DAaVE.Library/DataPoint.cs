using System;

namespace DAaVE.Library
{
    /// <summary>
    /// The raw value collected for a specific data point, at a specific time.
    /// </summary>
    public class DataPoint
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
