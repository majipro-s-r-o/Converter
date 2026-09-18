# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Commands

```bash
dotnet restore
dotnet build                                            # whole solution (Majipro.sln)
dotnet test                                             # all test projects
dotnet test Converter.Tests/Converter.Tests.csproj      # one project
dotnet test --filter "FullyQualifiedName~ConversionTests"                 # one class
dotnet test --filter "FullyQualifiedName~WhenConvertingBetweenTwoObjects" # one test
dotnet pack --configuration Release /p:Version=1.2.3 --output artifacts   # the one shipped package
```

There is no linter/formatter step; MSTest analyzers (MSTEST####) and Roslyn warnings surface during build.

## Projects

| Project | TFM | Purpose |
| --- | --- | --- |
| `Converter.Abstrations/` | netstandard2.0 **and** netstandard2.1 | The four converter interfaces + `IConvertingService`. Shared by the library **and** the generator, so neither side identifies them by name. Two assets because it has two homes, see *Packaging*. Deliberately dependency free — `IConverterOptions` stays in `Converter/` because it exposes `ServiceLifetime`, which would drag `Microsoft.Extensions.DependencyInjection.Abstractions` into the generator's analyzer context. Not packable on its own |
| `Converter/` | netstandard2.1 | The shipped library, and the project that **is** the NuGet package `Majipro.Converter` — all three assemblies are packed from here |
| `Converter.Tests/` | net10.0 | MSTest suite for the library |
| `Converter.Generator/` | netstandard2.0 | Roslyn `ISourceGenerator` that emits `IConverter<TFrom, TTo>` implementations. netstandard2.0 is what an analyzer is, see *Packaging*. Not packable on its own |
| `Converter.Generator.Test/` | net10.0 | MSTest suite for the generator |

`Directory.Build.props` applies to every project and rewrites identity: `AssemblyName` and `RootNamespace` become `Majipro.<ProjectFolderName>`. So the folder `Converter` is namespace `Majipro.Converter`, `Converter.Tests` is `Majipro.Converter.Tests`, and `Converter.Abstrations` is `Majipro.Converter.Abstrations` (the folder name carries the typo, so the namespace and the package id do too). It also sets `TargetFramework` to netstandard2.1 — the test projects override it to net10.0 and the generator to netstandard2.0. `Converter.Abstrations` multi-targets, and because the props file sets the **singular** `TargetFramework`, it has to clear it (`<TargetFramework />`) before `TargetFrameworks` means anything.

The library targets **netstandard2.1**: no implicit usings, no nullable context, explicit `using System;` everywhere, and only APIs available on that surface. `LangVersion` is `latest`, so modern syntax (file-scoped namespaces, `record struct`) is fine.

`Converter.Extensions/` is a stale empty folder, not part of the solution.

## Dependencies

Every `PackageReference` declares a **minimum**, written as an explicit range — `Version="[2.0,)"`, never a bare number and never a pin like `[2.0]`. The point is that consumers of `Majipro.Converter` are not forced onto our build's version: the packed nuspec carries the floor, so a project already on an older `Microsoft.Extensions.DependencyInjection` keeps it.

| Package | Project | Floor | Why that number |
| --- | --- | --- | --- |
| `Microsoft.Extensions.DependencyInjection.Abstractions` | `Converter` | `[2.0,)` | Only `IServiceCollection`, `ServiceDescriptor.Describe`, `TryAdd`/`TryAddSingleton` and `GetRequiredService` are used, all present in the netstandard2.0-era 2.0.0 |
| `Microsoft.SourceLink.GitHub` | `Converter` | `[8.0.0,)` | `PrivateAssets="All"`, build time only, never reaches consumers (the .NET 8+ SDK ships SourceLink anyway) |
| `Microsoft.CodeAnalysis.CSharp` | `Converter.Generator` | `[4.3.1,)` | This is the **oldest compiler the generator can be loaded into** — `ISourceGenerator`, the `SyntaxFactory` surface and `GetTypeByMetadataName` all exist there. Raising it raises the minimum Visual Studio / SDK of everybody using the generator |
| `Microsoft.CodeAnalysis.Analyzers` | `Converter.Generator` | `[3.3.3,)` | Analyzer tooling only |
| `Microsoft.CodeAnalysis.CSharp` | `Converter.Generator.Test` | `[4.3.1,)` | Deliberately the same floor, so the suite drives the generator on the oldest Roslyn it claims to support |
| `MSTest` | both test projects | `[4.4.0,)` | `Assert.Throws<T>` (MSTest 4 dropped `Assert.ThrowsException`) and Microsoft.Testing.Platform |
| `Microsoft.NET.Test.Sdk` | both test projects | `[18.10.1,)` | What the suites are validated against |
| `Microsoft.Extensions.Hosting`, `Microsoft.Extensions.DependencyInjection` | test projects | `[8.0.0,)` | `Host.CreateDefaultBuilder` and `BuildServiceProvider` |
| `System.Threading.Tasks.Extensions` | `Converter.Abstrations`, **netstandard2.0 asset only** | `[4.5.0,)` | `ValueTask<T>` of `IConvertingService`, which netstandard2.1 has in the box and netstandard2.0 does not. `PrivateAssets="all"`, so it reaches neither the netstandard2.1 asset nor the package |

`Converter.Abstrations` has no package reference on its netstandard2.1 asset, and that one should stay that way — see the note in the projects table. The netstandard2.0 asset carries `System.Threading.Tasks.Extensions` and nothing else, because that asset is loaded into the compiler's own process.

**It is fine to raise a floor.** If a feature is needed that the declared minimum does not have, bump that number to the lowest version that has it, in the same `[X,)` form, and say in the commit message which API forced it. Do not work around a missing API to keep an old floor; the floor is a statement about what we use, not a promise.

Two things to know when changing these:

- NuGet resolves the **lowest** version in range, so `dotnet restore` here really does compile against the floors — the build and the test suites are the verification that a floor is honest. After changing one, run `dotnet restore` (twice if the first pass leaves `NETSDK1064`/`NETSDK1127` behind, it writes the assets files before the packages land) and then `dotnet build` + `dotnet test`.
- A direct reference below what a transitive dependency demands is `NU1605` (a downgrade error), not a silent bump. `Converter.Generator.Test` has to keep `Microsoft.Extensions.DependencyInjection.Abstractions` at `[8.0.0,)` because its `Microsoft.Extensions.DependencyInjection [8.0.0,)` requires that much.

## Architecture

Conversion is done by DI-registered services, not by reflection-based mapping. Three moving parts:

**Converter interfaces** (`Converter.Abstrations/I*.cs`, namespace `Majipro.Converter.Abstrations`) — four contracts users implement:
`IConverter<TFrom, TTo>` (sync, new instance), `IAsyncConverter<TFrom, TTo>`, `IReferenceConverter<TFrom, TTo>` (fills an existing target; extends `IConverter`), `IAsyncReferenceConverter<TFrom, TTo>` (extends `IAsyncConverter`). Any change to this set must be mirrored in **all** of: `DiCompositor` (scan + registration filters), `DiCompositionValidator` (duplicate detection), and `ConvertingService`'s four `Get*Delegate*` resolvers. Everything in `Converter/` needs an explicit `using Majipro.Converter.Abstrations;` — the abstractions namespace is a child of `Majipro.Converter`, not an ancestor, so it is not found implicitly.

**`DiCompositor.AddConverting`** (`Converter/DiCompositor.cs`) — the registration entry point. It scans the supplied assemblies for classes implementing any converter interface (`TypeExtensions.IsAssignable`), runs `DiCompositionValidator.ValidateOrThrow` (throws `InvalidOperationException` on two implementations claiming the same `From -> To` pair; a reference converter's own inherited `IConverter`/`IAsyncConverter` pair is deliberately not counted as a duplicate), then `TryAdd`s each *closed* interface it implements at `ConverterOptions.ServiceLifetime` (default Singleton). Built-in `string -> primitive` converters are registered **after** user converters and always as Singleton, so a user registration of the same pair wins (`TryAdd` semantics). `IConverterOptions` is registered last as a singleton instance and can be injected into user converters.

**`ConvertingService`** (`Converter/ConvertingService.cs`, internal) — the `IConvertingService` facade. Every public overload funnels into one of four private resolvers that take `IServiceProvider` and build a delegate:
1. `TFrom == TTo` → identity cast, no converter needed.
2. Prefer the sync converter for `Convert`, the sync-or-async for `ConvertAsync`.
3. Sync falling back to an async converter blocks via `Task.Run(...).GetAwaiter().GetResult()`; async falling back to a sync converter wraps the result in `ValueTask`.
4. Otherwise throw `"There is no conversion from X to Y"`.
Collection/dictionary overloads (`ISet`, `IList`, `IEnumerable`, `IDictionary`) resolve the delegate once and apply it per element; each has a `nullFallback` overload, and the no-fallback overload defaults to returning `null`.

**`Extensions/ConvertingServiceExtensions.*.cs`** — a `partial static class` split by collection type. These are thin `ConvertExplicitly` wrappers that return concrete types (`List<T>`, `T[]`, `Dictionary<K,V>`, …) instead of the interfaces `IConvertingService` returns.

### Adding a built-in converter
Add the class under `Converter/Converters/`, register it in `DiCompositorConverters.AddBuildInConverters` with `TryAddSingleton`, add a row to the README table, and add a `Converter.Tests/Tests/Converters/` test class. Built-ins read formatting behaviour from the injected `IConverterOptions` (`FormatProvider`, `NumberStyles`, `DateTimeStyles`).

## Testing conventions

- MSTest 4.x on Microsoft.Testing.Platform. `Converter.Tests/MSTestSettings.cs` parallelizes at **class** level, `Converter.Generator.Test/MSTestSettings.cs` at method level.
- `ConversionTestBase.ConvertingService` is a **static** field, and the built-in converter tests that need non-default options (`StringToDateTime*`, `StringToDecimal/Double/Float`) call `ClassInitializeAsync(configure)` from inside `[TestMethod]`s rather than `[ClassInitialize]`. Combined with class-level parallelism that host is shared mutable state across concurrently running classes — treat option-dependent tests as order-sensitive and do not add more of them without reworking the base class.
- Test names are behavioural: `When<Condition>Then<Expectation>`.
- `Converter.Tests/Tests/ConversionTestBase.cs` spins up a real generic `Host` and calls `AddConverting<ListConversionTest>(...)`, i.e. **the whole `Converter.Tests` assembly is scanned**. Consequence: mock converters under `Converter.Tests/Mocks/` are live registrations for every test class, and adding a second converter for an existing `From -> To` pair anywhere in that assembly makes `DiCompositionValidator` throw and fails the entire suite. `ValidatorTests` deliberately calls the internal validator directly with hand-built type lists instead of going through the host.
- `Converter/Properties/AssemblyInfo.cs` grants `InternalsVisibleTo("Majipro.Converter.Tests")`; the generator grants it to `Majipro.Converter.Generator.Test` via csproj.
- The generator suite compiles C# source files in-memory (`CompilingHelper` builds a `CSharpCompilation`, runs `ConverterGenerator` through `CSharpGeneratorDriver`, and `CompilationExtension.Run` emits and reflection-invokes the result). Test-case source files are added as content and loaded from disk at runtime, so paths are written Windows-style and normalized by `StringExtensions.ToPath` (which resolves them against `AppContext.BaseDirectory`, not the working directory).

## Source generator

`Converter.Generator/` is much smaller than `github.com/paukertj/autoconverter` (the predecessor project, which the test harness is still modelled on), because this library registers converters by **runtime assembly scan**: the generator only has to emit a public class implementing a converter interface and `DiCompositor.AddConverting` finds it. There is no generated DI wiring and no `[WiringEntrypoint]` attribute. The architecture is not the predecessor's — see below.

The generator has a `PrivateAssets="all"` project reference to `Converter.Abstrations` and resolves the symbols it looks for through `compilation.GetTypeByMetadataName(typeof(IConvertingService).FullName)` (and `typeof(IConverter<,>)`, `typeof(IAsyncConverter<,>)`, `typeof(IReferenceConverter<,>)`), so renaming or moving an interface breaks the generator's build instead of silently generating nothing.

**There is no analysis pass and no model of a converter.** Pairs are travelled one at a time and each one is written out on the spot; the syntax tree is the only representation there is. Two consequences are worth knowing before changing anything here:

- `Generating/ConverterContext.Convert(from, to, value)` writes `_convertingService.Convert<A, B>(value)` **and** requests the `A -> B` pair **and** flips the flag that makes the field and the constructor appear. The three are one act, so a call cannot be emitted for a converter that was never requested, and `RequiresConvertingService` is observed from what generation did rather than predicted from a model. Never write that call by hand.
- Every rule yields **one node or nothing**, so the filter deciding whether the rule applies and the code producing its output are the same function. `RuleExtensions.FirstMatch` takes the first rule that produces something and never asks the rest — which is why **a rule must not do anything but decide until the moment it yields**. Side effects (`Convert`, and anything else reaching outside the node) belong in the expression handed to `yield return`, not before it.
- The class is decided by what the body turned out to be, never asked for beforehand. `Generating/ConverterBody` is the one node a body rule yields: the statement that creates a target, and — when that shape can fill one that already exists — the statements that do. Whether `Fill` is there is what makes the generated class an `IReferenceConverter` and what makes the second `Convert` appear, the same way `RequiresConvertingService` is read off what generation did.

Pipeline: `Receivers/ConvertCallsSyntaxReceiver` is the node stream — every `x.Method<A, B>(...)` invocation, which is the one filter that has to be a receiver → `Sources/ConvertCallSites` keeps those that bind to a method on `IConvertingService` and yields `From -> To` pairs, `Sources/ImplementedConverters` yields the pairs somebody wrote by hand → `ConverterGenerator` requests the first and suppresses the second on a `Conversions/ConversionQueue` → `ConversionQueue.Travel()` hands out each unhandled pair once, **lazily**, so a converter being written can request the converters it needs and they arrive in the same pass → `Generating/ConverterEmitter` writes each one as a Roslyn syntax tree (`SyntaxFactory`, rendered by `NormalizeWhitespace()`) with fully qualified type names, in namespace `Majipro.Converter.Generated`. Anything thrown is reported as diagnostic `MC0001` instead of killing the build.

A rule that recognizes a conversion but finds it impossible to write deterministically throws `Diagnostics/ConversionException`, which carries the `DiagnosticDescriptor` it is to be reported as. `ConverterGenerator` catches it ahead of the catch all and reports that descriptor instead of `MC0001` — today `MC0002` for enum members without a counterpart and `MC0003` for a target property nothing fills. Every one of them is `DiagnosticSeverity.Error`, so any of them fails the build, and all of them end the whole generation pass — the pairs after the failing one are never travelled. That is deliberate: the pair has to be dealt with by hand before anything else is worth generating.

A pair is marked as handled *before* it is handed out, which is also what stops a type that contains itself from looping forever. Pairs are keyed by `SymbolEqualityComparer`, not by display strings.

`ConverterEmitter` drops a pair outright when both sides are the same type (`ConvertingService` handles identity itself) or when either side is not nameable from the generated file — `IsVisibleToGeneratedCode` walks the type, everything it is nested in and everything it is built out of, and says no to anything below `internal` and to anything that is not a name a standalone file can carry. A **type parameter** is the one that matters in practice: a generic helper calling `Convert<TFrom, string>(x)` is a perfectly ordinary thing to write, and the pair it asks for means nothing outside the method it was written in. Everything else is up to the rules, wired in order in `ConverterGenerator.GetBodyRules` — **the composition root, and the only place order is decided**:

| Rule | Claims the conversion when | Writes |
| --- | --- | --- |
| `Rules/ToStringBodyRule` | the target is `string` — a string can never be built by an object initializer | `return System.Convert.ToString(from);` |
| `Rules/EnumBodyRule` | both sides are enums | `switch (from) { case A.X: return B.X; … default: throw new ArgumentOutOfRangeException(…); }` |
| `Rules/ObjectInitializerBodyRule` | both sides are mappable | `return new To { ... };`, and — when the target can be filled — `to.X = ...; return to;` |

`EnumBodyRule` matches members by **name only** (ordinal), so the two sides are free to number their members differently — which is the whole reason an enum conversion is not simply a cast. Every member the *source* declares must exist in the target; a member with no counterpart has no deterministic answer, so the rule claims the pair (no later rule would do better) and then throws `ConversionException` on `MC0002`, failing the build. The other direction is free: target members nothing maps to are simply never returned. The `default` arm is reached only by a number cast into the enum without being one of its members, and throws for the same reason. Two source names sharing one value emit a single `case`, since two labels of the same constant would not compile.

A pair no body rule claims gets no converter at all. `ObjectInitializerBodyRule` matches properties by **name**, with a public getter on the source and a public setter (or `init`) on the target, base type properties included, and asks the value rules for each pair it finds:

| Rule | Claims the property when | Writes |
| --- | --- | --- |
| `Rules/DirectValueRule` | `CSharpCompilation.ClassifyConversion(source, target).IsImplicit` — identity, `Guid -> Guid?`, a differing nullable annotation, an implicit numeric or reference conversion | `To = from.Prop` |
| `Rules/ConvertedValueRule` | both sides are mappable types | `To = _convertingService.Convert<A, B>(from.Prop)`, and `A -> B` is requested |
| `Rules/CollectionValueRule` | the source is an `IEnumerable<A>` (`string` excluded) and the target is one of `IEnumerable<>`, `ICollection<>`, `IList<>`, `IReadOnlyCollection<>`, `IReadOnlyList<>`, `List<>` of `B`, where `A` and `B` are the same type or a mappable pair — merely assignable items (`int` into `long`) are **not** enough, the per item `Convert<A, B>` would compile and then throw about a missing conversion | `To = from.Prop == null ? null : new List<B>(_convertingService.Convert<A, B>((IEnumerable<A>)from.Prop))`, and `A -> B` is requested |

Every writable target property has to end up with a value. One that does not — the source has no property of that name, or it has one and no value rule claims it — would be left at its default, and a converter that silently returns a half built object is worse than no converter at all. So `ObjectInitializerBodyRule` claims the pair (both sides are mappable, no later rule would do better) and then throws `ConversionException` on `MC0003`, naming the properties nothing fills. The other direction is free, the same way it is for enum members: a property the *source* knows and the target does not is dropped, which loses nothing the target was ever going to carry. A target property the object initializer could not write anyway — no setter, or a setter that is not public — is not the generator's to fill and is never counted, so a target with no writable properties at all still gets a converter that returns an empty instance.

Two things live outside the rules because they apply to all of them: a **`Nullable<T>`** on either side is unwrapped by `ConverterContext` — `SourceAccess` is `from.Value`, `Target` is the plain `T` the object initializer creates, and the converter signature keeps the nullable type — and a source that can be null gets a `from == null` guard prepended by `ConverterEmitter`.

The generated class is **as accessible as its pair is**, which `ConverterEmitter.GetAccessibility` reads the same way. Both `Convert` methods have to be public — they implement an interface — and a public method can neither take nor return a type that is only `internal`, so a pair with an internal type on either side gets an `internal sealed class` rather than a public one (`CS0050`/`CS0051` otherwise, and the consuming project does not build at all). Nothing is lost by it: `DiCompositor` scans `Assembly.GetTypes()`, not the exported ones, and `ServiceDescriptor` is happy to construct an internal type through its public constructor. Types named only inside a method body — the `Convert<A, B>` call for a nested property — are not part of this: an expression may reach an internal type from a public class in the same assembly.

### The two conversions
A generated converter carries **both conversions wherever it can**: `TTo Convert(TFrom from)` creating a new target, and `TTo Convert(TFrom from, TTo to)` filling one the caller already holds. It is one class either way, because `IReferenceConverter<TFrom, TTo>` extends `IConverter<TFrom, TTo>` — which is also what keeps `DiCompositionValidator` from seeing two registrations for one pair. A call site of either shape is the same request: `ConvertCallSites` reads the type arguments, not the overload.

Only `ObjectInitializerBodyRule` writes the second one, and it leaves it out in two cases. An **`init` only** property can be written while the target is being created and never again (`IsSettable`, as opposed to `IsWritable`), so such a target can be created but not filled. And a **value type** target has no instance the caller holds — a structure handed to a method is a copy of it — so filling one would enhance something nobody else can see; a `Nullable<T>` target falls out of the same check. `ToStringBodyRule` and `EnumBodyRule` never write it: a string and an enum are values, not instances to fill. Those pairs stay plain `IConverter`s, and asking the container for the reference converter of such a pair returns nothing.

What the fill method does is write the same values the object initializer writes, as assignments on `to` — the value rules are asked once and both methods are written from the same answers, so they cannot say different things about a property. Since `MC0003` means every writable property is filled, what the reference converter actually preserves is everything *else* the instance carries: its identity first of all (an entity the caller's change tracker knows), and whatever state the generator does not see. A nested object property is *replaced* by a converted one rather than filled recursively, the same as in the object initializer. `from == null` returns `to` untouched — there is nothing to enhance it with — while the creating method still returns `default`. A `to` of `null` is not guarded: the method's whole contract is the instance it was handed.

Having written **one** of the two conversions by hand is having written the pair. `Sources/ImplementedConverters` suppresses the pair as a whole, and it has to: the generated class would implement `IConverter<A, B>` on the way to `IReferenceConverter<A, B>`, so there is no way to generate the half somebody is missing without claiming the half they wrote. The reference converter they did not write is theirs to add. That is also why only `IConverter` and `IAsyncConverter` are looked for — a hand written `IReferenceConverter` arrives through the `IConverter` it extends.

### Extending the generator
Adding a shape means adding a rule class and one line in `ConverterGenerator.GetBodyRules`; nothing else in the generator changes. Not handled yet (each is a place to extend): the **async** converters — `IAsyncConverter`/`IAsyncReferenceConverter` are never generated, and a pair asked for only through `ConvertAsync` gets a sync converter that `ConvertingService` wraps. Adding them means a third and fourth method on `ConverterBody` and the interface choice in `ConversionSemantics.Converter` growing accordingly; note that what the class implements must **not** become part of the pair's identity, since two generated classes claiming one `From -> To` pair is exactly what `DiCompositionValidator` throws about. Also `ConvertExplicitly` extension methods (another `IPairSource`), sets, dictionaries and arrays as property types (value rules), generic and positional-record targets (body rules). Widening a value rule is now the only way to make a property pair convertible: since `MC0003`, a target property no value rule claims fails the build instead of being dropped, so every shape the rules do not cover is a shape a consuming project cannot point the generator at.

### Generator test cases
Test cases live in one folder per case, under one of two roots, and the root is the question the case asks. `Converter.Generator.Test/Conversions/` holds the pairs the generator is expected to write a converter for — the test resolves it out of the container and converts. `Converter.Generator.Test/Validations/` holds the pairs it is expected to refuse — the test asserts on `Diagnostic` and on `GeneratedSources` being empty and never touches `ServiceProvider`, since the compilation it describes is meant to fail. The harness both share, `ConversionTestBase` and `TestCompositionBase`, sits at the project root.

`Conversions/` mirrors the reference project's `Generator.Tests/TestCases/Basic`: `PropertyOfTheSameType` (the baseline), `DifferentSourcesAndTargets` (a source that knows more than the target, and both sides knowing something the other does not), `ObjectNesting` (nested converters and collection properties), `AccessModifiers` (private, `init` and get only properties), `FileScopedNamespace` (types declared straight in a file scoped namespace instead of nested in a test case class), `Structures` (structs, `Guid -> Guid?`, differing nullable annotations), `NullableTypes` (`Nullable<T>` on either side of the call site and `string` targets) `CollectionProperties` (every collection type `GetListItemType` accepts as a target, an array as the source, and items that are the same type on both sides), `Enums` (`enum -> string`, and an enum property copied to the same enum or its nullable counterpart), `EnumConversion` (`enum -> enum` call sites: the same members on both sides, source members that are a subset of the target's and numbered differently, and a nullable enum on either side), `ReferenceConversion` (the second conversion: the target that comes back is the one that went in, what the generator does not see survives it, a nested property is converted into it, and a null source leaves it alone) `ImplementedConverters` (a pair whose value conversion somebody wrote, a pair whose reference conversion somebody wrote, and a pair nobody wrote — only the last one is generated), `InternalTypes` (a pair the rest of the assembly can see and nobody outside of it can, which is the case that decides the accessibility of the generated class) and `GenericCallSite` (a generic method handing its own type parameter to the converting service, next to an ordinary pair — the first one is skipped, the second one is still written, and the build survives).

`InternalTypes` is the one case whose types live in the **composition** file rather than in a test case file, and they have to: the point of the case is a type internal to the assembly the converter is generated into, which is the assembly the composition is compiled as. That is also why it asserts on the generated source and on the container being built instead of on a converted instance — the test assembly's copy of those types is a different type from the one the generated converter talks about.

Whether a pair got the second conversion is asked the way a consumer asks: `FindReferenceConverter<TFrom, TTo>()` resolves it out of the container and returns `null` when there is none. `AccessModifiers`, `Structures` and `Enums` each use it on the shape that does not get one.

`Validations/` holds the shapes the generator refuses, two per diagnostic id and then some. `MC0002`, two enums: `MissingEnumMember` (the target has no counterpart for one source member) and `DisjointEnumMembers` (neither enum covers the other, which being a subset in the *other* direction does not help). `MC0003`, a target property nothing fills: `MissingProperty` (the source simply has fewer properties), `RenamedProperty` (the source has a `B` and the target wants a `BB`, and matching by name cannot tell a rename from an absence), `UnusedSourceProperty` (the same rename from the other side — the source having a property to spare is no consolation for the one the target is missing) and `UnconvertibleProperty` (the names line up and no value rule claims the pair, two different enum types being the shape that reaches this today).

Each refused pair is a case of its own rather than another composition next to an existing one, because the error ends the whole generation pass — a second refused pair in the same compilation would never be travelled.

### Adding a generator test case
Under `Converter.Generator.Test/Conversions/<Case>/` — or `Validations/<Case>/` when the point of the case is that the generator refuses the pair — add three files:
1. `<Case>TestCase.cs` — the types being converted. Compiled into the test assembly **only**; the in-memory compilation sees them through the assembly reference, so the test and the generated converter share the same runtime types.
2. `<Case>Composition.cs` — `public class <Case>Composition : TestCompositionBase` with the `convertingService.Convert<From, To>(...)` call sites that trigger the generator. The `**\*Composition*.cs` glob in the csproj copies it to the output directory, and `ConversionTestBase` feeds it (plus `TestCompositionBase.cs`, which the same glob copies) to the compilation as source text. It is compiled into the test assembly as well, hence the harmless CS0436 warnings. It is also the only source the generator sees, so anything else the compilation is meant to contain — a converter written by hand, for instance — goes in this file next to the composition class.
3. `<Case>Test.cs` — `[TestClass] class <Case>Test : ConversionTestBase<<Case>Composition>`, passing the composition file path to the base constructor. Resolve through `GetConverter<TFrom, TTo>()`, `GetReferenceConverter<TFrom, TTo>()`, `FindReferenceConverter<TFrom, TTo>()` (null instead of throwing), `GetConvertingService()` or `GetService<T>()`; assert on `Diagnostic` / `GeneratedSources` (`AssertGeneratedWithoutDiagnostics()` covers the common case).

## Packaging

**One package.** `Majipro.Converter` carries all three assemblies, and installing it is the whole setup — there is no second package to find and no `<Analyzer>` to write by hand:

```
lib/netstandard2.1/     Majipro.Converter.dll, Majipro.Converter.Abstrations.dll   <- what the program runs on
analyzers/dotnet/cs/    Majipro.Converter.Generator.dll, Majipro.Converter.Abstrations.dll
```

`Converter/Converter.csproj` **is** the package. `Converter.Abstrations` and `Converter.Generator` are both `IsPackable=false`, which means a reference to them produces neither a package dependency nor a file — so both assemblies are put into the package from `Converter.csproj` by hand, by two targets:

- `PackAbstrationsIntoLib` hangs off `TargetsForTfmSpecificBuildOutput` and adds the project reference's assembly to `BuildOutputInPackage`, which lands it in `lib/`. That is the **netstandard2.1** asset, the one `Converter` itself compiles against.
- `PackGeneratorIntoAnalyzers` hangs off `TargetsForTfmSpecificContentInPackage` and asks the two projects for their `GetTargetPath`, pinned to **netstandard2.0**. Both files go to `analyzers/dotnet/cs`.

Three things about that layout are load bearing:

- **netstandard2.0 for the analyzer half is not a preference.** An analyzer is loaded into the compiler's own process, and that process is .NET Framework inside Visual Studio and .NET on the command line. netstandard2.0 is the only thing both of them load, which is why `Converter.Generator` targets it and why `Converter.Abstrations` grew a second asset rather than being moved wholesale.
- **The abstractions travel twice**, once per half, because the generator resolves the converter interfaces through `typeof(IConverter<,>)` rather than by name — so the assembly has to be next to it in `analyzers/dotnet/cs`. It only ever asks those types for their names, never for their members (`typeof(...).FullName`, and a `nameof` the compiler turns into a literal), which is why the `ValueTask<T>` in `IConvertingService` never has to resolve there. Worth knowing when changing `ConversionSemantics`: the moment the generator touches a *member* of one of those interfaces, `System.Threading.Tasks.Extensions.dll` has to be packed into `analyzers/dotnet/cs` as well. The end to end check was run on the .NET hosted compiler; the .NET Framework host that Visual Studio uses is what the netstandard2.0 asset is *for*, but it has not been exercised here.
- **`PrivateAssets="all"`** on `Converter` → `Converter.Abstrations` is what keeps the reference out of the nuspec. It also keeps it from flowing to projects in this solution, which is why `Converter.Tests` and `Converter.Generator.Test` declare their own reference to the abstractions — they use those types directly. A consumer of the *package* needs no such thing: every assembly in `lib/` is a compile time reference.

`Converter.csproj` deliberately does **not** set `GeneratePackageOnBuild`: it writes a package on every build, Debug included, and it makes `dotnet pack` skip the build it is meant to pack, so packing a clean tree fails on `NU5026` instead of building it.

## CI / release

`.github/workflows/pr.yml` builds Release and runs tests on PRs into `main`. `.github/workflows/main.yaml` fires on `v*` tags: it strips the leading `v` into `$VERSION`, passes it to **`dotnet build` and to `dotnet pack`**, then pushes `artifacts/*.nupkg` — one file — to nuget.org. Versions come from the tag only; there is no version in any csproj (gitversion is still listed in `.config/dotnet-tools.json` but is no longer used by the workflows).

The version has to reach the **build**, not only the pack: packing does not compile, so a version handed to `dotnet pack --no-build` alone names the package and leaves every assembly inside it saying `1.0.0.0`. `Version` by itself is enough — `AssemblyVersion`, `FileVersion` and `InformationalVersion` are all derived from it, and a prerelease tag like `v1.2.3-rc1` keeps working because `AssemblyVersion` comes from the part before the dash. Passing `AssemblyVersion` explicitly would fail on such a tag.

`Converter.csproj` sets `BuildProjectReferences=false` when `NoBuild` is set, because packing with `NoBuild` does not set it and `ResolveReferences` then tries to build the referenced project and fails on `NETSDK1085`. That is exactly what the release workflow does, so the answer belongs in the project rather than in its command line.

## Current state of the working tree

The solution builds clean and both suites pass (135 + 94), and the generator has been run end to end out of the packed `Majipro.Converter` by a throwaway consumer project: the analyzer loads, the converters are generated, and both conversions come back right. Open items:
- A converter somebody wrote by hand is only found in the **assembly being compiled**: `ImplementedConverters` walks `compilation.Assembly.GlobalNamespace` and nothing else. So a solution with the hand written converters in a class library and the `Convert<X, Y>` call site in the application generates a second converter for that pair, and `AddConverting(library, application)` then throws about two implementations of it. Whether that is a bug or the boundary of what the generator is for is a decision, not an oversight — the cheap version of the other answer is to walk the referenced assemblies whose `IModuleSymbol.ReferencedAssemblySymbols` contain `Majipro.Converter.Abstrations`, which is one or two assemblies rather than the whole framework. Note that it cuts the other way too: a project that registers only its own assembly would lose the converter it generates today.
- `enum -> enum` is generated for a **call site** only. As a *property* type it is not: `ConvertedValueRule` gates on `IsMappableSource`, which is class-or-struct, so an enum property never reaches `EnumBodyRule`. Since `MC0003` that is no longer a silent drop — one enum per layer, the shape consuming projects write converters for by hand, now fails the build (`Validations/UnconvertibleProperty`). Letting it through is a one line change and it is now the *smaller* change of the two, since the pairs that line up would start converting and the ones that do not would trade an `MC0003` that says nothing fills the property for an `MC0002` that says which members are missing. Worth doing deliberately.
- The order sensitivity described under *Testing conventions* is real, not theoretical, and it is the **`StringTo{Decimal,Double,Float}ConverterTests` family** — the classes that call `ClassInitializeAsync(configure)` from inside a `[TestMethod]`. Three failures in about a dozen solution wide `dotnet test` runs, a *different* member of the family each time (`WhenInputIsCanBeDecimalThenReturnDecimalOtherwiseDefault("123,45", AllowDecimalPoint, "cs-CZ")` and `WhenInputIsCanBeFloatThenReturnFloatOtherwiseDefault("$ 123,456.78", Currency, "en-US")` so far), and never once when `Converter.Tests` runs alone. They read `FormatProvider`/`NumberStyles` off the static `ConversionTestBase.ConvertingService` while another class of the family is reconfiguring that same host in parallel, so whichever one loses the race is the one that fails. The fix is reworking that static field, not retrying the run.
