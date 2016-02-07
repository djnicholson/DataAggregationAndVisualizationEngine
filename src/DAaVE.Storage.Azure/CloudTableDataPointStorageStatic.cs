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
        /// Data points that were collected too far in the past are discarded (it is assumed that the aggregation for
        /// the window of time that would have represented this point has already been sealed.
        /// </summary>
        public static readonly TimeSpan MaximumFireHoseRecentDataPointAge = TimeSpan.FromMinutes(10.0);

        /// <summary>
        /// Data points wont be presented for aggregation until at least this much time has passed since 
        /// collection (must be significantly lower than <see cref="MaximumFireHoseRecentDataPointAge"/>).
        /// </summary>
        public static readonly TimeSpan ProcessingDelay = TimeSpan.FromMinutes(0.25);
    }
}
