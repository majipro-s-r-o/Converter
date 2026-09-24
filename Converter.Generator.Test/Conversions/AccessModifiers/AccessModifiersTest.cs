namespace Majipro.Converter.Generator.Test.Conversions.AccessModifiers;

[TestClass]
public class AccessModifiersTest : ConversionTestBase<AccessModifiersComposition>
{
    public AccessModifiersTest()
        : base(@".\Conversions\AccessModifiers\AccessModifiersComposition.cs")
    {
    }

    [TestMethod]
    public void WhenGeneratorRunsThenThereAreNoDiagnostics()
    {
        AssertGeneratedWithoutDiagnostics();

        Assert.AreEqual(
            5,
            GeneratedSources.Count,
            "A converter is expected even for a conversion that has nothing to map.");
    }

    [TestMethod]
    public void WhenThePropertyIsPublicThenItIsConverted()
    {
        AssertGeneratedWithoutDiagnostics();

        var from = new AccessModifiersTestCase.PublicPropertyFrom
        {
            PublicProperty = "hello"
        };

        var to = GetConvertingService()
            .Convert<AccessModifiersTestCase.PublicPropertyFrom, AccessModifiersTestCase.PublicPropertyTo>(from);

        Assert.IsNotNull(to);
        Assert.AreEqual(from.PublicProperty, to.PublicProperty);
    }

    [TestMethod]
    public void WhenThereIsNoPublicPropertyThenTheTargetIsStillCreated()
    {
        AssertGeneratedWithoutDiagnostics();

        var to = GetConvertingService()
            .Convert<AccessModifiersTestCase.PrivatePropertyFrom, AccessModifiersTestCase.PrivatePropertyTo>(
                new AccessModifiersTestCase.PrivatePropertyFrom());

        Assert.IsNotNull(to);
    }

    [TestMethod]
    public void WhenTheTargetPropertyIsInitOnlyThenItIsConverted()
    {
        AssertGeneratedWithoutDiagnostics();

        var from = new AccessModifiersTestCase.InitPropertyFrom
        {
            PublicProperty = "hello"
        };

        var to = GetConvertingService()
            .Convert<AccessModifiersTestCase.InitPropertyFrom, AccessModifiersTestCase.InitPropertyTo>(from);

        Assert.IsNotNull(to);
        Assert.AreEqual(from.PublicProperty, to.PublicProperty);
    }

    [TestMethod]
    public void WhenTheTargetPropertyHasNoSetterThenItStaysDefault()
    {
        AssertGeneratedWithoutDiagnostics();

        var to = GetConvertingService()
            .Convert<AccessModifiersTestCase.GetOnlyPropertyFrom, AccessModifiersTestCase.GetOnlyPropertyTo>(
                new AccessModifiersTestCase.GetOnlyPropertyFrom());

        Assert.IsNotNull(to);
        Assert.IsNull(to.PublicProperty);
    }

    [TestMethod]
    public void WhenTheTargetSetterIsPrivateThenThePropertyStaysDefault()
    {
        AssertGeneratedWithoutDiagnostics();

        var to = GetConvertingService()
            .Convert<AccessModifiersTestCase.PrivateSetterFrom, AccessModifiersTestCase.PrivateSetterTo>(
                new AccessModifiersTestCase.PrivateSetterFrom());

        Assert.IsNotNull(to);
        Assert.IsNull(to.PublicProperty);
    }

    [TestMethod]
    public void WhenTheTargetPropertyIsInitOnlyThenThereIsNoReferenceConverter()
    {
        AssertGeneratedWithoutDiagnostics();

        var converter = FindReferenceConverter<AccessModifiersTestCase.InitPropertyFrom,
            AccessModifiersTestCase.InitPropertyTo>();

        Assert.IsNull(
            converter,
            "An init only property is written while the target is created and never again.");
    }

    [TestMethod]
    public void WhenTheTargetPropertyHasAnOrdinarySetterThenThereIsAReferenceConverter()
    {
        AssertGeneratedWithoutDiagnostics();

        var from = new AccessModifiersTestCase.PublicPropertyFrom
        {
            PublicProperty = "hello"
        };

        var to = new AccessModifiersTestCase.PublicPropertyTo();

        var converted = GetReferenceConverter<AccessModifiersTestCase.PublicPropertyFrom,
            AccessModifiersTestCase.PublicPropertyTo>().Convert(from, to);

        Assert.AreSame(to, converted);
        Assert.AreEqual(from.PublicProperty, to.PublicProperty);
    }

    [TestMethod]
    public void WhenThereIsNothingToFillThenTheTargetComesBackUntouched()
    {
        AssertGeneratedWithoutDiagnostics();

        var to = new AccessModifiersTestCase.GetOnlyPropertyTo();

        var converted = GetReferenceConverter<AccessModifiersTestCase.GetOnlyPropertyFrom,
            AccessModifiersTestCase.GetOnlyPropertyTo>().Convert(new AccessModifiersTestCase.GetOnlyPropertyFrom(), to);

        Assert.AreSame(to, converted, "A target with nothing to fill is still a target that can be handed in.");
    }
}
