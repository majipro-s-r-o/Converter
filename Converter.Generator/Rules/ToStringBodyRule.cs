using System.Collections.Generic;
using Majipro.Converter.Generator.Generating;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Majipro.Converter.Generator.Rules;

/// <summary>
/// A <c>string</c> can not be built by an object initializer, but anything can be written as one.
/// <c>System.Convert.ToString</c> is what a hand written converter would do, and the only thing
/// that can be done without knowing the source type.
/// </summary>
/// <remarks>
/// A string the caller already has is not a target that can be filled - it is a value, and the only
/// way to change it is to return a different one. So this conversion writes no fill and the
/// generated class stays a plain <c>IConverter</c>.
/// </remarks>
internal sealed class ToStringBodyRule : IConverterBodyRule
{
    public IEnumerable<ConverterBody> Body(ConverterContext context)
    {
        if (context.Pair.To.SpecialType != SpecialType.System_String)
        {
            yield break;
        }

        yield return new ConverterBody(
            ReturnStatement(
                InvocationExpression(
                        ConverterSyntax.Member(
                            ConverterSyntax.TypeName(typeof(System.Convert)),
                            nameof(System.Convert.ToString)))
                    .WithArgumentList(
                        ArgumentList(
                            SingletonSeparatedList(
                                Argument(ConverterSyntax.From()))))));
    }
}
