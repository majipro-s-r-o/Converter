using Majipro.Converter.Abstrations;
using Majipro.Converter.Generator.Test;

namespace Majipro.Converter.Generator.Test.Conversions.FileScopedNamespace;

public class FileScopedNamespaceComposition : TestCompositionBase
{
    public FileScopedNamespaceTo Convert(
        IConvertingService convertingService,
        FileScopedNamespaceFrom from)
    {
        return convertingService.Convert<FileScopedNamespaceFrom, FileScopedNamespaceTo>(from);
    }
}
