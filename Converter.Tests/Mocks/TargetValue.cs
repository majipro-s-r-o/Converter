namespace Majipro.Converter.Tests.Mocks;

public sealed record TargetValue : IHasInteger
{
    public int Integer { get; init; }
}