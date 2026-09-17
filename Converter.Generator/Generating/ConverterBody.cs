using System.Collections.Generic;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Majipro.Converter.Generator.Generating;

/// <summary>
/// What a rule wrote for one conversion: the statement that produces a new target, and - when the
/// shape allows it - the statements that fill a target somebody else already holds.
/// </summary>
/// <remarks>
/// The second one is what decides the interface the generated class implements, and it is the rule
/// that decides whether there is one: only the rule knows whether the target is an instance that
/// can be written to after it has been created. There is nothing to ask afterwards, the same way
/// <see cref="ConverterContext.RequiresConvertingService"/> is read off what generation did.
/// </remarks>
internal sealed class ConverterBody
{
    /// <summary>Body of <c>TTo Convert(TFrom from)</c>.</summary>
    public StatementSyntax Create { get; }

    /// <summary>
    /// Body of <c>TTo Convert(TFrom from, TTo to)</c>, without the guard and the return the emitter
    /// writes around it. <c>null</c> when this conversion has no existing instance to fill, which is
    /// what keeps the generated class off <c>IReferenceConverter</c>. An empty list is not the same
    /// thing: it is a target with nothing to fill, and a reference converter that returns it
    /// untouched is the honest answer for that.
    /// </summary>
    public IReadOnlyList<StatementSyntax>? Fill { get; }

    public ConverterBody(StatementSyntax create)
    {
        Create = create;
    }

    public ConverterBody(StatementSyntax create, IReadOnlyList<StatementSyntax> fill)
    {
        Create = create;
        Fill = fill;
    }
}
