using System.Collections.Generic;
using Majipro.Converter.Generator.Generating;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Majipro.Converter.Generator.Rules;

/// <summary>
/// One way a whole conversion can be written. The rule yields the statement that produces the
/// result, or nothing when the conversion is not its shape. A pair no rule claims gets no converter.
/// </summary>
internal interface IConverterBodyRule
{
    IEnumerable<StatementSyntax> Body(ConverterContext context);
}
