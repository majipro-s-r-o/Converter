namespace Majipro.Converter.Tests.Mocks;

internal sealed class SourceRefToTargetRefReferenceConverter : IReferenceConverter<SourceRef, TargetRef>
{
    public TargetRef Convert(SourceRef from)
    {
        return new TargetRef
        {
            Integer = from.Integer
        };
    }

    public TargetRef Convert(SourceRef from, TargetRef to)
    {
        return to with
        {
            Integer = from.Integer
        };
    }
}