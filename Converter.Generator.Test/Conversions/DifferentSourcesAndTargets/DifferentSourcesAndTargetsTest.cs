using System;

namespace Majipro.Converter.Generator.Test.Conversions.DifferentSourcesAndTargets;

[TestClass]
public class DifferentSourcesAndTargetsTest : ConversionTestBase<DifferentSourcesAndTargetsComposition>
{
    public DifferentSourcesAndTargetsTest()
        : base(@".\Conversions\DifferentSourcesAndTargets\DifferentSourcesAndTargetsComposition.cs")
    {
    }

    [TestMethod]
    public void WhenGeneratorRunsThenThereAreNoDiagnostics()
    {
        AssertGeneratedWithoutDiagnostics();

        Assert.AreEqual(2, GeneratedSources.Count, "One converter is expected for each of the two conversions.");
    }

    [TestMethod]
    public void WhenSourceHasMorePropertiesThenTheOnesTheTargetKnowsAreConverted()
    {
        AssertGeneratedWithoutDiagnostics();

        var from = new DifferentSourcesAndTargetsTestCase.MoreProperties
        {
            Decimal = 1000.5m,
            UnsingnedLong = 111,
            Long = -100,
            UnsignedInteger = 11,
            Integer = -10,
            Byte = 1,
            Bool = true,
            String = "hello",
            Char = 'A'
        };

        var to = GetConvertingService()
            .Convert<DifferentSourcesAndTargetsTestCase.MoreProperties, DifferentSourcesAndTargetsTestCase.FewerProperties>(from);

        Assert.IsNotNull(to);

        Assert.AreEqual(from.Decimal, to.Decimal);
        Assert.AreEqual(from.Long, to.Long);
        Assert.AreEqual(from.Integer, to.Integer);
        Assert.AreEqual(from.Byte, to.Byte);
        Assert.AreEqual(from.Bool, to.Bool);
        Assert.AreEqual(from.String, to.String);
    }

    [TestMethod]
    public void WhenBothSidesHaveOwnPropertiesThenOnlyTheSharedOnesAreConverted()
    {
        AssertGeneratedWithoutDiagnostics();

        var from = new DifferentSourcesAndTargetsTestCase.WithCompany
        {
            Id = Guid.NewGuid(),
            FirstName = "John",
            LastName = "Smith",
            Company = "My Company"
        };

        var to = GetConvertingService()
            .Convert<DifferentSourcesAndTargetsTestCase.WithCompany, DifferentSourcesAndTargetsTestCase.WithoutCompany>(from);

        Assert.IsNotNull(to);

        Assert.AreEqual(from.Id, to.Id);
        Assert.AreEqual(from.FirstName, to.FirstName);
        Assert.AreEqual(from.LastName, to.LastName);
    }
}
