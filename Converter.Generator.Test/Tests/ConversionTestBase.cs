using System;
using System.Collections.Generic;
using System.Linq;
using Majipro.Converter.Generator.Test.Extensions;
using Majipro.Converter.Generator.Test.Helpers;
using Microsoft.CodeAnalysis;
using Microsoft.Extensions.DependencyInjection;

namespace Majipro.Converter.Generator.Test.Tests;

/// <summary>
/// Base class for generator test cases. It compiles the test case source files together with the
/// generator, builds a real dependency injection container out of the result and lets the test
/// resolve the generated converters from it.
/// </summary>
/// <remarks>
/// A test case consists of three files:
/// <list type="number">
/// <item>
/// <c>&lt;Name&gt;TestCase.cs</c> with the types being converted. It is compiled into this test
/// assembly only and referenced by the compiled test case, so the test and the generated converter
/// talk about the very same types.
/// </item>
/// <item>
/// <c>&lt;Name&gt;Composition.cs</c> deriving from <see cref="TestCompositionBase"/> with the
/// <c>IConvertingService.Convert&lt;TFrom, TTo&gt;</c> call sites the generator reacts on. It is
/// compiled by this test assembly and, because of the <c>Tests\**\*Composition*.cs</c> glob in the
/// csproj, copied to the output directory and fed to the generator as source code.
/// </item>
/// <item>
/// <c>&lt;Name&gt;Test.cs</c>, the test class itself, deriving from this class.
/// </item>
/// </list>
/// </remarks>
/// <typeparam name="TComposition">Composition of the test case.</typeparam>
public abstract class ConversionTestBase<TComposition>
{
    private const string CompositionBaseFile = @".\Tests\TestCompositionBase.cs";

    /// <summary>Diagnostics reported by the generator.</summary>
    public IReadOnlyList<Diagnostic> Diagnostic { get; private set; }

    /// <summary>Source code the generator produced, one item per generated file.</summary>
    public IReadOnlyList<string> GeneratedSources { get; private set; }

    private ICompilingHelper _compilingHelper;
    private ServiceProvider _serviceProvider;

    private readonly List<string> _files = new List<string>();

    protected ConversionTestBase(params string[] files)
    {
        _files.Add(CompositionBaseFile.ToPath());
        _files.AddRange(files.Select(f => f.ToPath()));
    }

    [TestInitialize]
    public void SetUpTest()
    {
        _compilingHelper = CompilingHelper
            .Create()
            .AddAssembly(GetType().Assembly);

        foreach (var file in _files)
        {
            _compilingHelper = _compilingHelper.AddSourceCode(file);
        }

        _compilingHelper = _compilingHelper.Compile();

        Diagnostic = _compilingHelper.GetDiagnosticOutput();
        GeneratedSources = _compilingHelper.GetGeneratedSources();
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
    protected ServiceProvider ServiceProvider => _serviceProvider ??= _compilingHelper
        .GetCompilationOutput()
        .Run<ServiceProvider, TComposition>();

    protected IConverter<TFrom, TTo> GetConverter<TFrom, TTo>()
    {
        return GetService<IConverter<TFrom, TTo>>();
    }

    protected IConvertingService GetConvertingService()
    {
        return GetService<IConvertingService>();
    }

    protected TService GetService<TService>()
    {
        return ServiceProvider.GetRequiredService<TService>();
    }

    protected void AssertGeneratedWithoutDiagnostics()
    {
        Assert.AreEqual(
            0,
            Diagnostic.Count,
            "Generator reported: " + string.Join(Environment.NewLine, Diagnostic));
    }
}
