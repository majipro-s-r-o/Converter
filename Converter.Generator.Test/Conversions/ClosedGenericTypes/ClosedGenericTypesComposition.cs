using System;
using Majipro.Converter.Abstrations;
using Majipro.Converter.Generator.Test;

namespace Majipro.Converter.Generator.Test.Conversions.ClosedGenericTypes;

public class ClosedGenericTypesComposition : TestCompositionBase
{
    public ClosedGenericTypesTestCase.ObjectWithOperation<Guid, ClosedGenericTypesTestCase.TargetObject>
        ConvertWrapper(
            IConvertingService convertingService,
            ClosedGenericTypesTestCase.ObjectWithOperation<Guid, ClosedGenericTypesTestCase.SourceObject> from)
    {
        return convertingService.Convert<
            ClosedGenericTypesTestCase.ObjectWithOperation<Guid, ClosedGenericTypesTestCase.SourceObject>,
            ClosedGenericTypesTestCase.ObjectWithOperation<Guid, ClosedGenericTypesTestCase.TargetObject>>(from);
    }

    public ClosedGenericTypesTestCase.To ConvertProperties(
        IConvertingService convertingService,
        ClosedGenericTypesTestCase.From from)
    {
        return convertingService.Convert<ClosedGenericTypesTestCase.From, ClosedGenericTypesTestCase.To>(from);
    }

    /// <summary>
    /// The same wrapper closed over a type parameter instead of over a type. Closed is what makes a
    /// generic type one the generator writes down, and this one is not: <c>TPayload</c> means
    /// something only inside this method, so the pair is skipped the way any other call site naming
    /// a type parameter is - without a word, and without taking the rest of the compilation with it.
    /// </summary>
    public ClosedGenericTypesTestCase.ObjectWithOperation<Guid, ClosedGenericTypesTestCase.TargetObject>
        ConvertPayload<TPayload>(
            IConvertingService convertingService,
            ClosedGenericTypesTestCase.ObjectWithOperation<Guid, TPayload> from)
        where TPayload : class, ClosedGenericTypesTestCase.IIdentificator<Guid>
    {
        return convertingService.Convert<
            ClosedGenericTypesTestCase.ObjectWithOperation<Guid, TPayload>,
            ClosedGenericTypesTestCase.ObjectWithOperation<Guid, ClosedGenericTypesTestCase.TargetObject>>(from);
    }
}
