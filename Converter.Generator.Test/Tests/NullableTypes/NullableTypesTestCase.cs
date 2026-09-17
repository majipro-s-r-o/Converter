namespace Majipro.Converter.Generator.Test.Tests.NullableTypes;

public class NullableTypesTestCase
{
    /// <summary>
    /// Converted from a nullable structure into a plain one.
    /// </summary>
    public struct NullableSourceFrom
    {
        public string SomeValue { get; set; }
    }

    public struct NullableSourceTo
    {
        public string SomeValue { get; set; }
    }

    /// <summary>
    /// Nullable on both sides, so a null source stays null.
    /// </summary>
    public struct NullableBothFrom
    {
        public string SomeValue { get; set; }
    }

    public struct NullableBothTo
    {
        public string SomeValue { get; set; }
    }

    /// <summary>
    /// Converted from a plain structure into a nullable one.
    /// </summary>
    public struct NullableTargetFrom
    {
        public string SomeValue { get; set; }
    }

    public struct NullableTargetTo
    {
        public string SomeValue { get; set; }
    }
}
