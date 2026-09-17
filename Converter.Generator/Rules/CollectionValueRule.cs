using System.Collections.Generic;
using Majipro.Converter.Generator.Extensions;
using Majipro.Converter.Generator.Generating;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Majipro.Converter.Generator.Rules;

/// <summary>
/// The source is an <c>IEnumerable&lt;A&gt;</c> and the target is a collection a
/// <c>List&lt;B&gt;</c> fits into. Items go through the converting service one by one and are
/// materialized into that list.
/// </summary>
internal sealed class CollectionValueRule : IPropertyValueRule
{
    public IEnumerable<ExpressionSyntax> Value(
        ConverterContext context,
        IPropertySymbol source,
        IPropertySymbol target,
        ExpressionSyntax value)
    {
        var sourceItem = context.Semantics.GetEnumeratedType(source.Type);
        var targetItem = context.Semantics.GetListItemType(target.Type);

        if (sourceItem == null || targetItem == null)
        {
            yield break;
        }

        // The converting service only knows an item pair that is either the same type or has a
        // converter. Being merely assignable, the way int is to long, is not enough here - it would
        // compile and then throw about a missing conversion.
        if (SymbolEqualityComparer.Default.Equals(sourceItem, targetItem) == false &&
            (sourceItem.GetUnderlyingType().IsMappableSource() == false ||
             targetItem.GetUnderlyingType().IsMappableTarget() == false))
        {
            yield break;
        }

        yield return GetValue(context, source, value, sourceItem, targetItem);
    }

    private static ExpressionSyntax GetValue(
        ConverterContext context,
        IPropertySymbol source,
        ExpressionSyntax value,
        ITypeSymbol sourceItem,
        ITypeSymbol targetItem)
    {
        var items = context.Convert(
            sourceItem,
            targetItem,
            CastExpression(
                ConverterSyntax.TypeName(typeof(IEnumerable<>), sourceItem),
                value));

        var list = ConverterSyntax.NewList(targetItem, items);

        return source.Type.CanBeNull()
            ? ConditionalExpression(ConverterSyntax.IsNull(value), ConverterSyntax.Null(), list)
            : list;
    }
}
