using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DAaVE.Library.DataCollection
{
    /// <summary>
    /// Capable of providing recent raw values for one or more data point types.
    /// </summary>
    /// <typeparam name="TDataPointTypeEnum">An enumeration of all possible data point types</typeparam>
    public interface IDataCollector<TDataPointTypeEnum>
        where TDataPointTypeEnum : struct, IComparable, IFormattable
    {
        /// <summary>
        /// Implementations can return an arbitrary (possibly empty) set of recently collected data points.
        /// This method must return within <see cref="DataCollectionOrchestrator.MaximumPollDuration"/>;
        /// after this time any results will be ignored and the implementation should return as soon as possible 
        /// to free resources.
        /// </summary>
        /// <returns>
        /// A (possibly empty, or null) map of data points collected recently and (most likely) not yet reported.
        /// </returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        Task<IDictionary<TDataPointTypeEnum, DataPoint>> Poll();

        /// <summary>
        /// After this method is invoked by the poller, instances will not be subject to any further 
        /// invocations of their <see cref="Poll"/> method.
        /// </summary>
        void OnPollingComplete();
    }
}
