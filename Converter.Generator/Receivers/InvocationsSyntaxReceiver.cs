using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Majipro.Converter.Generator.Receivers;

/// <summary>
/// The head of the pipe: every invocation in the compilation. Which of them call a method requiring a
/// converter is not a question of their shape - the type arguments are usually inferred, and the
/// method may be any one carrying <c>RequiresConverterAttribute</c>, whatever it is called - so it is
/// decided by <see cref="Sources.ConvertCallSites"/>, which knows the names of those methods and binds
/// the invocations calling one of them.
/// </summary>
/// <remarks>
/// The rest are needed as well. A call site written inside a generic method names the method's type
/// parameters, and what they stand for is decided where that method is called - which is usually
/// without any type argument written out, inferred from the arguments, so no shape of the node can
/// tell those calls apart from the rest either.
/// </remarks>
internal sealed class InvocationsSyntaxReceiver : ISyntaxReceiver
{
    private readonly List<InvocationExpressionSyntax> _invocations = new List<InvocationExpressionSyntax>();
    private readonly HashSet<string> _attributedLocalFunctions = new HashSet<string>();

    /// <summary>Every invocation in the compilation.</summary>
    public IReadOnlyList<InvocationExpressionSyntax> Invocations => _invocations;

    /// <summary>
    /// The names of the local functions carrying any attribute. A local function is not a member of
    /// any type, so the walk over the types that finds every other method requiring a converter never
    /// meets one - its name has to come from the syntax. Which attribute it carries is decided once
    /// its call is bound.
    /// </summary>
    public IReadOnlyCollection<string> AttributedLocalFunctions => _attributedLocalFunctions;

    public void OnVisitSyntaxNode(SyntaxNode syntaxNode)
    {
        if (syntaxNode is InvocationExpressionSyntax invocation)
        {
            _invocations.Add(invocation);
        }
        else if (syntaxNode is LocalFunctionStatementSyntax { AttributeLists.Count: > 0 } localFunction)
        {
            _attributedLocalFunctions.Add(localFunction.Identifier.ValueText);
        }
    }

    /// <summary>
    /// The name an invocation calls by: <c>Build(...)</c>, <c>Build&lt;T&gt;(...)</c>,
    /// <c>this.Build(...)</c>, <c>x?.Build(...)</c>. Anything else - a delegate held in a variable
    /// being invoked, for instance - calls no method by name.
    /// </summary>
    internal static string? GetCalledName(InvocationExpressionSyntax invocation)
    {
        return invocation.Expression switch
        {
            SimpleNameSyntax name => name.Identifier.ValueText,
            MemberAccessExpressionSyntax memberAccess => memberAccess.Name.Identifier.ValueText,
            MemberBindingExpressionSyntax memberBinding => memberBinding.Name.Identifier.ValueText,
            _ => null
        };
    }
}
