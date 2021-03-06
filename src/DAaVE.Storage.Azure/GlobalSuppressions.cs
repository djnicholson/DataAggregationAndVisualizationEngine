// <copyright file="GlobalSuppressions.cs" company="David Nicholson">
//     Copyright (c) David Nicholson. All rights reserved.
// </copyright>
// <summary>
//     FXCop suppressions that make more sense to (or must) be tracked at the assembly level.
// </summary>

using System.Diagnostics.CodeAnalysis;

[assembly: SuppressMessage("Microsoft.Design", "CA1020:AvoidNamespacesWithFewTypes", Scope = "namespace", Target = "DAaVE.Storage.Azure", Justification = "Building gradually")]

[assembly: SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Aa", Scope = "namespace", Target = "DAaVE.Storage.Azure", Justification = "DAaVE is the (deliberately mixed-case) library name")]
[assembly: SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Aa", Justification = "(as above)")]

[assembly: SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "Aa", Scope = "namespace", Target = "DAaVE.Storage.Azure", Justification = "DAaVE is the (deliberately mixed-case) library name")]
[assembly: SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "Aa", Justification = "(as above)")]

[assembly: SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "DAaVE", Scope = "member", Target = "DAaVE.Storage.Azure.DataPointTypeAttributes`1.#.ctor()", Justification = "DAaVE is the (deliberately mixed-case) library name")]