using System.Collections.Generic;
using Majipro.Converter.Generator.Generating;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Majipro.Converter.Generator.Rules;

/// <summary>
/// Two different enums, the shape a layered application ends up with when it declares its own enum
/// per layer. The value is handed to the converting service and the pair is requested by the act of
/// writing that call, so <see cref="EnumBodyRule"/> writes the converter in the same pass - matched
/// by name, exactly as for a call site.
/// </summary>
/// <remarks>
/// The same enum on both sides never gets here: <see cref="DirectValueRule"/> copies it first.
/// </remarks>
internal sealed class EnumValueRule : IPropertyValueRule
{
    public IEnumerable<ExpressionSyntax> Value(
        ConverterContext context,
        IPropertySymbol source,
        IPropertySymbol target,
        ExpressionSyntax value)
    {
        if (context.Semantics.IsEnumConversion(source.Type, target.Type) == false)
        {
            yield break;
        }

        yield return context.Convert(source.Type, target.Type, value);
    }
}
