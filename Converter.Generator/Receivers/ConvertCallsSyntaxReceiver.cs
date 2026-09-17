using System.Collections;
using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Majipro.Converter.Generator.Receivers;

/// <summary>
/// The head of the pipe: every invocation that looks like <c>something.Method&lt;TFrom, TTo&gt;(...)</c>.
/// This is the one filter that has to be a receiver, because it is the only one that gets to see the
/// nodes as they are parsed. Whether an invocation really belongs to <c>IConvertingService</c> needs
/// the semantic model and is decided by <see cref="Sources.ConvertCallSites"/>.
/// </summary>
internal sealed class ConvertCallsSyntaxReceiver : ISyntaxReceiver, IEnumerable<InvocationExpressionSyntax>
{
    private readonly List<InvocationExpressionSyntax> _convertCalls = new List<InvocationExpressionSyntax>();

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

    public IEnumerator<InvocationExpressionSyntax> GetEnumerator()
    {
        return _convertCalls.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}
