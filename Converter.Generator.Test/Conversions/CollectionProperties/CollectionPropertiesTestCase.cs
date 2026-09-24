using System;
using System.Collections.Generic;

namespace Majipro.Converter.Generator.Test.Conversions.CollectionProperties;

public class CollectionPropertiesTestCase
{
    /// <summary>
    /// Every collection type a generated <c>List&lt;T&gt;</c> is assigned to, on one pair. The
    /// items differ, so each property has to go through the converting service one item at a time.
    /// </summary>
    public class From
    {
        public List<FromItem> ListItems { get; set; }

        public IList<FromItem> ListInterfaceItems { get; set; }

        public ICollection<FromItem> CollectionItems { get; set; }

        public IEnumerable<FromItem> EnumerableItems { get; set; }

        public IReadOnlyCollection<FromItem> ReadOnlyCollectionItems { get; set; }

        /// <summary>An array is a source the generator reads, never a target it builds.</summary>
        public FromItem[] ArrayItems { get; set; }

        public class FromItem
        {
            public string Value { get; set; }
        }
    }

    public class To
    {
        public List<ToItem> ListItems { get; set; }

        public IList<ToItem> ListInterfaceItems { get; set; }

        public ICollection<ToItem> CollectionItems { get; set; }

        public IEnumerable<ToItem> EnumerableItems { get; set; }

        public IReadOnlyCollection<ToItem> ReadOnlyCollectionItems { get; set; }

        public List<ToItem> ArrayItems { get; set; }

        public class ToItem
        {
            public string Value { get; set; }
        }
    }

    /// <summary>
    /// The items are the same type on both sides, only the collection around them differs. There is
    /// no item converter to write, but the collection still has to be rebuilt - which is the whole
    /// difference from a property the compiler could have assigned as it is.
    /// </summary>
    public class SameItemsFrom
    {
        public string[] Tags { get; set; }

        public IReadOnlyCollection<Guid> Ids { get; set; }
    }

    public class SameItemsTo
    {
        public List<string> Tags { get; set; }

        public List<Guid> Ids { get; set; }
    }

    /// <summary>
    /// Items that are a scalar on one side and its text on the other. It is the same pair a
    /// property of those two types would be, so the item converter is the built-in one of
    /// <c>Converter\Converters</c> on the way in and a generated one on the way out.
    /// </summary>
    public class ScalarItemsFrom
    {
        public IReadOnlyList<string> Counts { get; set; }

        public List<int> Numbers { get; set; }
    }

    public class ScalarItemsTo
    {
        public List<int> Counts { get; set; }

        public List<string> Numbers { get; set; }
    }
}
