using System;
using System.Collections.Generic;
using System.Linq;

// The two constructions under test, written down once. Every name in this file is the same type the
// generated converter talks about, and a closed generic spelled out in full reads as noise.
using SourceWrapper = Majipro.Converter.Generator.Test.Conversions.ClosedGenericTypes
    .ClosedGenericTypesTestCase.ObjectWithOperation<System.Guid,
        Majipro.Converter.Generator.Test.Conversions.ClosedGenericTypes.ClosedGenericTypesTestCase.SourceObject>;
using TargetWrapper = Majipro.Converter.Generator.Test.Conversions.ClosedGenericTypes
    .ClosedGenericTypesTestCase.ObjectWithOperation<System.Guid,
        Majipro.Converter.Generator.Test.Conversions.ClosedGenericTypes.ClosedGenericTypesTestCase.TargetObject>;

namespace Majipro.Converter.Generator.Test.Conversions.ClosedGenericTypes;

/// <summary>
/// A generic type closed over types is a type like any other: the pair is named by writing it down,
/// the properties mapped are the properties of that construction, and the payload the two
/// constructions differ in is a pair of its own. Closed is the operative word - the same wrapper
/// closed over a type parameter is skipped, which is what the third call site of the composition is
/// there for.
/// </summary>
[TestClass]
public class ClosedGenericTypesTest : ConversionTestBase<ClosedGenericTypesComposition>
{
    public ClosedGenericTypesTest()
        : base(@".\Conversions\ClosedGenericTypes\ClosedGenericTypesComposition.cs")
    {
    }

    [TestMethod]
    public void WhenGeneratorRunsThenOnlyTheClosedPairsGetAConverter()
    {
        AssertGeneratedWithoutDiagnostics();

        Assert.AreEqual(
            3,
            GeneratedSources.Count,
            "The wrapper, the payload it is closed over and the class holding both are expected, " +
            "and nothing for the call site closed over a type parameter.");
    }

    [TestMethod]
    public void WhenTheTypeIsAClosedGenericThenItIsMappedPropertyByProperty()
    {
        AssertGeneratedWithoutDiagnostics();

        var from = GetWrapper();

        var to = GetConvertingService().Convert<SourceWrapper, TargetWrapper>(from);

        Assert.IsNotNull(to);
        Assert.AreEqual(ClosedGenericTypesTestCase.Operation.Update, to.Operation);
        Assert.IsNotNull(to.DataObject, "The payload is a pair of its own, and it is converted too.");
        Assert.AreEqual(from.DataObject.Id, to.DataObject.Id);
        Assert.AreEqual(from.DataObject.Name, to.DataObject.Name);
    }

    [TestMethod]
    public void WhenThePayloadIsNullThenTheWrapperIsStillConverted()
    {
        AssertGeneratedWithoutDiagnostics();

        var from = new SourceWrapper
        {
            DataObject = null,
            Operation = ClosedGenericTypesTestCase.Operation.Delete
        };

        var to = GetConvertingService().Convert<SourceWrapper, TargetWrapper>(from);

        Assert.IsNotNull(to);
        Assert.IsNull(to.DataObject);
        Assert.AreEqual(ClosedGenericTypesTestCase.Operation.Delete, to.Operation);
    }

    [TestMethod]
    public void WhenAClosedGenericIsAPropertyThenItGoesThroughItsOwnConverter()
    {
        AssertGeneratedWithoutDiagnostics();

        var from = new ClosedGenericTypesTestCase.From
        {
            Event = GetWrapper(),
            Events = new List<SourceWrapper>()
        };

        var to = GetConvertingService()
            .Convert<ClosedGenericTypesTestCase.From, ClosedGenericTypesTestCase.To>(from);

        Assert.IsNotNull(to.Event);
        Assert.AreEqual(from.Event.Operation, to.Event.Operation);
        Assert.AreEqual(from.Event.DataObject.Name, to.Event.DataObject.Name);
    }

    [TestMethod]
    public void WhenAClosedGenericIsTheItemOfACollectionThenEveryItemIsConverted()
    {
        AssertGeneratedWithoutDiagnostics();

        var from = new ClosedGenericTypesTestCase.From
        {
            Event = GetWrapper(),
            Events = new List<SourceWrapper>
            {
                GetWrapper("first"),
                GetWrapper("second")
            }
        };

        var to = GetConvertingService()
            .Convert<ClosedGenericTypesTestCase.From, ClosedGenericTypesTestCase.To>(from);

        Assert.IsNotNull(to.Events);
        Assert.AreEqual(2, to.Events.Count);
        Assert.AreEqual("first", to.Events.First().DataObject.Name);
        Assert.AreEqual("second", to.Events.Last().DataObject.Name);
    }

    [TestMethod]
    public void WhenTheTargetIsGivenThenItIsTheOneThatComesBack()
    {
        AssertGeneratedWithoutDiagnostics();

        var to = new TargetWrapper();

        var converted = GetReferenceConverter<SourceWrapper, TargetWrapper>().Convert(GetWrapper(), to);

        Assert.AreSame(to, converted, "A closed generic target is an instance the caller holds like any other.");
        Assert.AreEqual(ClosedGenericTypesTestCase.Operation.Update, to.Operation);
        Assert.IsNotNull(to.DataObject);
    }

    private static SourceWrapper GetWrapper(string name = "payload")
    {
        return new SourceWrapper
        {
            DataObject = new ClosedGenericTypesTestCase.SourceObject
            {
                Id = Guid.NewGuid(),
                Name = name
            },
            Operation = ClosedGenericTypesTestCase.Operation.Update
        };
    }
}
