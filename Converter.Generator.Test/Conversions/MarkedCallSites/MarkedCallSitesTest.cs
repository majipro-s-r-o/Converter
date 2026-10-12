using System;
using System.Collections.Generic;

namespace Majipro.Converter.Generator.Test.Conversions.MarkedCallSites;

/// <summary>
/// Methods that require a converter without being a method of <c>IConvertingService</c>: the
/// <c>ConvertExplicitly</c> extensions, marked methods of a referenced assembly (one of them in a
/// nested type) and of the compilation itself - called with inferred type arguments, from a generic
/// method, as an extension, through an implementation or an override of the marked member, with a
/// class level type parameter, and as a local function. The pair of each is read off
/// <c>RequiresConverterAttribute</c>, by the names of the type parameters, and is generated like that
/// of any <c>Convert</c> call.
/// </summary>
[TestClass]
public class MarkedCallSitesTest : ConversionTestBase<MarkedCallSitesComposition>
{
    public MarkedCallSitesTest()
        : base(@".\Conversions\MarkedCallSites\MarkedCallSitesComposition.cs")
    {
    }

    [TestMethod]
    public void WhenGeneratorRunsThenThereAreNoDiagnostics()
    {
        AssertGeneratedWithoutDiagnostics();

        Assert.AreEqual(
            11,
            GeneratedSources.Count,
            "One converter is expected for every marked call, and nothing else.");
    }

    [TestMethod]
    [DataRow(typeof(MarkedCallSitesTestCase.ExplicitListFrom), typeof(MarkedCallSitesTestCase.ExplicitListTo))]
    [DataRow(typeof(MarkedCallSitesTestCase.ExplicitArrayFrom), typeof(MarkedCallSitesTestCase.ExplicitArrayTo))]
    [DataRow(typeof(MarkedCallSitesTestCase.ReferencedFrom), typeof(MarkedCallSitesTestCase.ReferencedTo))]
    [DataRow(typeof(MarkedCallSitesTestCase.LocalFrom), typeof(MarkedCallSitesTestCase.LocalTo))]
    [DataRow(typeof(MarkedCallSitesTestCase.NestedFrom), typeof(MarkedCallSitesTestCase.NestedTo))]
    [DataRow(typeof(MarkedCallSitesTestCase.GenericFrom), typeof(MarkedCallSitesTestCase.GenericTo))]
    [DataRow(typeof(MarkedCallSitesTestCase.ExtensionFrom), typeof(MarkedCallSitesTestCase.ExtensionTo))]
    [DataRow(typeof(MarkedCallSitesTestCase.ContractFrom), typeof(MarkedCallSitesTestCase.ContractTo))]
    [DataRow(typeof(MarkedCallSitesTestCase.OverrideFrom), typeof(MarkedCallSitesTestCase.OverrideTo))]
    [DataRow(typeof(MarkedCallSitesTestCase.ClassLevelFrom), typeof(MarkedCallSitesTestCase.ClassLevelTo))]
    [DataRow(typeof(MarkedCallSitesTestCase.LocalFunctionFrom), typeof(MarkedCallSitesTestCase.LocalFunctionTo))]
    public void WhenAMarkedMethodIsCalledThenItsPairIsGenerated(Type from, Type to)
    {
        var converter = ServiceProvider.GetService(
            typeof(Majipro.Converter.Abstrations.IConverter<,>).MakeGenericType(from, to));

        Assert.IsNotNull(converter, $"No converter for {from.Name} -> {to.Name}.");
    }

    /// <summary>
    /// <c>Relay&lt;TTarget, TSource&gt;</c> declares its type parameters the other way round from the
    /// conversion it does, so a pair taken by position would be the reverse one.
    /// </summary>
    [TestMethod]
    public void WhenTheTypeParametersAreDeclaredInAnotherOrderThenThePairIsTakenByName()
    {
        var reverse = ServiceProvider.GetService(
            typeof(Majipro.Converter.Abstrations.IConverter<MarkedCallSitesTestCase.ReferencedTo, MarkedCallSitesTestCase.ReferencedFrom>));

        Assert.IsNull(reverse, "The pair is expected to be read by the names the attribute gives, not by position.");
    }

    [TestMethod]
    public void WhenAMarkedMethodOfAReferencedAssemblyIsCalledThenItConverts()
    {
        var to = MarkedCallSitesTestCase.Relay<MarkedCallSitesTestCase.ReferencedTo, MarkedCallSitesTestCase.ReferencedFrom>(
            GetConvertingService(),
            new MarkedCallSitesTestCase.ReferencedFrom { Name = "hello" });

        Assert.AreEqual("hello", to.Name);
    }

    [TestMethod]
    public void WhenConvertExplicitlyIsCalledThenTheItemsAreConverted()
    {
        var to = new MarkedCallSitesComposition().ConvertExplicitList(
            GetConvertingService(),
            new List<MarkedCallSitesTestCase.ExplicitListFrom>
            {
                new MarkedCallSitesTestCase.ExplicitListFrom { Name = "hello" }
            });

        Assert.AreEqual("hello", to[0].Name);
    }
}
