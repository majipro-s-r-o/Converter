using System;
using System.Collections.Generic;
using Majipro.Converter.Abstrations;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Majipro.Converter.Generator.Generating;

/// <summary>
/// The handful of syntax shapes the generated code is written out of. Every type is a fully
/// qualified name, so a generated file never needs a using directive and can not collide with
/// anything in the project it is generated into.
/// </summary>
internal static class ConverterSyntax
{
    internal const string GeneratedNamespace = "Majipro.Converter.Generated";

    internal const string ConvertMethodName = nameof(IConverter<object, object>.Convert);
    internal const string FromParameterName = "from";
    internal const string ToParameterName = "to";
    internal const string ConvertingServiceParameterName = "convertingService";
    internal const string ConvertingServiceFieldName = "_" + ConvertingServiceParameterName;

    /// <summary>
    /// <c>from</c> is a contextual keyword, so it has to be created as one.
    /// </summary>
    internal static SyntaxToken FromIdentifier()
    {
        return Identifier(
            TriviaList(),
            SyntaxKind.FromKeyword,
            FromParameterName,
            FromParameterName,
            TriviaList());
    }

    internal static ExpressionSyntax From()
    {
        return IdentifierName(FromIdentifier());
    }

    /// <summary>The target a reference converter was handed, which is a plain identifier.</summary>
    internal static ExpressionSyntax To()
    {
        return IdentifierName(ToParameterName);
    }

    internal static ExpressionSyntax Member(ExpressionSyntax instance, string name)
    {
        return MemberAccessExpression(
            SyntaxKind.SimpleMemberAccessExpression,
            instance,
            IdentifierName(name));
    }

    internal static ExpressionSyntax Null()
    {
        return LiteralExpression(SyntaxKind.NullLiteralExpression);
    }

    internal static ExpressionSyntax Default()
    {
        return LiteralExpression(
            SyntaxKind.DefaultLiteralExpression,
            Token(SyntaxKind.DefaultKeyword));
    }

    internal static ExpressionSyntax IsNull(ExpressionSyntax value)
    {
        return BinaryExpression(SyntaxKind.EqualsExpression, value, Null());
    }

    internal static TypeSyntax TypeName(ITypeSymbol type)
    {
        return ParseTypeName(type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat));
    }

    /// <summary>
    /// A framework type the generated code needs, written as a fully qualified name taken from the
    /// type itself, so there is no name written by hand.
    /// </summary>
    internal static TypeSyntax TypeName(Type type, ITypeSymbol? typeArgument = null)
    {
        var name = type.FullName ?? type.Name;
        var arity = name.IndexOf('`');

        if (arity >= 0)
        {
            name = name.Substring(0, arity);
        }

        name = "global::" + name;

        return ParseTypeName(typeArgument == null
            ? name
            : name + "<" + typeArgument.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat) + ">");
    }

    /// <summary><c>_convertingService.Convert&lt;TFrom, TTo&gt;(value)</c>.</summary>
    internal static ExpressionSyntax ConvertCall(ITypeSymbol from, ITypeSymbol to, ExpressionSyntax value)
    {
        return InvocationExpression(
                MemberAccessExpression(
                    SyntaxKind.SimpleMemberAccessExpression,
                    IdentifierName(ConvertingServiceFieldName),
                    GenericName(Identifier(ConvertMethodName))
                        .WithTypeArgumentList(
                            TypeArgumentList(
                                SeparatedList(new[] { TypeName(from), TypeName(to) })))))
            .WithArgumentList(
                ArgumentList(
                    SingletonSeparatedList(
                        Argument(value))));
    }

    /// <summary><c>new global::System.Collections.Generic.List&lt;TItem&gt;(items)</c>.</summary>
    internal static ExpressionSyntax NewList(ITypeSymbol item, ExpressionSyntax items)
    {
        return ObjectCreationExpression(TypeName(typeof(List<>), item))
            .WithArgumentList(
                ArgumentList(
                    SingletonSeparatedList(
                        Argument(items))));
    }
}
