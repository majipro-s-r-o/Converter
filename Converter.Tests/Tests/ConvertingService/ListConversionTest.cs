using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Majipro.Converter.Tests.Comparers;
using Majipro.Converter.Tests.Mocks;

namespace Majipro.Converter.Tests.Tests.ConvertingService;

[TestClass]
public sealed class ListConversionTest : ConversionTestBase
{
    [ClassInitialize]
    public static async Task ThisTestClassInitializeAsync(TestContext context)
    {
        await ClassInitializeAsync();
    }

    [TestMethod]
    public void WhenConvertingBetweenTwoListsWithoutFallbackThenConvertAllItems()
    {
        AssertConversionWithoutFallback<Source, Target>();
    }

    [TestMethod]
    public void WhenConvertingBetweenTwoListsWithoutFallbackUsingAsyncConverterThenConvertAllItems()
    {
        AssertConversionWithoutFallback<SourceAsync, TargetAsync>();
    }

    [TestMethod]
    public void WhenConvertingBetweenTwoListsWithFallbackThenConvertAllItems()
    {
        AssertConversionWithFallback<Source, Target>();
    }

    [TestMethod]
    public void WhenConvertingBetweenTwoListsWithFallbackUsingAsyncConverterThenConvertAllItems()
    {
        AssertConversionWithFallback<SourceAsync, TargetAsync>();
    }

    [TestMethod]
    public void WhenConvertingBetweenTwoListsWithFallbackThenUseFallbackIfInputIsNull()
    {
        AssertFallbackIfInputIsNull<Source, Target>();
    }

    [TestMethod]
    public void WhenConvertingBetweenTwoListsWithFallbackUsingAsyncConverterThenUseFallbackIfInputIsNull()
    {
        AssertFallbackIfInputIsNull<SourceAsync, TargetAsync>();
    }

    private void AssertConversionWithoutFallback<TSource, TTarget>()
        where TSource : IHasInteger, new()
        where TTarget : IHasInteger
    {
        // Arrange
        var expected = new List<TSource>
        {
            new TSource
            {
                Integer = 1
            }
        };

        // Act
        var actual = ConvertingService
            .Convert<TSource, TTarget>(expected)
            .ToList();

        // Assert
        CollectionAssert.AreEqual(expected, actual, new HasIntegerComparer());
    }

    private void AssertConversionWithFallback<TSource, TTarget>()
        where TSource : IHasInteger, new()
        where TTarget : IHasInteger
    {
        // Arrange
        var expected = new List<TSource>
        {
            new TSource
            {
                Integer = 1
            }
        };

        // Act
        var actual = ConvertingService
            .Convert<TSource, TTarget>(expected, () => new List<TTarget>())
            .ToList();

        // Assert
        CollectionAssert.AreEqual(expected, actual, new HasIntegerComparer());
    }

    private void AssertFallbackIfInputIsNull<TSource, TTarget>()
        where TSource : IHasInteger, new()
        where TTarget : IHasInteger
    {
        // Act
        var actual = ConvertingService
            .Convert<TSource, TTarget>(null, () => new List<TTarget>())
            .ToList();

        // Assert
        CollectionAssert.AreEqual(new List<TTarget>(), actual, new HasIntegerComparer());
    }

    [TestMethod]
    public async ValueTask WhenConvertingUsingAsyncOverloadBetweenTwoListsWithoutFallbackThenConvertAllItems()
    {
        await AssertConversionWithoutFallbackAsync<Source, Target>();
    }

    [TestMethod]
    public async ValueTask WhenConvertingUsingAsyncOverloadBetweenTwoListsWithoutFallbackUsingAsyncConverterThenConvertAllItems()
    {
        await AssertConversionWithoutFallbackAsync<SourceAsync, TargetAsync>();
    }

    [TestMethod]
    public async ValueTask WhenConvertingUsingAsyncOverloadBetweenTwoListsWithFallbackThenConvertAllItems()
    {
        await AssertConversionWithFallbackAsync<Source, Target>();
    }

    [TestMethod]
    public async ValueTask WhenConvertingUsingAsyncOverloadBetweenTwoListsWithFallbackUsingAsyncConverterThenConvertAllItems()
    {
        await AssertConversionWithFallbackAsync<SourceAsync, TargetAsync>();
    }

    [TestMethod]
    public async ValueTask WhenConvertingUsingAsyncOverloadBetweenTwoListsWithFallbackThenUseFallbackIfInputIsNull()
    {
        await AssertFallbackIfInputIsNullAsync<Source, Target>();
    }

    [TestMethod]
    public async ValueTask WhenConvertingUsingAsyncOverloadBetweenTwoListsWithFallbackUsingAsyncConverterThenUseFallbackIfInputIsNull()
    {
        await AssertFallbackIfInputIsNullAsync<SourceAsync, TargetAsync>();
    }

    private async ValueTask AssertConversionWithoutFallbackAsync<TSource, TTarget>()
        where TSource : IHasInteger, new()
        where TTarget : IHasInteger
    {
        // Arrange
        var expected = new List<TSource>
        {
            new TSource
            {
                Integer = 1
            }
        };

        // Act
        var actual = (await ConvertingService
            .ConvertAsync<TSource, TTarget>(expected))
            .ToList();

        // Assert
        CollectionAssert.AreEqual(expected, actual, new HasIntegerComparer());
    }

    private async ValueTask AssertConversionWithFallbackAsync<TSource, TTarget>()
        where TSource : IHasInteger, new()
        where TTarget : IHasInteger
    {
        // Arrange
        var expected = new List<TSource>
        {
            new TSource
            {
                Integer = 1
            }
        };

        // Act
        var actual = (await ConvertingService
            .ConvertAsync<TSource, TTarget>(expected, () => new List<TTarget>()))
            .ToList();

        // Assert
        CollectionAssert.AreEqual(expected, actual, new HasIntegerComparer());
    }

    private async ValueTask AssertFallbackIfInputIsNullAsync<TSource, TTarget>()
        where TSource : IHasInteger, new()
        where TTarget : IHasInteger
    {
        // Act
        var actual = (await ConvertingService
            .ConvertAsync<TSource, TTarget>((IList<TSource>)null, () => new List<TTarget>()))
            .ToList();

        // Assert
        CollectionAssert.AreEqual(new List<TTarget>(), actual, new HasIntegerComparer());
    }
}
