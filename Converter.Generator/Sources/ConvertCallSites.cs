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
        foreach (var callSite in CallSites())
        {
            yield return callSite.Pair;
        }
    }

    /// <summary>
    /// The pairs together with the invocation asking for them, for whoever needs to know where a
    /// pair was written - <see cref="GenericCallSites"/> follows a pair named by type parameters
    /// back to the methods calling the one it is written in.
    /// </summary>
    public IEnumerable<ConvertCallSite> CallSites()
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

            yield return new ConvertCallSite(
                new ConversionPair(method.TypeArguments[0], method.TypeArguments[1]),
                node);
        }
    }
}

internal readonly struct ConvertCallSite
{
    public ConversionPair Pair { get; }

    public InvocationExpressionSyntax Node { get; }

    public ConvertCallSite(ConversionPair pair, InvocationExpressionSyntax node)
    {
        Pair = pair;
        Node = node;
    }
}
