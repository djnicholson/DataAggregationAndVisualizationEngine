// <copyright file="ConsoleErrorSink.cs" company="David Nicholson">
//     Copyright (c) David Nicholson. All rights reserved.
// </copyright>

namespace DAaVE.Library.ErrorHandling.ErrorSinks
{
    using System;

    /// <summary>
    /// Writes all errors to the console with a common prefix string.
    /// </summary>
    public sealed class ConsoleErrorSink : IErrorSink
    {
        /// <summary>
        /// All messages will be prefixed with this string.
        /// </summary>
        private readonly string prefix;

        /// <summary>
        /// Initializes a new instance of the <see cref="ConsoleErrorSink"/> class.
        /// </summary>
        /// <param name="prefix">The prefix to write to the console before any errors.</param>
        public ConsoleErrorSink(string prefix)
        {
            this.prefix = prefix;
        }

        /// <inheritdoc/>
        public void OnError(string message)
        {
            Console.WriteLine(this.prefix + message);
        }

        /// <inheritdoc/>
        public void OnError(string message, Exception exception)
        {
            Console.WriteLine(this.prefix + message + " \n[" + exception + "]");
        }
    }
}
