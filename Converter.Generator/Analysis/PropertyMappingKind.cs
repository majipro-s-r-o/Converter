namespace Majipro.Converter.Generator.Analysis;

/// <summary>
/// How the value of one source property reaches its target property.
/// </summary>
internal enum PropertyMappingKind
{
    /// <summary>The source value is assignable to the target property as it is.</summary>
    Direct,

    /// <summary>The value is handed to <c>IConvertingService</c>, which needs its own converter.</summary>
    Converted,

    /// <summary>Every item of the source collection is converted into a new list.</summary>
    Collection
}
