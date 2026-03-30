namespace Majipro.Converter.Tests.Mocks;

public sealed record SourceRef : IHasInteger
{
    public int Integer { get; init; }
}