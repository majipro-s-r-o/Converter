using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Majipro.Converter.Abstrations;
using Majipro.Converter.Generator.Receivers;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;

namespace Majipro.Converter.Generator.Razor;

/// <summary>
/// The C# Razor makes of the project's <c>.razor</c> and <c>.cshtml</c> files, as syntax trees the
/// call sites written in them can be bound in. The Razor SDK compiles those files with a generator
/// of its own, whose output no other generator ever sees, so a <c>Convert&lt;A, B&gt;(...)</c> written
/// in a component was a call site nobody found, and a conversion that threw at runtime. The trees are
/// <see cref="RazorGenerator"/>'s, which is Razor's generator run once more, and they are added to a
/// compilation only this generator binds against - nothing of them reaches the build, which has its
/// own copy from the real run.
/// </summary>
/// <remarks>
/// Compiling the Razor files a second time is not free, so it is done only for a project that
/// injects the converting service into them, which is a question about the whole project rather
/// than about a file: Razor compiles a page together with every component it uses, and a service
/// injected once in <c>_Imports.razor</c> is called in files that never name it. A project does when
/// a Razor file says <c>IConvertingService</c> (an <c>@inject</c>, in the file or in an imports
/// file), or when a C# property of that type carries Blazor's <c>[Inject]</c> (a code behind file,
/// a base component), however the attribute is spelled. A Razor file reaching the service some other way - a service of the project's
/// own wrapping it, say - is not looked for.
/// </remarks>
internal sealed class RazorSources
{
    /// <summary>Where Razor's generator is, see <c>buildTransitive/Majipro.Converter.targets</c>.</summary>
    internal const string GeneratorOption = "build_property.MajiproConverterRazorGenerator";

    private const string ProjectDirectoryOption = "build_property.MSBuildProjectDirectory";
    /// <summary>
    /// Blazor's <c>[Inject]</c>. Its name is the one thing about Blazor the generator knows: an analyzer
    /// can not reference ASP.NET Core, so it is resolved in the compilation, and a project that does not
    /// reference Blazor has nothing injected this way. <c>Converter.Generator.Razor.Test</c>, which does
    /// reference it, holds the name against the real type.
    /// </summary>
    internal const string InjectAttribute = "Microsoft.AspNetCore.Components.InjectAttribute";

    private static readonly RazorSources None = new RazorSources(
        ImmutableArray<SyntaxTree>.Empty,
        new Dictionary<string, AdditionalText>(),
        null);

    private readonly HashSet<SyntaxTree> _trees;
    private readonly IReadOnlyDictionary<string, AdditionalText> _files;

    private RazorSources(
        ImmutableArray<SyntaxTree> syntaxTrees,
        IReadOnlyDictionary<string, AdditionalText> files,
        string? problem)
    {
        SyntaxTrees = syntaxTrees;
        Problem = problem;
        _trees = new HashSet<SyntaxTree>(syntaxTrees);
        _files = files;
    }

    /// <summary>Razor's C#, empty unless the project injects the converting service into Razor files.</summary>
    public ImmutableArray<SyntaxTree> SyntaxTrees { get; }

    /// <summary>
    /// Why the Razor files of a project that injects the converting service into them were not
    /// compiled after all, or <c>null</c>.
    /// </summary>
    public string? Problem { get; }

    public static RazorSources Create(GeneratorExecutionContext context, InvocationsSyntaxReceiver receiver)
    {
        var razorFiles = context.AdditionalFiles.Where(IsRazor).ToImmutableArray();

        if (razorFiles.IsEmpty ||
            context.Compilation.GetTypeByMetadataName(typeof(IConvertingService).FullName!) is not { } convertingService ||
            IsInjected(context, razorFiles, receiver, convertingService) == false)
        {
            return None;
        }

        var files = razorFiles
            .GroupBy(f => f.Path, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(g => g.Key, g => g.First(), StringComparer.OrdinalIgnoreCase);

        var options = context.AnalyzerConfigOptions;

        if (options.GlobalOptions.TryGetValue(GeneratorOption, out var generator) == false ||
            string.IsNullOrWhiteSpace(generator))
        {
            return new RazorSources(
                ImmutableArray<SyntaxTree>.Empty,
                files,
                $"Razor's source generator is not known to the build, '{GeneratorOption}' is not set");
        }

        options.GlobalOptions.TryGetValue(ProjectDirectoryOption, out var projectDirectory);

        var run = RazorGenerator.Run(
            generator,
            projectDirectory + "|" + context.Compilation.AssemblyName,
            context.Compilation,
            razorFiles,
            context.ParseOptions,
            options,
            context.CancellationToken);

        return new RazorSources(run.SyntaxTrees, files, run.Problem);
    }

    /// <summary>The compilation with Razor's C# in it, to bind its call sites in.</summary>
    public Compilation AddTo(Compilation compilation)
    {
        return SyntaxTrees.IsEmpty ? compilation : compilation.AddSyntaxTrees(SyntaxTrees);
    }

    /// <summary>
    /// <paramref name="location"/> in the Razor file the C# at it was written in. A location in one of
    /// Razor's trees can not be reported as it is - the trees are no part of the compilation being
    /// built, and reporting a diagnostic located in one throws - so it is turned into a location in
    /// the Razor file, which Razor's <c>#line</c> directives say. C# Razor wrote by itself maps to no
    /// file and is reported without a location.
    /// </summary>
    public Location Map(Location location)
    {
        if (location.SourceTree == null || _trees.Contains(location.SourceTree) == false)
        {
            return location;
        }

        var mapped = location.GetMappedLineSpan();

        if (mapped.HasMappedPath == false)
        {
            return Location.None;
        }

        var span = _files.TryGetValue(mapped.Path, out var file) &&
                   file.GetText() is { } text &&
                   mapped.EndLinePosition.Line < text.Lines.Count
            ? text.Lines.GetTextSpan(mapped.Span)
            : default(TextSpan);

        return Location.Create(mapped.Path, span, mapped.Span);
    }

    private static bool IsInjected(
        GeneratorExecutionContext context,
        ImmutableArray<AdditionalText> razorFiles,
        InvocationsSyntaxReceiver receiver,
        INamedTypeSymbol convertingService)
    {
        if (razorFiles.Any(f => f.GetText(context.CancellationToken)?.ToString().Contains(convertingService.Name) == true))
        {
            return true;
        }

        if (context.Compilation.GetTypeByMetadataName(InjectAttribute) is not { } injectAttribute)
        {
            return false;
        }

        foreach (var property in receiver.AttributedProperties)
        {
            var model = context.Compilation.GetSemanticModel(property.SyntaxTree);

            if (model.GetDeclaredSymbol(property, context.CancellationToken) is { } symbol &&
                SymbolEqualityComparer.Default.Equals(symbol.Type, convertingService) &&
                symbol.GetAttributes().Any(a => SymbolEqualityComparer.Default.Equals(a.AttributeClass, injectAttribute)))
            {
                return true;
            }
        }

        return false;
    }

    private static bool IsRazor(AdditionalText file)
    {
        return file.Path.EndsWith(".razor", StringComparison.OrdinalIgnoreCase) ||
               file.Path.EndsWith(".cshtml", StringComparison.OrdinalIgnoreCase);
    }
}
