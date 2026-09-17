using System.Collections.Generic;
using System.Linq;
using System.Text;
using Majipro.Converter.Generator.Extensions;
using Microsoft.CodeAnalysis;

namespace Majipro.Converter.Generator.Analysis;

/// <summary>
/// Everything the generator needs to emit one converter.
/// </summary>
internal sealed class ConversionInfo
{
    private static readonly IReadOnlyList<PropertyMapping> NoProperties = new PropertyMapping[0];

    /// <summary>Source type as the call site wrote it, a <see cref="System.Nullable{T}"/> included.</summary>
    public string FromFullName { get; }

    /// <summary>Target type as the call site wrote it, a <see cref="System.Nullable{T}"/> included.</summary>
    public string ToFullName { get; }

    /// <summary>Target type the object initializer creates, so never a <see cref="System.Nullable{T}"/>.</summary>
    public string ToValueFullName { get; }

    /// <summary>Fully qualified <c>IConverter&lt;TFrom, TTo&gt;</c> the generated class implements.</summary>
    public string ConverterInterfaceFullName { get; }

    public IReadOnlyList<PropertyMapping> Properties { get; }

    /// <summary>A <c>string</c> target is produced by <c>System.Convert.ToString</c>, not by mapping.</summary>
    public bool IsToStringConversion { get; }

    /// <summary>Source properties are reached through <c>from.Value</c>.</summary>
    public bool SourceIsNullableValueType { get; }

    /// <summary>Value types that are not nullable are never null, their converters skip the check.</summary>
    public bool RequiresNullCheck { get; }

    /// <summary>True when at least one property is converted by <c>IConvertingService</c>.</summary>
    public bool RequiresConvertingService => Properties.Any(p => p.Kind != PropertyMappingKind.Direct);

    public string ClassName { get; }

    public string FileName => ClassName + ".g.cs";

    private ConversionInfo(
        ITypeSymbol from,
        ITypeSymbol to,
        INamedTypeSymbol converterInterface,
        IReadOnlyList<PropertyMapping> properties,
        bool isToStringConversion)
    {
        Properties = properties;
        IsToStringConversion = isToStringConversion;

        FromFullName = from.ToFullyQualifiedName();
        ToFullName = to.ToFullyQualifiedName();
        ToValueFullName = to.GetUnderlyingType().ToFullyQualifiedName();
        ConverterInterfaceFullName = converterInterface.ToFullyQualifiedName();

        SourceIsNullableValueType = from.GetNullableUnderlyingType() != null;
        RequiresNullCheck = from.CanBeNull();

        ClassName = GetClassName(FromFullName, ToFullName);
    }

    internal static ConversionInfo FromProperties(
        ITypeSymbol from,
        ITypeSymbol to,
        INamedTypeSymbol converterInterface,
        IReadOnlyList<PropertyMapping> properties)
    {
        return new ConversionInfo(from, to, converterInterface, properties, false);
    }

    internal static ConversionInfo ToStringCall(
        ITypeSymbol from,
        ITypeSymbol to,
        INamedTypeSymbol converterInterface)
    {
        return new ConversionInfo(from, to, converterInterface, NoProperties, true);
    }

    private static string GetClassName(string fromFullName, string toFullName)
    {
        return Flatten(fromFullName) + "To" + Flatten(toFullName) + "Converter";
    }

    /// <summary>
    /// Turns a fully qualified name into an identifier. <c>?</c> becomes <c>Nullable</c> so that
    /// <c>Guid</c> and <c>Guid?</c> do not end up claiming the same class name.
    /// </summary>
    private static string Flatten(string fullName)
    {
        var result = new StringBuilder();
        var capitalize = true;

        foreach (var character in fullName.Replace("global::", string.Empty))
        {
            if (character == '?')
            {
                result.Append("Nullable");
            }
            else if (char.IsLetterOrDigit(character) || character == '_')
            {
                // Keywords like 'int' and 'string' are written in lower case, which would leave
                // the generated class name starting with one.
                result.Append(capitalize ? char.ToUpperInvariant(character) : character);
                capitalize = false;

                continue;
            }

            capitalize = true;
        }

        return result.ToString();
    }
}
