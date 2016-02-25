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
        /// Backing storage for the <see cref="AggregatorImplementation"/> property.
        /// </summary>
        private Type aggregatorImplementation;

        /// <summary>
        /// Initializes a new instance of the AggregateWithAttribute class (by specifying the
        /// type of aggregator).
        /// </summary>
        /// <param name="aggregatorImplementation">
        /// A type that is a concrete implementation of <see cref="IDataPointAggregator"/>.
        /// </param>
        public AggregateWithAttribute(Type aggregatorImplementation)
        {
            this.AggregatorImplementation = aggregatorImplementation;
        }

        /// <summary>
        /// Gets the type of a concrete implementation of <see cref="IDataPointAggregator"/>
        /// to use when aggregating data of the annotated type.
        /// </summary>
        /// <value>The type of the aggregator implementation.</value>
        public Type AggregatorImplementation
        {
            get
            {
                return this.aggregatorImplementation;
            }

            private set
            {
                if (!value.GetInterfaces().Contains(typeof(IDataPointAggregator)))
                {
                    throw new ArgumentException(
                        "AggregatorImplementationmust inherit IDataPointAggregator, " + value + " does not", 
                        "value");
                }

                this.aggregatorImplementation = value;
            }
        }
    }
}
