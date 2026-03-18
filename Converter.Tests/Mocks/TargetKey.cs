namespace Majipro.Converter.Tests.Mocks;

public sealed record TargetKey : IHasInteger
{
    public int Integer { get; init; }
}