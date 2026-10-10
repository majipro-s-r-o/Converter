# Majipro.Converter — 2.1.0

Changelog and migration guide. Covers `v2.0.0..v2.1.0` (PR #11 better support for conversions with types that have generics).

## Changelog

### Breaking

- **A class of your own that implements `IEnumerable` and also carries properties is no longer mapped as an object.** Converting one as the pair itself — `Convert<SourceBag, TargetBag>(from)` — was mapped property by property on 2.0.0 and is `MC0004` on 2.1.0. `List<>`, `Dictionary<>`, `HashSet<>` and arrays were never mapped this way; what changed is that non-generic collections now follow the same rule.

  Reason: admitting closed generic types made `List<A>` a class the generator could read, and `ConvertedValueRule` is asked before `CollectionValueRule` — so a `List<A> -> List<B>` property would have been "mapped" into an empty list with its capacity copied. The rule is now that a collection means the items it holds wherever it appears, rather than meaning one thing as a property and another as a pair.

  Nothing else is breaking: no type, namespace, signature or default changed.

### Added

- **Closed generic types are mapped like any other class.**

  ```csharp
  _convertingService.Convert<ObjectWithOperation<Guid, PersonDb>, ObjectWithOperation<Guid, Person>>(from);
  ```

  Both sides are mapped property by property, and the payload the two constructions differ in (`PersonDb -> Person`) is requested as a pair of its own and written in the same pass. Constraints on the type parameters are not the generator's business — the compiler has already checked them where the call is written.
  - Works for the pair itself, for a generic type as a **property**, and for one as the **items of a collection** property.
  - The reference conversion `Convert(from, to)` is generated alongside it, as for any other class target.
  - *Closed* is the operative word: the same wrapper closed over a **type parameter** of the method calling `Convert` is still skipped without a diagnostic, since that pair means nothing outside the method it is written in.

  Previously `MC0004`.

- **A pair the compiler already takes is answered by handing the value back.** Where no other rule can write the pair and the source is already assignable to the target, the generated converter returns it as it is:

  | Conversion | Example |
  | --- | --- |
  | Covariance (`out T`) | `IProducer<Dog> -> IProducer<Animal>` |
  | Contravariance (`in T`) | `IConsumer<Animal> -> IConsumer<Dog>` |
  | Framework variance | `IEnumerable<Dog> -> IEnumerable<Animal>` |
  | Implicit numeric | `short -> int` |
  | Nullable | `int -> int?` |
  | An `implicit operator` of your own | `decimal -> Money` |

  The same instance comes back and nothing is copied, so such a pair has no `IReferenceConverter` — there is no target to fill. A pair that can **also** be mapped property by property is still mapped into a new instance: `Convert<Dog, Animal>` projects rather than upcasts, because the object initializer is asked first.

  Previously `MC0004`.

### Changed

- **`MC0004` fires in fewer places.** The two shapes above used to be build errors and are now written for you. No new diagnostic ids, and no change to the meaning of the existing four.
- **A generic type constructed over itself ends the build instead of hanging it.** A `Node<T>` with a property of type `Node<Node<T>>` asks for a converter whose own property asks for the next one, without end. Names whose type arguments nest deeper than eight are no longer mapped, so such a property is reported as `MC0003` — one nothing fills — rather than the generation never coming back.

### Unchanged

`Majipro.Converter` and `Majipro.Converter.Abstrations` are untouched: no new type, no changed signature, no changed default. So are the package layout and its three assemblies, the minimum dependency versions, `DiCompositor.AddConverting` and its assembly scan, `IConverterOptions`, the built-in `string -> primitive` converters, the `ConvertExplicitly` extensions, the duplicate-registration validator, `ConvertingService`'s runtime resolution, and the netstandard2.1 target.

## Migration guide

### 1. Nothing to install or change

2.1.0 is a drop-in replacement for 2.0.0. Same package, same assemblies, same layout, same dependency floors, no `using` to add and no API to adapt to. Bump the version and build.

### 2. Build once and read the one error that is new

Only one diagnostic can appear where 2.0.0 built clean:

| Diagnostic | What it means | What to do |
| --- | --- | --- |
| `MC0004` on a type of your own that implements `IEnumerable` | The pair was mapped property by property on 2.0.0 and a collection is no longer an object to be mapped. | Convert its **items** instead — a collection property is mapped item by item, and so is `Convert<IEnumerable<A>, IEnumerable<B>>` — or write that one converter by hand. |

Everything else moves the other way: pairs that failed on `MC0004` on 2.0.0 now build. `MC0002` and `MC0003` are unchanged, except that a self-nesting generic type reaches `MC0003` where 2.0.0 would not have finished generating at all.

### 3. Optional: delete converters you wrote only to satisfy the build

If you hand-wrote a converter on 2.0.0 purely because `MC0004` demanded one — a generic wrapper mapped name for name, a variance pair, `int -> long` — you can delete it and let the generator write it. Keep it if it does anything a generated one would not: a converter you implemented is never generated over, in this assembly or in any referenced one, so keeping it costs nothing and changes nothing.

### 4. Nothing else

No behaviour change in `ConvertingService`, the built-in converters, the options or the DI validator, and no change to how registration works. A project that upgrades and builds clean needs nothing beyond step 1.
