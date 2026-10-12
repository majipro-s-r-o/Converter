using System.Collections.Generic;
using Majipro.Converter.Generator.Conversions;
using Majipro.Converter.Generator.Extensions;
using Majipro.Converter.Generator.Receivers;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Majipro.Converter.Generator.Sources;

/// <summary>
/// The pairs a <b>generic</b> call site stands for. <c>Convert&lt;TKind, Target&gt;(kind)</c> written
/// inside <c>Build&lt;TKind&gt;</c> is not a pair anybody can write a converter for, but every call of
/// <c>Build</c> says what <c>TKind</c> is - <c>Build(variable, v =&gt; v.Kind)</c> closes it over the
/// type of <c>v.Kind</c> without a type argument being written anywhere. Those are the pairs the call
/// site asks for at runtime, so they are the ones requested.
/// </summary>
/// <remarks>
/// <para>
/// The call site is followed back through the calls of the method it is written in. A call that
/// closes every type parameter yields a pair; a call that only renames them - a generic method
/// handing its own type parameter on, or calling itself - is followed back through the calls of
/// <i>its</i> method in turn. Type parameters of the containing type are closed the same way, by the
/// type the called method is a member of (<c>repository.Fetch()</c> on a <c>Repository&lt;Person&gt;</c>).
/// </para>
/// <para>
/// What this does not see is any way of reaching the method other than calling it by name: a method
/// group handed over as a delegate, an override reached through its base, an interface implementation
/// reached through the interface, or a public method called from another assembly. A call site none of
/// whose pairs could be found is what <see cref="Unresolved"/> is - its conversion is decided only at
/// runtime, and nothing here can say whether there is a converter for it - unless the method it is
/// written in says it requires that very pair with <c>RequiresConverterAttribute</c>, which hands the
/// question to its callers wherever they are, so the call site is not unresolved at all.
/// </para>
/// </remarks>
internal sealed class GenericCallSites : IPairSource
{
    /// <summary>
    /// How many renamed pairs one call site may be followed through before it is given up on. The
    /// depth bound on names is what makes the set finite, this keeps a pathological one cheap.
    /// </summary>
    private const int MaxStatesPerCallSite = 1024;

    private readonly ConversionSemantics _semantics;
    private readonly ConvertCallSites _callSites;
    private readonly IReadOnlyList<InvocationExpressionSyntax> _invocations;
    private readonly List<(ConvertCallSite CallSite, IMethodSymbol? Method)> _unresolved =
        new List<(ConvertCallSite CallSite, IMethodSymbol? Method)>();

    private Dictionary<string, List<InvocationExpressionSyntax>>? _invocationsByName;

    public GenericCallSites(
        ConversionSemantics semantics,
        ConvertCallSites callSites,
        IReadOnlyList<InvocationExpressionSyntax> invocations)
    {
        _semantics = semantics;
        _callSites = callSites;
        _invocations = invocations;
    }

    /// <summary>
    /// Generic call sites no closing call was found for, with the method they are written in. Filled while <see cref="Pairs"/> is
    /// enumerated, so it means something only once that is over.
    /// </summary>
    public IReadOnlyList<(ConvertCallSite CallSite, IMethodSymbol? Method)> Unresolved => _unresolved;

    public IEnumerable<ConversionPair> Pairs()
    {
        foreach (var callSite in _callSites.CallSites())
        {
            if (IsOpen(callSite.Pair) == false)
            {
                continue;
            }

            var owner = GetCallingMethod(callSite.Node);
            var resolved = false;

            if (owner != null)
            {
                foreach (var pair in Close(callSite.Pair, owner))
                {
                    resolved = true;

                    yield return pair;
                }
            }

            if (resolved == false && (owner == null || _callSites.IsRequiredBy(owner, callSite.Pair) == false))
            {
                _unresolved.Add((callSite, owner));
            }
        }
    }

