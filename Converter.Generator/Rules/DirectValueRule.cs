using System.Collections.Generic;
using Majipro.Converter.Generator.Generating;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Majipro.Converter.Generator.Rules;

/// <summary>
/// The compiler takes the value as it is: identity, <c>Guid -&gt; Guid?</c>, a differing nullable
/// annotation, an implicit numeric or reference conversion.
/// </summary>
internal sealed class DirectValueRule : IPropertyValueRule
{
    public IEnumerable<ExpressionSyntax> Value(
        ConverterContext context,
        IPropertySymbol source,
        IPropertySymbol target,
        ExpressionSyntax value)
    {
        if (context.Semantics.IsAssignable(source.Type, target.Type) == false)
        {
            yield break;
        }

        yield return value;
    }
}
