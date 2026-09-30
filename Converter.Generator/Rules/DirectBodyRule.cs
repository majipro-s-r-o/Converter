using System.Collections.Generic;
using Majipro.Converter.Generator.Generating;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Majipro.Converter.Generator.Rules;

/// <summary>
/// The compiler would take the source value for the target as it is, so the converter hands it back
/// and writes nothing. It is <see cref="DirectValueRule"/>'s question asked of the whole pair
/// instead of one property: an implicit reference conversion, variance - an
/// <c>IProducer&lt;out T&gt;</c> of a derived type already being one of the base type, an
/// <c>IConsumer&lt;in T&gt;</c> the other way round - an implicit numeric conversion, a differing
/// nullable annotation, or a conversion somebody declared as an implicit operator.
/// </summary>
/// <remarks>
/// Wired last on purpose. A pair that is assignable <em>and</em> mappable - a derived class into its
/// base - is claimed by <see cref="ObjectInitializerBodyRule"/> before this rule is asked, so it
/// keeps being converted into a new instance, which is what asking a converter for one means. What
/// reaches this rule is a pair nothing else can write, where the only alternative is <c>MC0004</c>
/// on a conversion the compiler performs by itself.
///
/// The value handed back is the one that came in, so there is nothing to fill: a caller already
/// holding a target holds something this conversion can not improve on. The pair stays a plain
/// <c>IConverter</c>.
/// </remarks>
internal sealed class DirectBodyRule : IConverterBodyRule
{
    public IEnumerable<ConverterBody> Body(ConverterContext context)
    {
        if (context.Semantics.IsAssignable(context.Pair.From, context.Pair.To) == false)
        {
            yield break;
        }

        yield return new ConverterBody(ReturnStatement(ConverterSyntax.From()));
    }
}
