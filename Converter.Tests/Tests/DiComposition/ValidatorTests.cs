using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Majipro.Converter.Tests.Tests.DiComposition;

[TestClass]
public sealed class ValidatorTests
{
    [TestMethod]
    public void WhenSingleConverterRegisteredThenDoNotThrow()
    {
        var types = new List<Type> { typeof(ConverterAB) };

        DiCompositionValidator.ValidateOrThrow(types);
    }

    [TestMethod]
    public void WhenMultipleDistinctConvertersRegisteredThenDoNotThrow()
    {
        var types = new List<Type> { typeof(ConverterAB), typeof(ConverterCD) };

        DiCompositionValidator.ValidateOrThrow(types);
    }

    [TestMethod]
    public void WhenDuplicateConvertersRegisteredThenThrow()
    {
        var types = new List<Type> { typeof(ConverterAB), typeof(ConverterAB) };

        Assert.ThrowsException<InvalidOperationException>(() =>
            DiCompositionValidator.ValidateOrThrow(types));
    }

    [TestMethod]
    public void WhenReferenceConverterRegisteredThenDoNotThrow()
    {
        var types = new List<Type> { typeof(ReferenceConverterEF) };

        DiCompositionValidator.ValidateOrThrow(types);
    }

    [TestMethod]
    public void WhenAsyncReferenceConverterRegisteredThenDoNotThrow()
    {
        var types = new List<Type> { typeof(AsyncReferenceConverterGH) };

        DiCompositionValidator.ValidateOrThrow(types);
    }

    [TestMethod]
    public void WhenDuplicateReferenceConvertersRegisteredThenThrow()
    {
        var types = new List<Type> { typeof(ReferenceConverterEF), typeof(ReferenceConverterEF) };

        Assert.ThrowsException<InvalidOperationException>(() =>
            DiCompositionValidator.ValidateOrThrow(types));
    }

    [TestMethod]
    public void WhenDuplicateAsyncReferenceConvertersRegisteredThenThrow()
    {
        var types = new List<Type> { typeof(AsyncReferenceConverterGH), typeof(AsyncReferenceConverterGH) };

        Assert.ThrowsException<InvalidOperationException>(() =>
            DiCompositionValidator.ValidateOrThrow(types));
    }

    [TestMethod]
    public void WhenAsyncConverterRegisteredThenDoNotThrow()
    {
        var types = new List<Type> { typeof(AsyncConverterIJ) };

        DiCompositionValidator.ValidateOrThrow(types);
    }

    [TestMethod]
    public void WhenDuplicateAsyncConvertersRegisteredThenThrow()
    {
        var types = new List<Type> { typeof(AsyncConverterIJ), typeof(AsyncConverterIJ) };

        Assert.ThrowsException<InvalidOperationException>(() =>
            DiCompositionValidator.ValidateOrThrow(types));
    }

    [TestMethod]
    public void WhenSingleTypeImplementsBothConverterAndAsyncConverterThenThrow()
    {
        var types = new List<Type> { typeof(SyncAndAsyncConverterAB) };

        Assert.ThrowsException<InvalidOperationException>(() =>
            DiCompositionValidator.ValidateOrThrow(types));
    }

    #region Test types

    private sealed class TypeA;
    private sealed class TypeB;
    private sealed class TypeC;
    private sealed class TypeD;
    private sealed class TypeE;
    private sealed class TypeF;
    private sealed class TypeG;
    private sealed class TypeH;
    private sealed class TypeI;
    private sealed class TypeJ;

    private sealed class ConverterAB : IConverter<TypeA, TypeB>
    {
        public TypeB Convert(TypeA from) => new();
    }

    private sealed class ConverterCD : IConverter<TypeC, TypeD>
    {
        public TypeD Convert(TypeC from) => new();
    }

    private sealed class ReferenceConverterEF : IReferenceConverter<TypeE, TypeF>
    {
        public TypeF Convert(TypeE from) => new();
        public TypeF Convert(TypeE from, TypeF to) => to;
    }

    private sealed class AsyncConverterIJ : IAsyncConverter<TypeI, TypeJ>
    {
        public Task<TypeJ> ConvertAsync(TypeI from, CancellationToken cancellationToken) =>
            Task.FromResult(new TypeJ());
    }

    private sealed class AsyncReferenceConverterGH : IAsyncReferenceConverter<TypeG, TypeH>
    {
        public Task<TypeH> ConvertAsync(TypeG from, CancellationToken cancellationToken) =>
            Task.FromResult(new TypeH());

        public Task<TypeH> ConvertAsync(TypeG from, TypeH to, CancellationToken cancellationToken) =>
            Task.FromResult(to);
    }

    // Autowiring wire just classes, we want to prevent autowire this
    private struct SyncAndAsyncConverterAB : IConverter<TypeA, TypeB>, IAsyncConverter<TypeA, TypeB>
    {
        public TypeB Convert(TypeA from) => new();

        public Task<TypeB> ConvertAsync(TypeA from, CancellationToken cancellationToken) =>
            Task.FromResult(new TypeB());
    }

    #endregion
}
