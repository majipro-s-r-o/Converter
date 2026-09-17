namespace Majipro.Converter.Generator.Test.Conversions.Enums;

[TestClass]
public class EnumsTest : ConversionTestBase<EnumsComposition>
{
    public EnumsTest()
        : base(@".\Conversions\Enums\EnumsComposition.cs")
    {
    }

    [TestMethod]
    public void WhenGeneratorRunsThenThereAreNoDiagnostics()
    {
        AssertGeneratedWithoutDiagnostics();

        Assert.AreEqual(3, GeneratedSources.Count, "One converter is expected for each of the three conversions.");
    }

    [TestMethod]
    public void WhenTheTargetIsAStringThenTheEnumMemberNameIsWrittenOut()
    {
        AssertGeneratedWithoutDiagnostics();

        var to = GetConvertingService().Convert<EnumsTestCase.Status, string>(EnumsTestCase.Status.Active);

        Assert.AreEqual(nameof(EnumsTestCase.Status.Active), to);
    }

    [TestMethod]
    public void WhenTheTargetIsAStringThenANullableEnumIsWrittenOut()
    {
        AssertGeneratedWithoutDiagnostics();

        EnumsTestCase.Status? from = EnumsTestCase.Status.Closed;

        var to = GetConvertingService().Convert<EnumsTestCase.Status?, string>(from);

        Assert.AreEqual(nameof(EnumsTestCase.Status.Closed), to);
    }

    [TestMethod]
    public void WhenTheTargetIsAStringAndTheNullableEnumIsNullThenTheResultIsNull()
    {
        AssertGeneratedWithoutDiagnostics();

        EnumsTestCase.Status? from = null;

        var to = GetConvertingService().Convert<EnumsTestCase.Status?, string>(from);

        Assert.IsNull(to);
    }

    [TestMethod]
    public void WhenThePropertyIsTheSameEnumThenItIsCopied()
    {
        AssertGeneratedWithoutDiagnostics();

        var from = new EnumsTestCase.From
        {
            Status = EnumsTestCase.Status.Active
        };

        var to = GetConvertingService().Convert<EnumsTestCase.From, EnumsTestCase.To>(from);

        Assert.IsNotNull(to);
        Assert.AreEqual(EnumsTestCase.Status.Active, to.Status);
    }

    [TestMethod]
    public void WhenThePropertyIsTheNullableCounterpartThenItIsCopied()
    {
        AssertGeneratedWithoutDiagnostics();

        var from = new EnumsTestCase.From
        {
            NullableStatus = EnumsTestCase.Status.Closed
        };

        var to = GetConvertingService().Convert<EnumsTestCase.From, EnumsTestCase.To>(from);

        Assert.IsNotNull(to);
        Assert.AreEqual(EnumsTestCase.Status.Closed, to.NullableStatus);
    }

    [TestMethod]
    public void WhenTheTargetIsAStringThenThereIsNoReferenceConverter()
    {
        AssertGeneratedWithoutDiagnostics();

        var converter = FindReferenceConverter<EnumsTestCase.Status, string>();

        Assert.IsNull(converter, "A string is a value, not an instance somebody holds and can have filled.");
    }
}
