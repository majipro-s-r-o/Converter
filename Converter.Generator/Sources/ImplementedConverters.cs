using System.Collections.Generic;
using Majipro.Converter.Generator.Conversions;
using Microsoft.CodeAnalysis;

namespace Majipro.Converter.Generator.Sources;

/// <summary>
/// Pairs somebody already implemented by hand. Generating them again would make
/// DiCompositionValidator throw about two implementations of the same <c>From -&gt; To</c> pair, so
/// these are suppressed rather than requested.
/// </summary>
/// <remarks>
/// Having written one of the two conversions is having written the pair. A generated converter is
/// an <c>IConverter</c> and, when it can fill a target, an <c>IReferenceConverter</c> - and since
/// that one extends the other, there is no way to generate the half somebody is missing without
/// claiming the half they already wrote. So the pair is left alone as a whole, and the reference
/// converter they did not write is theirs to write.
///
/// That is also why only <c>IConverter</c> and <c>IAsyncConverter</c> are looked for: a hand written
/// <c>IReferenceConverter</c> is found through the <c>IConverter</c> it extends, and an
/// <c>IAsyncReferenceConverter</c> through its <c>IAsyncConverter</c>, so both arrive here anyway.
/// </remarks>
internal sealed class ImplementedConverters : IPairSource
{
    private readonly ConversionSemantics _semantics;

    public ImplementedConverters(ConversionSemantics semantics)
    {
        _semantics = semantics;
    }

    public IEnumerable<ConversionPair> Pairs()
    {
        foreach (var type in GetTypes(_semantics.Assembly.GlobalNamespace))
        {
            foreach (var iface in type.AllInterfaces)
            {
                var definition = iface.OriginalDefinition;

                if (SymbolEqualityComparer.Default.Equals(definition, _semantics.ConverterInterface) == false &&
                    SymbolEqualityComparer.Default.Equals(definition, _semantics.AsyncConverterInterface) == false)
                {
                    continue;
                }

                if (iface.TypeArguments.Length != 2)
                {
                    continue;
                }

                yield return new ConversionPair(iface.TypeArguments[0], iface.TypeArguments[1]);
            }
        }
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
}
