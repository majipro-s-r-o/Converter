using System.Collections.Generic;
using Majipro.Converter.Generator.Generating;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Majipro.Converter.Generator.Rules;

/// <summary>
/// One way a source property can reach a target property. The rule yields the expression the target
/// property is assigned, or nothing at all when it does not know how - which is the same statement
/// as "this is not my case", so the filter and the generator are one function.
/// </summary>
internal interface IPropertyValueRule
{
    IEnumerable<ExpressionSyntax> Value(
        ConverterContext context,
        IPropertySymbol source,
        IPropertySymbol target,
        ExpressionSyntax value);
}
