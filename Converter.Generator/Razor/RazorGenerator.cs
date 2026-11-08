using System.Collections.Concurrent;
using System.Collections.Immutable;
using System.Reflection;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Majipro.Converter.Generator.Razor;

/// <summary>
/// Razor's own source generator, run by this one. Generators do not see each other's output - every
/// one of them is handed the same compilation, in no particular order - so the only way to the C#
/// Razor makes of a <c>.razor</c> or <c>.cshtml</c> file is to make it once more: load the generator
/// the build runs, hand it the very inputs the build hands it, and read what it writes.
/// </summary>
/// <remarks>
/// <para>
/// The generator is not a package anything could depend on; it ships with the SDK and is built
/// against the compiler of the same SDK, which is the compiler loading this assembly too. Where it is
/// comes from the build (<c>buildTransitive/Majipro.Converter.targets</c>), as a list of assemblies whose
/// <see cref="GeneratorAttribute"/> types are the generators - no name of a Razor type is known here.
/// </para>
/// <para>
/// Razor's generator is incremental, and incremental is something a driver is, not a generator: the
/// work it saves is the work the same driver did the run before. So the driver of a project is kept
/// and handed the new compilation, which in an IDE, where this runs on every change, is what makes it
/// cheap. A handful of projects at most are kept, the driver holding on to the last compilation it was
/// run with.
/// </para>
/// </remarks>
internal static class RazorGenerator
{
    private const int MaxDrivers = 16;

    private static readonly ConcurrentDictionary<string, Lazy<Loaded>> LoadedGenerators =
        new ConcurrentDictionary<string, Lazy<Loaded>>(StringComparer.Ordinal);

    private static readonly ConcurrentDictionary<string, KeptDriver> Drivers =
        new ConcurrentDictionary<string, KeptDriver>(StringComparer.Ordinal);

    private static long _runs;

    /// <summary>
    /// Runs the generators of <paramref name="assemblies"/> over <paramref name="compilation"/>.
    /// </summary>
    /// <param name="assemblies">
    /// Paths of the assemblies the generators are in, separated by <c>|</c> - the build hands them over
    /// in an .editorconfig file, where <c>;</c> starts a comment.
    /// </param>
    /// <param name="project">What tells one project from another, so each keeps a driver of its own.</param>
    public static RazorRun Run(
        string assemblies,
        string project,
        Compilation compilation,
        ImmutableArray<AdditionalText> additionalTexts,
        ParseOptions parseOptions,
        AnalyzerConfigOptionsProvider options,
        CancellationToken cancellationToken)
    {
        var loaded = LoadedGenerators
            .GetOrAdd(assemblies, paths => new Lazy<Loaded>(() => Load(paths)))
            .Value;

        if (loaded.Problem != null)
        {
            return RazorRun.Failed(loaded.Problem);
        }

        var key = assemblies + "|" + project;
        var csharpParseOptions = parseOptions as CSharpParseOptions;

        var driver = Drivers.TryGetValue(key, out var kept)
            ? kept.Driver
                .ReplaceAdditionalTexts(additionalTexts)
                .WithUpdatedParseOptions(parseOptions)
                .WithUpdatedAnalyzerConfigOptions(options)
            : CSharpGeneratorDriver.Create(loaded.Generators, additionalTexts, csharpParseOptions, options);

        driver = driver.RunGenerators(compilation, cancellationToken);
        Keep(key, driver);

        var result = driver.GetRunResult();
        var failed = result.Results.FirstOrDefault(r => r.Exception != null);

        return failed.Exception != null
            ? RazorRun.Failed($"{failed.Generator.GetType().FullName} threw {failed.Exception.GetType().Name}: {failed.Exception.Message}")
            : RazorRun.Succeeded(result.GeneratedTrees);
    }

    private static void Keep(string key, GeneratorDriver driver)
    {
        Drivers[key] = new KeptDriver(driver, Interlocked.Increment(ref _runs));

        while (Drivers.Count > MaxDrivers)
        {
            var oldest = Drivers.OrderBy(d => d.Value.Run).First();
            Drivers.TryRemove(oldest.Key, out _);
        }
    }

    private static Loaded Load(string assemblies)
    {
        var generators = new List<ISourceGenerator>();

        foreach (var path in assemblies.Split('|').Select(p => p.Trim()).Where(p => p.Length > 0))
        {
            Assembly assembly;

            try
            {
                assembly = Assembly.LoadFrom(path);
            }
            catch (Exception e)
            {
                return new Loaded($"'{path}' could not be loaded, {e.GetType().Name}: {e.Message}");
            }

            foreach (var type in GetTypes(assembly).Where(IsCSharpGenerator))
            {
                try
                {
                    switch (Activator.CreateInstance(type))
                    {
                        case IIncrementalGenerator incremental:
                            generators.Add(incremental.AsSourceGenerator());
                            break;
                        case ISourceGenerator generator:
                            generators.Add(generator);
                            break;
                    }
                }
                catch (Exception e)
                {
                    return new Loaded($"'{type.FullName}' could not be created, {e.GetType().Name}: {e.Message}");
                }
            }
        }

        return generators.Count == 0
            ? new Loaded($"there is no source generator in '{assemblies}'")
            : new Loaded(generators);
    }

    /// <summary>
    /// The types of an assembly that could be loaded. A generator assembly sits next to assemblies
    /// that are no generators, and a type whose dependency is missing is not one this class needs.
    /// </summary>
    private static IEnumerable<Type> GetTypes(Assembly assembly)
    {
        try
        {
            return assembly.GetTypes();
        }
        catch (ReflectionTypeLoadException e)
        {
            return e.Types.Where(t => t != null)!;
        }
    }

    private static bool IsCSharpGenerator(Type type)
    {
        return type is { IsAbstract: false, IsClass: true } &&
               type.GetCustomAttributes(typeof(GeneratorAttribute), false)
                   .OfType<GeneratorAttribute>()
                   .Any(a => a.Languages.Contains(LanguageNames.CSharp));
    }

    private sealed class Loaded
    {
        public Loaded(IReadOnlyList<ISourceGenerator> generators)
        {
            Generators = generators;
        }

        public Loaded(string problem)
        {
            Generators = Array.Empty<ISourceGenerator>();
            Problem = problem;
        }

        public IReadOnlyList<ISourceGenerator> Generators { get; }

        public string? Problem { get; }
    }

    private readonly struct KeptDriver
    {
        public KeptDriver(GeneratorDriver driver, long run)
        {
            Driver = driver;
            Run = run;
        }

        public GeneratorDriver Driver { get; }

        public long Run { get; }
    }
}

/// <summary>What a run of Razor's generator came to: its syntax trees, or why there are none.</summary>
internal readonly struct RazorRun
{
    private RazorRun(ImmutableArray<SyntaxTree> syntaxTrees, string? problem)
    {
        SyntaxTrees = syntaxTrees;
        Problem = problem;
    }

    public ImmutableArray<SyntaxTree> SyntaxTrees { get; }

    public string? Problem { get; }

    public static RazorRun Succeeded(ImmutableArray<SyntaxTree> syntaxTrees) => new RazorRun(syntaxTrees, null);

    public static RazorRun Failed(string problem) => new RazorRun(ImmutableArray<SyntaxTree>.Empty, problem);
}
