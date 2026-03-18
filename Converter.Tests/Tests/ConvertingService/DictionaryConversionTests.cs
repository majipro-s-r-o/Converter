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
        AssertConversionWithoutFallback<SourceKey, SourceValue, TargetKey, TargetValue>();
    }

    [TestMethod]
    public void WhenConvertingBetweenTwoDictionariesWithoutFallbackUsingAsyncConverterThenConvertAllItems()
    {
        AssertConversionWithoutFallback<SourceKeyAsync, SourceValueAsync, TargetKeyAsync, TargetValueAsync>();
    }

    [TestMethod]
    public void WhenConvertingBetweenTwoDictionariesWithFallbackThenConvertAllItems()
    {
        AssertConversionWithFallback<SourceKey, SourceValue, TargetKey, TargetValue>();
    }

    [TestMethod]
    public void WhenConvertingBetweenTwoDictionariesWithFallbackUsingAsyncConverterThenConvertAllItems()
    {
        AssertConversionWithFallback<SourceKeyAsync, SourceValueAsync, TargetKeyAsync, TargetValueAsync>();
    }

    [TestMethod]
    public void WhenConvertingBetweenTwoDictionariesWithFallbackThenUseFallbackIfInputIsNull()
    {
        AssertFallbackIfInputIsNull<SourceKey, SourceValue, TargetKey, TargetValue>();
    }

    [TestMethod]
    public void WhenConvertingBetweenTwoDictionariesWithFallbackUsingAsyncConverterThenUseFallbackIfInputIsNull()
    {
        AssertFallbackIfInputIsNull<SourceKeyAsync, SourceValueAsync, TargetKeyAsync, TargetValueAsync>();
    }

    private void AssertConversionWithoutFallback<TSourceKey, TSourceValue, TTargetKey, TTargetValue>()
        where TSourceKey : IHasInteger, new()
        where TSourceValue : IHasInteger, new()
        where TTargetKey : IHasInteger
        where TTargetValue : IHasInteger
    {
        // Arrange
        var expected = new Dictionary<TSourceKey, TSourceValue>
        {
            {
                new TSourceKey
                {
                    Integer = 1
                },
                new TSourceValue
                {
                    Integer = 1
                }
            }
        };

        // Act
        var actual = ConvertingService
            .Convert<TSourceKey, TSourceValue, TTargetKey, TTargetValue>(expected)
            .ToDictionary();

        // Assert
        DictionaryAssert.AreEqual(expected, actual, new HasIntegerComparer(), new HasIntegerComparer());
    }

    private void AssertConversionWithFallback<TSourceKey, TSourceValue, TTargetKey, TTargetValue>()
        where TSourceKey : IHasInteger, new()
        where TSourceValue : IHasInteger, new()
        where TTargetKey : IHasInteger
        where TTargetValue : IHasInteger
    {
        // Arrange
        var expected = new Dictionary<TSourceKey, TSourceValue>
        {
            {
                new TSourceKey
                {
                    Integer = 1
                },
                new TSourceValue
                {
                    Integer = 1
                }
            }
        };

        // Act
        var actual = ConvertingService
            .Convert(expected, () => new Dictionary<TTargetKey, TTargetValue>())
            .ToDictionary();

        // Assert
        DictionaryAssert.AreEqual(expected, actual, new HasIntegerComparer(), new HasIntegerComparer());
    }

    private void AssertFallbackIfInputIsNull<TSourceKey, TSourceValue, TTargetKey, TTargetValue>()
        where TSourceKey : IHasInteger, new()
        where TSourceValue : IHasInteger, new()
        where TTargetKey : IHasInteger
        where TTargetValue : IHasInteger
    {
        // Act
        var actual = ConvertingService
            .Convert<TSourceKey, TSourceValue, TTargetKey, TTargetValue>(null, () => new Dictionary<TTargetKey, TTargetValue>())
            .ToDictionary();

        // Assert
        DictionaryAssert.AreEqual(new Dictionary<TSourceKey, TSourceValue>(), actual, new HasIntegerComparer(), new HasIntegerComparer());
    }

    [TestMethod]
    public async ValueTask WhenConvertingUsingAsyncOverloadBetweenTwoDictionariesWithoutFallbackThenConvertAllItems()
    {
        await AssertConversionWithoutFallbackAsync<SourceKey, SourceValue, TargetKey, TargetValue>();
    }

    [TestMethod]
    public async ValueTask WhenConvertingUsingAsyncOverloadBetweenTwoDictionariesWithoutFallbackUsingAsyncConverterThenConvertAllItems()
    {
        await AssertConversionWithoutFallbackAsync<SourceKeyAsync, SourceValueAsync, TargetKeyAsync, TargetValueAsync>();
    }

    [TestMethod]
    public async ValueTask WhenConvertingUsingAsyncOverloadBetweenTwoDictionariesWithFallbackThenConvertAllItems()
    {
        await AssertConversionWithFallbackAsync<SourceKey, SourceValue, TargetKey, TargetValue>();
    }

    [TestMethod]
    public async ValueTask WhenConvertingUsingAsyncOverloadBetweenTwoDictionariesWithFallbackUsingAsyncConverterThenConvertAllItems()
    {
        await AssertConversionWithFallbackAsync<SourceKeyAsync, SourceValueAsync, TargetKeyAsync, TargetValueAsync>();
    }

    [TestMethod]
    public async ValueTask WhenConvertingUsingAsyncOverloadBetweenTwoDictionariesWithFallbackThenUseFallbackIfInputIsNull()
    {
        await AssertFallbackIfInputIsNullAsync<SourceKey, SourceValue, TargetKey, TargetValue>();
    }

    [TestMethod]
    public async ValueTask WhenConvertingUsingAsyncOverloadBetweenTwoDictionariesWithFallbackUsingAsyncConverterThenUseFallbackIfInputIsNull()
    {
        await AssertFallbackIfInputIsNullAsync<SourceKeyAsync, SourceValueAsync, TargetKeyAsync, TargetValueAsync>();
    }

    private async ValueTask AssertConversionWithoutFallbackAsync<TSourceKey, TSourceValue, TTargetKey, TTargetValue>()
        where TSourceKey : IHasInteger, new()
        where TSourceValue : IHasInteger, new()
        where TTargetKey : IHasInteger
        where TTargetValue : IHasInteger
    {
        // Arrange
        var expected = new Dictionary<TSourceKey, TSourceValue>
        {
            {
                new TSourceKey
                {
                    Integer = 1
                },
                new TSourceValue
                {
                    Integer = 1
                }
            }
        };

        // Act
        var actual = (await ConvertingService
            .ConvertAsync<TSourceKey, TSourceValue, TTargetKey, TTargetValue>(expected))
            .ToDictionary();

        // Assert
        DictionaryAssert.AreEqual(expected, actual, new HasIntegerComparer(), new HasIntegerComparer());
    }

    private async ValueTask AssertConversionWithFallbackAsync<TSourceKey, TSourceValue, TTargetKey, TTargetValue>()
        where TSourceKey : IHasInteger, new()
        where TSourceValue : IHasInteger, new()
        where TTargetKey : IHasInteger
        where TTargetValue : IHasInteger
    {
        // Arrange
        var expected = new Dictionary<TSourceKey, TSourceValue>
        {
            {
                new TSourceKey
                {
                    Integer = 1
                },
                new TSourceValue
                {
                    Integer = 1
                }
            }
        };

        // Act
        var actual = (await ConvertingService
            .ConvertAsync(expected, () => new Dictionary<TTargetKey, TTargetValue>()))
            .ToDictionary();

        // Assert
        DictionaryAssert.AreEqual(expected, actual, new HasIntegerComparer(), new HasIntegerComparer());
    }

    private async ValueTask AssertFallbackIfInputIsNullAsync<TSourceKey, TSourceValue, TTargetKey, TTargetValue>()
        where TSourceKey : IHasInteger, new()
        where TSourceValue : IHasInteger, new()
        where TTargetKey : IHasInteger
        where TTargetValue : IHasInteger
    {
        // Act
        var actual = (await ConvertingService
            .ConvertAsync<TSourceKey, TSourceValue, TTargetKey, TTargetValue>(null, () => new Dictionary<TTargetKey, TTargetValue>()))
            .ToDictionary();

        // Assert
        DictionaryAssert.AreEqual(new Dictionary<TSourceKey, TSourceValue>(), actual, new HasIntegerComparer(), new HasIntegerComparer());
    }
}
