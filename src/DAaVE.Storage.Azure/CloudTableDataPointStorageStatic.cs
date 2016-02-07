// <copyright file="CloudTableDataPointStorageStatic.cs" company="David Nicholson">
//     Copyright (c) David Nicholson. All rights reserved.
// </copyright>

namespace DAaVE.Storage.Azure
{
    using System;

    /// <summary>
    /// Constants used within CloudTableDataPointStorage.
    /// </summary>
    public static class CloudTableDataPointStorage
    {
        /// <summary>
        /// Data points returned by pollers that were collected too far in the past are discarded
        /// (it is assumed that this window of time has already been sealed so there is no way
        /// to get them into the permenent store any more).
        /// </summary>
        public static readonly TimeSpan MaximumFireHoseRecentDataPointAge = TimeSpan.FromMinutes(10.0);

        /// <summary>
        /// Data points wont be presented for aggregation until at least this much time has passed since 
        /// collection (must be significantly lower than <see cref="MaximumFireHoseRecentDataPointAge"/>).
        /// </summary>
        public static readonly TimeSpan ProcessingDelay = TimeSpan.FromMinutes(0.5);
    }
}
