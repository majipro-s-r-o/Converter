using System.Collections.Generic;
using Majipro.Converter.Generator.Extensions;
using Majipro.Converter.Generator.Generating;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Majipro.Converter.Generator.Rules;

/// <summary>
/// Both sides are types the generator can map, so the value goes through the converting service and
/// the pair is requested by the act of writing that call.
/// </summary>
internal sealed class ConvertedValueRule : IPropertyValueRule
{
    public IEnumerable<ExpressionSyntax> Value(
        ConverterContext context,
        IPropertySymbol source,
        IPropertySymbol target,
        ExpressionSyntax value)
    {
        if (source.Type.GetUnderlyingType().IsMappableSource() == false ||
            target.Type.GetUnderlyingType().IsMappableTarget() == false)
        {
            yield break;
        }

        yield return context.Convert(source.Type, target.Type, value);
    }
}
