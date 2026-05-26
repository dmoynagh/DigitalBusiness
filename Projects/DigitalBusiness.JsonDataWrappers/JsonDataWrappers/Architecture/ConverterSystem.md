# Converter System

## Purpose

The converter system extracts typed values from `JsonData` and creates `JsonData` from typed values.
It decouples type-specific conversion logic from the wrapper structs and makes the system extensible
without modifying core code.

---

## Entry Point — `JsonDataConverter<T>`

This is the static class you call from application code and extension methods.

```csharp
// Get a value (throws on failure)
string title = JsonDataConverter<string>.Get(data["title"]);

// Try get (returns default on failure)
int? count = JsonDataConverter<int>.TryGet(data["count"]);

// Try get with out param
if (JsonDataConverter<Guid>.TryGet(data["id"], out var id))
{
	// use id
}

// Create JsonData from a typed value
JsonData node = JsonDataConverter<DateOnly>.Create(DateOnly.Today);
```

`JsonDataConverter<T>` resolves the converter for `T` once at first use and caches it.
Subsequent calls are a direct delegate invocation — no lookup overhead.

---

## Converter Resolution Pipeline

When a converter is first requested for `T`, `JsonDataConverterProvider` resolves it in priority order:

| Priority | Source | Covers |
|---|---|---|
| 1 | Primitive converters | `string`, `bool`, all numeric types, `DateTime` |
| 2 | Nullable converters | `int?`, `bool?`, etc. — wraps the underlying primitive converter |
| 3 | JSON type converters | `JsonElement`, `JsonNode`, `JsonObject`, `JsonArray` |
| 4 | Defined converters | `JsonData`, `Guid`, `DateTimeOffset`, `Uri`, etc. |
| 5 | Enum converters | Any `struct, Enum` — reads/writes as JSON string |
| 6 | JsonData converters | `JsonData<T>` typed wrappers |
| 7 | Custom converters | Assembly-scanned implementations of `IJsonDataConverter<T>` |
| 8 | Serialization fallback | `JsonSerializer` for any other type |
| 9 | Undefined | Returns a no-op converter that always returns default |

---

## Built-In Converter Coverage

### Primitives
`string`, `bool`, `byte`, `sbyte`, `short`, `ushort`, `int`, `uint`, `long`, `ulong`,
`float`, `double`, `decimal`, `DateTime`, and all nullable variants.

### Extended
`Guid`, `DateTimeOffset`, `Uri`, `JsonElement`, `JsonNode`, `JsonObject`, `JsonArray`, `JsonData`, `JsonData<T>`

### Enums
All `struct, Enum` types. Values are stored as their string name in JSON.

---

## Custom Converters — Simple Type

Implement `IJsonDataConverter<T>` as a public, non-abstract, non-interface class.
It will be discovered automatically via assembly scanning at startup. No registration required.

```csharp
public class DateOnlyConverter : IJsonDataConverter<DateOnly>
{
	public bool TryGet(in JsonData jsonData, out DateOnly value)
	{
		var str = JsonDataConverter<string>.TryGet(jsonData);
		if (str is not null && DateOnly.TryParse(str, out value))
			return true;

		value = default;
		return false;
	}

	public JsonData Create(DateOnly value) =>
		JsonDataConverter<string>.Create(value.ToString("yyyy-MM-dd"));
}
```

`TryGet` should never throw — return `false` if conversion is not possible.
`Create` should produce a `JsonData` that `TryGet` can round-trip.

---

## Custom Converters — Factory (Open Generic or Family)

When a single converter class handles a family of types, implement `IJsonDataConverterFactory`:

```csharp
public class MyCollectionConverterFactory : IJsonDataConverterFactory
{
	public bool CanConvert(Type type) =>
		type.IsGenericType && type.GetGenericTypeDefinition() == typeof(List<>);

	public IJsonDataConverter? CreateConverter(Type typeToConvert)
	{
		var elementType = typeToConvert.GetGenericArguments()[0];
		var converterType = typeof(ListConverter<>).MakeGenericType(elementType);
		return Activator.CreateInstance(converterType) as IJsonDataConverter;
	}
}
```

Factories are also discovered automatically — just make the class public and non-abstract.

---

## Enum Converters

Enums are handled by `JsonDataEnumValueConverter<TEnum>`, which reads and writes
enum values as their string names in JSON.

Extension methods are available via `JsonDataEnumExtensions`:

```csharp
// From a value node
ArticleStatus status = data.GetEnum<ArticleStatus>();
ArticleStatus? status = data.TryGetEnum<ArticleStatus>();

// From a named property
ArticleStatus status = data.GetEnum<ArticleStatus>("status");

// Write
data.SetEnum("status", ArticleStatus.Published);

// Create a JsonData from an enum value
JsonData node = JsonData.CreateFromEnum(ArticleStatus.Published);
```

---

## Using Converters in Extension Methods

Extension methods on `JsonData<T>` typically use the converter system for typed access:

```csharp
extension(JsonData<ArticleItem> article)
{
	// Simple — uses explicit cast operator (which calls converter internally)
	public string? Title => (string?)article.Json["title"];

	// Explicit converter use — useful for non-primitive types
	public DateOnly? Published =>
		JsonDataConverter<DateOnly>.TryGet(article.Json["published"] ?? default);
}
```

---

## Considerations

- Converter resolution is **per-type**, resolved once and cached. There is no per-call overhead after first use.
- The assembly scan runs at startup. In applications with many assemblies, this is done once on `AppDomain.CurrentDomain`.
- The `Undefined` fallback converter does not throw — it returns `default` for `TryGet` and a null `JsonData` for `Create`. If a type has no converter and the serialization fallback also fails, this is what you get.
- For performance-critical paths, prefer the built-in primitive converters or register a custom converter rather than relying on the `JsonSerializer` fallback.
