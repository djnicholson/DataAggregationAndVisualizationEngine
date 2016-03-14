// <copyright file="AggregateWithAttribute.cs" company="David Nicholson">
//     Copyright (c) David Nicholson. All rights reserved.
// </copyright>
// <summary>See class header.</summary>

namespace DAaVE.Library.DataAggregation
{
    using System;
    using System.Linq;

    /// <summary>
    /// Can be applied to individual data point types to indicate the approximate frequency 
    /// at which they are expected to be observed.
    /// </summary>
    [AttributeUsage(AttributeTargets.All, AllowMultiple = true)]
    public sealed class AggregateWithAttribute : Attribute
    {
        /// <summary>
        /// Backing storage for the <see cref="AggregatorType"/> property.
        /// </summary>
        private Type aggregatorType;

        /// <summary>
        /// Initializes a new instance of the AggregateWithAttribute class (by specifying the
        /// type of aggregator).
        /// </summary>
        /// <param name="aggregatorType">
        /// A type that is a concrete implementation of <see cref="IDataPointAggregator"/>.
        /// </param>
        public AggregateWithAttribute(Type aggregatorType)
        {
            this.AggregatorType = aggregatorType;
        }

        /// <summary>
        /// Gets the type of a concrete implementation of <see cref="IDataPointAggregator"/>
        /// to use when aggregating data of the annotated type.
        /// </summary>
        /// <value>The type of the aggregator implementation.</value>
        public Type AggregatorType
        {
            get
            {
                return this.aggregatorType;
            }

            private set
            {
                if (!value.GetInterfaces().Contains(typeof(IDataPointAggregator)))
                {
                    throw new ArgumentException(
                        "AggregatorType must inherit IDataPointAggregator, " + value + " does not", 
                        "value");
                }

                this.aggregatorType = value;
            }
        }
    }
}
