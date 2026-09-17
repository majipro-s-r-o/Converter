using Majipro.Converter.Generator.Extensions;
using Microsoft.CodeAnalysis;

namespace Majipro.Converter.Generator.Analysis;

/// <summary>
/// A <c>From -&gt; To</c> pair somebody asked for, either by a call site or by a property of another
/// conversion that has to be converted on its own.
/// </summary>
internal sealed class ConversionRequest
{
    public ITypeSymbol From { get; }

    public ITypeSymbol To { get; }

    /// <summary>Identity of the pair, used to generate every pair exactly once.</summary>
    public string Key { get; }

    public ConversionRequest(ITypeSymbol from, ITypeSymbol to)
    {
        From = from;
        To = to;
        Key = from.ToFullyQualifiedName() + "->" + to.ToFullyQualifiedName();
    }
}
