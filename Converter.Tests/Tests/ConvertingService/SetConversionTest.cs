using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Majipro.Converter.Tests.Comparers;
using Majipro.Converter.Tests.Mocks;

namespace Majipro.Converter.Tests.Tests.ConvertingService;

[TestClass]
public sealed class SetConversionTest : ConversionTestBase
{
    [ClassInitialize]
    public static async Task ThisTestClassInitializeAsync(TestContext context)
    {
        await ClassInitializeAsync();
    }

    [TestMethod]
    public void WhenConvertingBetweenTwoHashSetsWithoutFallbackThenConvertAllItems()
    {
        AssertConversionWithoutFallback<Source, Target>();
    }

    [TestMethod]
    public void WhenConvertingBetweenTwoHashSetsWithoutFallbackUsingAsyncConverterThenConvertAllItems()
    {
        AssertConversionWithoutFallback<SourceAsync, TargetAsync>();
    }

    [TestMethod]
    public void WhenConvertingBetweenTwoHashSetsWithFallbackThenConvertAllItems()
    {
        AssertConversionWithFallback<Source, Target>();
    }

    [TestMethod]
    public void WhenConvertingBetweenTwoHashSetsWithFallbackUsingAsyncConverterThenConvertAllItems()
    {
        AssertConversionWithFallback<SourceAsync, TargetAsync>();
    }

    [TestMethod]
    public void WhenConvertingBetweenTwoHashSetWithFallbackThenUseFallbackIfInputIsNull()
    {
        AssertFallbackIfInputIsNull<Source, Target>();
    }

    [TestMethod]
    public void WhenConvertingBetweenTwoHashSetWithFallbackUsingAsyncConverterThenUseFallbackIfInputIsNull()
    {
        AssertFallbackIfInputIsNull<SourceAsync, TargetAsync>();
    }

    private void AssertConversionWithoutFallback<TSource, TTarget>()
        where TSource : IHasInteger, new()
        where TTarget : IHasInteger
    {
        // Arrange
        var expected = new HashSet<TSource>
        {
            new TSource
            {
                Integer = 1
            }
        };

        // Act
        var actual = ConvertingService
            .Convert<TSource, TTarget>(expected)
            .ToHashSet();

        // Assert
        HashSetAssert.AreEqual(expected, actual, new HasIntegerComparer());
    }

    private void AssertConversionWithFallback<TSource, TTarget>()
        where TSource : IHasInteger, new()
        where TTarget : IHasInteger
    {
        // Arrange
        var expected = new HashSet<TSource>
        {
            new TSource
            {
                Integer = 1
            }
        };

        // Act
        var actual = ConvertingService
            .Convert<TSource, TTarget>(expected, () => new HashSet<TTarget>())
            .ToHashSet();

        // Assert
        HashSetAssert.AreEqual(expected, actual, new HasIntegerComparer());
    }

    private void AssertFallbackIfInputIsNull<TSource, TTarget>()
        where TSource : IHasInteger, new()
        where TTarget : IHasInteger
    {
        // Act
        var actual = ConvertingService
            .Convert<TSource, TTarget>(null, () => new List<TTarget>())
            .ToHashSet();

        // Assert
        HashSetAssert.AreEqual(new HashSet<TTarget>(), actual, new HasIntegerComparer());
    }

    [TestMethod]
    public async ValueTask WhenConvertingUsingAsyncOverloadBetweenTwoHashSetsWithoutFallbackThenConvertAllItems()
    {
        await AssertConversionWithoutFallbackAsync<Source, Target>();
    }

    [TestMethod]
    public async ValueTask WhenConvertingUsingAsyncOverloadBetweenTwoHashSetsWithoutFallbackUsingAsyncConverterThenConvertAllItems()
    {
        await AssertConversionWithoutFallbackAsync<SourceAsync, TargetAsync>();
    }

    [TestMethod]
    public async ValueTask WhenConvertingUsingAsyncOverloadBetweenTwoHashSetsWithFallbackThenConvertAllItems()
    {
        await AssertConversionWithFallbackAsync<Source, Target>();
    }

    [TestMethod]
    public async ValueTask WhenConvertingUsingAsyncOverloadBetweenTwoHashSetsWithFallbackUsingAsyncConverterThenConvertAllItems()
    {
        await AssertConversionWithFallbackAsync<SourceAsync, TargetAsync>();
    }

    [TestMethod]
    public async ValueTask WhenConvertingUsingAsyncOverloadBetweenTwoHashSetsWithFallbackThenUseFallbackIfInputIsNull()
    {
        await AssertFallbackIfInputIsNullAsync<Source, Target>();
    }

    [TestMethod]
    public async ValueTask WhenConvertingUsingAsyncOverloadBetweenTwoHashSetsWithFallbackUsingAsyncConverterThenUseFallbackIfInputIsNull()
    {
        await AssertFallbackIfInputIsNullAsync<SourceAsync, TargetAsync>();
    }

    private async ValueTask AssertConversionWithoutFallbackAsync<TSource, TTarget>()
        where TSource : IHasInteger, new()
        where TTarget : IHasInteger
    {
        // Arrange
        var expected = new HashSet<TSource>
        {
            new TSource
            {
                Integer = 1
            }
        };

        // Act
        var actual = (await ConvertingService
            .ConvertAsync<TSource, TTarget>(expected))
            .ToHashSet();

        // Assert
        HashSetAssert.AreEqual(expected, actual, new HasIntegerComparer());
    }

    private async ValueTask AssertConversionWithFallbackAsync<TSource, TTarget>()
        where TSource : IHasInteger, new()
        where TTarget : IHasInteger
    {
        // Arrange
        var expected = new HashSet<TSource>
        {
            new TSource
            {
                Integer = 1
            }
        };

        // Act
        var actual = (await ConvertingService
            .ConvertAsync<TSource, TTarget>(expected, () => new HashSet<TTarget>()))
            .ToHashSet();

        // Assert
        HashSetAssert.AreEqual(expected, actual, new HasIntegerComparer());
    }

    private async ValueTask AssertFallbackIfInputIsNullAsync<TSource, TTarget>()
        where TSource : IHasInteger, new()
        where TTarget : IHasInteger
    {
        // Act
        var actual = (await ConvertingService
            .ConvertAsync<TSource, TTarget>((ISet<TSource>)null, () => new HashSet<TTarget>()))
            .ToHashSet();

        // Assert
        HashSetAssert.AreEqual(new HashSet<TTarget>(), actual, new HasIntegerComparer());
    }
}
