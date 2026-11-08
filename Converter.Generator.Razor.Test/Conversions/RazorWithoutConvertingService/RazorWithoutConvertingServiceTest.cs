using Majipro.Converter.Abstrations;
using Microsoft.Extensions.DependencyInjection;
using static Majipro.Converter.Generator.Razor.Test.Conversions.RazorWithoutConvertingService.RazorWithoutConvertingServiceTestCase;

namespace Majipro.Converter.Generator.Razor.Test.Conversions.RazorWithoutConvertingService;

/// <summary>
/// No Razor file injects the converting service and no C# property has it injected, so the Razor
/// files are not compiled - a project whose pages convert nothing does not pay for compiling them
/// twice. What that leaves out is exactly this page: a service of the project's own wrapping the
/// converting service is not looked for.
/// </summary>
[TestClass]
public class RazorWithoutConvertingServiceTest : RazorConversionTestBase<RazorWithoutConvertingServiceComposition>
{
    internal const string Folder = @".\Conversions\RazorWithoutConvertingService\";

    public RazorWithoutConvertingServiceTest()
        : base(
            Folder + "RazorWithoutConvertingServiceComposition.cs",
            Folder + @"Wrapped\_Imports.razor",
            Folder + @"Wrapped\PersonPage.razor")
    {
    }

    protected override string ProjectDirectory => Folder + @"Wrapped\";

    protected override string RootNamespace => "Majipro.Converter.Generator.Razor.Test.Conversions.RazorWithoutConvertingService";

    [TestMethod]
    public void WhenNoRazorFileInjectsTheConvertingServiceThenTheRazorFilesAreNotCompiled()
    {
        AssertGeneratedWithoutDiagnostics();
        Assert.AreEqual(0, GeneratedSources.Count);
        Assert.IsNull(ServiceProvider.GetService<IConverter<Person, WrappedView>>());
    }
}

/// <summary>
/// The same page in a project that does inject the converting service into its Razor files: they are
/// compiled, and the wrapped call is found like any other - which is what says it was not compiling
/// them that left it out above, not the call.
/// </summary>
[TestClass]
public class RazorWithConvertingServiceTest : RazorConversionTestBase<RazorWithoutConvertingServiceComposition>
{
    public RazorWithConvertingServiceTest()
        : base(
            RazorWithoutConvertingServiceTest.Folder + "RazorWithoutConvertingServiceComposition.cs",
            RazorWithoutConvertingServiceTest.Folder + @"Injected\_Imports.razor",
            RazorWithoutConvertingServiceTest.Folder + @"Injected\PersonPage.razor")
    {
    }

    protected override string ProjectDirectory => RazorWithoutConvertingServiceTest.Folder + @"Injected\";

    protected override string RootNamespace => "Majipro.Converter.Generator.Razor.Test.Conversions.RazorWithoutConvertingService";

    [TestMethod]
    public void WhenARazorFileInjectsTheConvertingServiceThenEveryCallInThemIsFound()
    {
        AssertGeneratedWithoutDiagnostics();
        Assert.AreEqual("Jo", GetConverter<Person, WrappedView>().Convert(new Person { Name = "Jo" }).Name);
    }
}
