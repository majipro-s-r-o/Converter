# Converter
Library for manual class mapping. This library helps you with type mapping using dependency-injected services (Converters).

## What it is
This library helps you to build dependency-injected services that can perform object mapping. We call these services "Converters" because they are performing object conversion from one type to another. The fundamental idea was to build some easy manual mapping tool that will enforce a normalized way, how mapping is done. This tool is an alternative to a common solution for manual mapping that uses static methods. Since each mapper (we call it Converter to avoid confusion with [Automapper](https://github.com/AutoMapper/AutoMapper)) is an independent dependency-injected service, you can use full advantage of that (or you don't have to if you consider that as a disadvantage) — so you can mock converters in tests, or you can leverage them by injecting other services.

## How to install it
```
dotnet add package Majipro.Converter
```
One package is the whole setup. It carries the library, the abstractions and the source generator (as a Roslyn analyzer), so there is no second package to reference and no `<Analyzer>` to write by hand.

## Contents
- [How to use it](#how-to-use-it) — registration, configuration, writing a Converter
- [Build in Converters](#build-in-converters) — `string` to primitives
- [Reference Converters](#reference-converters) — filling an instance you already hold
- [Collections and dictionaries](#collections-and-dictionaries)
- [Async conversions](#async-conversions)
- [Generated Converters](#generated-converters) — what the source generator writes for you
- [Options reference](#options-reference)
- [Advanced scenarios](#advanced-scenarios)
- [Troubleshooting](#troubleshooting)
- [About the AI-generated parts](#about-the-ai-generated-parts)

## How to use it
Read the sections below to get familiar with the basic usage of this library.

### How to register it
Register converters for assembly, where `MyClass` is:
```csharp
builder.Services.AddConverting<MyClass>();
```
Register converters for assembly, where `MyClass` and `YourClass` is:
```csharp
builder.Services.AddConverting(typeof(MyClass).Assembly, typeof(YourClass).Assembly);
```

### How to configure it
You can use `configureOptions` when you calling `AddConverting` or `AddConverting<T>`, such as:
```csharp
builder.Services.AddConverting<MyClass>(options => 
{
    options.FormatProvider = new NumberFormatInfo();
    options.NumberStyles = NumberStyles.AllowThousands;
    options.ServiceLifetime = ServiceLifetime.Scoped;
    // And more
});
```

### How to create your own converter
Consider you want to convert instance database entity `Entity`:
```csharp
public sealed record Entity
{
    public string Firstname { get; init; }
    public string Lastname { get; init; }
}
```
To newly created instance domain instance of `Domain`
```csharp
public sealed record Domain
{
    public string Firstname { get; init; }
    public string Lastname { get; init; }
}
```
For this you have to create converting service, such as `EntityToDomainConverter`:
```csharp
internal sealed class EntityToDomainConverter : IConverter<Entity, Domain>
{
    public Domain Convert(Entity from)
    {
        if (from == null) // Or you can just relay on anotations 
        {
            throw new ArgumentNullException(nameof(from)); // Or do whatever 
        }
        
        return new Domain
        {
            Firstname = from.Firstname,
            Lastname = from.Lastname
        }
    }
}
```
If you have a such converter in the place, and you have the converting library registered using `AddConverting`, you can now just call conversion:
```csharp
public sealed class BusinessLogicService
{
    private readonly IConvertingService _convertingService;
    
    public BusinessLogicService(IConvertingService convertingService)
    {
        _convertingService = convertingService;
    }
    
    public Domain GetDomains()
    {
        var entity = StaticHelper.GetEntities();
        
        // This call will call your IConverter<Entity, Domain> converter
        var domain = _convertingService.Convert<Entity, Domain>(entity);
        
        return domain;
    }
}
```

### How to customize
You can configure library behaviour globally using the `ConvertOptions` parameter during service registration such as:
```csharp
builder.Services.AddConverting<MyClass>(options => 
{
    options.FormatProvider = new NumberFormatInfo();
    options.NumberStyles = NumberStyles.AllowThousands;
    options.ServiceLifetime = ServiceLifetime.Scoped;
    // And more
});
```
This setting is available to you by IConverterOptions singleton service, that you can inject into other dependency-injected services, including Converters:
```csharp
internal sealed class EntityToDomainConverter : IConverter<Entity, Domain>
{
    private readonly IConverterOptions _converterOptions;

    public EntityToDomainConverter(IConverterOptions converterOptions)
    {
        _converterOptions = converterOptions;
    }
    
    public Domain Convert(Entity from)
    {
        throw new NotImplementedException();
    }
}
```

## Build in Converters
### Available build in converters
Library offers build in Converters that you can use using standard API such as `_convertingService.Convert<TFrom, TTo>()`. Lifetime of these converters is fixed and it is always `Singleton`. See the list of build in conversions:

| From | To | Options |
| --- | --- | --- |
| `string` | `bool` | |
| `string` | `byte` | |
| `string` | `int` | |
| `string` | `long` | |
| `string` | `DateTime` | `FormatProvider`, `DateTimeStyles` |
| `string` | `DateTimeOffset` | `FormatProvider`, `DateTimeStyles` |
| `string` | `decimal` | `FormatProvider`, `NumberStyles` |
| `string` | `double` | `FormatProvider`, `NumberStyles` |
| `string` | `float` | `FormatProvider`, `NumberStyles` |

### How to override
It is recommended to override default converters by manually registering them **before** you call `AddConverting`, since `AddConverting` uses [`TryAdd`](https://learn.microsoft.com/en-us/dotnet/api/microsoft.extensions.dependencyinjection.extensions.servicecollectiondescriptorextensions.tryadd) registering internally:
```csharp
builder.Services.AddSingleton<IConverter<string, bool>, MyStringToBoolConverter>(); // Your overriden converter
builder.Services.AddConverting<MyClass>();
```

## Reference Converters
A Converter creates a new instance. A Reference Converter fills one you already hold — an entity your change tracker knows about, for instance — so its identity and everything the Converter does not touch survive the conversion. Implement `IReferenceConverter<TFrom, TTo>`, which extends `IConverter<TFrom, TTo>`, so one class carries both conversions:
```csharp
internal sealed class DomainToEntityReferenceConverter : IReferenceConverter<Domain, Entity>
{
    public Entity Convert(Domain from)
    {
        return Convert(from, new Entity());
    }

    public Entity Convert(Domain from, Entity to)
    {
        if (from == null) // There is nothing to enhance the instance with
        {
            return to;
        }

        to.Firstname = from.Firstname;
        to.Lastname = from.Lastname;

        return to;
    }
}
```
Call it through the two argument overload:
```csharp
var entity = await _repository.GetAsync(id);

// This call will call your IReferenceConverter<Domain, Entity> converter
entity = _convertingService.Convert<Domain, Entity>(domain, entity);
```
Because `IReferenceConverter<TFrom, TTo>` **is** an `IConverter<TFrom, TTo>`, registering it covers both calls and does not count as a duplicate registration of the pair.

## Collections and dictionaries
`IConvertingService` converts collections for you. The item Converter is resolved once and applied to every element, so you only ever write the Converter for a single item:
```csharp
IEnumerable<Domain> domains = _convertingService.Convert<Entity, Domain>(entities);
IList<Domain> list = _convertingService.Convert<Entity, Domain>(entityList);
ISet<Domain> set = _convertingService.Convert<Entity, Domain>(entitySet);

// Dictionaries convert the key and the value, each with its own Converter
IDictionary<Guid, Domain> map = _convertingService.Convert<string, Entity, Guid, Domain>(entityMap);
```
A `null` source returns `null`. Every overload has a `nullFallback` counterpart if you want something else:
```csharp
IList<Domain> never = _convertingService.Convert<Entity, Domain>(entityList, () => new List<Domain>());
```

### Getting a concrete type back
`Convert` returns the interface you asked for. `ConvertExplicitly` extension methods return the concrete type, which is what you usually want at the edge of a method:
```csharp
List<Domain> list = _convertingService.ConvertExplicitly<Entity, Domain>(entityList);
Domain[] array = _convertingService.ConvertExplicitly<Entity, Domain>(entityArray);
Dictionary<string, Domain> map = _convertingService.ConvertExplicitly<Entity, Domain>(entityMap);
IReadOnlyList<Domain> readOnly = _convertingService.ConvertExplicitly<Entity, Domain>(readOnlyEntities);
```
`List<T>`, `T[]`, `IReadOnlyList<T>`, `Dictionary<TKey, T>` and `IReadOnlyDictionary<TKey, T>` are covered, dictionary keys being `string`, `int` or `Guid`.

## Async conversions
Every `Convert` method on `IConvertingService` has an async counterpart `ConvertAsync` that returns `ValueTask<T>` and accepts an optional `CancellationToken`. This applies to single-object, reference, collection (`ISet`, `IList`, `IEnumerable`), and dictionary conversions.

### How to create an async converter
Implement `IAsyncConverter<TFrom, TTo>` instead of `IConverter<TFrom, TTo>`:
```csharp
internal sealed class EntityToDomainAsyncConverter : IAsyncConverter<Entity, Domain>
{
    private readonly IExternalService _externalService;

    public EntityToDomainAsyncConverter(IExternalService externalService)
    {
        _externalService = externalService;
    }

    public async Task<Domain> ConvertAsync(Entity from, CancellationToken cancellationToken)
    {
        var additionalData = await _externalService.GetDataAsync(from.Id, cancellationToken);

        return new Domain
        {
            Firstname = from.Firstname,
            Lastname = from.Lastname,
            AdditionalData = additionalData
        };
    }
}
```

### How to create an async reference converter
Implement `IAsyncReferenceConverter<TFrom, TTo>` instead of `IReferenceConverter<TFrom, TTo>`:
```csharp
internal sealed class EntityToDomainAsyncReferenceConverter : IAsyncReferenceConverter<Entity, Domain>
{
    public async Task<Domain> ConvertAsync(Entity from, CancellationToken cancellationToken)
    {
        // Required by IAsyncConverter<TFrom, TTo> (base interface)
        throw new NotSupportedException();
    }

    public async Task<Domain> ConvertAsync(Entity from, Domain to, CancellationToken cancellationToken)
    {
        to.Firstname = from.Firstname;
        to.Lastname = from.Lastname;
        return to;
    }
}
```

### How to call async conversions
Use `ConvertAsync` instead of `Convert`:
```csharp
public sealed class BusinessLogicService
{
    private readonly IConvertingService _convertingService;

    public BusinessLogicService(IConvertingService convertingService)
    {
        _convertingService = convertingService;
    }

    public async Task<Domain> GetDomainAsync(CancellationToken cancellationToken)
    {
        var entity = await GetEntityAsync();

        // Single object
        var domain = await _convertingService.ConvertAsync<Entity, Domain>(entity, cancellationToken);

        return domain;
    }

    public async Task<IList<Domain>> GetDomainsAsync(CancellationToken cancellationToken)
    {
        var entities = await GetEntitiesAsync();

        // Collection — converts each item using the converter
        var domains = await _convertingService.ConvertAsync<Entity, Domain>(entities, cancellationToken);

        return domains;
    }
}
```

### Sync and async interoperability
Sync and async converters are interoperable:
- If you call `Convert` (sync) and only an `IAsyncConverter` is registered, the library will block on the async converter.
- If you call `ConvertAsync` and only an `IConverter` (sync) is registered, the sync result is wrapped in a `ValueTask`.

This means you can migrate converters from sync to async (or vice versa) without changing the calling code.

## Generated Converters
You do not have to write a Converter for every pair. The package ships a source generator that writes the obvious ones for you at compile time, and `AddConverting` picks them up the same way it picks up yours — they are ordinary classes in the assembly being compiled.

### How it works
The generator looks at your `Convert<TFrom, TTo>` / `ConvertAsync<TFrom, TTo>` call sites. Every pair it finds that nobody wrote a Converter for gets one generated, in namespace `Majipro.Converter.Generated`:
```csharp
// You write this call, and nothing else
var domain = _convertingService.Convert<Entity, Domain>(entity);
```
```csharp
// The generator writes this
public sealed class EntityToDomainConverter : IReferenceConverter<Entity, Domain>
{
    public Domain Convert(Entity from)
    {
        if (from == null)
        {
            return default;
        }

        return new Domain
        {
            Firstname = from.Firstname,
            Lastname = from.Lastname
        };
    }

    public Domain Convert(Entity from, Domain to) { /* the same values, assigned to to */ }
}
```
Generation is recursive: a property whose type is itself a mappable pair is converted through `IConvertingService`, and that nested pair gets a generated Converter too.

### What it can convert
| Shape | Example | Notes |
| --- | --- | --- |
| Property of the same type | `string` to `string` | Also implicit conversions — `Guid` to `Guid?`, `int` to `long`, differing nullable annotations |
| Nested objects | `Address` to `AddressDto` | Converted through `IConvertingService`, the nested pair is generated as well |
| Collections | `List<Address>` to `List<AddressDto>` | Source is any `IEnumerable<T>`, target is `IEnumerable<>`, `ICollection<>`, `IList<>`, `IReadOnlyCollection<>`, `IReadOnlyList<>` or `List<>` |
| Scalar to its text | `int` to `string`, `DateTime` to `string` | Written as `System.Convert.ToString` |
| Text to a scalar | `string` to `int` | Only for the build in Converters listed above, so `FormatProvider` and the other options apply |
| Enum to enum | `EntityState` to `DomainState` | Matched by member **name**, so the two sides may number their members differently |
| Enum to `string` | `EntityState` to `string` | |
| `Nullable<T>` on either side | `int?` to `string` | Unwrapped, the Converter still takes and returns the nullable type |

Both conversions are generated wherever they make sense — `Convert(from)` creating a new instance and `Convert(from, to)` filling one you already hold (see *Reference converters*). The second one is left out when the target has `init` only properties or is a value type, since there is no instance to enhance in that case.

### What it refuses
The generator never guesses. When it recognizes a pair but can not write it deterministically, it fails the build with an error instead of generating a Converter that silently loses data:

| Diagnostic | Means |
| --- | --- |
| `MC0002` | An enum member on the source has no counterpart of that name on the target |
| `MC0003` | A writable target property has nothing to fill it — the source has no property of that name, or the two types are not convertible |
| `MC0004` | The pair itself can not be written, and no Converter for it is implemented in this assembly or in any assembly it references — the `Convert` call would compile and throw at runtime, so it fails the build instead |
| `MC0001` | Anything else that went wrong while generating |

A pair you ask for always ends one of three ways, and never in silence: a Converter is generated for it, one you wrote or one the library ships answers it, or the build fails. The two exceptions are a pair whose sides are the same type, which `IConvertingService` handles by itself, and a call site whose type is a generic type parameter, which is not a pair until the method is used.

### Where it looks for your Converters
Across assemblies, which is what makes `MC0004` a statement about the pair rather than about the project it was asked for in. The generator looks for **implementations** — the same thing `AddConverting` looks for at runtime — in the assembly being compiled **and in every referenced assembly that references this library**. So the ordinary shape, Converters in a class library and `Convert` calls in an application, is seen for what it is: the pair is implemented, nothing is generated for it, and nothing is reported about it.

That cuts both ways, and the second half is the one to know: nothing is generated for a pair that is already implemented, **wherever** it was implemented. The assembly holding those Converters is therefore one you have to hand to `AddConverting` — the generated assembly carries nothing for that pair:
```csharp
builder.Services.AddConverting(typeof(EntityToDomainConverter).Assembly, typeof(Program).Assembly);
```
Two shapes stay outside of this, and neither is found by a type scan at runtime either, so `MC0004` can name a pair you consider answered:

| Shape | Why it is not seen | What to do |
| --- | --- | --- |
| A Converter registered as a factory or an instance — `AddSingleton<IConverter<int, long>>(sp => ...)` | There is no class implementing the interface to find | Write it as a class, or suppress the pair by writing a Converter that delegates to it |
| A Converter that is `internal` to a **referenced** assembly | The compiler does not show a referenced assembly's internals to this compilation, while that assembly's own `AddConverting` registers it at runtime | Make it public, or move it into the assembly holding the call site |

### How to take over
Write the Converter yourself, in this assembly or in any assembly it references. A pair you implemented — `IConverter<TFrom, TTo>`, `IAsyncConverter<TFrom, TTo>` or a Reference Converter extending either — is skipped by the generator entirely, so the two never collide. That is the answer to every refusal: `MC0002` and `MC0003` are the shapes only you can decide, and `MC0004` is the pair nobody has written yet.

## Options reference
| Option | Default | Applies to |
| --- | --- | --- |
| `ServiceLifetime` | `Singleton` | The lifetime every Converter found by `AddConverting` is registered with. Build in Converters are always `Singleton` |
| `FormatProvider` | `CultureInfo.InvariantCulture` | `string` to number, `DateTime` and `DateTimeOffset` |
| `NumberStyles` | `NumberStyles.Any` | `string` to `decimal`, `double`, `float` |
| `DateTimeStyles` | `DateTimeStyles.None` | `string` to `DateTime`, `DateTimeOffset` |

The options are one singleton `IConverterOptions`, so a Converter of yours can inject it and read the same settings the build in ones read.

## Advanced scenarios
### Inject `IConvert<TFrom, TTo>` directly
Since Converters are dependency-injected services, it is possible to inject them independently without using `IConvertingService`
```csharp
public sealed class BusinessLogicService
{
    private readonly IConverter<Entity, Domain> _converter;
    
    public BusinessLogicService(IConverter<Entity, Domain> converter)
    {
        _converter = converter;
    }
    
    public Domain GetDomains()
    {
        var entity = StaticHelper.GetEntities();
        
        // This call will call your IConverter<Entity, Domain> converter
        var domain = _converter.Convert<Entity, Domain>(entity);
        
        return domain;
    }
}
```

### `IConvertingService` overriding
It is also possible to override `IConvertingService` in the same way how you can override default Converter, if the provided implementation does not fit to your needs:
```csharp
builder.Services.AddSingleton<IConvertingService, MyConvertingService>(); // Your overriden converting service
builder.Services.AddConverting<MyClass>();
```

## Troubleshooting
| What you see | What it means |
| --- | --- |
| `There is no conversion from 'X' to 'Y'` at runtime | Nothing is registered for the pair. The generator reports the pairs it can see as `MC0004` at build time, so what is left is what it can not see: the assembly holding the Converter was not passed to `AddConverting`, or the call site is generic (a type parameter is not a pair until the method is used) |
| `Duplicate converter registrations found` at startup | Two classes claim the same `From -> To` pair. A Reference Converter and the `IConverter` it extends are one registration and are not this |
| `MC0002`, `MC0003`, `MC0004` at build time | The generator refuses the pair, see [What it refuses](#what-it-refuses). Write the Converter yourself, as a class — in this assembly or in one it references |
| The generated Converter is not found at runtime | It is generated into the assembly holding the call site, so that assembly has to be one of the ones you hand to `AddConverting` |
| A hand written Converter is duplicated by a generated one | The Converter is `internal` to a referenced assembly, so the compilation it is generated into can not see it. Make it public, or move it next to the call site |


## About the AI-generated parts
This library is written with the help of Claude Code, and it is worth saying plainly which parts:

- **`Converter.Generator` (the source generator) is largely AI-generated**, together with its test suite in `Converter.Generator.Test` and the architecture notes in `CLAUDE.md`. The design decisions are reviewed and owned by a human; a lot of the code writing them down is not hand typed.
- **The runtime library** (`Converter`, `Converter.Abstrations`) is mostly hand written, with AI help in the build in Converters, tests and packaging.

Everything is reviewed before it is merged and covered by tests, but treat that as context when reading the code or reporting a bug.
