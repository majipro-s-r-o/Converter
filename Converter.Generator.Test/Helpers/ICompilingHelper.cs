using System.Collections.Generic;
using System.Reflection;
using Microsoft.CodeAnalysis;

namespace Majipro.Converter.Generator.Test.Helpers;

/// <summary>
/// The project a test case pretends to be, put together piece by piece and compiled with the
/// generator under test running over it, the way a build runs it.
/// </summary>
public interface ICompilingHelper
{
    /// <summary>A C# file the compilation is made of.</summary>
    ICompilingHelper AddSourceCode(string filePath);

    /// <summary>
    /// A file the generator gets as an <c>AdditionalFiles</c> item, the way the Razor SDK hands it <c>.razor</c> and <c>.cshtml</c>
    /// files, with the item metadata the project makes visible (<c>build_metadata.AdditionalFiles.TargetPath</c>, for one).
    /// </summary>
    ICompilingHelper AddAdditionalFile(string filePath, IReadOnlyDictionary<string, string> metadata = null);

    /// <summary>A <c>build_property.*</c> value, the way a <c>CompilerVisibleProperty</c> reaches the generator.</summary>
    ICompilingHelper AddGlobalOption(string key, string value);

    /// <summary>An assembly the compilation references.</summary>
    ICompilingHelper AddReference(Assembly assembly);

    /// <summary>An assembly the compilation references, by its path - one this process need not have loaded.</summary>
    ICompilingHelper AddReference(string filePath);

    /// <summary>
    /// Another generator of the same build - the Razor SDK's, say. What it writes is part of the compilation that is emitted,
    /// and, like every generator's output, never seen by the generator under test.
    /// </summary>
    ICompilingHelper AddSiblingGenerator(ISourceGenerator generator);

    /// <summary>Compiles what was added, running the generator under test and every sibling generator over it.</summary>
    CompilingResult Compile();
}

/// <summary>What compiling a test case came to.</summary>
public sealed class CompilingResult
{
    public CompilingResult(Compilation compilation, IReadOnlyList<Diagnostic> diagnostics, IReadOnlyList<string> generatedSources)
    {
        Compilation = compilation;
        Diagnostics = diagnostics;
        GeneratedSources = generatedSources;
    }

    /// <summary>The compilation with every generator's output in it, ready to be emitted.</summary>
    public Compilation Compilation { get; }

    /// <summary>Diagnostics reported by the generators.</summary>
    public IReadOnlyList<Diagnostic> Diagnostics { get; }

    /// <summary>Source code produced by the generator under test, one item per generated file - a sibling generator's is not counted.</summary>
    public IReadOnlyList<string> GeneratedSources { get; }
}
