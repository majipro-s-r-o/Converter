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

`Converter.Generator/` mirrors `github.com/paukertj/autoconverter` (the predecessor project) but is much smaller, because this library registers converters by **runtime assembly scan**: the generator only has to emit a public class implementing `IConverter<TFrom, TTo>` and `DiCompositor.AddConverting` finds it. There is no generated DI wiring and no `[WiringEntrypoint]` attribute.

The generator has a `PrivateAssets="all"` project reference to `Converter.Abstrations` and resolves the symbols it looks for through `compilation.GetTypeByMetadataName(typeof(IConvertingService).FullName)` (and `typeof(IConverter<,>)`, `typeof(IAsyncConverter<,>)`), so renaming or moving an interface breaks the generator's build instead of silently generating nothing.

Pipeline: `ConvertCallsSyntaxReceiver` collects every `x.Method<A, B>(...)` invocation → `Analysis/ConversionAnalyzer` keeps those that bind to a method on `Majipro.Converter.IConvertingService`, dedupes the `From -> To` pairs, drops pairs somebody already implemented by hand (otherwise `DiCompositionValidator` would throw about duplicates) and pairs it cannot map, and matches properties → `Generating/ConverterSourceBuilder` builds the class as a Roslyn syntax tree (`SyntaxFactory`, rendered by `NormalizeWhitespace()`, like the reference project) with fully qualified type names, in namespace `Majipro.Converter.Generated`. Anything thrown is reported as diagnostic `MC0001` instead of killing the build.

Current mapping rule is deliberately dumb: **same property name + exactly the same property type**, public getter on the source, public setter (or `init`) on the target, base type properties included. Not handled yet (each is a place to extend): type mismatches needing a nested converter, collections, `ConvertAsync`/reference-converter call sites (a plain `IConverter` gets generated for them), `ConvertExplicitly` extension methods, generic and positional-record targets, and diagnostics for target properties that stay unmapped.

### Adding a generator test case
Under `Converter.Generator.Test/Tests/<Case>/` add three files:
1. `<Case>TestCase.cs` — the types being converted. Compiled into the test assembly **only**; the in-memory compilation sees them through the assembly reference, so the test and the generated converter share the same runtime types.
2. `<Case>Composition.cs` — `public class <Case>Composition : TestCompositionBase` with the `convertingService.Convert<From, To>(...)` call sites that trigger the generator. The `Tests\**\*Composition*.cs` glob in the csproj copies it to the output directory, and `ConversionTestBase` feeds it (plus `Tests/TestCompositionBase.cs`) to the compilation as source text. It is compiled into the test assembly as well, hence the harmless CS0436 warnings.
3. `<Case>Test.cs` — `[TestClass] class <Case>Test : ConversionTestBase<<Case>Composition>`, passing the composition file path to the base constructor. Resolve through `GetConverter<TFrom, TTo>()`, `GetConvertingService()` or `GetService<T>()`; assert on `Diagnostic` / `GeneratedSources` (`AssertGeneratedWithoutDiagnostics()` covers the common case).

## CI / release

`.github/workflows/pr.yml` builds Release and runs tests on PRs into `main`. `.github/workflows/main.yaml` fires on `v*` tags: it strips the leading `v` and passes the tag as `Version`/`AssemblyVersion`/`InformationalVersion` to `dotnet pack`, then pushes to nuget.org. Versions come from the tag only — there is no version in any csproj (gitversion is still listed in `.config/dotnet-tools.json` but is no longer used by the workflows).

## Current state of the working tree

The solution builds clean and both suites pass (136 + 5). Open items:
- `Converter.Tests/Tests/Converters/StringToDateTimeOffsetConverterTests.cs` warns MSTEST0042 — two identical `DataRow` attributes (indices 3 and 4), most likely a copy/paste error hiding a case that was meant to be covered.
- `Converter.Generator` targets net10.0, so it cannot be loaded as an analyzer by the compiler yet — it only runs in-process from the tests. Packing it means moving it (and `Converter.Abstrations`, which it now references) to netstandard2.0 and adding the analyzer packaging bits: both dlls into `analyzers/dotnet/cs` (see the reference project's csproj). `Converter.Abstrations` has no package references, so those two dlls are all the analyzer context needs.
- `dotnet pack` on the solution currently produces three packages — `Majipro.Converter`, `Majipro.Converter.Abstrations` (a proper dependency of the first one) and `Majipro.Converter.Generator`. The last one is a plain `lib/net10.0` package that does nothing when installed, and `.github/workflows/main.yaml` pushes `artifacts/*.nupkg` on a `v*` tag, so it would land on nuget.org as is. Set `IsPackable=false` on the generator until the analyzer packaging is done.
- `Converter.Generator.Test/Test1.cs` is leftover `dotnet new mstest` scaffolding.
