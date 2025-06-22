# Converter
Library for manual class mapping. This library helps you with type mapping using dependency-injected services (Converters).

## What it is
This library helps you to build dependency-injected services that can perform object mapping. We call these services "Converters" because they are performing object conversion from one type to another. The fundamental idea was to build some easy manual mapping tool that will enforce a normalized way, how mapping is done. This tool is an alternative to a common solution for manual mapping that uses static methods. Since each mapper (we call it Converter to avoid confusion with [Automapper](https://github.com/AutoMapper/AutoMapper)) is an independent dependency-injected service, you can use full advantage of that (or you don't have to if you consider that as a disadvantage) — so you can mock converters in tests, or you can leverage them by injecting other services.

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
| ------- | ------- | ------- |
| `string` | `bool` ||
| `string` | `byte` ||
| `string` | `int` ||
| `string` | `long` ||
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