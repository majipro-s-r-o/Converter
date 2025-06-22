namespace Majipro.Converter.Tests.Mocks;

internal sealed class SourceValueToTargetValueConverter : IConverter<SourceValue, TargetValue>
{
    public TargetValue Convert(SourceValue from)
    {
        return new TargetValue
        {
            Integer = from.Integer
        };
    }
}