using System.Collections.Generic;

namespace Majipro.Converter.Generator.Test.Conversions.EnumProperties;

public class EnumPropertiesTestCase
{
    /// <summary>
    /// Each side declares its own enum inside itself, the shape a layered application ends up with:
    /// two types of the same name and the same members, which are still two different types.
    /// </summary>
    public class Person
    {
        public enum Kind
        {
            Unknown = 0,
            Employee = 1,
            Contractor = 2
        }

        public Kind Status { get; set; }

        public Kind? Previous { get; set; }

        public List<Kind> History { get; set; } = new();
    }

    /// <summary>
    /// The same names numbered differently, which the mapping is not allowed to care about.
    /// </summary>
    public class PersonDto
    {
        public enum Kind
        {
            Unknown = 10,
            Employee = 20,
            Contractor = 30
        }

        public Kind Status { get; set; }

        public Kind? Previous { get; set; }

        public List<Kind> History { get; set; } = new();
    }
}