    /// <summary>
    /// Every closed pair <paramref name="pair"/>, written inside <paramref name="owner"/>, turns into
    /// at the places <paramref name="owner"/> is called from - and the places those are called from,
    /// for as long as a call only renames the type parameters.
    /// </summary>
    private IEnumerable<ConversionPair> Close(ConversionPair pair, IMethodSymbol owner)
    {
        var visited = new Dictionary<IMethodSymbol, HashSet<ConversionPair>>(SymbolEqualityComparer.Default);
        var pending = new Queue<(ConversionPair Pair, IMethodSymbol Owner)>();
        var closed = new HashSet<ConversionPair>();
        var states = 0;

        Visit(pair, owner);

        while (pending.Count > 0)
        {
            var (current, method) = pending.Dequeue();

            foreach (var invocation in GetInvocationsOf(method))
            {
                var arguments = ConversionSemantics.GetTypeArguments(invocation.Method);
                var from = _semantics.Substitute(current.From, arguments);
                var to = _semantics.Substitute(current.To, arguments);

                if (from == null || to == null ||
                    from.IsWithinTypeArgumentDepth() == false ||
                    to.IsWithinTypeArgumentDepth() == false)
                {
                    continue;
                }

                var substituted = new ConversionPair(from, to);

                if (IsOpen(substituted) == false)
                {
                    if (closed.Add(substituted))
                    {
                        yield return substituted;
                    }

                    continue;
                }

                var caller = GetCallingMethod(invocation.Node);

                if (caller != null && states < MaxStatesPerCallSite)
                {
                    Visit(substituted, caller);
                }
            }
        }

        void Visit(ConversionPair open, IMethodSymbol method)
        {
            if (visited.TryGetValue(method, out var pairs) == false)
            {
                pairs = new HashSet<ConversionPair>();
                visited.Add(method, pairs);
            }

            if (pairs.Add(open))
            {
                states++;
                pending.Enqueue((open, method));
            }
        }
    }

    /// <summary>
    /// Every invocation in the compilation that calls <paramref name="method"/>, with the method as
    /// it is called there - type arguments inferred or written out, and the type it is a member of
    /// constructed. Only the invocations spelling the method's name are bound at all.
    /// </summary>
    private IEnumerable<(IMethodSymbol Method, InvocationExpressionSyntax Node)> GetInvocationsOf(IMethodSymbol method)
    {
        if (GetInvocationsByName().TryGetValue(method.Name, out var candidates) == false)
        {
            yield break;
        }

        var definition = method.OriginalDefinition;

        foreach (var node in candidates)
        {
            if (_semantics.GetSemanticModel(node.SyntaxTree).GetSymbolInfo(node).Symbol is not IMethodSymbol called)
            {
                continue;
            }

            // An extension method called as one is the reduced form, whose type parameters are what
            // is left once the receiver has been accounted for. The constructed static method is the
            // one whose type parameters are the ones the body was written with.
            var unreduced = called.ReducedFrom != null
                ? called.GetConstructedReducedFrom() ?? called
                : called;

            if (SymbolEqualityComparer.Default.Equals(unreduced.OriginalDefinition, definition) == false)
            {
                continue;
            }

            yield return (unreduced, node);
        }
    }

    private Dictionary<string, List<InvocationExpressionSyntax>> GetInvocationsByName()
    {
        if (_invocationsByName != null)
        {
            return _invocationsByName;
        }

        _invocationsByName = new Dictionary<string, List<InvocationExpressionSyntax>>();

        foreach (var invocation in _invocations)
        {
            var name = InvocationsSyntaxReceiver.GetCalledName(invocation);

            if (name == null)
            {
                continue;
            }

            if (_invocationsByName.TryGetValue(name, out var invocations) == false)
            {
                invocations = new List<InvocationExpressionSyntax>();
                _invocationsByName.Add(name, invocations);
            }

            invocations.Add(invocation);
        }

        return _invocationsByName;
    }

    /// <summary>
    /// The method a node is written in, which is what has to be called for the node to run. A
    /// lambda is not called by any name, so it is the method the lambda is written in; a local
    /// function is, so it is the local function.
    /// </summary>
    private IMethodSymbol? GetCallingMethod(SyntaxNode node)
    {
        var symbol = _semantics.GetSemanticModel(node.SyntaxTree).GetEnclosingSymbol(node.SpanStart);

        while (symbol is IMethodSymbol { MethodKind: MethodKind.AnonymousFunction })
        {
            symbol = symbol.ContainingSymbol;
        }

        return symbol as IMethodSymbol;
    }

    private static bool IsOpen(ConversionPair pair)
    {
        return pair.From.ContainsTypeParameter() || pair.To.ContainsTypeParameter();
    }
}
