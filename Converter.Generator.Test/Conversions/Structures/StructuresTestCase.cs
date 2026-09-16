using System;

namespace Majipro.Converter.Generator.Test.Conversions.Structures;

public class StructuresTestCase
{
    /// <summary>
    /// A value is assignable to its nullable counterpart, so no converter is needed for the property.
    /// </summary>
    public class ClassWithId
    {
        public Guid Id { get; set; }
    }

    public class ClassWithNullableId
    {
        public Guid? Id { get; set; }
    }

    public struct StructWithId
    {
        public Guid Id { get; set; }
    }

    public struct OtherStructWithId
    {
        public Guid Id { get; set; }
    }

    public class ClassSource
    {
        public Guid Id { get; set; }
    }

    public struct StructTarget
    {
        public Guid Id { get; set; }
    }

    public struct StructSource
    {
        public Guid Id { get; set; }
    }

    public class ClassTarget
    {
        public Guid Id { get; set; }
    }

    /// <summary>
    /// A nullable annotation is not a different type, the property is mapped either way.
    /// </summary>
#nullable enable
    public record RecordWithAnnotatedId
    {
        public string? Id { get; set; }
    }
#nullable restore

    public class ClassWithStringId
    {
        public string Id { get; set; }
    }
}
