using System.Collections.Generic;
using Majipro.Converter.Generator.Generating;

namespace Majipro.Converter.Generator.Rules;

/// <summary>
/// One way a whole conversion can be written. The rule yields the body that produces the result, or
/// nothing when the conversion is not its shape. A pair no rule claims gets no converter.
/// </summary>
internal interface IConverterBodyRule
{
    IEnumerable<ConverterBody> Body(ConverterContext context);
}
