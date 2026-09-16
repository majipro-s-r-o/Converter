using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Majipro.Converter.Generator.Receivers;

/// <summary>
/// Collects every invocation that looks like <c>something.Method&lt;TFrom, TTo&gt;(...)</c>.
/// Whether the invocation really belongs to <c>IConvertingService</c> is decided later,
/// by <see cref="Analysis.ConversionAnalyzer"/>, where the semantic model is available.
/// </summary>
internal sealed class ConvertCallsSyntaxReceiver : ISyntaxReceiver
{
    private readonly List<InvocationExpressionSyntax> _convertCalls = new List<InvocationExpressionSyntax>();

    public IReadOnlyList<InvocationExpressionSyntax> ConvertCalls => _convertCalls;

    public void OnVisitSyntaxNode(SyntaxNode syntaxNode)
    {
        if (syntaxNode is not InvocationExpressionSyntax invocation)
        {
            return;
        }

        if (invocation.Expression is not MemberAccessExpressionSyntax memberAccess)
        {
            return;
        }

        if (memberAccess.Name is not GenericNameSyntax genericName)
        {
            return;
        }

        if (genericName.TypeArgumentList.Arguments.Count != 2)
        {
            return;
        }

        _convertCalls.Add(invocation);
    }
}
