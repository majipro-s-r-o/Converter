using System.Collections.Generic;

namespace Majipro.Converter.Generator.Test.Conversions.EnumProperties;

/// <summary>
/// An enum as the type of a property rather than as the pair of a call site: the property asks for
/// the enum pair, and the enum pair is written by name exactly as if somebody had asked for it
/// directly.
/// </summary>
[TestClass]
public class EnumPropertiesTest : ConversionTestBase<EnumPropertiesComposition>
{
    public EnumPropertiesTest()
        : base(@".\Conversions\EnumProperties\EnumPropertiesComposition.cs")
    {
    }

    [TestMethod]
    public void WhenGeneratorRunsThenThereAreNoDiagnostics()
    {
        AssertGeneratedWithoutDiagnostics();

        Assert.AreEqual(
            3,
            GeneratedSources.Count,
            "Expected the object, the enum and the nullable enum converters.");
    }

    [TestMethod]
    public void WhenThePropertyIsAnEnumThenTheMemberIsMappedByNameAndNotByValue()
    {
        AssertGeneratedWithoutDiagnostics();

        var to = Convert(new EnumPropertiesTestCase.Person
        {
            Status = EnumPropertiesTestCase.Person.Kind.Contractor
        });

        Assert.AreEqual(EnumPropertiesTestCase.PersonDto.Kind.Contractor, to.Status);
    }

    [TestMethod]
    public void WhenThePropertyIsANullableEnumThenTheMemberIsMapped()
    {
        AssertGeneratedWithoutDiagnostics();

        var to = Convert(new EnumPropertiesTestCase.Person
        {
            Previous = EnumPropertiesTestCase.Person.Kind.Employee
        });

        Assert.AreEqual(EnumPropertiesTestCase.PersonDto.Kind.Employee, to.Previous);
    }

    [TestMethod]
    public void WhenTheNullableEnumPropertyIsNullThenTheTargetIsNull()
    {
        AssertGeneratedWithoutDiagnostics();

        var to = Convert(new EnumPropertiesTestCase.Person { Previous = null });

        Assert.IsNull(to.Previous);
    }

    [TestMethod]
    public void WhenThePropertyIsACollectionOfEnumsThenEveryItemIsMapped()
    {
        AssertGeneratedWithoutDiagnostics();

        var to = Convert(new EnumPropertiesTestCase.Person
        {
            History = new List<EnumPropertiesTestCase.Person.Kind>
            {
                EnumPropertiesTestCase.Person.Kind.Unknown,
                EnumPropertiesTestCase.Person.Kind.Contractor
            }
        });

        CollectionAssert.AreEqual(
            new[] { EnumPropertiesTestCase.PersonDto.Kind.Unknown, EnumPropertiesTestCase.PersonDto.Kind.Contractor },
            to.History);
    }

    private EnumPropertiesTestCase.PersonDto Convert(EnumPropertiesTestCase.Person from)
    {
        return GetConvertingService()
            .Convert<EnumPropertiesTestCase.Person, EnumPropertiesTestCase.PersonDto>(from);
    }
}
