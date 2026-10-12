using System.Linq;
using Microsoft.CodeAnalysis;

namespace Majipro.Converter.Generator.Test.Validations.UnknownRequiredTypeParameter;

/// <summary>
/// <c>RequiresConverterAttribute</c> naming something that is not a type parameter of the method it is
/// put on. Ignoring it would leave the call to compile and then throw at runtime about a conversion
/// nobody generated, so the build has to fail instead.
/// </summary>
[TestClass]
public class UnknownRequiredTypeParameterTest : ConversionTestBase<UnknownRequiredTypeParameterComposition>
{
    public UnknownRequiredTypeParameterTest()
        : base(@".\Validations\UnknownRequiredTypeParameter\UnknownRequiredTypeParameterComposition.cs")
    {
    }

    [TestMethod]
    public void WhenTheAttributeNamesNoTypeParameterThenAnErrorIsReported()
    {
        var diagnostic = Diagnostic.Single();

        Assert.AreEqual("MC0006", diagnostic.Id);
        Assert.AreEqual(DiagnosticSeverity.Error, diagnostic.Severity);
    }

    [TestMethod]
    public void WhenTheAttributeNamesNoTypeParameterThenTheErrorNamesTheMethodAndTheName()
    {
        var message = Diagnostic.Single().GetMessage();

        StringAssert.Contains(message, nameof(UnknownRequiredTypeParameterTestCase.RelayWithTypo));
        StringAssert.Contains(message, "'TTarget' is not a type parameter");
    }

    [TestMethod]
    public void WhenTheAttributeNamesNoTypeParameterThenNoConverterIsGenerated()
    {
        Assert.AreEqual(0, GeneratedSources.Count);
    }
}
