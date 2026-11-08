using System.Linq;
using Majipro.Converter.Abstractions;
using Majipro.Converter.Generator.Diagnostics;
using Majipro.Converter.Generator.Razor;
using Majipro.Converter.Generator.Test.Helpers;
using Microsoft.CodeAnalysis;
using Microsoft.Extensions.DependencyInjection;
using static Majipro.Converter.Generator.Test.Conversions.RazorGeneratorUnavailable.RazorGeneratorUnavailableTestCase;

namespace Majipro.Converter.Generator.Test.Conversions.RazorGeneratorUnavailable;

/// <summary>
/// Razor files injecting the converting service, and no Razor generator to compile them with: the
/// build does not say where it is. Their conversions can not be generated, which is a warning
/// rather than a silence, and the C# of the project is generated as always.
/// </summary>
[TestClass]
public class RazorGeneratorUnavailableTest : ConversionTestBase<RazorGeneratorUnavailableComposition>
{
    internal const string Folder = @".\Conversions\RazorGeneratorUnavailable\";

    public RazorGeneratorUnavailableTest()
        : base(
            Folder + "RazorGeneratorUnavailableComposition.cs",
            Folder + @"Injected\_Imports.razor",
            Folder + @"Injected\PersonPage.razor")
    {
    }

    protected override string ProjectDirectory => Folder + @"Injected\";

    [TestMethod]
    public void WhenRazorsGeneratorIsNotKnownThenTheRazorFilesAreAWarning()
    {
        var diagnostic = Diagnostic.Single();

        Assert.AreEqual(ConverterDiagnostics.RazorNotCompiled.Id, diagnostic.Id);
        Assert.AreEqual(DiagnosticSeverity.Warning, diagnostic.Severity);
        StringAssert.Contains(diagnostic.GetMessage(), RazorSources.GeneratorOption);
    }

    [TestMethod]
    public void WhenRazorsGeneratorIsNotKnownThenTheCSharpIsGeneratedAnyway()
    {
        Assert.IsNotNull(GetConverter<Person, PersonView>());
        Assert.IsNull(ServiceProvider.GetService<IConverter<Person, RazorView>>());
    }
}

/// <summary>The build says where Razor's generator is, and there is nothing there.</summary>
[TestClass]
public class RazorGeneratorUnloadableTest : ConversionTestBase<RazorGeneratorUnavailableComposition>
{
    private const string Missing = "Missing.Razor.Generator.dll";

    public RazorGeneratorUnloadableTest()
        : base(
            RazorGeneratorUnavailableTest.Folder + "RazorGeneratorUnavailableComposition.cs",
            RazorGeneratorUnavailableTest.Folder + @"Injected\_Imports.razor",
            RazorGeneratorUnavailableTest.Folder + @"Injected\PersonPage.razor")
    {
    }

    protected override string ProjectDirectory => RazorGeneratorUnavailableTest.Folder + @"Injected\";

    protected override ICompilingHelper Configure(ICompilingHelper compilingHelper)
    {
        return compilingHelper.AddGlobalOption(RazorSources.GeneratorOption, Missing);
    }

    [TestMethod]
    public void WhenRazorsGeneratorCanNotBeLoadedThenTheWarningSaysWhatWasNotThere()
    {
        var diagnostic = Diagnostic.Single();

        Assert.AreEqual(ConverterDiagnostics.RazorNotCompiled.Id, diagnostic.Id);
        StringAssert.Contains(diagnostic.GetMessage(), Missing);
        Assert.IsNotNull(GetConverter<Person, PersonView>());
    }
}

/// <summary>
/// Razor files that do not inject the converting service: nothing in them needs compiling, so not
/// knowing where Razor's generator is costs nothing and is worth no warning.
/// </summary>
[TestClass]
public class RazorWithoutConvertingServiceTest : ConversionTestBase<RazorGeneratorUnavailableComposition>
{
    public RazorWithoutConvertingServiceTest()
        : base(
            RazorGeneratorUnavailableTest.Folder + "RazorGeneratorUnavailableComposition.cs",
            RazorGeneratorUnavailableTest.Folder + @"Plain\PersonPage.razor")
    {
    }

    protected override string ProjectDirectory => RazorGeneratorUnavailableTest.Folder + @"Plain\";

    [TestMethod]
    public void WhenNoRazorFileInjectsTheConvertingServiceThenNothingIsReported()
    {
        AssertGeneratedWithoutDiagnostics();
        Assert.IsNotNull(GetConverter<Person, PersonView>());
    }
}
