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
/// <remarks>
/// Every other invocation is kept as well, in <see cref="Invocations"/>. A call site written inside a
/// generic method names the method's type parameters, and what they stand for is decided where that
/// method is called - which is usually without any type argument written out, inferred from the
/// arguments, so no shape of the node can tell those calls apart from the rest.
/// </remarks>
internal sealed class ConvertCallsSyntaxReceiver : ISyntaxReceiver, IEnumerable<InvocationExpressionSyntax>
{
    private readonly List<InvocationExpressionSyntax> _convertCalls = new List<InvocationExpressionSyntax>();
    private readonly List<InvocationExpressionSyntax> _invocations = new List<InvocationExpressionSyntax>();

    /// <summary>Every invocation in the compilation, the ones that look like a conversion included.</summary>
    public IReadOnlyList<InvocationExpressionSyntax> Invocations => _invocations;

    public void OnVisitSyntaxNode(SyntaxNode syntaxNode)
    {
        if (syntaxNode is not InvocationExpressionSyntax invocation)
        {
            return;
        }

        _invocations.Add(invocation);

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
