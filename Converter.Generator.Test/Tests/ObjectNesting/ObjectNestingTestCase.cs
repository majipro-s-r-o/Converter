using System;
using System.Collections.Generic;

namespace Majipro.Converter.Generator.Test.Tests.ObjectNesting;

public class ObjectNestingTestCase
{
    /// <summary>
    /// Four levels of nesting, every level is a different type, so every level needs a converter
    /// of its own.
    /// </summary>
    public class From
    {
        public FromLevel1 Level1 { get; set; }

        public class FromLevel1
        {
            public FromLevel2 Level2 { get; set; }

            public class FromLevel2
            {
                public FromLevel3 Level3 { get; set; }

                public class FromLevel3
                {
                    public string Level4 { get; set; }
                }
            }
        }
    }

    public class To
    {
        public ToLevel1 Level1 { get; set; }

        public class ToLevel1
        {
            public ToLevel2 Level2 { get; set; }

            public class ToLevel2
            {
                public ToLevel3 Level3 { get; set; }

                public class ToLevel3
                {
                    public string Level4 { get; set; }
                }
            }
        }
    }

    /// <summary>
    /// A collection of items that are not convertible to each other by themselves.
    /// </summary>
    public class CollectionFrom
    {
        public IReadOnlyList<FromItem> Items { get; set; }

        public class FromItem
        {
            public DateTimeOffset? First { get; set; }

            public DateTimeOffset? Second { get; set; }
        }
    }

    public class CollectionTo
    {
        public IReadOnlyList<ToItem> Items { get; set; }

        public class ToItem
        {
            public DateTimeOffset? First { get; set; }

            public DateTimeOffset? Second { get; set; }
        }
    }
}
