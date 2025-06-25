namespace Majipro.Converter.Tests.Mocks;

internal sealed class SourceToTargetConverter : IConverter<Source, Target>
{
    public Target Convert(Source from)
    {
        return new Target
        {
            Integer = from.Integer
        };
    }
}