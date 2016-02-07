// <copyright file="GlobalSuppressions.cs" company="David Nicholson">
//     Copyright (c) David Nicholson. All rights reserved.
// </copyright>

using System.Diagnostics.CodeAnalysis;

[assembly: SuppressMessage("Microsoft.Design", "CA1020:AvoidNamespacesWithFewTypes", Scope = "namespace", Target = "DAaVE.Library.Storage", Justification = "Building gradually")]
[assembly: SuppressMessage("Microsoft.Design", "CA1020:AvoidNamespacesWithFewTypes", Scope = "namespace", Target = "DAaVE.Library", Justification = "Building gradually")]
[assembly: SuppressMessage("Microsoft.Design", "CA1020:AvoidNamespacesWithFewTypes", Scope = "namespace", Target = "DAaVE.Library.DataAggregation", Justification = "Building gradually")]
[assembly: SuppressMessage("Microsoft.Design", "CA1020:AvoidNamespacesWithFewTypes", Scope = "namespace", Target = "DAaVE.Library.DataAggregation.Aggregators", Justification = "Building gradually")]
[assembly: SuppressMessage("Microsoft.Design", "CA1020:AvoidNamespacesWithFewTypes", Scope = "namespace", Target = "DAaVE.Library.DataCollection", Justification = "Building gradually")]
[assembly: SuppressMessage("Microsoft.Design", "CA1020:AvoidNamespacesWithFewTypes", Scope = "namespace", Target = "DAaVE.Library.ErrorHandling", Justification = "Building gradually")]
[assembly: SuppressMessage("Microsoft.Design", "CA1020:AvoidNamespacesWithFewTypes", Scope = "namespace", Target = "DAaVE.Library.ErrorHandling.ErrorSinks", Justification = "Building gradually")]

[assembly: SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Aa", Scope = "namespace", Target = "DAaVE.Library", Justification = "DAaVE is the (deliberetely mixed-case) library name")]
[assembly: SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Aa", Scope = "namespace", Target = "DAaVE.Library.DataAggregation", Justification = "(as above)")]
[assembly: SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Aa", Scope = "namespace", Target = "DAaVE.Library.DataAggregation.Aggregators", Justification = "(as above)")]
[assembly: SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Aa", Scope = "namespace", Target = "DAaVE.Library.DataCollection", Justification = "(as above)")]
[assembly: SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Aa", Scope = "namespace", Target = "DAaVE.Library.ErrorHandling", Justification = "(as above)")]
[assembly: SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Aa", Scope = "namespace", Target = "DAaVE.Library.ErrorHandling.ErrorSinks", Justification = "(as above)")]
[assembly: SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Aa", Scope = "namespace", Target = "DAaVE.Library.Storage", Justification = "(as above)")]
[assembly: SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Aa", Justification = "(as above)")]

[assembly: SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "Aa", Justification = "DAaVE is the (deliberetely mixed-case) library name")]
[assembly: SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "Aa", Scope = "namespace", Target = "DAaVE.Library", Justification = "(as above)")]
[assembly: SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "Aa", Scope = "namespace", Target = "DAaVE.Library.DataAggregation", Justification = "(as above)")]
[assembly: SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "Aa", Scope = "namespace", Target = "DAaVE.Library.DataAggregation.Aggregators", Justification = "(as above)")]
[assembly: SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "Aa", Scope = "namespace", Target = "DAaVE.Library.DataCollection", Justification = "(as above)")]
[assembly: SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "Aa", Scope = "namespace", Target = "DAaVE.Library.ErrorHandling", Justification = "(as above)")]
[assembly: SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "Aa", Scope = "namespace", Target = "DAaVE.Library.ErrorHandling.ErrorSinks", Justification = "(as above)")]
[assembly: SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "Aa", Scope = "namespace", Target = "DAaVE.Library.Storage", Justification = "(as above)")]

// Tracking localization debt:
[assembly: SuppressMessage("Microsoft.Globalization", "CA1303:Do not pass literals as localized parameters", MessageId = "DAaVE.Library.ErrorHandling.IErrorSink.OnError(System.String,System.Exception)", Scope = "member", Target = "DAaVE.Library.DataAggregation.DataAggregationThread`1.#.ctor(!0,DAaVE.Library.DataAggregation.IDataPointAggregator,DAaVE.Library.Storage.IDataPointPager`1<!0>,DAaVE.Library.Storage.IDataPointFireHose`1<!0>,DAaVE.Library.ErrorHandling.IErrorSink)", Justification = "Not localized")]
[assembly: SuppressMessage("Microsoft.Globalization", "CA1303:Do not pass literals as localized parameters", MessageId = "DAaVE.Library.ErrorHandling.IErrorSink.OnError(System.String,System.Exception)", Scope = "member", Target = "DAaVE.Library.DataCollection.DataCollectorPollerThread`1.#PollLoop(System.Object)", Justification = "(as above)")]
[assembly: SuppressMessage("Microsoft.Globalization", "CA1303:Do not pass literals as localized parameters", MessageId = "DAaVE.Library.ErrorHandling.IErrorSink.OnError(System.String)", Scope = "member", Target = "DAaVE.Library.DataCollection.DataCollectorPollerThread`1.#IndividualPoll()", Justification = "(as above)")]
[assembly: SuppressMessage("Microsoft.Globalization", "CA1303:Do not pass literals as localized parameters", MessageId = "DAaVE.Library.ErrorHandling.IErrorSink.OnError(System.String,System.Exception)", Scope = "member", Target = "DAaVE.Library.DataCollection.DataCollectorPollerThread`1+<TryInvokePoll>d__12.#MoveNext()", Justification = "(as above)")]
[assembly: SuppressMessage("Microsoft.Globalization", "CA1303:Do not pass literals as localized parameters", MessageId = "System.Console.WriteLine(System.String)", Scope = "member", Target = "DAaVE.Library.ErrorHandling.ErrorSinks.ConsoleErrorSink.#OnError(System.String,System.Exception)", Justification = "(as above)")]
[assembly: SuppressMessage("Microsoft.Globalization", "CA1303:Do not pass literals as localized parameters", MessageId = "DAaVE.Library.ErrorHandling.IErrorSink.OnError(System.String,System.Exception)", Scope = "member", Target = "DAaVE.Library.DataAggregation.DataAggregationThread`1.#.ctor(!0,DAaVE.Library.DataAggregation.IDataPointAggregator,DAaVE.Library.Storage.IDataPointPager`1<!0>,DAaVE.Library.Storage.IDataPointFireHose`2<!0,DAaVE.Library.Storage.IDataPointPager`1<!0>>,DAaVE.Library.ErrorHandling.IErrorSink)", Justification = "(as above)")]
[assembly: SuppressMessage("Microsoft.Globalization", "CA1303:Do not pass literals as localized parameters", MessageId = "DAaVE.Library.ErrorHandling.IErrorSink.OnError(System.String,System.Exception)", Scope = "member", Target = "DAaVE.Library.DataAggregation.DataAggregationThread`1.#.ctor(!0,DAaVE.Library.DataAggregation.IDataPointAggregator,DAaVE.Library.Storage.IDataPointPager`1<!0>,DAaVE.Library.ErrorHandling.IErrorSink)", Justification = "(as above)")]
[assembly: SuppressMessage("Microsoft.Globalization", "CA1303:Do not pass literals as localized parameters", MessageId = "DAaVE.Library.ErrorHandling.IErrorSink.OnError(System.String,System.Exception)", Scope = "member", Target = "DAaVE.Library.DataCollection.DataCollectorPollerThread`1.#InvokePoll()", Justification = "(as above)")]