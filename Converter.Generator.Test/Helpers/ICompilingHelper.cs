using System.Collections.Generic;
using System.Reflection;
using Microsoft.CodeAnalysis;

namespace Majipro.Converter.Generator.Test.Helpers;

public interface ICompilingHelper
{
    ICompilingHelper AddSourceCode(string filePath);

    ICompilingHelper AddAssemblyFromType<TType>();

    ICompilingHelper AddAssembly(Assembly assembly);

    ICompilingHelper Compile();

    IReadOnlyList<Diagnostic> GetDiagnosticOutput();

    /// <summary>Source code produced by the generator, one item per generated file.</summary>
    IReadOnlyList<string> GetGeneratedSources();

    Compilation GetCompilationOutput();
}
