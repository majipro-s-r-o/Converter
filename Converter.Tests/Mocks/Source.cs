namespace Majipro.Converter.Tests.Mocks;

public sealed record Source : IHasInteger
{
    public int Integer { get; init; }
}