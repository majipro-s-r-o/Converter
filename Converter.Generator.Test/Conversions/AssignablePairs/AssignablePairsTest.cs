using System.Collections.Generic;

namespace Majipro.Converter.Generator.Test.Conversions.AssignablePairs;

/// <summary>
/// Pairs the compiler would take as they are, which <c>in</c> and <c>out</c> on a type argument are
/// the interesting way of being: an <c>IProducer&lt;Dog&gt;</c> already is an
/// <c>IProducer&lt;Animal&gt;</c>, so the converter for that pair hands the value back instead of
/// the build failing on a conversion the compiler performs by itself. A pair that is assignable and
/// mappable both is still converted, because the object initializer is asked first.
/// </summary>
[TestClass]
public class AssignablePairsTest : ConversionTestBase<AssignablePairsComposition>
{
    public AssignablePairsTest()
        : base(@".\Conversions\AssignablePairs\AssignablePairsComposition.cs")
    {
    }

    [TestMethod]
    public void WhenGeneratorRunsThenEveryAssignablePairIsAnswered()
    {
        AssertGeneratedWithoutDiagnostics();

        Assert.AreEqual(
            4,
            GeneratedSources.Count,
            "Three pairs the compiler takes as they are and one that is mapped are expected.");
    }

    [TestMethod]
    public void WhenTheTypeArgumentIsCovariantThenTheValueIsHandedBack()
    {
        AssertGeneratedWithoutDiagnostics();

        AssignablePairsTestCase.IProducer<AssignablePairsTestCase.Dog> from = new AssignablePairsTestCase.DogProducer();

        var to = GetConvertingService().Convert<
            AssignablePairsTestCase.IProducer<AssignablePairsTestCase.Dog>,
            AssignablePairsTestCase.IProducer<AssignablePairsTestCase.Animal>>(from);

        Assert.AreSame(from, to, "An IProducer<Dog> is an IProducer<Animal>, there is nothing to convert.");
    }

    [TestMethod]
    public void WhenTheTypeArgumentIsContravariantThenTheValueIsHandedBack()
    {
        AssertGeneratedWithoutDiagnostics();

        AssignablePairsTestCase.IConsumer<AssignablePairsTestCase.Animal> from =
            new AssignablePairsTestCase.AnimalConsumer();

        var to = GetConvertingService().Convert<
            AssignablePairsTestCase.IConsumer<AssignablePairsTestCase.Animal>,
            AssignablePairsTestCase.IConsumer<AssignablePairsTestCase.Dog>>(from);

        Assert.AreSame(from, to, "The same fact the other way round: an IConsumer<Animal> is an IConsumer<Dog>.");
    }

    [TestMethod]
    public void WhenAFrameworkInterfaceIsVariantThenTheValueIsHandedBack()
    {
        AssertGeneratedWithoutDiagnostics();

        IEnumerable<AssignablePairsTestCase.Dog> from = new List<AssignablePairsTestCase.Dog>
        {
            new AssignablePairsTestCase.Dog { Name = "Rex" }
        };

        var to = GetConvertingService().Convert<
            IEnumerable<AssignablePairsTestCase.Dog>,
            IEnumerable<AssignablePairsTestCase.Animal>>(from);

        Assert.AreSame(from, to, "IEnumerable<T> is covariant, so the collection is not rebuilt either.");
    }

    [TestMethod]
    public void WhenThePairIsHandedBackThenThereIsNoReferenceConverter()
    {
        AssertGeneratedWithoutDiagnostics();

        var converter = FindReferenceConverter<
            AssignablePairsTestCase.IProducer<AssignablePairsTestCase.Dog>,
            AssignablePairsTestCase.IProducer<AssignablePairsTestCase.Animal>>();

        Assert.IsNull(
            converter,
            "The value handed back is the one that came in, so there is no target to fill.");
    }

    [TestMethod]
    public void WhenAnAssignablePairIsMappableAsWellThenItIsConverted()
    {
        AssertGeneratedWithoutDiagnostics();

        var from = new AssignablePairsTestCase.Dog
        {
            Name = "Rex",
            Fetches = true
        };

        var to = GetConvertingService()
            .Convert<AssignablePairsTestCase.Dog, AssignablePairsTestCase.Animal>(from);

        Assert.AreNotSame(from, to, "The object initializer is asked before the pair is handed back.");
        Assert.AreEqual(from.Name, to.Name);
    }
}
