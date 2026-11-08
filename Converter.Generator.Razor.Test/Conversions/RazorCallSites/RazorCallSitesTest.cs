using System;
using System.IO;
using System.Linq;
using Majipro.Converter.Abstrations;
using Majipro.Converter.Generator.Test.Extensions;
using Microsoft.CodeAnalysis;
using Microsoft.Extensions.DependencyInjection;
using static Majipro.Converter.Generator.Razor.Test.Conversions.RazorCallSites.RazorCallSitesTestCase;

namespace Majipro.Converter.Generator.Razor.Test.Conversions.RazorCallSites;

/// <summary>
/// Conversions written in <c>.razor</c> and <c>.cshtml</c> files. The Razor SDK compiles those with a
/// generator of its own, whose output no other generator sees, so the compilation handed to this one
/// has none of their call sites - only the files, as additional files. Every place a conversion can
/// be written at in markup has a target of its own, so the converter being there says the place was
/// found; the composition converts nothing.
/// </summary>
[TestClass]
public class RazorCallSitesTest : RazorConversionTestBase<RazorCallSitesComposition>
{
    private const string Folder = @".\Conversions\RazorCallSites\";

    public RazorCallSitesTest()
        : base(
            Folder + "RazorCallSitesComposition.cs",
            Folder + "_Imports.razor",
            Folder + @"Pages\PersonPage.razor",
            Folder + @"Pages\PersonPage.razor.cs",
            Folder + @"Pages\OrderPage.razor",
            Folder + @"Pages\OrderPage.razor.cs",
            Folder + @"Shared\Grid.razor",
            Folder + @"Shared\PersonCard.razor",
            Folder + @"Views\_ViewImports.cshtml",
            Folder + @"Views\Home\Index.cshtml")
    {
    }

    protected override string ProjectDirectory => Folder;

    protected override string RootNamespace => "Majipro.Converter.Generator.Razor.Test.Conversions.RazorCallSites";

    [TestMethod]
    public void WhenAConversionIsWrittenInMarkupThenItsConverterIsGenerated()
    {
        AssertConverts<InlineView>();
        AssertConverts<LoopView>();
        AssertConverts<ElseView>();
    }

    [TestMethod]
    public void WhenTheTypesAreInferredFromTheCodeBehindThenItsConverterIsGenerated()
    {
        AssertConverts<ImplicitView>();
    }

    [TestMethod]
    public void WhenTheCodeBehindConvertsThroughAnInjectedServiceThenItsConverterIsGenerated()
    {
        AssertConverts<CodeBehindView>();
        AssertConverts<InjectedCodeBehindView>();
    }

    [TestMethod]
    public void WhenCodeConvertsThroughTheComponentsOwnInjectThenItsConverterIsGenerated()
    {
        AssertConverts<InjectedCodeView>();
        AssertConverts<InjectedInferredView>();
        AssertConverts<InjectedAsyncView>();
    }

    [TestMethod]
    public void WhenAConversionIsWrittenInCodeThenItsConverterIsGenerated()
    {
        AssertConverts<BlockView>();
        AssertConverts<CodeView>();
        AssertConverts<MarkupInCodeView>();
        AssertConverts<TemplateView>();
    }

    [TestMethod]
    public void WhenAnAttributeIsCSharpThenItsConverterIsGenerated()
    {
        AssertConverts<AttributeView>();
        AssertConverts<HandlerView>();
    }

    [TestMethod]
    public void WhenChildContentConvertsItsContextThenItsConverterIsGenerated()
    {
        AssertConverts<ContextView>();
    }

    [TestMethod]
    public void WhenAViewConvertsItsModelThenItsConverterIsGenerated()
    {
        AssertConverts<ModelView>();
    }

    [TestMethod]
    public void WhenAConversionIsOnlyTextThenNothingIsGeneratedForIt()
    {
        Assert.IsNull(ServiceProvider.GetService<IConverter<Person, IgnoredView>>());
        Assert.AreEqual(
            17,
            GeneratedSources.Count,
            "One converter per place a conversion is written at is expected, and nothing else.");
    }

    [TestMethod]
    public void WhenAGenericComponentConvertsItsTypeParameterThenTheWarningIsInTheRazorFile()
    {
        var diagnostic = Diagnostic.Single();
        var span = diagnostic.Location.GetLineSpan();
        var grid = (Folder + @"Shared\Grid.razor").ToPath();
        var line = Array.FindIndex(File.ReadAllLines(grid), l => l.Contains("Convert<TItem, string>"));

        Assert.AreEqual("MC0005", diagnostic.Id);
        Assert.AreEqual(grid, span.Path);
        Assert.AreEqual(line, span.StartLinePosition.Line);
        Assert.AreEqual(
            "Converter.Convert<TItem, string>(item)",
            File.ReadAllLines(grid)[line].Substring(
                span.StartLinePosition.Character,
                span.EndLinePosition.Character - span.StartLinePosition.Character));
    }

    private void AssertConverts<TView>()
        where TView : NamedView
    {
        var person = new Person
        {
            Name = typeof(TView).Name
        };

        var view = GetConverter<Person, TView>().Convert(person);

        Assert.IsNotNull(view);
        Assert.AreEqual(person.Name, view.Name);
    }
}
