// <copyright file="SampleDataPointType.cs" company="David Nicholson">
//     Copyright (c) David Nicholson. All rights reserved.
// </copyright>
// <summary>See class header.</summary>

namespace DAaVE.Samples
{
    using DAaVE.Library.DataAggregation;
    using DAaVE.Library.DataAggregation.Aggregators;
    using DAaVE.Library.DataCollection;
    
    /// <summary>
    /// Some example data types that illustrate potentially differing observation
    /// rates.
    /// </summary>
    public enum SampleDataPointType
    {
        /// <summary>
        /// How many minutes of day light were there during this calendar day?
        /// Observed once per day.
        /// </summary>
        [ExpectedObservationRate(InHours = 24.0)]
        [AggregateWith(typeof(AverageBySecondDataPointAggregator))]
        MinutesOfDaylight = 0,

        /// <summary>
        /// Amount of web requests seen in some fixed interval.
        /// </summary>
        [ExpectedObservationRate(InSeconds = 5.0)]
        [AggregateWith(typeof(AverageBySecondDataPointAggregator))]
        [AggregateWith(typeof(AverageBySecondDataPointAggregator))]
        HttpRequests = 1,

        /// <summary>
        /// What is the current 'ask' for BTC? Observed very frequently.
        /// </summary>
        ////[AggregateWith(typeof(Baz))]
        [ExpectedObservationRate(InMinutes = 0.001)]
        PriceOfBitcoin = 2,
    }
}
