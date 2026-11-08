using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using Majipro.Converter.Abstrations;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;
using Microsoft.Extensions.DependencyInjection;

namespace Majipro.Converter.Generator.Test.Helpers;

public sealed class CompilingHelper : ICompilingHelper
{
    private const string AssemblyName = "Converter.Generator.Test";

    private readonly List<SyntaxTree> _syntaxTrees = new List<SyntaxTree>();
    private readonly List<AdditionalText> _additionalFiles = new List<AdditionalText>();
    private readonly Dictionary<string, string> _globalOptions = new Dictionary<string, string>();
    private readonly HashSet<string> _references = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
    private readonly List<ISourceGenerator> _siblingGenerators = new List<ISourceGenerator>();

    /// <summary>
    /// Everything this process has loaded, so a test case can use whatever its test does, and the
    /// assemblies every compiled test case needs, which may not have been loaded yet.
    /// </summary>
    private CompilingHelper()
    {
        foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
        {
            AddReference(assembly);
        }

        AddReference(typeof(object).Assembly);
        AddReference(typeof(IServiceCollection).Assembly);
        AddReference(typeof(ServiceProvider).Assembly);
        AddReference(typeof(IConvertingService).Assembly);
        AddReference(typeof(DiCompositor).Assembly);
    }

    public static ICompilingHelper Create()
    {
        return new CompilingHelper();
    }

    public ICompilingHelper AddSourceCode(string filePath)
    {
        _syntaxTrees.Add(CSharpSyntaxTree.ParseText(ReadFile(filePath), path: filePath));

        return this;
    }

    public ICompilingHelper AddAdditionalFile(string filePath, IReadOnlyDictionary<string, string> metadata = null)
    {
        _additionalFiles.Add(new FileText(filePath, ReadFile(filePath), metadata ?? new Dictionary<string, string>()));

        return this;
    }

    public ICompilingHelper AddGlobalOption(string key, string value)
    {
        _globalOptions[key] = value;

        return this;
    }

    public ICompilingHelper AddReference(Assembly assembly)
    {
        return assembly.IsDynamic || string.IsNullOrWhiteSpace(assembly.Location)
            ? this
            : AddReference(assembly.Location);
    }

    public ICompilingHelper AddReference(string filePath)
    {
        _references.Add(filePath);

        return this;
    }

    public ICompilingHelper AddSiblingGenerator(ISourceGenerator generator)
    {
        _siblingGenerators.Add(generator);

        return this;
    }

    public CompilingResult Compile()
    {
        if (_syntaxTrees.Count == 0)
        {
            throw new AssertInconclusiveException($"There is no source code! Call '{nameof(AddSourceCode)}' first!");
        }

        var compilation = CSharpCompilation.Create(
            AssemblyName,
            _syntaxTrees,
            _references.Select(path => MetadataReference.CreateFromFile(path)),
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

        var generator = new ConverterGenerator();

        var driver = CSharpGeneratorDriver
            .Create(
                new ISourceGenerator[] { generator }.Concat(_siblingGenerators),
                _additionalFiles,
                optionsProvider: new OptionsProvider(_globalOptions))
            .RunGeneratorsAndUpdateCompilation(compilation, out var output, out var diagnostics);

        var generatedSources = driver
            .GetRunResult()
            .Results
            .Single(r => r.Generator == generator)
            .GeneratedSources
            .Select(s => s.SyntaxTree.ToString())
            .ToList();

        return new CompilingResult(output, diagnostics, generatedSources);
    }

    private static string ReadFile(string filePath)
    {
        if (File.Exists(filePath) == false)
        {
            throw new FileNotFoundException($"There is no file '{filePath}', check that the path is valid and the file is copied to the output directory.", filePath);
        }

        return File.ReadAllText(filePath);
    }

    /// <summary>A file handed to the generator the way MSBuild hands it an <c>AdditionalFiles</c> item.</summary>
    private sealed class FileText : AdditionalText
    {
        private readonly SourceText _text;

        public FileText(string path, string text, IReadOnlyDictionary<string, string> metadata)
        {
            Path = path;
            Metadata = metadata;
            _text = SourceText.From(text);
        }

        public override string Path { get; }

        /// <summary>Its <c>build_metadata.AdditionalFiles.*</c> values, the item metadata a project makes visible to the compiler.</summary>
        public IReadOnlyDictionary<string, string> Metadata { get; }

        public override SourceText GetText(CancellationToken cancellationToken = default)
        {
            return _text;
        }
    }

    /// <summary>The <c>build_property.*</c> values a project makes visible to the compiler, and the metadata of its additional files.</summary>
    private sealed class OptionsProvider : AnalyzerConfigOptionsProvider
    {
        private static readonly AnalyzerConfigOptions Empty = new Options(new Dictionary<string, string>());

        public OptionsProvider(IReadOnlyDictionary<string, string> globalOptions)
        {
            GlobalOptions = new Options(globalOptions);
        }

        public override AnalyzerConfigOptions GlobalOptions { get; }

        public override AnalyzerConfigOptions GetOptions(SyntaxTree tree) => Empty;

        public override AnalyzerConfigOptions GetOptions(AdditionalText textFile) =>
            textFile is FileText file ? new Options(file.Metadata) : Empty;

        private sealed class Options : AnalyzerConfigOptions
        {
            private readonly IReadOnlyDictionary<string, string> _values;

            public Options(IReadOnlyDictionary<string, string> values)
            {
                _values = values;
            }

            public override bool TryGetValue(string key, out string value)
            {
                return _values.TryGetValue(key, out value);
            }
        }
    }
}
