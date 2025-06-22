using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Majipro.Converter.Extensions;
using Majipro.Converter.Tests.Comparers;
using Majipro.Converter.Tests.Mocks;

namespace Majipro.Converter.Tests.Tests.Extensions;

[TestClass]
public sealed class ConvertingServiceExtensionsDictionaryTests : ConversionTestBase
{
    [ClassInitialize]
    public static async Task ThisTestClassInitializeAsync(TestContext context)
    {
        await ClassInitializeAsync();
    }
    
    [TestMethod]
    public void WhenConvertingBetweenTwoDictionariesWhereKeyIsStringWithoutFallbackThenConvertAllItems()
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
    public void WhenConvertingBetweenTwoDictionariesWhereKeyIsIntWithoutFallbackThenConvertAllItems()
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
    public void WhenConvertingBetweenTwoDictionariesWhereKeyIsGuidWithoutFallbackThenConvertAllItems()
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
    public void WhenConvertingBetweenTwoDictionariesWhereKeyIsStringWithFallbackThenConvertAllItems()
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
            .ConvertExplicitly<SourceValue, TargetValue>(expected, () => new Dictionary<string, TargetValue>());
        
        // Assert
        DictionaryAssert.AreEqual(expected, actual, new SourceValueTargetValueComparer());
    }
    
    [TestMethod]
    public void WhenConvertingBetweenTwoDictionariesWhereKeyIsIntWithFallbackThenConvertAllItems()
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
            .ConvertExplicitly<SourceValue, TargetValue>(expected, () => new Dictionary<int, TargetValue>());
        
        // Assert
        DictionaryAssert.AreEqual(expected, actual, new SourceValueTargetValueComparer());
    }
    
    [TestMethod]
    public void WhenConvertingBetweenTwoDictionariesWhereKeyIsGuidWithFallbackThenConvertAllItems()
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
            .ConvertExplicitly<SourceValue, TargetValue>(expected, () => new Dictionary<Guid, TargetValue>());
        
        // Assert
        DictionaryAssert.AreEqual(expected, actual, new SourceValueTargetValueComparer());
    }
    
    [TestMethod]
    public void WhenConvertingBetweenTwoDictionariesWhereKeyIsStringWithFallbackThenUseFallbackIfInputIsNull()
    {
        // Act
        var actual = ConvertingService
            .ConvertExplicitly<SourceValue, TargetValue>(null, () => new Dictionary<string, TargetValue>());
        
        // Assert
        CollectionAssert.AreEqual(Array.Empty<Target>(), actual, new SourceValueTargetValueComparer());
    }
    
    [TestMethod]
    public void WhenConvertingBetweenTwoDictionariesWhereKeyIsIntWithFallbackThenUseFallbackIfInputIsNull()
    {
        // Act
        var actual = ConvertingService
            .ConvertExplicitly<SourceValue, TargetValue>(null, () => new Dictionary<string, TargetValue>());
        
        // Assert
        CollectionAssert.AreEqual(Array.Empty<Target>(), actual, new SourceValueTargetValueComparer());
    }
    
    [TestMethod]
    public void WhenConvertingBetweenTwoDictionariesWhereKeyIsGuidWithFallbackThenUseFallbackIfInputIsNull()
    {
        // Act
        var actual = ConvertingService
            .ConvertExplicitly<SourceValue, TargetValue>(null, () => new Dictionary<Guid, TargetValue>());
        
        // Assert
        CollectionAssert.AreEqual(Array.Empty<Target>(), actual, new SourceValueTargetValueComparer());
    }
}