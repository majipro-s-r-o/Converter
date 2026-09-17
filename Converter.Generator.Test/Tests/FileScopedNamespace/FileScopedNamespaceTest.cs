namespace Majipro.Converter.Generator.Test.Tests.FileScopedNamespace;

[TestClass]
public class FileScopedNamespaceTest : ConversionTestBase<FileScopedNamespaceComposition>
{
    public FileScopedNamespaceTest()
        : base(@".\Tests\FileScopedNamespace\FileScopedNamespaceComposition.cs")
    {
    }

    [TestMethod]
    public void WhenGeneratorRunsThenThereAreNoDiagnostics()
    {
        AssertGeneratedWithoutDiagnostics();

        Assert.AreEqual(1, GeneratedSources.Count, "Exactly one converter is expected for one conversion.");
    }

    [TestMethod]
    public void WhenTypesAreDeclaredInAFileScopedNamespaceThenTheyAreConverted()
    {
        AssertGeneratedWithoutDiagnostics();

        var from = new FileScopedNamespaceFrom
        {
            Property = "hello"
        };

        var to = GetConvertingService().Convert<FileScopedNamespaceFrom, FileScopedNamespaceTo>(from);

        Assert.IsNotNull(to);
        Assert.AreEqual(from.Property, to.Property);
    }
}
