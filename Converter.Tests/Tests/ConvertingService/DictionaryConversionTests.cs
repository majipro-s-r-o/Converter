using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Majipro.Converter.Tests.Comparers;
using Majipro.Converter.Tests.Mocks;

namespace Majipro.Converter.Tests.Tests.ConvertingService;

[TestClass]
public sealed class DictionaryConversionTests : ConversionTestBase
{
    [ClassInitialize]
    public static async Task ThisTestClassInitializeAsync(TestContext context)
    {
        await ClassInitializeAsync();
    }

    [TestMethod]
    public void WhenConvertingBetweenTwoDictionariesWithoutFallbackThenConvertAllItems()
    {
        // Arrange
        var expected = new Dictionary<SourceKey, SourceValue>
        {
            {
                new SourceKey
                {
                    Integer = 1
                },
                new SourceValue
                {
                    Integer = 1
                }
            }
        };

        // Act
        var actual = ConvertingService
            .Convert<SourceKey, SourceValue, TargetKey, TargetValue>(expected)
            .ToDictionary();
        
        // Assert
        DictionaryAssert.AreEqual(expected, actual, new SourceKeyTargetKeyComparer(), new SourceValueTargetValueComparer());
    }

    [TestMethod]
    public void WhenConvertingBetweenTwoDictionariesWithFallbackThenConvertAllItems()
    {
        // Arrange
        var expected = new Dictionary<SourceKey, SourceValue>
        {
            {
                new SourceKey
                {
                    Integer = 1
                },
                new SourceValue
                {
                    Integer = 1
                }
            }
        };
        
        // Act
        var actual = ConvertingService
            .Convert(expected, () => new Dictionary<TargetKey, TargetValue>())
            .ToDictionary();

        // Assert
        DictionaryAssert.AreEqual(expected, actual, new SourceKeyTargetKeyComparer(), new SourceValueTargetValueComparer());
    }

    [TestMethod]
    public void WhenConvertingBetweenTwoDictionariesWithFallbackThenUseFallbackIfInputIsNull()
    {
        // Act
        var actual = ConvertingService
            .Convert<SourceKey, SourceValue, TargetKey, TargetValue>(null, () => new Dictionary<TargetKey, TargetValue>())
            .ToDictionary();

        // Assert
        DictionaryAssert.AreEqual(new Dictionary<SourceKey, SourceValue>(), actual, new SourceKeyTargetKeyComparer(), new SourceValueTargetValueComparer());
    }
}