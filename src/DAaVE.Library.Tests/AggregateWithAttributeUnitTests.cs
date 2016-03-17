// <copyright file="AggregateWithAttributeUnitTests.cs" company="David Nicholson">
//     Copyright (c) David Nicholson. All rights reserved.
// </copyright>
// <summary>See class header.</summary>

namespace DAaVE.Library.Tests
{
    using System;
    using System.Diagnostics.CodeAnalysis;

    using DAaVE.Library.DataAggregation;
    using DAaVE.Samples;

    using Microsoft.VisualStudio.TestTools.UnitTesting;
    
    /// <summary>
    /// Unit tests for the <see cref="AggregateWithAttribute"/> class.
    /// </summary>
    [TestClass]
    public class AggregateWithAttributeUnitTests
    {
        /// <summary>
        /// Tests that the <see cref="AggregateWithAttribute.AggregatorType"/> property can be set through the
        /// constructor.
        /// </summary>
        [TestMethod]
        public void AggregatorTypeIsSetByConstructor()
        {
            AggregateWithAttribute target = new AggregateWithAttribute(typeof(SampleDataPointAggregator));
            Assert.AreEqual<Type>(typeof(SampleDataPointAggregator), target.AggregatorType);
        }

        /// <summary>
        /// Tests that the <see cref="AggregateWithAttribute(Type)"/> constructor rejects
        /// invalid (non- <see cref="IDataPointAggregator"/> implementing) types.
        /// </summary>
        [SuppressMessage(
            "Microsoft.Usage",
            "CA1806:DoNotIgnoreMethodResults", 
            MessageId = "DAaVE.Library.DataAggregation.AggregateWithAttribute",
            Justification = "Test code expects an exception.")]
        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void ConstructorRejectsInvalidTypes()
        {
            new AggregateWithAttribute(typeof(object));
        }
    }
}
