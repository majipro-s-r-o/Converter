namespace Majipro.Converter.Tests.Mocks;

public sealed record TargetRef : IHasInteger
{
    public int Integer { get; init; }

    public string Text { get; init; }
}