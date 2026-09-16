using System;
using System.Collections.Generic;
using System.Linq;
using Majipro.Converter.Abstrations;
using Majipro.Converter.Generator.Extensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Majipro.Converter.Generator.Analysis;

/// <summary>
/// Turns <c>IConvertingService.Convert&lt;TFrom, TTo&gt;(...)</c> call sites into a list of
/// converters that have to be generated.
/// </summary>
internal sealed class ConversionAnalyzer
{
    private readonly Compilation _compilation;
    private readonly Dictionary<SyntaxTree, SemanticModel> _semanticModels = new Dictionary<SyntaxTree, SemanticModel>();

    private readonly INamedTypeSymbol? _convertingService;
    private readonly INamedTypeSymbol? _converter;
    private readonly INamedTypeSymbol? _asyncConverter;

    public ConversionAnalyzer(Compilation compilation)
    {
        _compilation = compilation;

        _convertingService = GetTypeSymbol(compilation, typeof(IConvertingService));
        _converter = GetTypeSymbol(compilation, typeof(IConverter<,>));
        _asyncConverter = GetTypeSymbol(compilation, typeof(IAsyncConverter<,>));
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

        foreach (var convertCall in convertCalls)
        {
            var conversion = GetConversion(convertCall, handled, _converter);

            if (conversion != null)
            {
                result.Add(conversion);
            }
        }

        return result;
    }

    private ConversionInfo? GetConversion(
        InvocationExpressionSyntax convertCall,
        HashSet<string> handled,
        INamedTypeSymbol converter)
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

        var from = method.TypeArguments[0];
        var to = method.TypeArguments[1];

        // Same type conversions are handled by ConvertingService itself.
        if (SymbolEqualityComparer.Default.Equals(from, to))
        {
            return null;
        }

        if (from.IsMappableSource() == false || to.IsMappableTarget() == false)
        {
            return null;
        }

        if (handled.Add(GetConversionKey(from, to)) == false)
        {
            return null;
        }

        var properties = GetPropertyMappings(from, to);

        if (properties.Count <= 0)
        {
            // Nothing to map. Let the runtime report there is no conversion instead of
            // generating a converter that returns an empty instance.
            return null;
        }

        return new ConversionInfo(from, to, converter.Construct(from, to), properties);
    }

    /// <summary>
    /// The whole intelligence of the generator: same name, same type.
    /// </summary>
    private static IReadOnlyList<PropertyMapping> GetPropertyMappings(ITypeSymbol from, ITypeSymbol to)
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

            if (SymbolEqualityComparer.Default.Equals(source.Type, target.Type) == false)
            {
                continue;
            }

            result.Add(new PropertyMapping(source, target));
        }

        return result;
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

                result.Add(GetConversionKey(iface.TypeArguments[0], iface.TypeArguments[1]));
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

    private static string GetConversionKey(ITypeSymbol from, ITypeSymbol to)
    {
        return from.ToFullyQualifiedName() + "->" + to.ToFullyQualifiedName();
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
