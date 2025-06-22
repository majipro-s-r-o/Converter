namespace Majipro.Converter.Tests.Mocks;

internal sealed  class SourceKeyToTargetKeyConverter : IConverter<SourceKey, TargetKey>
{
    public TargetKey Convert(SourceKey from)
    {
        return new TargetKey
        {
            Integer = from.Integer
        };
    }
}