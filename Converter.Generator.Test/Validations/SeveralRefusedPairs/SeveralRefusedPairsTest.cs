using System.Linq;
using Microsoft.CodeAnalysis;

namespace Majipro.Converter.Generator.Test.Validations.SeveralRefusedPairs;

/// <summary>
/// Several pairs that have to be dealt with by hand are reported by one build, rather than one per
/// build with a fix in between.
/// </summary>
[TestClass]
public class SeveralRefusedPairsTest : ConversionTestBase<SeveralRefusedPairsComposition>
{
    public SeveralRefusedPairsTest()
        : base(@".\Validations\SeveralRefusedPairs\SeveralRefusedPairsComposition.cs")
    {
    }

    [TestMethod]
    public void WhenSeveralPairsAreRefusedThenEachOfThemIsReported()
    {
        CollectionAssert.AreEquivalent(
            new[] { "MC0002", "MC0003", "MC0004" },
            Diagnostic.Select(d => d.Id).ToArray());
        Assert.IsTrue(Diagnostic.All(d => d.Severity == DiagnosticSeverity.Error));
    }

    [TestMethod]
    public void WhenSeveralPairsAreRefusedThenEachErrorNamesItsPair()
    {
        var messages = Diagnostic.ToDictionary(d => d.Id, d => d.GetMessage());

        StringAssert.Contains(messages["MC0003"], typeof(SeveralRefusedPairsTestCase.To).FullName!.Replace("+", "."));
        StringAssert.Contains(messages["MC0002"], nameof(SeveralRefusedPairsTestCase.Wide.Active));
        StringAssert.Contains(messages["MC0004"], "float");
    }

    /// <summary>
    /// A refused pair is the end of that pair only: the one nothing is wrong with is still written.
    /// </summary>
    [TestMethod]
    public void WhenSeveralPairsAreRefusedThenThePairNothingIsWrongWithIsGenerated()
    {
        Assert.AreEqual(1, GeneratedSources.Count);
        StringAssert.Contains(
            GeneratedSources.Single(),
            nameof(SeveralRefusedPairsTestCase.PersonDto));
    }
}
