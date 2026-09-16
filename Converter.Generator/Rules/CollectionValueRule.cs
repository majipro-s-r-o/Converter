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

        if (IsConvertible(context, sourceItem, targetItem) == false)
        {
            yield break;
        }

        yield return GetValue(context, source, value, sourceItem, targetItem);
    }

    /// <summary>
    /// Whether the converting service is going to have a converter for the items, which is the only
    /// thing that separates a collection this rule can write from one it can not. The three ways
    /// there can be one are the three value rules asked of an ordinary property, minus the one the
    /// compiler would have performed itself: the items are the same type, they are a scalar and its
    /// text, or they are a pair a converter is generated for.
    /// </summary>
    /// <remarks>
    /// Being merely assignable, the way <c>int</c> is to <c>long</c>, is deliberately not one of
    /// them: the per item <c>Convert&lt;int, long&gt;</c> would compile and then throw about a
    /// missing conversion, which is the one outcome worth refusing outright.
    /// </remarks>
    private static bool IsConvertible(ConverterContext context, ITypeSymbol sourceItem, ITypeSymbol targetItem)
    {
        if (SymbolEqualityComparer.Default.Equals(sourceItem, targetItem))
        {
            return true;
        }

        if (context.Semantics.IsTextConversion(sourceItem, targetItem))
        {
            return true;
        }

        return sourceItem.GetUnderlyingType().IsMappableSource() &&
               targetItem.GetUnderlyingType().IsMappableTarget();
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
