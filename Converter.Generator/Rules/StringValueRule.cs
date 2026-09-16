using System.Collections.Generic;
using Majipro.Converter.Generator.Generating;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Majipro.Converter.Generator.Rules;

/// <summary>
/// A scalar and its text form, in either direction. The value is handed to the converting service
/// the same way <see cref="ConvertedValueRule"/> hands one over, so the conversion is done by a
/// converter that really exists rather than by something written into this property.
/// </summary>
/// <remarks>
/// Which pairs those are, and why the two directions are recognized differently, is
/// <see cref="Conversions.ConversionSemantics.IsTextConversion"/> - the same question
/// <see cref="CollectionValueRule"/> asks about the items of a collection, so a property and a
/// collection of it are never claimed on different grounds.
/// </remarks>
internal sealed class StringValueRule : IPropertyValueRule
{
    public IEnumerable<ExpressionSyntax> Value(
        ConverterContext context,
        IPropertySymbol source,
        IPropertySymbol target,
        ExpressionSyntax value)
    {
        if (context.Semantics.IsTextConversion(source.Type, target.Type) == false)
        {
            yield break;
        }

        yield return context.Convert(source.Type, target.Type, value);
    }
}
