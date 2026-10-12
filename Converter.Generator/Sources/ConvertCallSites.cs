using System.Collections.Generic;
using System.Linq;
using Majipro.Converter.Generator.Conversions;
using Majipro.Converter.Generator.Diagnostics;
using Majipro.Converter.Generator.Receivers;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Majipro.Converter.Generator.Sources;

/// <summary>
/// Every call of a method that requires a converter - one carrying <c>RequiresConverterAttribute</c>.
/// That is every method of <c>IConvertingService</c>, the <c>ConvertExplicitly</c> extensions, and any
/// method somebody else marks the same way, in this assembly or a referenced one. The pairs are read
/// off the attribute of the method the call binds to - or of a member it overrides or implements -
/// so type arguments the compiler inferred are as good as written out, and nothing here knows a
/// method of the converting service by its name or by the order of its type parameters.
/// </summary>
internal sealed class ConvertCallSites : IPairSource
{
    private readonly ConversionSemantics _semantics;
    private readonly InvocationsSyntaxReceiver _receiver;

    private HashSet<string>? _requiringNames;

    public ConvertCallSites(ConversionSemantics semantics, InvocationsSyntaxReceiver receiver)
    {
        _semantics = semantics;
        _receiver = receiver;
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
        var names = GetRequiringNames();

        foreach (var node in _receiver.Invocations)
        {
            var name = InvocationsSyntaxReceiver.GetCalledName(node);

            if (name == null || names.Contains(name) == false)
            {
                continue;
            }

            if (_semantics.GetSemanticModel(node.SyntaxTree).GetSymbolInfo(node).Symbol is not IMethodSymbol method)
            {
                continue;
            }

            foreach (var pair in GetPairs(method))
            {
                yield return new ConvertCallSite(pair, node);
            }
        }
    }

    /// <summary>
    /// The pairs one call asks the container for: one per <c>RequiresConverterAttribute</c> of the
    /// method it binds to, each type parameter the attribute names replaced by what the call closes it
    /// over. A dictionary overload carries two, its keys and its values.
    /// </summary>
    /// <remarks>
    /// An extension method called as one is bound in its reduced form, which has lost the
    /// <c>this</c> parameter but none of its type parameters - it is unreduced first all the same, so
    /// the type arguments are always those of the method the attribute was written on.
    /// </remarks>
    private IEnumerable<ConversionPair> GetPairs(IMethodSymbol method)
    {
        var called = method.ReducedFrom != null
            ? method.GetConstructedReducedFrom() ?? method
            : method;
        var arguments = ConversionSemantics.GetTypeArguments(called);

        foreach (var required in GetRequiredPairs(called.OriginalDefinition))
        {
            yield return new ConversionPair(Close(required.From, arguments), Close(required.To, arguments));
        }
    }

    /// <summary>
    /// Whether a pair asked for inside <paramref name="method"/> - or inside a method it is a local
    /// function of - is one the method says it requires. Such a pair is not unresolved: the method has
    /// put it in its signature, so every call of it requests the closed pair wherever that call is, the
    /// ones in other assemblies included. A pair the method asks for <b>without</b> saying so (the
    /// attribute names <c>TFrom -&gt; TTo</c> and the body converts into a <c>List&lt;TTo&gt;</c>) is
    /// not covered by it and stays reported.
    /// </summary>
    public bool IsRequiredBy(IMethodSymbol method, ConversionPair pair)
    {
        for (ISymbol? symbol = method; symbol is IMethodSymbol current; symbol = current.ContainingSymbol)
        {
            foreach (var required in GetRequiredPairs(current.OriginalDefinition))
            {
                if (required.Equals(pair))
                {
                    return true;
                }
            }
        }

        return false;
    }

    /// <summary>
    /// The pairs <paramref name="definition"/> requires, in its own type parameters - what its
    /// attributes say, wherever along its overrides and contracts they were written.
    /// </summary>
    private IEnumerable<ConversionPair> GetRequiredPairs(IMethodSymbol definition)
    {
        foreach (var declaring in GetDeclarations(definition))
        {
            foreach (var attribute in declaring.OriginalDefinition.GetAttributes())
            {
                if (IsRequiresConverter(attribute) == false)
                {
                    continue;
                }

                var from = attribute.ConstructorArguments[0].Value as string;
                var to = attribute.ConstructorArguments[1].Value as string;

                yield return new ConversionPair(
                    Resolve(declaring, definition, from, from, to),
                    Resolve(declaring, definition, to, from, to));
            }
        }
    }

    /// <summary>
    /// Where the attribute of a method may have been written: on the method itself, on every method
    /// it overrides, and on every interface member any of those implements. A contract is the natural
    /// place to say what calling it asks for, and a call written against the concrete type binds to the
    /// implementation, which says nothing - the attribute is not inherited, and Roslyn does not merge
    /// the attributes of an overridden or implemented member into the one implementing it.
    /// </summary>
    /// <remarks>
    /// Every method yielded is seen from the type of <paramref name="definition"/>: an overridden
    /// method is a member of the base type as the derived one constructs it, and an interface member
    /// is one of the interfaces as that type constructs them. So whatever their containing types are
    /// built of is already expressed in the type parameters of <paramref name="definition"/>.
    /// </remarks>
    private static IEnumerable<IMethodSymbol> GetDeclarations(IMethodSymbol definition)
    {
        for (var method = definition; method != null; method = method.OverriddenMethod)
        {
            yield return method;

            if (method.ContainingType == null)
            {
                continue;
            }

            foreach (var contract in method.ContainingType.AllInterfaces)
            {
                foreach (var member in contract.GetMembers(method.Name).OfType<IMethodSymbol>())
                {
                    var implementation = method.ContainingType.FindImplementationForInterfaceMember(member);

                    if (implementation != null &&
                        SymbolEqualityComparer.Default.Equals(implementation.OriginalDefinition, method.OriginalDefinition))
                    {
                        yield return member;
                    }
                }
            }
        }
    }

