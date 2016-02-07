﻿// <copyright file="DataCollectionOrchestratorStatic.cs" company="David Nicholson">
//     Copyright (c) David Nicholson. All rights reserved.
// </copyright>

namespace DAaVE.Library.DataCollection
{
    using System;

    /// <summary>
    /// Constants used during the orchestration of various collectors.
    /// </summary>
    public static class DataCollectionOrchestrator
    {
        /// <summary>
        /// Collectors are only given a short amount of time to successfully return a result when being
        /// polled. This facilitates easy creation of naive pollers that make a synchronous blocking
        /// network request); more advanced pollers will typically return synchronously and do actual
        /// processing in their own background thread.
        /// </summary>
        public static readonly TimeSpan MaximumPollDuration = TimeSpan.FromSeconds(30.0);
    }
}
