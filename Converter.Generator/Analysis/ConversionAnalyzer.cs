using System;
using System.Collections.Generic;
using System.Linq;
using Majipro.Converter.Abstrations;
using Majipro.Converter.Generator.Extensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Majipro.Converter.Generator.Analysis;

/// <summary>
/// Turns <c>IConvertingService.Convert&lt;TFrom, TTo&gt;(...)</c> call sites into a list of
/// converters that have to be generated. A property that needs a converter of its own puts that
/// pair back into the queue, so one call site can produce a whole tree of converters.
/// </summary>
internal sealed class ConversionAnalyzer
{
    private readonly Compilation _compilation;

    /// <summary>The compilation again, typed, because only the C# one classifies conversions.</summary>
    private readonly CSharpCompilation? _cSharpCompilation;

    private readonly Dictionary<SyntaxTree, SemanticModel> _semanticModels = new Dictionary<SyntaxTree, SemanticModel>();

    private readonly INamedTypeSymbol? _convertingService;
    private readonly INamedTypeSymbol? _converter;
    private readonly INamedTypeSymbol? _asyncConverter;
    private readonly INamedTypeSymbol? _enumerable;
    private readonly IReadOnlyList<INamedTypeSymbol> _listLikeTargets;

    public ConversionAnalyzer(Compilation compilation)
    {
        _compilation = compilation;
        _cSharpCompilation = compilation as CSharpCompilation;

        _convertingService = GetTypeSymbol(compilation, typeof(IConvertingService));
        _converter = GetTypeSymbol(compilation, typeof(IConverter<,>));
        _asyncConverter = GetTypeSymbol(compilation, typeof(IAsyncConverter<,>));
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

    public IReadOnlyList<ConversionInfo> Analyze(IReadOnlyList<InvocationExpressionSyntax> convertCalls)
    {
        var result = new List<ConversionInfo>();

        if (_convertingService == null || _converter == null)
        {
            // Majipro.Converter.Abstrations is not referenced, there is nothing to generate.
            return result;
        }

        // Conversions somebody already implemented by hand win, generating them again would make
        // DiCompositionValidator throw about two implementations of the same From -> To pair.
        var handled = GetAlreadyImplementedConversions();
        var pending = new Queue<ConversionRequest>();

        foreach (var convertCall in convertCalls)
        {
            var requested = GetRequestedConversion(convertCall);

            if (requested != null)
            {
                pending.Enqueue(requested);
            }
        }

        while (pending.Count > 0)
        {
            var request = pending.Dequeue();

            // Marking the pair before it is built also stops a type that contains itself.
            if (handled.Add(request.Key) == false)
            {
                continue;
            }

            var conversion = GetConversion(request, pending, _converter);

            if (conversion != null)
            {
                result.Add(conversion);
            }
        }

        return result;
    }

    private ConversionRequest? GetRequestedConversion(InvocationExpressionSyntax convertCall)
    {
        if (GetSemanticModel(convertCall.SyntaxTree).GetSymbolInfo(convertCall).Symbol is not IMethodSymbol method)
        {
            return null;
        }

        if (method.ContainingType == null ||
            SymbolEqualityComparer.Default.Equals(method.ContainingType.OriginalDefinition, _convertingService) == false)
        {
            return null;
        }

        if (method.TypeArguments.Length != 2)
        {
            return null;
        }

        return new ConversionRequest(method.TypeArguments[0], method.TypeArguments[1]);
    }

    private ConversionInfo? GetConversion(
        ConversionRequest request,
        Queue<ConversionRequest> pending,
        INamedTypeSymbol converter)
    {
        var from = request.From;
        var to = request.To;

        // Same type conversions are handled by ConvertingService itself.
        if (SymbolEqualityComparer.Default.Equals(from, to))
        {
            return null;
        }

        var fromValue = from.GetUnderlyingType();
        var toValue = to.GetUnderlyingType();

        if (fromValue.IsVisibleToGeneratedCode() == false)
        {
            return null;
        }

        // A string can not be built by an object initializer, but anything can be written as one.
        if (to.SpecialType == SpecialType.System_String)
        {
            return ConversionInfo.ToStringCall(from, to, converter.Construct(from, to));
        }

        if (fromValue.IsMappableSource() == false || toValue.IsMappableTarget() == false)
        {
            return null;
        }

        var properties = GetPropertyMappings(fromValue, toValue, pending);

        return ConversionInfo.FromProperties(from, to, converter.Construct(from, to), properties);
    }

    /// <summary>
    /// The whole intelligence of the generator: same name, and a value that either fits the target
    /// property as it is, or is something the converting service can convert on its own.
    /// </summary>
    private IReadOnlyList<PropertyMapping> GetPropertyMappings(
        ITypeSymbol from,
        ITypeSymbol to,
        Queue<ConversionRequest> pending)
    {
        var sources = from.GetInstanceProperties()
            .Where(p => p.IsReadable())
            .ToDictionary(p => p.Name);

        var result = new List<PropertyMapping>();

        foreach (var target in to.GetInstanceProperties().Where(p => p.IsWritable()))
        {
            if (sources.TryGetValue(target.Name, out var source) == false)
            {
                continue;
            }

            var mapping = GetPropertyMapping(source, target, pending);

            if (mapping != null)
            {
                result.Add(mapping);
            }
        }

        return result;
    }

    private PropertyMapping? GetPropertyMapping(
        IPropertySymbol source,
        IPropertySymbol target,
        Queue<ConversionRequest> pending)
    {
        if (IsAssignable(source.Type, target.Type))
        {
            return PropertyMapping.Direct(source, target);
        }

        if (source.Type.GetUnderlyingType().IsMappableSource() &&
            target.Type.GetUnderlyingType().IsMappableTarget())
        {
            pending.Enqueue(new ConversionRequest(source.Type, target.Type));

            return PropertyMapping.Converted(source, target);
        }

        return GetCollectionPropertyMapping(source, target, pending);
    }

    private PropertyMapping? GetCollectionPropertyMapping(
        IPropertySymbol source,
        IPropertySymbol target,
        Queue<ConversionRequest> pending)
    {
        var sourceItem = GetEnumeratedType(source.Type);
        var targetItem = GetListItemType(target.Type);

        if (sourceItem == null || targetItem == null)
        {
            return null;
        }

        // Items go through the converting service one by one, and that only knows an item pair that
        // is either the same type or has a converter. Being merely assignable, the way int is to
        // long, is not enough here - it would compile and then throw about a missing conversion.
        if (SymbolEqualityComparer.Default.Equals(sourceItem, targetItem) == false)
        {
            if (sourceItem.GetUnderlyingType().IsMappableSource() == false ||
                targetItem.GetUnderlyingType().IsMappableTarget() == false)
            {
                return null;
            }

            pending.Enqueue(new ConversionRequest(sourceItem, targetItem));
        }

        return PropertyMapping.Collection(source, target, sourceItem, targetItem);
    }

    /// <summary>
    /// Whether the compiler would take the source value for the target property without a cast.
    /// </summary>
    private bool IsAssignable(ITypeSymbol source, ITypeSymbol target)
    {
        return _cSharpCompilation == null
            ? SymbolEqualityComparer.Default.Equals(source, target)
            : _cSharpCompilation.ClassifyConversion(source, target).IsImplicit;
    }

    /// <summary>
    /// <c>T</c> of the <see cref="IEnumerable{T}"/> the type is. A <c>string</c> is an
    /// <see cref="IEnumerable{T}"/> of characters and is deliberately not treated as a collection.
    /// </summary>
    private ITypeSymbol? GetEnumeratedType(ITypeSymbol type)
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
    private ITypeSymbol? GetListItemType(ITypeSymbol type)
    {
        if (type is not INamedTypeSymbol named || named.TypeArguments.Length != 1)
        {
            return null;
        }

        return _listLikeTargets.Any(t => SymbolEqualityComparer.Default.Equals(named.OriginalDefinition, t))
            ? named.TypeArguments[0]
            : null;
    }

    private HashSet<string> GetAlreadyImplementedConversions()
    {
        var result = new HashSet<string>();

        foreach (var type in GetTypes(_compilation.Assembly.GlobalNamespace))
        {
            foreach (var iface in type.AllInterfaces)
            {
                var definition = iface.OriginalDefinition;

                if (SymbolEqualityComparer.Default.Equals(definition, _converter) == false &&
                    SymbolEqualityComparer.Default.Equals(definition, _asyncConverter) == false)
                {
                    continue;
                }

                if (iface.TypeArguments.Length != 2)
                {
                    continue;
                }

                result.Add(new ConversionRequest(iface.TypeArguments[0], iface.TypeArguments[1]).Key);
            }
        }

        return result;
    }

    private static IEnumerable<INamedTypeSymbol> GetTypes(INamespaceOrTypeSymbol namespaceOrType)
    {
        foreach (var member in namespaceOrType.GetMembers())
        {
            if (member is INamespaceOrTypeSymbol namespaceOrTypeMember)
            {
                if (member is INamedTypeSymbol type && type.TypeKind == TypeKind.Class)
                {
                    yield return type;
                }

                foreach (var nested in GetTypes(namespaceOrTypeMember))
                {
                    yield return nested;
                }
            }
        }
    }

    /// <summary>
    /// Looks the type up in the analyzed compilation by the metadata name of the type the generator
    /// itself was compiled against, so there is no name to keep in sync by hand.
    /// </summary>
    private static INamedTypeSymbol? GetTypeSymbol(Compilation compilation, Type type)
    {
        return type.FullName == null
            ? null
            : compilation.GetTypeByMetadataName(type.FullName);
    }

    private SemanticModel GetSemanticModel(SyntaxTree syntaxTree)
    {
        if (_semanticModels.TryGetValue(syntaxTree, out var semanticModel) == false)
        {
            semanticModel = _compilation.GetSemanticModel(syntaxTree);
            _semanticModels.Add(syntaxTree, semanticModel);
        }

        return semanticModel;
    }
}
