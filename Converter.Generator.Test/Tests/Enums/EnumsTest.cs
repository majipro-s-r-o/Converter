namespace Majipro.Converter.Generator.Test.Tests.Enums;

[TestClass]
public class EnumsTest : ConversionTestBase<EnumsComposition>
{
    public EnumsTest()
        : base(@".\Tests\Enums\EnumsComposition.cs")
    {
    }

    [TestMethod]
    public void WhenGeneratorRunsThenThereAreNoDiagnostics()
    {
        AssertGeneratedWithoutDiagnostics();

        Assert.AreEqual(4, GeneratedSources.Count, "One converter is expected for each of the four conversions.");
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

    /// <summary>
    /// The boundary that makes the enum converters in the consuming projects hand written: the
    /// property is dropped silently, so the generator can not be pointed at a pair like this.
    /// </summary>
    [TestMethod]
    public void WhenTheTwoSidesAreDifferentEnumTypesThenThePropertyIsLeftUnmapped()
    {
        AssertGeneratedWithoutDiagnostics();

        var from = new EnumsTestCase.UnmappedFrom
        {
            Status = EnumsTestCase.Status.Active
        };

        var to = GetConvertingService().Convert<EnumsTestCase.UnmappedFrom, EnumsTestCase.UnmappedTo>(from);

        Assert.IsNotNull(to);
        Assert.AreEqual(EnumsTestCase.StatusEntity.Unknown, to.Status);
    }
}
