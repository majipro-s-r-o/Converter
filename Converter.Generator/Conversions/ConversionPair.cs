using System;
using Microsoft.CodeAnalysis;

namespace Majipro.Converter.Generator.Conversions;

/// <summary>
/// A <c>From -&gt; To</c> pair somebody asked for, either by a call site or by the generation of
/// another converter that ran into a property it can not assign directly.
/// </summary>
/// <remarks>
/// Identity is the pair of symbols, not their names: <see cref="SymbolEqualityComparer.Default"/>
/// tells <c>Guid</c> from <c>Guid?</c> the same way a written out name would, and costs nothing,
/// while formatting a display string for every pair that is enqueued does not.
/// </remarks>
internal readonly struct ConversionPair : IEquatable<ConversionPair>
{
    public ITypeSymbol From { get; }

    public ITypeSymbol To { get; }

    public ConversionPair(ITypeSymbol from, ITypeSymbol to)
    {
        From = from;
        To = to;
    }

    /// <summary>Both sides are the same type, which <c>ConvertingService</c> handles by itself.</summary>
    public bool IsIdentity => SymbolEqualityComparer.Default.Equals(From, To);

    public bool Equals(ConversionPair other)
    {
        return SymbolEqualityComparer.Default.Equals(From, other.From) &&
               SymbolEqualityComparer.Default.Equals(To, other.To);
    }

    public override bool Equals(object? obj)
    {
        return obj is ConversionPair other && Equals(other);
    }

    public override int GetHashCode()
    {
        unchecked
        {
            var from = From == null ? 0 : SymbolEqualityComparer.Default.GetHashCode(From);
            var to = To == null ? 0 : SymbolEqualityComparer.Default.GetHashCode(To);

            return from * 397 ^ to;
        }
    }

    public override string ToString()
    {
        return From + " -> " + To;
    }
}
