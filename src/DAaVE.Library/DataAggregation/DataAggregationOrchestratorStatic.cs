// <copyright file="DataAggregationOrchestratorStatic.cs" company="David Nicholson">
//     Copyright (c) David Nicholson. All rights reserved.
// </copyright>

namespace DAaVE.Library.DataAggregation
{
    using System;

    /// <summary>
    /// Constants used during the orchestration of various aggregators.
    /// </summary>
    public static class DataAggregationOrchestrator
    {
        /// <summary>
        /// Pagers will be polled this often for the next page of data.
        /// </summary>
        public static readonly TimeSpan SleepDurationOnDataExhaustion = TimeSpan.FromMinutes(0.5);

        /// <summary>
        /// Time aggregation will sleep for upon an error.
        /// </summary>
        public static readonly TimeSpan SleepDurationOnError = TimeSpan.FromMinutes(0.5);
    }
}