    /// <summary>
    /// The type parameter the attribute of <paramref name="declaring"/> calls by that name, seen from
    /// <paramref name="definition"/> - the method the call bound to. The name is looked up the way the
    /// compiler looks up a name in the attribute: the method's own type parameters first, then those of
    /// whatever it is declared in, out to the outermost type.
    /// </summary>
    /// <remarks>
    /// The method's own type parameters are matched by <b>position</b> once found by name, since an
    /// override or an implementation is free to call them differently while they stay the same
    /// parameters. A local function's enclosing method is one of the things it is declared in; its type
    /// parameters are returned as they are, and the pair they leave open is for
    /// <see cref="GenericCallSites"/> to close. A name found nowhere is <c>MC0006</c>: the attribute
    /// promises a conversion nobody could ask for.
    /// </remarks>
    private static ITypeSymbol Resolve(
        IMethodSymbol declaring,
        IMethodSymbol definition,
        string? name,
        string? from,
        string? to)
    {
        var own = IndexOf(declaring.TypeParameters, name);

        if (own >= 0 && own < definition.TypeParameters.Length)
        {
            return definition.TypeParameters[own];
        }

        for (var container = declaring.ContainingSymbol; container != null; container = container.ContainingSymbol)
        {
            if (container is IMethodSymbol enclosing)
            {
                var index = IndexOf(enclosing.TypeParameters, name);

                if (index >= 0)
                {
                    return enclosing.TypeParameters[index];
                }
            }
            else if (container is INamedTypeSymbol type)
            {
                var index = IndexOf(type.OriginalDefinition.TypeParameters, name);

                if (index >= 0 && index < type.TypeArguments.Length)
                {
                    return type.TypeArguments[index];
                }
            }
        }

        throw new ConversionException(
            ConverterDiagnostics.RequiresConverterWithoutTypeParameter,
            declaring.OriginalDefinition.ToDisplayString(SymbolDisplayFormat.CSharpErrorMessageFormat),
            from ?? "null",
            to ?? "null",
            name ?? "null");
    }

    private static int IndexOf(IReadOnlyList<ITypeParameterSymbol> typeParameters, string? name)
    {
        for (var i = 0; i < typeParameters.Count; i++)
        {
            if (typeParameters[i].Name == name)
            {
                return i;
            }
        }

        return -1;
    }

    /// <summary>What the type means at the call: the type parameters it names replaced by the call's.</summary>
    private ITypeSymbol Close(ITypeSymbol type, IReadOnlyDictionary<ITypeParameterSymbol, ITypeSymbol> arguments)
    {
        return _semantics.Substitute(type, arguments) ?? type;
    }

    private bool IsRequiresConverter(AttributeData attribute)
    {
        return attribute.ConstructorArguments.Length == 2 &&
               SymbolEqualityComparer.Default.Equals(attribute.AttributeClass, _semantics.RequiresConverterAttribute);
    }

    /// <summary>
    /// The names of every method requiring a converter, which is what lets the invocations be
    /// filtered by their syntax before any of them is bound. A method carrying the attribute is
    /// declared either by the abstractions themselves or by an assembly referencing them - the same
    /// assemblies <c>ConversionSemantics.ConverterAssemblies</c> lists - so those are the only ones
    /// walked. An override or an implementation of a marked member is called by the same name, so it
    /// passes the filter without being marked itself. A local function is no member of a type and is
    /// taken from the syntax instead.
    /// </summary>
    private HashSet<string> GetRequiringNames()
    {
        if (_requiringNames != null)
        {
            return _requiringNames;
        }

        var assemblies = _semantics.ConverterAssemblies.ToList();
        var abstractions = _semantics.RequiresConverterAttribute?.ContainingAssembly;

        if (abstractions != null)
        {
            assemblies.Add(abstractions);
        }

        _requiringNames = new HashSet<string>(assemblies
            .SelectMany(a => GetTypes(a.GlobalNamespace))
            .SelectMany(t => t.GetMembers())
            .OfType<IMethodSymbol>()
            .Where(m => m.GetAttributes().Any(IsRequiresConverter))
            .Select(m => m.Name)
            .Concat(_receiver.AttributedLocalFunctions));

        return _requiringNames;
    }

    private static IEnumerable<INamedTypeSymbol> GetTypes(INamespaceOrTypeSymbol namespaceOrType)
    {
        foreach (var member in namespaceOrType.GetMembers())
        {
            if (member is INamespaceOrTypeSymbol namespaceOrTypeMember)
            {
                if (member is INamedTypeSymbol type)
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
