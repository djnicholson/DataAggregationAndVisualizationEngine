using System;
using System.Collections.Generic;

namespace DAaVE.Library.Storage
{
    /// <summary>
    /// Exposes a continuous stream of raw collected data points as a sequence of pages.
    /// </summary>
    /// <typeparam name="TDataPointTypeEnum">An enumeration of all possible types of data point.</typeparam>
    public interface IDataPointPager<TDataPointTypeEnum>
        where TDataPointTypeEnum : struct, IComparable, IFormattable
    {
        /// <summary>
        /// Get the next page of raw values collected for a specific type of data point.
        /// </summary>
        /// <param name="type">The type of data point to return.</param>
        /// <param name="continuationToken">
        /// An object that can be used to maintain state between a series of invocations of NextPage.
        /// Providing a non-null token is interpreted as a signal that the page corresponding to that
        /// token has now been successfully, irrevocably aggregated.
        /// </param>
        /// <returns>A set of raw data points suitable for aggregation</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1045:DoNotPassTypesByReference", MessageId = "1#")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1007:UseGenericsWhereAppropriate")]
        IEnumerable<DataPoint> NextPage(TDataPointTypeEnum type, ref object continuationToken);
    }
}
