using Majipro.Converter.Abstrations;
using Majipro.Converter.Generator.Test;

namespace Majipro.Converter.Generator.Test.Conversions.InternalTypes;

/// <summary>
/// The types of this case are declared here rather than in a test case file, because the point of
/// the case is a type that is internal to the assembly the converter is generated into - which is
/// the assembly this file is compiled as, not the test assembly.
/// </summary>
internal class InternalFrom
{
    public string Property { get; set; }

    public InternalNestedFrom Nested { get; set; }
}

internal class InternalTo
{
    public string Property { get; set; }

    public InternalNestedTo Nested { get; set; }
}

internal class InternalNestedFrom
{
    public string Property { get; set; }
}

internal class InternalNestedTo
{
    public string Property { get; set; }
}

public class InternalTypesComposition : TestCompositionBase
{
    internal InternalTo Convert(IConvertingService convertingService, InternalFrom from)
    {
        return convertingService.Convert<InternalFrom, InternalTo>(from);
    }
}
