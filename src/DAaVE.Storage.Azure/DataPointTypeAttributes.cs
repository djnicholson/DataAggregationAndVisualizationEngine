// <copyright file="DataPointTypeAttributes.cs" company="David Nicholson">
//     Copyright (c) David Nicholson. All rights reserved.
// </copyright>
// <summary>See class header.</summary>

namespace DAaVE.Storage.Azure
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;

    using DAaVE.Library.DataCollection;
    
    /// <summary>
    /// Allows properties of the various data point types to be retrieved.
    /// </summary>
    /// <typeparam name="TDataPointTypeEnum">
    /// An enumeration of all possible data point types.
    /// </typeparam>
    internal class DataPointTypeAttributes<TDataPointTypeEnum>
        where TDataPointTypeEnum : struct, IComparable, IFormattable
    {
        /// <summary>
        /// The amount of observations that we typically aim to place in a single Azure Table Storage partition.
        /// This depends on accurate <see cref="ExpectedObservationRateAttribute"/> annotations on elements of
        /// <typeparamref name="TDataPointTypeEnum"/>.
        /// </summary>
        private const int DesiredObservationsPerFirehosePartition = 1024;

        /// <summary>
        /// Lookup table used by <see cref="GetAggregationInputWindowSizeInMinutes(TDataPointTypeEnum)"/>.
        /// </summary>
        private IDictionary<TDataPointTypeEnum, double> aggregationInputWindowSizeInMinutes;

        /// <summary>
        /// Initializes a new instance of the DataPointTypeAttributes class. Reflects over 
        /// <typeparamref name="TDataPointTypeEnum"/> to inspect their attributes (building lookup
        /// tables to facilitate later resolution of these attributes without performing reflection.
        /// </summary>
        public DataPointTypeAttributes()
        {
            Type typeInformation = typeof(TDataPointTypeEnum);

            if (!typeInformation.IsEnum)
            {
                throw new NotSupportedException("DAaVE.Storage.Azure requires that " + typeInformation + " be an enum.");
            }

            ICollection<Tuple<string, double>> aggregationInputWindowSizeInMinutesByName = new List<Tuple<string, double>>();

            foreach (string dataPointTypeName in typeInformation.GetEnumNames())
            {
                MemberInfo[] memberInfos = typeInformation.GetMember(dataPointTypeName);

                // typeInformation.IsEnum => memberInfos.Length <= 1
                // dataPointTypeName in typeInformation.GetEnumNames() => memberInfos.Length != 0
                MemberInfo dataPointTypeInfo = memberInfos[0];

                ExpectedObservationRateAttribute expectedObservationRate =
                    dataPointTypeInfo.GetCustomAttribute<ExpectedObservationRateAttribute>();
                
                if (expectedObservationRate == null)
                {
                    Type attributeType = typeof(ExpectedObservationRateAttribute);
                    throw new NotSupportedException(
                        "DAaVE.Storage.Azure requires that each item in " + typeInformation + " have an " + attributeType + " " +
                        "attribute; " + dataPointTypeInfo + " does not.");
                }

                double windowSizeInMinutes = expectedObservationRate.InMinutes * DesiredObservationsPerFirehosePartition;
                aggregationInputWindowSizeInMinutesByName.Add(
                    new Tuple<string, double>(dataPointTypeName, windowSizeInMinutes));
            }
            
            this.aggregationInputWindowSizeInMinutes = aggregationInputWindowSizeInMinutesByName.ToDictionary(
                keySelector: entry => (TDataPointTypeEnum)Enum.Parse(typeInformation, entry.Item1, ignoreCase: false),
                elementSelector: entry => entry.Item2);
        }

        /// <summary>
        /// Gets the minimum size of the input sent into an aggregator when requesting a complete
        /// aggregation (less points may be offered, but the aggregator wont be obligated to generate
        /// output).  A higher value provides flexibility to aggregate over larger windows of time 
        /// (providing an efficient way to present a data series over a very long time) but can lead
        /// to inefficient aggregation for data types that are observed frequently (as many raw 
        /// observations will be crammed into a single Azure Table Storage partition).
        /// </summary>
        /// <param name="dataPointType">The type of data point.</param>
        /// <returns>Minimum amount of raw data to provide as input to an aggregator.</returns>
        public double GetAggregationInputWindowSizeInMinutes(TDataPointTypeEnum dataPointType)
        {
            return this.aggregationInputWindowSizeInMinutes[dataPointType];
        }
    }
}
