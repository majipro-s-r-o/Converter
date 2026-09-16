using System;

namespace Majipro.Converter.Generator.Test.Conversions.StringProperties;

public class StringPropertiesTestCase
{
    public enum Status
    {
        Unknown = 0,
        Active = 1
    }

    /// <summary>
    /// Every property is a value with a text form of its own and the target wants that text. The
    /// converter for each one of those pairs is written by the generator itself.
    /// </summary>
    public class ScalarFrom
    {
        public int Integer { get; set; }

        public decimal Decimal { get; set; }

        public bool Bool { get; set; }

        public Guid Id { get; set; }

        public Status Status { get; set; }

        public int? NullableInteger { get; set; }
    }

    public class TextTo
    {
        public string Integer { get; set; }

        public string Decimal { get; set; }

        public string Bool { get; set; }

        public string Id { get; set; }

        public string Status { get; set; }

        public string NullableInteger { get; set; }
    }

    /// <summary>
    /// The other direction, the one the library already has converters for. Nothing is generated
    /// for these pairs - <c>Converter\Converters</c> is what answers them, down to what an
    /// unparseable string turns into.
    /// </summary>
    public class TextFrom
    {
        public string Integer { get; set; }

        public string Long { get; set; }

        public string Decimal { get; set; }

        public string Bool { get; set; }

        public string Created { get; set; }
    }

    public class ScalarTo
    {
        public int Integer { get; set; }

        public long Long { get; set; }

        public decimal Decimal { get; set; }

        public bool Bool { get; set; }

        public DateTime Created { get; set; }
    }
}
