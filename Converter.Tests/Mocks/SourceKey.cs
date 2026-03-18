namespace Majipro.Converter.Tests.Mocks;

public sealed record SourceKey : IHasInteger
{
    public int Integer { get; init; }
}