// <copyright file="DataCollectorAttribute.cs" company="David Nicholson">
//     Copyright (c) David Nicholson. All rights reserved.
// </copyright>
// <summary>See class header.</summary>

namespace DAaVE.Library.DataCollection
{
    using System;

    /// <summary>
    /// Denotes that a class (implementing <see cref="DataCollector{DataPointType}"/>) should be created and
    /// polled by instances of <see cref="DataCollectionOrchestrator{DataPointType}"/>.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
    public sealed class DataCollectorAttribute : Attribute
    {
    }
}
