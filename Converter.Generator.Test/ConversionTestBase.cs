using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Majipro.Converter.Abstractions;
using Majipro.Converter.Generator.Test.Extensions;
using Majipro.Converter.Generator.Test.Helpers;
using Microsoft.CodeAnalysis;
using Microsoft.Extensions.DependencyInjection;

namespace Majipro.Converter.Generator.Test;

/// <summary>
/// Base class for generator test cases. It compiles the test case source files together with the
/// generator, builds a real dependency injection container out of the result and lets the test
/// resolve the generated converters from it.
/// </summary>
/// <remarks>
/// Test cases live in one of two folders, and the folder is the question the case asks.
/// <c>Conversions\</c> holds the cases the generator is expected to write a converter for, and the
/// test resolves it and converts. <c>Validations\</c> holds the cases it is expected to refuse, and
/// the test asserts on <see cref="Diagnostic"/> and on <see cref="GeneratedSources"/> being empty -
/// it never touches <see cref="ServiceProvider"/>, because the compilation it describes is meant to
/// fail. Each refusal gets a compilation of its own, so the diagnostic asserted on is the
/// only one reported, which is why a validation case is a folder of its own rather than another composition next to a
/// conversion.
///
/// Either way a test case consists of three files:
/// <list type="number">
/// <item>
/// <c>&lt;Name&gt;TestCase.cs</c> with the types being converted. It is compiled into this test
/// assembly only and referenced by the compiled test case, so the test and the generated converter
/// talk about the very same types.
/// </item>
/// <item>
/// <c>&lt;Name&gt;Composition.cs</c> deriving from <see cref="TestCompositionBase"/> with the
/// <c>IConvertingService.Convert&lt;TFrom, TTo&gt;</c> call sites the generator reacts on. It is
/// compiled by this test assembly and, because of the <c>**\*Composition*.cs</c> glob in the
/// csproj, copied to the output directory and fed to the generator as source code.
/// </item>
/// <item>
/// <c>&lt;Name&gt;Test.cs</c>, the test class itself, deriving from this class.
/// </item>
/// </list>
/// A <c>.razor</c> or <c>.cshtml</c> file among the files is handed to the generator as an additional
/// file instead of being compiled, which is how the Razor SDK hands it over - with the
/// <c>TargetPath</c> the SDK gives it, relative to <see cref="ProjectDirectory"/>; such a case says
/// what project it is with <see cref="ProjectDirectory"/> and <see cref="RootNamespace"/>. Whatever
/// else a case needs of the compilation - references, the Razor generator itself running next to the
/// generator under test - it adds in <see cref="Configure"/>.
/// </remarks>
/// <typeparam name="TComposition">Composition of the test case.</typeparam>
public abstract class ConversionTestBase<TComposition>
{
    private const string CompositionBaseFile = @".\TestCompositionBase.cs";

    /// <summary>Diagnostics reported by the generator.</summary>
    public IReadOnlyList<Diagnostic> Diagnostic { get; private set; }

    /// <summary>Source code the generator produced, one item per generated file.</summary>
    public IReadOnlyList<string> GeneratedSources { get; private set; }

    private CompilingResult _compiled;
    private ServiceProvider _serviceProvider;

    private readonly List<string> _files = new List<string>();
    private readonly List<string> _additionalFiles = new List<string>();

    protected ConversionTestBase(params string[] files)
    {
        _files.Add(CompositionBaseFile.ToPath());

        foreach (var file in files.Select(f => f.ToPath()))
        {
            (IsRazor(file) ? _additionalFiles : _files).Add(file);
        }
    }

    /// <summary>
    /// The folder of the project the test case pretends to be, Windows style and relative to the test
    /// assembly like the files - <c>build_property.MSBuildProjectDirectory</c>, which the namespace of
    /// a Razor file is made of. Nothing unless a case says so.
    /// </summary>
    protected virtual string ProjectDirectory => null;

    /// <summary><c>build_property.RootNamespace</c> of the project the test case pretends to be.</summary>
    protected virtual string RootNamespace => null;

    /// <summary>Anything else the compilation of the case needs, added before it is compiled.</summary>
    protected virtual ICompilingHelper Configure(ICompilingHelper compilingHelper)
    {
        return compilingHelper;
    }

    [TestInitialize]
    public void SetUpTest()
    {
        var compilingHelper = CompilingHelper
            .Create()
            .AddReference(GetType().Assembly);

        foreach (var file in _files)
        {
            compilingHelper = compilingHelper.AddSourceCode(file);
        }

        foreach (var file in _additionalFiles)
        {
            compilingHelper = compilingHelper.AddAdditionalFile(file, GetFileMetadata(file));
        }

        if (ProjectDirectory != null)
        {
            compilingHelper = compilingHelper.AddGlobalOption("build_property.MSBuildProjectDirectory", ProjectDirectory.ToPath());
        }

        if (RootNamespace != null)
        {
            compilingHelper = compilingHelper.AddGlobalOption("build_property.RootNamespace", RootNamespace);
        }

        _compiled = Configure(compilingHelper).Compile();

        Diagnostic = _compiled.Diagnostics;
        GeneratedSources = _compiled.GeneratedSources;
    }

    [TestCleanup]
    public void TearDownTest()
    {
        _serviceProvider?.Dispose();
        _serviceProvider = null;
    }

    /// <summary>
    /// Container built by <see cref="TComposition"/> out of the compiled test case.
    /// </summary>
    protected ServiceProvider ServiceProvider => _serviceProvider ??= _compiled.Compilation
        .Run<ServiceProvider, TComposition>();

    protected IConverter<TFrom, TTo> GetConverter<TFrom, TTo>()
    {
        return GetService<IConverter<TFrom, TTo>>();
    }

    protected IReferenceConverter<TFrom, TTo> GetReferenceConverter<TFrom, TTo>()
    {
        return GetService<IReferenceConverter<TFrom, TTo>>();
    }

    /// <summary>
    /// The reference converter for the pair, or <c>null</c> when there is none. Asking the container
    /// is the only way to ask, which is the point: this is exactly what a consumer of the generated
    /// code sees.
    /// </summary>
    protected IReferenceConverter<TFrom, TTo> FindReferenceConverter<TFrom, TTo>()
    {
        return ServiceProvider.GetService<IReferenceConverter<TFrom, TTo>>();
    }

    protected IConvertingService GetConvertingService()
    {
        return GetService<IConvertingService>();
    }

    protected TService GetService<TService>()
    {
        return ServiceProvider.GetRequiredService<TService>();
    }

    /// <summary>
    /// The item metadata the Razor SDK gives a Razor file: its path relative to the project, base 64
    /// encoded - which is what Razor's generator names the class and the namespace after, and it
    /// compiles no file without it.
    /// </summary>
    private IReadOnlyDictionary<string, string> GetFileMetadata(string file)
    {
        if (ProjectDirectory == null)
        {
            return null;
        }

        var targetPath = Path.GetRelativePath(ProjectDirectory.ToPath(), file);

        return new Dictionary<string, string>
        {
            ["build_metadata.AdditionalFiles.TargetPath"] = Convert.ToBase64String(Encoding.UTF8.GetBytes(targetPath))
        };
    }

    private static bool IsRazor(string file)
    {
        return file.EndsWith(".razor", StringComparison.OrdinalIgnoreCase) ||
               file.EndsWith(".cshtml", StringComparison.OrdinalIgnoreCase);
    }

    protected void AssertGeneratedWithoutDiagnostics()
    {
        Assert.AreEqual(
            0,
            Diagnostic.Count,
            "Generator reported: " + string.Join(Environment.NewLine, Diagnostic));
    }
}
