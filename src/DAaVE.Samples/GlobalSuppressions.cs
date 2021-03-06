// <copyright file="GlobalSuppressions.cs" company="David Nicholson">
//     Copyright (c) David Nicholson. All rights reserved.
// </copyright>
// <summary>
//     FXCop suppressions that make more sense to (or must) be tracked at the assembly level.
// </summary>

using System.Diagnostics.CodeAnalysis;

[assembly: SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Bitcoin", Scope = "member", Target = "DAaVE.Samples.SampleDataPointType.#PriceOfBitcoin", Justification = "Product name")]

[assembly: SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Aa", Scope = "namespace", Target = "DAaVE.Samples", Justification = "DAaVE is the (deliberately mixed-case) library name")]
[assembly: SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Aa", Justification = "(as above)")]

[assembly: SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "Aa", Scope = "namespace", Target = "DAaVE.Samples", Justification = "DAaVE is the (deliberately mixed-case) library name")]
[assembly: SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "Aa", Justification = "(as above)")]
