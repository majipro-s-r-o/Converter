using Majipro.Converter.Generator.Razor;
using Microsoft.AspNetCore.Components;
using static Majipro.Converter.Generator.Razor.Test.Conversions.RazorInjectAttribute.RazorInjectAttributeTestCase;

namespace Majipro.Converter.Generator.Razor.Test.Conversions.RazorInjectAttribute;

/// <summary>
/// The converting service injected by <c>[Inject]</c> in a code behind file: no Razor file names it,
/// and the project's Razor files are compiled all the same.
/// </summary>
[TestClass]
public class RazorInjectAttributeCodeBehindTest : RazorConversionTestBase<RazorInjectAttributeComposition>
{
    private const string Folder = @".\Conversions\RazorInjectAttribute\";

    public RazorInjectAttributeCodeBehindTest()
        : base(
            Folder + "RazorInjectAttributeComposition.cs",
            Folder + @"CodeBehind\_Imports.razor",
            Folder + @"CodeBehind\PersonPage.razor",
            Folder + @"CodeBehind\PersonPage.razor.cs")
    {
    }

    protected override string ProjectDirectory => Folder + @"CodeBehind\";

    protected override string RootNamespace => "Majipro.Converter.Generator.Razor.Test.Conversions.RazorInjectAttribute.CodeBehind";

    [TestMethod]
    public void WhenBlazorsInjectAttributeIsLookedForThenItIsLookedForByItsRealName()
    {
        // The generator can not reference ASP.NET Core, so it names the attribute as a string; this is
        // what nameof would otherwise be.
        Assert.AreEqual(RazorSources.InjectAttribute, typeof(InjectAttribute).FullName);
    }

    [TestMethod]
    public void WhenACodeBehindFileInjectsTheConvertingServiceThenTheRazorFilesAreCompiled()
    {
        AssertGeneratedWithoutDiagnostics();
        Assert.AreEqual("Jo", GetConverter<Person, CodeBehindView>().Convert(new Person { Name = "Jo" }).Name);
    }
}

/// <summary>
/// The converting service injected by <c>[Inject]</c> in a base component the page inherits: no
/// Razor file names it, and the project's Razor files are compiled all the same.
/// </summary>
[TestClass]
public class RazorInjectAttributeBaseComponentTest : RazorConversionTestBase<RazorInjectAttributeComposition>
{
    private const string Folder = @".\Conversions\RazorInjectAttribute\";

    public RazorInjectAttributeBaseComponentTest()
        : base(
            Folder + "RazorInjectAttributeComposition.cs",
            Folder + @"BaseComponent\ConvertingComponentBase.cs",
            Folder + @"BaseComponent\_Imports.razor",
            Folder + @"BaseComponent\PersonPage.razor")
    {
    }

    protected override string ProjectDirectory => Folder + @"BaseComponent\";

    protected override string RootNamespace => "Majipro.Converter.Generator.Razor.Test.Conversions.RazorInjectAttribute.BaseComponent";

    [TestMethod]
    public void WhenABaseComponentInjectsTheConvertingServiceThenTheRazorFilesAreCompiled()
    {
        AssertGeneratedWithoutDiagnostics();
        Assert.AreEqual("Jo", GetConverter<Person, BaseComponentView>().Convert(new Person { Name = "Jo" }).Name);
    }
}
