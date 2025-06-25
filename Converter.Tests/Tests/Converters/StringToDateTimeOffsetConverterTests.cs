using System;
using System.Globalization;
using System.Threading.Tasks;

namespace Majipro.Converter.Tests.Tests.Converters;

[TestClass]
public sealed class StringToDateTimeOffsetConverterTests : ConversionTestBase
{
    [TestMethod]
    [DataRow("2025-06-25T10:34:56", DateTimeStyles.RoundtripKind, "2025-06-25 10:34:56")]
    [DataRow("2025-06-25T12:34:56 ", DateTimeStyles.AllowTrailingWhite, "2025-06-25 12:34:56")]
    [DataRow("    2025-06-25T12:34:56", DateTimeStyles.AllowLeadingWhite, "2025-06-25 12:34:56")]
    [DataRow("    2025-06-25T12:34:56", DateTimeStyles.AllowLeadingWhite, "2025-06-25 12:34:56")]
    [DataRow("invalid", DateTimeStyles.RoundtripKind, "0001-01-01 00:00:00+00")]
    public async Task WhenInputIsCanBeDateTimeOffsetThenReturnDateTimeOffsetOtherwiseDefault(string source, DateTimeStyles dateTimeStyles, string expected)
    {
        // Arrange
        await ClassInitializeAsync(o =>
        {
            o.DateTimeStyles = dateTimeStyles;
        });
        
        // Act
        var actual = ConvertingService.Convert<string, DateTimeOffset>(source);
        
        // Assert
        var expectedDateTime = DateTimeOffset.Parse(expected, CultureInfo.InvariantCulture);
        
        Assert.AreEqual(expectedDateTime, actual);
    }   
}