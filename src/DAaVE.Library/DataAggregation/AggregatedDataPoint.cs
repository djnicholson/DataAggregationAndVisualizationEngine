using System;

namespace DAaVE.Library.DataAggregation
{
    /// <summary>
    /// An illustrative value of a specific data point at a certain time created by
    /// aggregating one or more actual data points.
    /// </summary>
    public class AggregatedDataPoint
    {
        /// <summary>
        /// The time being represented (in UTC).
        /// </summary>
        public DateTime UtcTimestamp { get; set; }

        /// <summary>
        /// Expected data point value at this time (based on aggregating nearby actual
        /// data point collections).
        /// </summary>
        public double AggregatedValue { get; set; }
    }
}
