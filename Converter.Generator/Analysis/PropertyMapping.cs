using Majipro.Converter.Generator.Extensions;
using Microsoft.CodeAnalysis;

namespace Majipro.Converter.Generator.Analysis;

/// <summary>
/// One source property assigned to one target property.
/// </summary>
internal sealed class PropertyMapping
{
    public IPropertySymbol From { get; }

    public IPropertySymbol To { get; }

    public PropertyMappingKind Kind { get; }

    /// <summary>
    /// Source type of the nested conversion, the item type for <see cref="PropertyMappingKind.Collection"/>.
    /// Empty for <see cref="PropertyMappingKind.Direct"/>.
    /// </summary>
    public string ConvertedFromFullName { get; }

    /// <summary>Target counterpart of <see cref="ConvertedFromFullName"/>.</summary>
    public string ConvertedToFullName { get; }

    /// <summary>A collection the generated code has to null check before it materializes it.</summary>
    public bool SourceCanBeNull { get; }

    private PropertyMapping(
        IPropertySymbol from,
        IPropertySymbol to,
        PropertyMappingKind kind,
        ITypeSymbol? convertedFrom,
        ITypeSymbol? convertedTo)
    {
        From = from;
        To = to;
        Kind = kind;
        ConvertedFromFullName = convertedFrom == null ? string.Empty : convertedFrom.ToFullyQualifiedName();
        ConvertedToFullName = convertedTo == null ? string.Empty : convertedTo.ToFullyQualifiedName();
        SourceCanBeNull = from.Type.CanBeNull();
    }

    internal static PropertyMapping Direct(IPropertySymbol from, IPropertySymbol to)
    {
        return new PropertyMapping(from, to, PropertyMappingKind.Direct, null, null);
    }

    internal static PropertyMapping Converted(IPropertySymbol from, IPropertySymbol to)
    {
        return new PropertyMapping(from, to, PropertyMappingKind.Converted, from.Type, to.Type);
    }

    internal static PropertyMapping Collection(
        IPropertySymbol from,
        IPropertySymbol to,
        ITypeSymbol fromItem,
        ITypeSymbol toItem)
    {
        return new PropertyMapping(from, to, PropertyMappingKind.Collection, fromItem, toItem);
    }
}
