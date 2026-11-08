namespace Majipro.Converter.Generator.Test.Validations.EnumPropertyMissingMember;

public class EnumPropertyMissingMemberTestCase
{
    public enum Wide
    {
        Unknown = 0,
        Active = 1,
        Closed = 2
    }

    public enum Narrow
    {
        Unknown = 0,
        Closed = 2
    }

    public class From
    {
        public Wide Status { get; set; }
    }

    public class To
    {
        public Narrow Status { get; set; }
    }
}
