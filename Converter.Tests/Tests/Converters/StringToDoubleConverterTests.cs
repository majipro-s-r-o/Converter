using System.Globalization;
using System.Threading.Tasks;

namespace Majipro.Converter.Tests.Tests.Converters;

[TestClass]
public class StringToDoubleConverterTests : ConversionTestBase
{
    [TestMethod]
    [DataRow("123.45", NumberStyles.AllowDecimalPoint, "en-US", 123.45)]
    [DataRow("123,45", NumberStyles.AllowDecimalPoint, "cs-CZ", 123.45)]
    [DataRow("$ 123,456.78", NumberStyles.Currency, "en-US", 123456.78)]
    public async Task WhenInputIsCanBeDoubleThenReturnDoubleOtherwiseDefault(string source, NumberStyles numberStyles, string culture, double expected)
    {
        // Arrange
        await ClassInitializeAsync(o =>
        {
            o.NumberStyles = numberStyles;
            o.FormatProvider = new CultureInfo(culture);
        });
        
        // Act
        var actual = ConvertingService.Convert<string, double>(source);
        
        // Assert
        Assert.AreEqual(expected, actual);
    }  
}