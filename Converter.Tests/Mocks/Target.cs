namespace Majipro.Converter.Tests.Mocks;

public sealed record Target : IHasInteger
{
    public int Integer { get; init; }
}