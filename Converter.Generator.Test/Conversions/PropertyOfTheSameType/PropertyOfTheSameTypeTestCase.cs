namespace Majipro.Converter.Generator.Test.Conversions.PropertyOfTheSameType;

public class PropertyOfTheSameTypeTestCase
{
    public class From
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
    
    public class To
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
}
