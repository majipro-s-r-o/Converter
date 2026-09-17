using Majipro.Converter.Abstrations;
using Majipro.Converter.Generator.Test;

namespace Majipro.Converter.Generator.Test.Conversions.ImplementedConverters;

public class ImplementedConvertersComposition : TestCompositionBase
{
    public ImplementedConvertersTestCase.WrittenConverterTo ConvertWrittenConverter(
        IConvertingService convertingService,
        ImplementedConvertersTestCase.WrittenConverterFrom from)
    {
        return convertingService
            .Convert<ImplementedConvertersTestCase.WrittenConverterFrom,
                ImplementedConvertersTestCase.WrittenConverterTo>(from);
    }

    public ImplementedConvertersTestCase.WrittenReferenceTo ConvertWrittenReference(
        IConvertingService convertingService,
        ImplementedConvertersTestCase.WrittenReferenceFrom from,
        ImplementedConvertersTestCase.WrittenReferenceTo to)
    {
        return convertingService
            .Convert<ImplementedConvertersTestCase.WrittenReferenceFrom,
                ImplementedConvertersTestCase.WrittenReferenceTo>(from, to);
    }

    public ImplementedConvertersTestCase.GeneratedTo ConvertGenerated(
        IConvertingService convertingService,
        ImplementedConvertersTestCase.GeneratedFrom from)
    {
        return convertingService
            .Convert<ImplementedConvertersTestCase.GeneratedFrom,
                ImplementedConvertersTestCase.GeneratedTo>(from);
    }
}

/// <summary>
/// Half of what a generated converter would be: the value conversion, without the reference one.
/// The generator can not write the missing half without claiming this one too, so it writes nothing
/// for the pair - the reference converter is the author's to add.
/// </summary>
public class WrittenConverter :
    IConverter<ImplementedConvertersTestCase.WrittenConverterFrom, ImplementedConvertersTestCase.WrittenConverterTo>
{
    public const string Marker = "written converter: ";

    public ImplementedConvertersTestCase.WrittenConverterTo Convert(
        ImplementedConvertersTestCase.WrittenConverterFrom from)
    {
        return new ImplementedConvertersTestCase.WrittenConverterTo
        {
            Name = Marker + from.Name
        };
    }
}

/// <summary>
/// Both conversions, written by hand. Nothing is generated for the pair either.
/// </summary>
public class WrittenReferenceConverter :
    IReferenceConverter<ImplementedConvertersTestCase.WrittenReferenceFrom,
        ImplementedConvertersTestCase.WrittenReferenceTo>
{
    public const string Marker = "written reference converter: ";

    public ImplementedConvertersTestCase.WrittenReferenceTo Convert(
        ImplementedConvertersTestCase.WrittenReferenceFrom from)
    {
        return Convert(from, new ImplementedConvertersTestCase.WrittenReferenceTo());
    }

    public ImplementedConvertersTestCase.WrittenReferenceTo Convert(
        ImplementedConvertersTestCase.WrittenReferenceFrom from,
        ImplementedConvertersTestCase.WrittenReferenceTo to)
    {
        to.Name = Marker + from.Name;

        return to;
    }
}
