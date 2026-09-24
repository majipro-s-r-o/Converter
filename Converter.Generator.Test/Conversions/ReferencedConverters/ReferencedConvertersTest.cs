using Majipro.Converter.Abstrations;
using Microsoft.Extensions.DependencyInjection;

namespace Majipro.Converter.Generator.Test.Conversions.ReferencedConverters;

/// <summary>
/// A converter is looked for the way <c>AddConverting</c> looks for one: in the implementations, in
/// every assembly one could be registered from. The pair here is implemented in the test assembly
/// and asked for in the compiled test case, so it is a pair somebody wrote in another assembly -
/// nothing is generated for it, and nothing is reported about it either.
/// </summary>
[TestClass]
public class ReferencedConvertersTest : ConversionTestBase<ReferencedConvertersComposition>
{
    public ReferencedConvertersTest()
        : base(@".\Conversions\ReferencedConverters\ReferencedConvertersComposition.cs")
    {
    }

    [TestMethod]
    public void WhenTheConverterIsInAReferencedAssemblyThenNothingIsGenerated()
    {
        AssertGeneratedWithoutDiagnostics();

        Assert.AreEqual(
            0,
            GeneratedSources.Count,
            "The pair is implemented, generating it again is what DiCompositionValidator throws about.");
    }

    /// <summary>
    /// The other half of the same fact, and the consequence to know about: the generated assembly
    /// carries nothing for this pair, so the assembly that does implement it has to be one of the
    /// assemblies handed to <c>AddConverting</c>.
    /// </summary>
    [TestMethod]
    public void WhenTheConverterIsInAReferencedAssemblyThenTheCompiledAssemblyCarriesNoConverter()
    {
        AssertGeneratedWithoutDiagnostics();

        var converter = ServiceProvider
            .GetService<IConverter<ReferencedConvertersTestCase.From, ReferencedConvertersTestCase.To>>();

        Assert.IsNull(converter);
    }
}
