using System;
using System.Collections.Generic;

namespace Majipro.Converter.Generator.Test.Conversions.ClosedGenericTypes;

public class ClosedGenericTypesTestCase
{
    public enum Operation
    {
        Insert,
        Update,
        Delete
    }

    public interface IIdentificator<TKey>
    {
        TKey Id { get; set; }
    }

    /// <summary>
    /// A payload and the operation it arrived with, the shape a layer that carries metadata of its
    /// own is written in. Both sides of the conversion are this same definition closed over a
    /// different payload, so the wrapper is mapped property by property and the payload it differs
    /// in is a pair of its own. The constraint is part of the case: whether the type arguments
    /// satisfy it is settled where the call site is written, long before the generator is asked
    /// about the pair.
    /// </summary>
    public class ObjectWithOperation<TKey, T> where T : class, IIdentificator<TKey>
    {
        public T DataObject { get; set; }

        public Operation Operation { get; set; }
    }

    public class SourceObject : IIdentificator<Guid>
    {
        public Guid Id { get; set; }

        public string Name { get; set; }
    }

    public class TargetObject : IIdentificator<Guid>
    {
        public Guid Id { get; set; }

        public string Name { get; set; }
    }

    /// <summary>
    /// A closed generic as a property and as the items of a collection property, which are the two
    /// places a value rule has to recognize one. The collection is a <c>List&lt;T&gt;</c> on both
    /// sides on purpose: it is a closed generic as well, and the one that must not be mapped as an
    /// object - what it means is its items.
    /// </summary>
    public class From
    {
        public ObjectWithOperation<Guid, SourceObject> Event { get; set; }

        public List<ObjectWithOperation<Guid, SourceObject>> Events { get; set; }
    }

    public class To
    {
        public ObjectWithOperation<Guid, TargetObject> Event { get; set; }

        public List<ObjectWithOperation<Guid, TargetObject>> Events { get; set; }
    }
}
