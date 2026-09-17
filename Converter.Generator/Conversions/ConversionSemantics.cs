using System;
using System.Collections.Generic;
using System.Linq;
using Majipro.Converter.Abstrations;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Majipro.Converter.Generator.Conversions;

/// <summary>
/// What the compilation knows: the converter interfaces the generator looks for, and the questions
/// about types that only the compiler can answer. Every symbol is resolved by the metadata name of
/// the type the generator itself was compiled against, so there is no name kept in sync by hand.
/// </summary>
internal sealed class ConversionSemantics
{
    private readonly Compilation _compilation;

    /// <summary>The compilation again, typed, because only the C# one classifies conversions.</summary>
    private readonly CSharpCompilation? _cSharpCompilation;

    private readonly Dictionary<SyntaxTree, SemanticModel> _semanticModels =
        new Dictionary<SyntaxTree, SemanticModel>();

    private readonly INamedTypeSymbol? _enumerable;
    private readonly IReadOnlyList<INamedTypeSymbol> _listLikeTargets;

    public INamedTypeSymbol? ConvertingService { get; }

    public INamedTypeSymbol? ConverterInterface { get; }

    public INamedTypeSymbol? AsyncConverterInterface { get; }

    public INamedTypeSymbol? ReferenceConverterInterface { get; }

    public IAssemblySymbol Assembly => _compilation.Assembly;

    /// <summary>
    /// Majipro.Converter.Abstrations is referenced, so there is something to generate at all.
    /// </summary>
    public bool IsAvailable => ConvertingService != null && ConverterInterface != null;

    public ConversionSemantics(Compilation compilation)
    {
        _compilation = compilation;
        _cSharpCompilation = compilation as CSharpCompilation;

        ConvertingService = GetTypeSymbol(compilation, typeof(IConvertingService));
        ConverterInterface = GetTypeSymbol(compilation, typeof(IConverter<,>));
        AsyncConverterInterface = GetTypeSymbol(compilation, typeof(IAsyncConverter<,>));
        ReferenceConverterInterface = GetTypeSymbol(compilation, typeof(IReferenceConverter<,>));

        _enumerable = GetTypeSymbol(compilation, typeof(IEnumerable<>));

        // Collection properties are materialized as a List<T>, so every target type a List<T> fits into.
        _listLikeTargets = new[]
            {
                typeof(IEnumerable<>),
                typeof(ICollection<>),
                typeof(IList<>),
                typeof(IReadOnlyCollection<>),
                typeof(IReadOnlyList<>),
                typeof(List<>)
            }
            .Select(t => GetTypeSymbol(compilation, t))
            .Where(s => s != null)
            .Select(s => s!)
            .ToList();
    }

    /// <summary>
    /// The interface the generated class implements, closed over the pair:
    /// <c>IReferenceConverter&lt;TFrom, TTo&gt;</c> when the converter also fills a target the
    /// caller already holds, <c>IConverter&lt;TFrom, TTo&gt;</c> when it only creates one. The
    /// first one extends the second, so there is never a class implementing both by name - which is
    /// also what keeps DiCompositionValidator from seeing two registrations for one pair.
    /// </summary>
    public INamedTypeSymbol Converter(ConversionPair pair, bool reference)
    {
        var definition = reference && ReferenceConverterInterface != null
            ? ReferenceConverterInterface
            : ConverterInterface!;

        return definition.Construct(pair.From, pair.To);
    }

    /// <summary>
    /// Whether the compiler would take the source value for the target property without a cast.
    /// </summary>
    public bool IsAssignable(ITypeSymbol source, ITypeSymbol target)
    {
        return _cSharpCompilation == null
            ? SymbolEqualityComparer.Default.Equals(source, target)
            : _cSharpCompilation.ClassifyConversion(source, target).IsImplicit;
    }

    /// <summary>
    /// <c>T</c> of the <see cref="IEnumerable{T}"/> the type is. A <c>string</c> is an
    /// <see cref="IEnumerable{T}"/> of characters and is deliberately not treated as a collection.
    /// </summary>
    public ITypeSymbol? GetEnumeratedType(ITypeSymbol type)
    {
        if (type is IArrayTypeSymbol array)
        {
            return array.ElementType;
        }

        if (_enumerable == null || type.SpecialType == SpecialType.System_String)
        {
            return null;
        }

        if (type is INamedTypeSymbol named &&
            SymbolEqualityComparer.Default.Equals(named.OriginalDefinition, _enumerable))
        {
            return named.TypeArguments[0];
        }

        foreach (var iface in type.AllInterfaces)
        {
            if (SymbolEqualityComparer.Default.Equals(iface.OriginalDefinition, _enumerable))
            {
                return iface.TypeArguments[0];
            }
        }

        return null;
    }

    /// <summary>
    /// <c>T</c> of a property a freshly built <c>List&lt;T&gt;</c> can be assigned to.
    /// </summary>
    public ITypeSymbol? GetListItemType(ITypeSymbol type)
    {
        if (type is not INamedTypeSymbol named || named.TypeArguments.Length != 1)
        {
            return null;
        }

        return _listLikeTargets.Any(t => SymbolEqualityComparer.Default.Equals(named.OriginalDefinition, t))
            ? named.TypeArguments[0]
            : null;
    }

    public SemanticModel GetSemanticModel(SyntaxTree syntaxTree)
    {
        if (_semanticModels.TryGetValue(syntaxTree, out var semanticModel) == false)
        {
            semanticModel = _compilation.GetSemanticModel(syntaxTree);
            _semanticModels.Add(syntaxTree, semanticModel);
        }

        return semanticModel;
    }

    private static INamedTypeSymbol? GetTypeSymbol(Compilation compilation, Type type)
    {
        return type.FullName == null
            ? null
            : compilation.GetTypeByMetadataName(type.FullName);
    }
}
