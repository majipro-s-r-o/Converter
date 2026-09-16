using System.Collections.Generic;
using Majipro.Converter.Generator.Extensions;
using Microsoft.CodeAnalysis;

namespace Majipro.Converter.Generator.Analysis;

/// <summary>
/// Everything the generator needs to emit one converter.
/// </summary>
internal sealed class ConversionInfo
{
    public ITypeSymbol From { get; }

    public ITypeSymbol To { get; }

    public IReadOnlyList<PropertyMapping> Properties { get; }

    /// <summary>Fully qualified <c>IConverter&lt;TFrom, TTo&gt;</c> the generated class implements.</summary>
    public string ConverterInterfaceFullName { get; }

    public string FromFullName { get; }

    public string ToFullName { get; }

    public string ClassName { get; }

    public string FileName => ClassName + ".g.cs";

    /// <summary>Value types are never null, so their converters do not need a null check.</summary>
    public bool RequiresNullCheck => From.IsValueType == false;

    public ConversionInfo(
        ITypeSymbol from,
        ITypeSymbol to,
        INamedTypeSymbol converterInterface,
        IReadOnlyList<PropertyMapping> properties)
    {
        From = from;
        To = to;
        Properties = properties;

        FromFullName = from.ToFullyQualifiedName();
        ToFullName = to.ToFullyQualifiedName();
        ConverterInterfaceFullName = converterInterface.ToFullyQualifiedName();
        ClassName = GetClassName(FromFullName, ToFullName);
    }

    private static string GetClassName(string fromFullName, string toFullName)
    {
        return Flatten(fromFullName) + "To" + Flatten(toFullName) + "Converter";
    }

    private static string Flatten(string fullName)
    {
        return fullName
            .Replace("global::", string.Empty)
            .Replace(".", string.Empty);
    }
}
