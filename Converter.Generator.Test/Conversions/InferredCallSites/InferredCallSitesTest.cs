using System;

namespace Majipro.Converter.Generator.Test.Conversions.InferredCallSites;

/// <summary>
/// Call sites whose pair is not two type arguments written out: inferred ones and the four of a
/// dictionary. They used to be invisible to the generator, so they compiled and threw at runtime
/// about a conversion that did not exist.
/// </summary>
[TestClass]
public class InferredCallSitesTest : ConversionTestBase<InferredCallSitesComposition>
{
    public InferredCallSitesTest()
        : base(@".\Conversions\InferredCallSites\InferredCallSitesComposition.cs")
    {
    }

    [TestMethod]
    public void WhenGeneratorRunsThenThereAreNoDiagnostics()
    {
        AssertGeneratedWithoutDiagnostics();

        Assert.AreEqual(
            4,
            GeneratedSources.Count,
            "One converter is expected for every item pair, and one for the dictionary keys.");
    }

    [TestMethod]
    [DataRow(typeof(InferredCallSitesTestCase.ListFrom), typeof(InferredCallSitesTestCase.ListTo))]
    [DataRow(typeof(InferredCallSitesTestCase.SetFrom), typeof(InferredCallSitesTestCase.SetTo))]
    [DataRow(typeof(InferredCallSitesTestCase.DictionaryFrom), typeof(InferredCallSitesTestCase.DictionaryTo))]
    [DataRow(typeof(Guid), typeof(string))]
    public void WhenTheTypeArgumentsAreNotWrittenOutAsAPairThenThePairIsStillGenerated(Type from, Type to)
    {
        AssertGeneratedWithoutDiagnostics();

        var converter = ServiceProvider.GetService(
            typeof(Majipro.Converter.Abstrations.IConverter<,>).MakeGenericType(from, to));

        Assert.IsNotNull(converter, $"No converter for {from.Name} -> {to.Name}.");
    }

    [TestMethod]
    public void WhenTheTypeArgumentsAreInferredThenTheItemsAreConverted()
    {
        AssertGeneratedWithoutDiagnostics();

        var to = GetConvertingService()
            .Convert<InferredCallSitesTestCase.ListFrom, InferredCallSitesTestCase.ListTo>(
                new InferredCallSitesTestCase.ListFrom { Name = "hello" });

        Assert.AreEqual("hello", to.Name);
    }
}
