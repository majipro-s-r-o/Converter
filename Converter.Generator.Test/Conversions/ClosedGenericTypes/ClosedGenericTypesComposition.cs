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
    /// something only inside this method, so nothing is written for the pair as it stands here. What
    /// it is closed over is decided by <see cref="ConvertSourcePayload"/>, and that pair is the one
    /// <see cref="ConvertWrapper"/> already asks for - so it costs no converter of its own either.
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

    public ClosedGenericTypesTestCase.ObjectWithOperation<Guid, ClosedGenericTypesTestCase.TargetObject>
        ConvertSourcePayload(
            IConvertingService convertingService,
            ClosedGenericTypesTestCase.ObjectWithOperation<Guid, ClosedGenericTypesTestCase.SourceObject> from)
    {
        return ConvertPayload(convertingService, from);
    }
}
