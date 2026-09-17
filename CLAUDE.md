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
dotnet pack --configuration Release /p:Version=1.2.3 --output artifacts   # what CI does on a v* tag
```

There is no linter/formatter step; MSTest analyzers (MSTEST####) and Roslyn warnings surface during build.

## Projects

| Project | TFM | Purpose |
| --- | --- | --- |
| `Converter.Abstrations/` | netstandard2.1 | The four converter interfaces + `IConvertingService`, packed as NuGet `Majipro.Converter.Abstrations`. Shared by the library **and** the generator, so neither side identifies them by name. Deliberately dependency free — `IConverterOptions` stays in `Converter/` because it exposes `ServiceLifetime`, which would drag `Microsoft.Extensions.DependencyInjection.Abstractions` into the generator's analyzer context |
| `Converter/` | netstandard2.1 | The shipped library, packed as NuGet `Majipro.Converter` (`GeneratePackageOnBuild`) |
| `Converter.Tests/` | net10.0 | MSTest suite for the library |
| `Converter.Generator/` | net10.0 | Roslyn `ISourceGenerator` that emits `IConverter<TFrom, TTo>` implementations |
| `Converter.Generator.Test/` | net10.0 | MSTest suite for the generator |

`Directory.Build.props` applies to every project and rewrites identity: `AssemblyName` and `RootNamespace` become `Majipro.<ProjectFolderName>`. So the folder `Converter` is namespace `Majipro.Converter`, `Converter.Tests` is `Majipro.Converter.Tests`, and `Converter.Abstrations` is `Majipro.Converter.Abstrations` (the folder name carries the typo, so the namespace and the package id do too). It also sets `TargetFramework` to netstandard2.1 — the test and generator projects override it to net10.0.

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

`Converter.Abstrations` has no package references at all, and it should stay that way — see the note in the projects table.

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

`Converter.Generator/` is much smaller than `github.com/paukertj/autoconverter` (the predecessor project, which the test harness is still modelled on), because this library registers converters by **runtime assembly scan**: the generator only has to emit a public class implementing `IConverter<TFrom, TTo>` and `DiCompositor.AddConverting` finds it. There is no generated DI wiring and no `[WiringEntrypoint]` attribute. The architecture is not the predecessor's — see below.

The generator has a `PrivateAssets="all"` project reference to `Converter.Abstrations` and resolves the symbols it looks for through `compilation.GetTypeByMetadataName(typeof(IConvertingService).FullName)` (and `typeof(IConverter<,>)`, `typeof(IAsyncConverter<,>)`), so renaming or moving an interface breaks the generator's build instead of silently generating nothing.

**There is no analysis pass and no model of a converter.** Pairs are travelled one at a time and each one is written out on the spot; the syntax tree is the only representation there is. Two consequences are worth knowing before changing anything here:

- `Generating/ConverterContext.Convert(from, to, value)` writes `_convertingService.Convert<A, B>(value)` **and** requests the `A -> B` pair **and** flips the flag that makes the field and the constructor appear. The three are one act, so a call cannot be emitted for a converter that was never requested, and `RequiresConvertingService` is observed from what generation did rather than predicted from a model. Never write that call by hand.
- Every rule yields **one node or nothing**, so the filter deciding whether the rule applies and the code producing its output are the same function. `RuleExtensions.FirstMatch` takes the first rule that produces something and never asks the rest — which is why **a rule must not do anything but decide until the moment it yields**. Side effects (`Convert`, and anything else reaching outside the node) belong in the expression handed to `yield return`, not before it.

Pipeline: `Receivers/ConvertCallsSyntaxReceiver` is the node stream — every `x.Method<A, B>(...)` invocation, which is the one filter that has to be a receiver → `Sources/ConvertCallSites` keeps those that bind to a method on `IConvertingService` and yields `From -> To` pairs, `Sources/ImplementedConverters` yields the pairs somebody wrote by hand → `ConverterGenerator` requests the first and suppresses the second on a `Conversions/ConversionQueue` → `ConversionQueue.Travel()` hands out each unhandled pair once, **lazily**, so a converter being written can request the converters it needs and they arrive in the same pass → `Generating/ConverterEmitter` writes each one as a Roslyn syntax tree (`SyntaxFactory`, rendered by `NormalizeWhitespace()`) with fully qualified type names, in namespace `Majipro.Converter.Generated`. Anything thrown is reported as diagnostic `MC0001` instead of killing the build.

A pair is marked as handled *before* it is handed out, which is also what stops a type that contains itself from looping forever. Pairs are keyed by `SymbolEqualityComparer`, not by display strings.

`ConverterEmitter` drops a pair outright when both sides are the same type (`ConvertingService` handles identity itself) or when the source is not even nameable from the generated file. Everything else is up to the rules, wired in order in `ConverterGenerator.GetBodyRules` — **the composition root, and the only place order is decided**:

| Rule | Claims the conversion when | Writes |
| --- | --- | --- |
| `Rules/ToStringBodyRule` | the target is `string` — a string can never be built by an object initializer | `return System.Convert.ToString(from);` |
| `Rules/ObjectInitializerBodyRule` | both sides are mappable | `return new To { ... };` |

A pair no body rule claims gets no converter at all. `ObjectInitializerBodyRule` matches properties by **name**, with a public getter on the source and a public setter (or `init`) on the target, base type properties included, and asks the value rules for each pair it finds:

| Rule | Claims the property when | Writes |
| --- | --- | --- |
| `Rules/DirectValueRule` | `CSharpCompilation.ClassifyConversion(source, target).IsImplicit` — identity, `Guid -> Guid?`, a differing nullable annotation, an implicit numeric or reference conversion | `To = from.Prop` |
| `Rules/ConvertedValueRule` | both sides are mappable types | `To = _convertingService.Convert<A, B>(from.Prop)`, and `A -> B` is requested |
| `Rules/CollectionValueRule` | the source is an `IEnumerable<A>` (`string` excluded) and the target is one of `IEnumerable<>`, `ICollection<>`, `IList<>`, `IReadOnlyCollection<>`, `IReadOnlyList<>`, `List<>` of `B`, where `A` and `B` are the same type or a mappable pair — merely assignable items (`int` into `long`) are **not** enough, the per item `Convert<A, B>` would compile and then throw about a missing conversion | `To = from.Prop == null ? null : new List<B>(_convertingService.Convert<A, B>((IEnumerable<A>)from.Prop))`, and `A -> B` is requested |

A target property no value rule claims is left unmapped. A conversion with *no* mapped properties still gets a converter that returns an empty instance.

Two things live outside the rules because they apply to all of them: a **`Nullable<T>`** on either side is unwrapped by `ConverterContext` — `SourceAccess` is `from.Value`, `Target` is the plain `T` the object initializer creates, and the converter signature keeps the nullable type — and a source that can be null gets the `if (from == null) return default;` guard prepended by `ConverterEmitter`.

### Extending the generator
Adding a shape means adding a rule class and one line in `ConverterGenerator.GetBodyRules`; nothing else in the generator changes. Not handled yet (each is a place to extend): `ConvertAsync`/reference-converter call sites — a new `IPairSource` plus, if the generated converter is to implement a different interface, a shape carried on `ConversionPair`; note the shape must **not** become part of the pair's identity, since two generated classes claiming one `From -> To` pair is exactly what `DiCompositionValidator` throws about. Also `ConvertExplicitly` extension methods (another `IPairSource`), sets, dictionaries and arrays as property types (value rules), generic and positional-record targets (body rules), and diagnostics for target properties that stay unmapped — those are the target properties for which `ObjectInitializerBodyRule.GetAssignments` yields nothing.

### Generator test cases
`Converter.Generator.Test/Tests/` mirrors the reference project's `Generator.Tests/TestCases/Basic`, one folder per case: `PropertyOfTheSameType` (the baseline), `DifferentSourcesAndTargets` (asymmetric property sets), `ObjectNesting` (nested converters and collection properties), `AccessModifiers` (private, `init` and get only properties), `FileScopedNamespace` (types declared straight in a file scoped namespace instead of nested in a test case class), `Structures` (structs, `Guid -> Guid?`, differing nullable annotations) and `NullableTypes` (`Nullable<T>` on either side of the call site and `string` targets).

### Adding a generator test case
Under `Converter.Generator.Test/Tests/<Case>/` add three files:
1. `<Case>TestCase.cs` — the types being converted. Compiled into the test assembly **only**; the in-memory compilation sees them through the assembly reference, so the test and the generated converter share the same runtime types.
2. `<Case>Composition.cs` — `public class <Case>Composition : TestCompositionBase` with the `convertingService.Convert<From, To>(...)` call sites that trigger the generator. The `Tests\**\*Composition*.cs` glob in the csproj copies it to the output directory, and `ConversionTestBase` feeds it (plus `Tests/TestCompositionBase.cs`) to the compilation as source text. It is compiled into the test assembly as well, hence the harmless CS0436 warnings.
3. `<Case>Test.cs` — `[TestClass] class <Case>Test : ConversionTestBase<<Case>Composition>`, passing the composition file path to the base constructor. Resolve through `GetConverter<TFrom, TTo>()`, `GetConvertingService()` or `GetService<T>()`; assert on `Diagnostic` / `GeneratedSources` (`AssertGeneratedWithoutDiagnostics()` covers the common case).

## CI / release

`.github/workflows/pr.yml` builds Release and runs tests on PRs into `main`. `.github/workflows/main.yaml` fires on `v*` tags: it strips the leading `v` and passes the tag as `Version`/`AssemblyVersion`/`InformationalVersion` to `dotnet pack`, then pushes to nuget.org. Versions come from the tag only — there is no version in any csproj (gitversion is still listed in `.config/dotnet-tools.json` but is no longer used by the workflows).

## Current state of the working tree

The solution builds clean and both suites pass (136 + 32). Open items:
- `Converter.Tests/Tests/Converters/StringToDateTimeOffsetConverterTests.cs` warns MSTEST0042 — two identical `DataRow` attributes (indices 3 and 4), most likely a copy/paste error hiding a case that was meant to be covered.
- `Converter.Generator` targets net10.0, so it cannot be loaded as an analyzer by the compiler yet — it only runs in-process from the tests. Packing it means moving it (and `Converter.Abstrations`, which it now references) to netstandard2.0 and adding the analyzer packaging bits: both dlls into `analyzers/dotnet/cs` (see the reference project's csproj). `Converter.Abstrations` has no package references, so those two dlls are all the analyzer context needs.
- `dotnet pack` on the solution currently produces three packages — `Majipro.Converter`, `Majipro.Converter.Abstrations` (a proper dependency of the first one) and `Majipro.Converter.Generator`. The last one is a plain `lib/net10.0` package that does nothing when installed, and `.github/workflows/main.yaml` pushes `artifacts/*.nupkg` on a `v*` tag, so it would land on nuget.org as is. Set `IsPackable=false` on the generator until the analyzer packaging is done.
