using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Majipro.Converter;
using Majipro.Converter.Abstrations;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.Extensions.DependencyInjection;

namespace Majipro.Converter.Generator.Test.Helpers;

public class CompilingHelper : ICompilingHelper
{
    private Compilation _compilationOutput = null;
    private IReadOnlyList<Diagnostic> _diagnosticOutput = null;
    private IReadOnlyList<string> _generatedSources = null;

    private readonly List<string> _sourceCode = new List<string>();
    private readonly List<Assembly> _assemblies;

    private CompilingHelper()
    {
        _assemblies = AppDomain.CurrentDomain
            .GetAssemblies()
            .Where(assembly => !assembly.IsDynamic && !string.IsNullOrWhiteSpace(assembly.Location))
            .ToList();

        AddAssemblyFromType<IServiceCollection>();
        AddAssemblyFromType<IConvertingService>();
        // Abstrations and Converter are two assemblies now, the compiled test case needs both.
        AddAssembly(typeof(DiCompositor).Assembly);
        AddAssemblyFromType<ServiceProvider>();
        AddAssemblyFromType<Exception>();
    }

    public static ICompilingHelper Create()
    {
        return new CompilingHelper();
    }

    public ICompilingHelper AddSourceCode(string filePath)
    {
        if (File.Exists(filePath) == false)
        {
            throw new FileNotFoundException($"Unable to add source code because target file does not exists! Check if path '{filePath}' is valid.");
        }

        string fileContent = File.ReadAllText(filePath);
        _sourceCode.Add(fileContent);

        return this;
    }

    public ICompilingHelper AddAssemblyFromType<TType>()
    {
        return AddAssembly(typeof(TType).Assembly);
    }

    public ICompilingHelper AddAssembly(Assembly assembly)
    {
        if (assembly.IsDynamic || string.IsNullOrWhiteSpace(assembly.Location))
        {
            return this;
        }

        if (_assemblies.Contains(assembly) == false)
        {
            _assemblies.Add(assembly);
        }

        return this;
    }

    public ICompilingHelper Compile()
    {
        if (_sourceCode.Count <= 0)
        {
            throw new AssertInconclusiveException($"There is no source code! Call '{nameof(AddSourceCode)}' first!");
        }

        var syntaxTree = _sourceCode
            .Select(s => CSharpSyntaxTree.ParseText(s))
            .ToArray();

        var references = _assemblies
            .Select(assembly => MetadataReference.CreateFromFile(assembly.Location))
            .Cast<MetadataReference>();

        var compilation = CSharpCompilation.Create(
            "Converter.Generator.Test",
            syntaxTree,
            references,
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

        // Source Generator to test 
        var generator = new ConverterGenerator();

        CSharpGeneratorDriver.Create(generator)
                             .RunGeneratorsAndUpdateCompilation(compilation, out var compilationOutput, out var diagnosticOutput);

        _compilationOutput = compilationOutput;
        _diagnosticOutput = diagnosticOutput;
        _generatedSources = compilationOutput.SyntaxTrees
            .Except(compilation.SyntaxTrees)
            .Select(t => t.ToString())
            .ToList();

        return this;
    }

    public IReadOnlyList<Diagnostic> GetDiagnosticOutput()
    {
        return _diagnosticOutput;
    }

    public IReadOnlyList<string> GetGeneratedSources()
    {
        return _generatedSources;
    }

    public Compilation GetCompilationOutput()
    {
        return _compilationOutput;
    }
}
