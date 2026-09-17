using System.Collections.Generic;
using Majipro.Converter.Generator.Conversions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Majipro.Converter.Generator.Sources;

/// <summary>
/// Every <c>IConvertingService.Convert&lt;TFrom, TTo&gt;(...)</c> the compilation contains. The
/// nodes arrive pre filtered by their shape, this is where the semantic model decides whether they
/// really are calls on the converting service.
/// </summary>
internal sealed class ConvertCallSites : IPairSource
{
    private readonly ConversionSemantics _semantics;
    private readonly IEnumerable<InvocationExpressionSyntax> _nodes;

    public ConvertCallSites(ConversionSemantics semantics, IEnumerable<InvocationExpressionSyntax> nodes)
    {
        _semantics = semantics;
        _nodes = nodes;
    }

    public IEnumerable<ConversionPair> Pairs()
    {
        foreach (var node in _nodes)
        {
            if (_semantics.GetSemanticModel(node.SyntaxTree).GetSymbolInfo(node).Symbol is not IMethodSymbol method)
            {
                continue;
            }

            if (method.ContainingType == null ||
                SymbolEqualityComparer.Default.Equals(
                    method.ContainingType.OriginalDefinition,
                    _semantics.ConvertingService) == false)
            {
                continue;
            }

            if (method.TypeArguments.Length != 2)
            {
                continue;
            }

            yield return new ConversionPair(method.TypeArguments[0], method.TypeArguments[1]);
        }
    }
}
