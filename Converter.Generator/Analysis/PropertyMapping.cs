using Microsoft.CodeAnalysis;

namespace Majipro.Converter.Generator.Analysis;

/// <summary>
/// One source property assigned to one target property.
/// </summary>
internal sealed class PropertyMapping
{
    public IPropertySymbol From { get; }

    public IPropertySymbol To { get; }

    public PropertyMapping(IPropertySymbol from, IPropertySymbol to)
    {
        From = from;
        To = to;
    }
}
