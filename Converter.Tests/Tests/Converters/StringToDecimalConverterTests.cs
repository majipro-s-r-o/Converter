using System.Globalization;
using System.Threading.Tasks;

namespace Majipro.Converter.Tests.Tests.Converters;

[TestClass]
public sealed class StringToDecimalConverterTests : ConversionTestBase
{
    [TestMethod]
    [DataRow("123.45", NumberStyles.AllowDecimalPoint, "en-US", 123.45)]
    [DataRow("123,45", NumberStyles.AllowDecimalPoint, "cs-CZ", 123.45)]
    [DataRow("1.345,978", NumberStyles.AllowDecimalPoint | NumberStyles.AllowThousands, "es-ES", 1345.978)]
    [DataRow("invalid", NumberStyles.AllowExponent, "en-US", 0)]
    public async Task WhenInputIsCanBeDecimalThenReturnDecimalOtherwiseDefault(string source, NumberStyles numberStyles, string culture, double expected)
    {
        // Arrange
        await ClassInitializeAsync(o =>
        {
            o.NumberStyles = numberStyles;
            o.FormatProvider = new CultureInfo(culture);
        });
        
        // Act
        var actual = ConvertingService.Convert<string, decimal>(source);
        
        // Assert
        Assert.AreEqual((decimal)expected, actual);
    }  
}