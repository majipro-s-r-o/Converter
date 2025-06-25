using System.Globalization;
using System.Threading.Tasks;

namespace Majipro.Converter.Tests.Tests.Converters;

[TestClass]
public sealed class StringToFloatConverterTests : ConversionTestBase
{
    [TestMethod]
    [DataRow("123.45", NumberStyles.AllowDecimalPoint, "en-US", 123.45f)]
    [DataRow("123,45", NumberStyles.AllowDecimalPoint, "cs-CZ", 123.45f)]
    [DataRow("$ 123,456.78", NumberStyles.Currency, "en-US", 123456.78f)]
    [DataRow("invalid", NumberStyles.AllowDecimalPoint, "en-US", 0)]
    public async Task WhenInputIsCanBeFloatThenReturnFloatOtherwiseDefault(string source, NumberStyles numberStyles, string culture, float expected)
    {
        // Arrange
        await ClassInitializeAsync(o =>
        {
            o.NumberStyles = numberStyles;
            o.FormatProvider = new CultureInfo(culture);
        });
        
        // Act
        var actual = ConvertingService.Convert<string, float>(source);
        
        // Assert
        Assert.AreEqual(expected, actual);
    }  
}