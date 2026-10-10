namespace Majipro.Converter.Generator.Test.Conversions.ClosedGenericCallSite;

public class ClosedGenericCallSiteTestCase
{
    /// <summary>
    /// The target every source below is converted into. It knows more members than any of them and
    /// numbers them differently, so a converter that cast instead of mapping by name would show.
    /// </summary>
    public enum VariableKind
    {
        Node,
        Variable,
        Template,
        Link,
        QrCode,
        Barcode
    }

    public sealed class NotificationVariable
    {
        public string Key { get; set; }

        public NotificationKind Kind { get; set; }

        public enum NotificationKind
        {
            Variable,
            Template,
            Link
        }
    }

    public sealed class PrintVariable
    {
        public string Key { get; set; }

        public PrintKind Kind { get; set; }

        public enum PrintKind
        {
            Variable,
            QrCode,
            Barcode,
            Template
        }
    }

    public enum RenamedKind
    {
        Link,
        Template
    }

    public enum ExtensionKind
    {
        Barcode,
        QrCode
    }

    public enum MapperKind
    {
        Template,
        Variable
    }

    public enum LocalKind
    {
        QrCode,
        Link
    }
}
