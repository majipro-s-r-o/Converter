using System;

namespace Majipro.Converter.Generator.Test.Tests.DifferentSourcesAndTargets;

public class DifferentSourcesAndTargetsTestCase
{
    /// <summary>
    /// The source knows more than the target, the properties the target does not have are dropped.
    /// </summary>
    public class MoreProperties
    {
        public decimal Decimal { get; set; }

        public ulong UnsingnedLong { get; set; }

        public long Long { get; set; }

        public uint UnsignedInteger { get; set; }

        public int Integer { get; set; }

        public byte Byte { get; set; }

        public bool Bool { get; set; }

        public string String { get; set; }

        public char Char { get; set; }
    }

    public class FewerProperties
    {
        public decimal Decimal { get; set; }

        public long Long { get; set; }

        public int Integer { get; set; }

        public byte Byte { get; set; }

        public bool Bool { get; set; }

        public string String { get; set; }
    }

    /// <summary>
    /// The target knows more than the source, <see cref="WithId.Id"/> has nothing to come from.
    /// </summary>
    public class WithoutId
    {
        public string FirstName { get; set; }

        public string LastName { get; set; }
    }

    public class WithId
    {
        public Guid Id { get; set; }

        public string FirstName { get; set; }

        public string LastName { get; set; }
    }

    /// <summary>
    /// Both sides know something the other one does not.
    /// </summary>
    public class WithCompany
    {
        public Guid Id { get; set; }

        public string FirstName { get; set; }

        public string LastName { get; set; }

        public string Company { get; set; }
    }

    public class WithoutCompany
    {
        public Guid Id { get; set; }

        public string FirstName { get; set; }

        public string LastName { get; set; }
    }
}
