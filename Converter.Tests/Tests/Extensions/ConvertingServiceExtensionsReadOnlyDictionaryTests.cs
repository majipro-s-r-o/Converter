using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Majipro.Converter.Extensions;
using Majipro.Converter.Tests.Comparers;
using Majipro.Converter.Tests.Mocks;

namespace Majipro.Converter.Tests.Tests.Extensions;

[TestClass]
public sealed class ConvertingServiceExtensionsReadOnlyDictionaryTests : ConversionTestBase
{
    [ClassInitialize]
    public static async Task ThisTestClassInitializeAsync(TestContext context)
    {
        await ClassInitializeAsync();
    }
    
    [TestMethod]
    public void WhenConvertingBetweenTwoReadOnlyDictionariesWhereKeyIsStringWithoutFallbackThenConvertAllItems()
    {
        // Arrange
        var expected = new Dictionary<string, SourceValue>
        {
            {
                "1",
                new SourceValue
                {
                    Integer = 1
                }
            }
        };

        // Act
        var actual = ConvertingService
            .ConvertExplicitly<SourceValue, TargetValue>(expected);
        
        // Assert
        DictionaryAssert.AreEqual(expected, actual, new SourceValueTargetValueComparer());
    }
    
    [TestMethod]
    public void WhenConvertingBetweenTwoReadOnlyDictionariesWhereKeyIsIntWithoutFallbackThenConvertAllItems()
    {
        // Arrange
        var expected = new Dictionary<int, SourceValue>
        {
            {
                1,
                new SourceValue
                {
                    Integer = 1
                }
            }
        };

        // Act
        var actual = ConvertingService
            .ConvertExplicitly<SourceValue, TargetValue>(expected);
        
        // Assert
        DictionaryAssert.AreEqual(expected, actual, new SourceValueTargetValueComparer());
    }
    
    [TestMethod]
    public void WhenConvertingBetweenTwoReadOnlyDictionariesWhereKeyIsGuidWithoutFallbackThenConvertAllItems()
    {
        // Arrange
        var expected = new Dictionary<Guid, SourceValue>
        {
            {
                Guid.Parse("15bdbd7b-d264-4bb7-a4fc-8a412f0cb940"),
                new SourceValue
                {
                    Integer = 1
                }
            }
        };

        // Act
        var actual = ConvertingService
            .ConvertExplicitly<SourceValue, TargetValue>(expected);
        
        // Assert
        DictionaryAssert.AreEqual(expected, actual, new SourceValueTargetValueComparer());
    }
    
    [TestMethod]
    public void WhenConvertingBetweenTwoReadOnlyDictionariesWhereKeyIsStringWithFallbackThenConvertAllItems()
    {
        // Arrange
        var expected = new Dictionary<string, SourceValue>
        {
            {
                "1",
                new SourceValue
                {
                    Integer = 1
                }
            }
        } as IReadOnlyDictionary<string, SourceValue>;

        // Act
        var actual = ConvertingService
            .ConvertExplicitly<SourceValue, TargetValue>(expected, () => new Dictionary<string, TargetValue>());
        
        // Assert
        DictionaryAssert.AreEqual(expected, actual, new SourceValueTargetValueComparer());
    }
    
    [TestMethod]
    public void WhenConvertingBetweenTwoReadOnlyDictionariesWhereKeyIsIntWithFallbackThenConvertAllItems()
    {
        // Arrange
        var expected = new Dictionary<int, SourceValue>
        {
            {
                1,
                new SourceValue
                {
                    Integer = 1
                }
            }
        } as IReadOnlyDictionary<int, SourceValue>;

        // Act
        var actual = ConvertingService
            .ConvertExplicitly<SourceValue, TargetValue>(expected, () => new Dictionary<int, TargetValue>());
        
        // Assert
        DictionaryAssert.AreEqual(expected, actual, new SourceValueTargetValueComparer());
    }
    
    [TestMethod]
    public void WhenConvertingBetweenTwoReadOnlyDictionariesWhereKeyIsGuidWithFallbackThenConvertAllItems()
    {
        // Arrange
        var expected = new Dictionary<Guid, SourceValue>
        {
            {
                Guid.Parse("15bdbd7b-d264-4bb7-a4fc-8a412f0cb940"),
                new SourceValue
                {
                    Integer = 1
                }
            }
        } as IReadOnlyDictionary<Guid, SourceValue>;

        // Act
        var actual = ConvertingService
            .ConvertExplicitly<SourceValue, TargetValue>(expected, () => new Dictionary<Guid, TargetValue>());
        
        // Assert
        DictionaryAssert.AreEqual(expected, actual, new SourceValueTargetValueComparer());
    }
}