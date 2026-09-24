using System.Linq;

namespace Majipro.Converter.Generator.Test.Conversions.InternalTypes;

/// <summary>
/// A converter for a pair the rest of the assembly can see but nobody outside of it can. The two
/// <c>Convert</c> methods are public - they implement an interface - so the class around them can
/// not be, and a generated class that says otherwise does not compile at all.
/// </summary>
/// <remarks>
/// The types live in the composition file, so the test assembly has a copy of them that is a
/// different type from the one the generated converter talks about. That is why this case asserts
/// on the generated source and on the container being built rather than on a converted instance:
/// building it is what compiles the generated code and what scans the assembly for it.
/// </remarks>
[TestClass]
public class InternalTypesTest : ConversionTestBase<InternalTypesComposition>
{
    public InternalTypesTest()
        : base(@".\Conversions\InternalTypes\InternalTypesComposition.cs")
    {
    }

    [TestMethod]
    public void WhenGeneratorRunsThenThereAreNoDiagnostics()
    {
        AssertGeneratedWithoutDiagnostics();

        Assert.AreEqual(2, GeneratedSources.Count, "The pair and the pair its property needs are expected.");
    }

    [TestMethod]
    public void WhenTypesAreInternalThenTheGeneratedConverterIsInternalToo()
    {
        AssertGeneratedWithoutDiagnostics();

        Assert.IsTrue(
            GeneratedSources.All(s => s.Contains("internal sealed class")),
            "A public class can neither take nor return an internal type.");

        Assert.IsFalse(
            GeneratedSources.Any(s => s.Contains("public sealed class")),
            "A public class can neither take nor return an internal type.");
    }

    [TestMethod]
    public void WhenTypesAreInternalThenTheGeneratedConverterCompilesAndIsRegistered()
    {
        AssertGeneratedWithoutDiagnostics();

        // Building the container is what emits the generated code and what scans for the converters.
        Assert.IsNotNull(GetConvertingService());
    }
}
