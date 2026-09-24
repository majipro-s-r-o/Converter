using System;

namespace Majipro.Converter.Generator.Test.Conversions.EnumConversion;

[TestClass]
public class EnumConversionTest : ConversionTestBase<EnumConversionComposition>
{
    public EnumConversionTest()
        : base(@".\Conversions\EnumConversion\EnumConversionComposition.cs")
    {
    }

    [TestMethod]
    public void WhenGeneratorRunsThenThereAreNoDiagnostics()
    {
        AssertGeneratedWithoutDiagnostics();

        Assert.AreEqual(4, GeneratedSources.Count, "One converter is expected for each of the four conversions.");
    }

    [TestMethod]
    public void WhenTheMembersAreTheSameThenTheMemberOfTheSameNameIsReturned()
    {
        AssertGeneratedWithoutDiagnostics();

        var to = GetConvertingService()
            .Convert<EnumConversionTestCase.Status, EnumConversionTestCase.StatusEntity>(
                EnumConversionTestCase.Status.Active);

        Assert.AreEqual(EnumConversionTestCase.StatusEntity.Active, to);
    }

    /// <summary>
    /// The source members are a subset of the target's, which is enough, and the two sides number
    /// <c>Closed</c> differently, which the mapping is not allowed to care about.
    /// </summary>
    [TestMethod]
    public void WhenTheSourceHasFewerMembersThenTheMemberIsMappedByNameAndNotByValue()
    {
        AssertGeneratedWithoutDiagnostics();

        var to = GetConvertingService()
            .Convert<EnumConversionTestCase.Trimmed, EnumConversionTestCase.Full>(
                EnumConversionTestCase.Trimmed.Closed);

        Assert.AreEqual(EnumConversionTestCase.Full.Closed, to);
        Assert.AreNotEqual((int)EnumConversionTestCase.Trimmed.Closed, (int)to);
    }

    [TestMethod]
    public void WhenTheSourceIsANullableEnumThenTheMemberIsMapped()
    {
        AssertGeneratedWithoutDiagnostics();

        EnumConversionTestCase.Status? from = EnumConversionTestCase.Status.Closed;

        var to = GetConvertingService()
            .Convert<EnumConversionTestCase.Status?, EnumConversionTestCase.StatusEntity>(from);

        Assert.AreEqual(EnumConversionTestCase.StatusEntity.Closed, to);
    }

    [TestMethod]
    public void WhenTheNullableSourceIsNullThenTheResultIsTheDefault()
    {
        AssertGeneratedWithoutDiagnostics();

        EnumConversionTestCase.Status? from = null;

        var to = GetConvertingService()
            .Convert<EnumConversionTestCase.Status?, EnumConversionTestCase.StatusEntity>(from);

        Assert.AreEqual(default(EnumConversionTestCase.StatusEntity), to);
    }

    [TestMethod]
    public void WhenTheTargetIsANullableEnumThenTheMemberIsMapped()
    {
        AssertGeneratedWithoutDiagnostics();

        var to = GetConvertingService()
            .Convert<EnumConversionTestCase.Status, EnumConversionTestCase.StatusEntity?>(
                EnumConversionTestCase.Status.Active);

        Assert.AreEqual(EnumConversionTestCase.StatusEntity.Active, to);
    }

    /// <summary>
    /// Every declared member is mapped, so the only way into the default arm is a number cast into
    /// the enum. There is no member to return for it and returning anything would be made up.
    /// </summary>
    [TestMethod]
    public void WhenTheValueIsNotAMemberOfTheSourceEnumThenTheConverterThrows()
    {
        AssertGeneratedWithoutDiagnostics();

        var converter = GetConverter<EnumConversionTestCase.Status, EnumConversionTestCase.StatusEntity>();

        Assert.Throws<ArgumentOutOfRangeException>(() => converter.Convert((EnumConversionTestCase.Status)42));
    }
}
