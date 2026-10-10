using System.Linq;

namespace Majipro.Converter.Generator.Test.Conversions.ClosedGenericCallSite;

/// <summary>
/// A call site naming a type parameter asks, at runtime, for whatever the callers of its method close
/// it over - and those pairs are generated, the way a call site writing them down would have them.
/// Before, nothing was written for such a call site and the conversion threw at runtime about a
/// converter that did not exist.
/// </summary>
[TestClass]
public class ClosedGenericCallSiteTest : ConversionTestBase<ClosedGenericCallSiteComposition>
{
    public ClosedGenericCallSiteTest()
        : base(@".\Conversions\ClosedGenericCallSite\ClosedGenericCallSiteComposition.cs")
    {
    }

    [TestMethod]
    public void WhenGeneratorRunsThenThereAreNoDiagnostics()
    {
        AssertGeneratedWithoutDiagnostics();

        Assert.AreEqual(
            5,
            GeneratedSources.Count,
            "One converter is expected for every closed pair but the one written by hand.");
    }

    [TestMethod]
    public void WhenTheTypeArgumentIsInferredThenThePairItClosesIntoIsGenerated()
    {
        AssertGeneratedWithoutDiagnostics();

        var to = GetConvertingService()
            .Convert<ClosedGenericCallSiteTestCase.NotificationVariable.NotificationKind, ClosedGenericCallSiteTestCase.VariableKind>(
                ClosedGenericCallSiteTestCase.NotificationVariable.NotificationKind.Link);

        Assert.AreEqual(ClosedGenericCallSiteTestCase.VariableKind.Link, to);
    }

    [TestMethod]
    public void WhenThePairItClosesIntoIsWrittenByHandThenNothingIsGeneratedForIt()
    {
        AssertGeneratedWithoutDiagnostics();

        Assert.IsFalse(
            GeneratedSources.Any(s => s.Contains("PrintKind")),
            "The pair is answered by PrintKindConverter.");

        var to = GetConvertingService()
            .Convert<ClosedGenericCallSiteTestCase.PrintVariable.PrintKind, ClosedGenericCallSiteTestCase.VariableKind>(
                ClosedGenericCallSiteTestCase.PrintVariable.PrintKind.QrCode);

        Assert.AreEqual(ClosedGenericCallSiteTestCase.VariableKind.Barcode, to);
    }

    [TestMethod]
    public void WhenTheTypeParameterIsHandedOnThenTheOutermostCallerClosesIt()
    {
        AssertGeneratedWithoutDiagnostics();

        var to = GetConvertingService()
            .Convert<ClosedGenericCallSiteTestCase.RenamedKind, ClosedGenericCallSiteTestCase.VariableKind>(
                ClosedGenericCallSiteTestCase.RenamedKind.Template);

        Assert.AreEqual(ClosedGenericCallSiteTestCase.VariableKind.Template, to);
    }

    [TestMethod]
    public void WhenTheMethodIsAnExtensionThenItsReceiverClosesIt()
    {
        AssertGeneratedWithoutDiagnostics();

        var to = GetConvertingService()
            .Convert<ClosedGenericCallSiteTestCase.ExtensionKind, ClosedGenericCallSiteTestCase.VariableKind>(
                ClosedGenericCallSiteTestCase.ExtensionKind.QrCode);

        Assert.AreEqual(ClosedGenericCallSiteTestCase.VariableKind.QrCode, to);
    }

    [TestMethod]
    public void WhenTheTypeParameterBelongsToTheTypeThenTheTypeItIsCalledOnClosesIt()
    {
        AssertGeneratedWithoutDiagnostics();

        var to = GetConvertingService()
            .Convert<ClosedGenericCallSiteTestCase.MapperKind, ClosedGenericCallSiteTestCase.VariableKind>(
                ClosedGenericCallSiteTestCase.MapperKind.Variable);

        Assert.AreEqual(ClosedGenericCallSiteTestCase.VariableKind.Variable, to);
    }

    [TestMethod]
    public void WhenTheMethodIsALocalFunctionThenItsCallClosesIt()
    {
        AssertGeneratedWithoutDiagnostics();

        var to = GetConvertingService()
            .Convert<ClosedGenericCallSiteTestCase.LocalKind, ClosedGenericCallSiteTestCase.VariableKind>(
                ClosedGenericCallSiteTestCase.LocalKind.Link);

        Assert.AreEqual(ClosedGenericCallSiteTestCase.VariableKind.Link, to);
    }
}
