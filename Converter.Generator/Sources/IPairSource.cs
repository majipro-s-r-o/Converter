using System.Collections.Generic;
using Majipro.Converter.Generator.Conversions;

namespace Majipro.Converter.Generator.Sources;

/// <summary>
/// Somewhere <c>From -&gt; To</c> pairs come from. What the generator does with them - request them
/// or take them out of circulation - is decided where the sources are wired together, not here.
/// </summary>
internal interface IPairSource
{
    IEnumerable<ConversionPair> Pairs();
}
