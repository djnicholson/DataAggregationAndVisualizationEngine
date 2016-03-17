// <copyright file="CallbackErrorSink.cs" company="David Nicholson">
//     Copyright (c) David Nicholson. All rights reserved.
// </copyright>
// <summary>See class header.</summary>

namespace DAaVE.Library.ErrorHandling.ErrorSinks
{
    using System;

    /// <summary>
    /// Invokes a callback whenever there is an error.
    /// </summary>
    public sealed class CallbackErrorSink : IErrorSink
    {
        /// <summary>
        /// Client-supplied code that is invoked whenever there is an error.
        /// </summary>
        private readonly Action<string, bool, Exception> onError;

        /// <summary>
        /// Initializes a new instance of the CallbackErrorSink class.
        /// </summary>
        /// <param name="onError">
        /// Code that is invoked whenever there is an error. 
        /// The first parameter is the error message.
        /// The second parameter indicates whether an <see cref="Exception"/> is available.
        /// The third parameter is the <see cref="Exception"/> thrown (if available).
        /// </param>
        public CallbackErrorSink(Action<string, bool, Exception> onError)
        {
            if (onError == null)
            {
                throw new ArgumentNullException("onError");
            }

            this.onError = onError;
        }

        /// <inheritdoc/>
        public void OnError(string message)
        {
            this.OnError(message, exception: null);
        }

        /// <inheritdoc/>
        public void OnError(string message, Exception exception)
        {
            this.onError(message, exception != null, exception);
        }
    }
}
