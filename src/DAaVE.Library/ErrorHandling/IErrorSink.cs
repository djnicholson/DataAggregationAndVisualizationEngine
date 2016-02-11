// <copyright file="IErrorSink.cs" company="David Nicholson">
//     Copyright (c) David Nicholson. All rights reserved.
// </copyright>
// <summary>See class header.</summary>

namespace DAaVE.Library.ErrorHandling
{
    using System;

    /// <summary>
    /// A receiver for error messages.
    /// </summary>
    public interface IErrorSink
    {
        /// <summary>
        /// Invoked when an exception has occurred that indicates an error.
        /// </summary>
        /// <param name="message">A description of the condition.</param>
        /// <param name="exception">The exception that was thrown.</param>
        void OnError(string message, Exception exception);

        /// <summary>
        /// Invoked when an error condition is encountered.
        /// </summary>
        /// <param name="message">A description of the condition.</param>
        void OnError(string message);
    }
}
