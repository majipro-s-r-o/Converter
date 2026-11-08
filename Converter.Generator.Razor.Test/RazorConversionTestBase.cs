using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Majipro.Converter.Generator.Razor;
using Majipro.Converter.Generator.Test;
using Majipro.Converter.Generator.Test.Helpers;
using Microsoft.AspNetCore.Components;
using Microsoft.CodeAnalysis;

namespace Majipro.Converter.Generator.Razor.Test;

/// <summary>
/// A test case that is a Razor project: compiled against what a Blazor or MVC project compiles
/// against, with Razor's source generator of the SDK running next to the generator under test, the
/// way it does in the build, and the generator under test told where it is, the way the package's
/// <c>buildTransitive/Majipro.Converter.targets</c> tells it.
/// </summary>
/// <remarks>
/// Razor's generator writes the real classes of the Razor files into the compilation that is emitted
/// - which is what lets a case compile and run at all, with its code behind files and components -
/// and the generator under test never sees them. What it finds, it found by running Razor's
/// generator itself.
/// </remarks>
public abstract class RazorConversionTestBase<TComposition> : ConversionTestBase<TComposition>
{
    /// <summary>The assemblies of Razor's source generator in the SDK that built this suite, separated by <c>|</c>.</summary>
    internal static readonly string RazorGenerator = typeof(RazorConversionTestBase<>).Assembly
        .GetCustomAttributes<AssemblyMetadataAttribute>()
        .Single(a => a.Key == nameof(RazorGenerator))
        .Value;

    private static readonly Lazy<IReadOnlyList<string>> References = new Lazy<IReadOnlyList<string>>(GetReferences);

    protected RazorConversionTestBase(params string[] files)
        : base(files)
    {
    }

    protected override ICompilingHelper Configure(ICompilingHelper compilingHelper)
    {
        compilingHelper = compilingHelper
            .AddGlobalOption(RazorSources.GeneratorOption, RazorGenerator)
            .AddGlobalOption("build_property.RazorLangVersion", "latest");

        foreach (var reference in References.Value)
        {
            compilingHelper = compilingHelper.AddReference(reference);
        }

        foreach (var generator in GetRazorGenerators())
        {
            compilingHelper = compilingHelper.AddSiblingGenerator(generator);
        }

        return compilingHelper;
    }

    /// <summary>A fresh instance of Razor's generator, the one the build runs next to the converter generator.</summary>
    private static IEnumerable<ISourceGenerator> GetRazorGenerators()
    {
        return RazorGenerator
            .Split('|')
            .Select(Assembly.LoadFrom)
            .SelectMany(GetTypes)
            .Where(t => t is { IsAbstract: false, IsClass: true } && t.GetCustomAttributes<GeneratorAttribute>().Any())
            .Select(Activator.CreateInstance)
            .Select(g => g as ISourceGenerator ?? ((IIncrementalGenerator)g).AsSourceGenerator());
    }

    private static IEnumerable<Type> GetTypes(Assembly assembly)
    {
        try
        {
            return assembly.GetTypes();
        }
        catch (ReflectionTypeLoadException e)
        {
            return e.Types.Where(t => t != null);
        }
    }

    /// <summary>Every assembly of the runtime and of ASP.NET Core, what a web project references.</summary>
    private static IReadOnlyList<string> GetReferences()
    {
        return new[] { typeof(object), typeof(ComponentBase) }
            .Select(t => Path.GetDirectoryName(t.Assembly.Location))
            .SelectMany(d => Directory.GetFiles(d, "*.dll"))
            .Where(IsAssembly)
            .ToList();
    }

    private static bool IsAssembly(string path)
    {
        try
        {
            AssemblyName.GetAssemblyName(path);
            return true;
        }
        catch (BadImageFormatException)
        {
            return false;
        }
    }
}
