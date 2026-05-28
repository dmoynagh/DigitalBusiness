# JsonDataWrappers

A lightweight, allocation-efficient system for working with JSON data without deserializing to objects.
Wraps `JsonElement` (readonly, BCL) or `JsonNode` (read/write, BCL) behind a unified API, and supports
strongly typed access through a phantom-type key system.

---

## Why This Exists

Standard approaches to JSON in .NET require either:
- **Deserializing** to a POCO — overhead, loses unknown fields, tight coupling to a schema
- **Working raw** with `JsonElement` or `JsonNode` directly — no unified API, verbose, source-specific code

JsonDataWrappers sits between these: you work directly on the JSON without serialization, but with a clean,
consistent API that doesn't care whether the data came from a `JsonElement` or `JsonNode`.

It is designed for CMS contexts where:
- JSON structures are partially known and may contain extra fields
- Data is read far more than it is written
- Wrappers are created and discarded frequently (structs, no heap cost)
- Domain logic needs typed, scoped access to specific JSON structures

---

## Core Concepts

### `JsonData` — the untyped wrapper

The core struct. Holds either a `JsonElement` or a `JsonNode`, and exposes a unified API over both.

```csharp
JsonData data = JsonDocument.Parse(json).RootElement;   // Element-backed (readonly)
JsonData data = JsonNode.Parse(json);                    // Node-backed (read/write by default)
JsonData data = JsonData.CreateNull();                   // Explicit null
```

Key properties:

| Property | Description |
|---|---|
| `IsElement` | True if backed by a `JsonElement` |
| `IsNode` | True if backed by a `JsonNode` |
| `ReadOnly` | True for all Element-backed instances; configurable for Node-backed |
| `ValueKind` | The JSON kind: Object, Array, String, Number, etc. |
| `IsNull` | True if JSON null or uninitialized |

Indexers provide child access:

```csharp
JsonData? title = data["title"];     // property access
JsonData? first  = data[0];          // array index access
```

Primitive values can be assigned directly via implicit cast:

```csharp
JsonData name = "Alice";
JsonData count = 42;
JsonData active = true;
```

Extracted with explicit cast:

```csharp
string? name  = (string?)data["name"];
int count     = (int)data["count"];
bool? active  = (bool?)data["active"];
```

---

### `JsonData<T>` — the typed wrapper

A zero-overhead typed shell around `JsonData`. `T` is a **phantom type key** — it carries no runtime data.
Its purpose is to allow extension methods to be scoped to a specific JSON structure.

```csharp
// T adds no data — it only tells the compiler what kind of JSON this is
JsonData<ArticleItem> article = JsonData<ArticleItem>.From(data);
```

Implicit cast back to `JsonData` allows typed instances to be passed anywhere untyped data is accepted:

```csharp
void Process(JsonData data) { ... }

JsonData<ArticleItem> article = ...;
Process(article);   // implicit, no cast needed
```

---

### `IJsonDataKey` — the key marker

Any class used as `T` must implement `IJsonDataKey`. This is a marker interface — no members required.

```csharp
public class ArticleItem : IJsonDataKey { }
public class FeaturedArticleItem : ArticleItem { }  // inherits ArticleItem's extensions
```

Key types can inherit to layer extension methods. Extensions defined on `ArticleItem` are also
available on `JsonData<FeaturedArticleItem>`.

---

### Extension Methods — domain logic

All domain-specific logic lives in extension methods scoped to a specific `T`. This keeps the
wrapper structs clean and makes domain logic easy to locate, test, and extend.

```csharp
// Extensions scoped to ArticleItem
public static class ArticleItemExtensions
{
	extension(JsonData<ArticleItem> article)
	{
		public string? Title => (string?)article.Json["title"];
		public string? Body  => (string?)article.Json["body"];
		public bool IsPublished => (bool?)article.Json["published"] ?? false;
	}
}
```

Usage:

```csharp
JsonData<ArticleItem> article = ...;
Console.WriteLine(article.Title);
Console.WriteLine(article.IsPublished);
```

---

## Value Converters

The converter system extracts and creates strongly typed values from `JsonData`.

`JsonDataConverter<T>` is the entry point — it resolves the correct converter for `T` at startup
and caches it. Supported by default:

- All primitives (`string`, `int`, `bool`, `Guid`, `DateTime`, etc.)
- Nullable versions of all primitives
- Enums (stored as strings in JSON)
- `JsonElement`, `JsonNode`, `JsonData`
- Any type registered via the custom converter system
- Fallback to `JsonSerializer` for unknown types

```csharp
string? title  = JsonDataConverter<string>.TryGet(data["title"]);
int count      = JsonDataConverter<int>.Get(data["count"]);
JsonData node  = JsonDataConverter<JsonData>.Create("hello");
```

### Custom Converters

Implement `IJsonDataConverter<T>` and register it by making it a public non-abstract class —
it is discovered automatically via assembly scanning at startup.

```csharp
public class DateOnlyConverter : IJsonDataConverter<DateOnly>
{
	public bool TryGet(in JsonData jsonData, out DateOnly value)
	{
		var str = JsonDataConverter<string>.TryGet(jsonData);
		return DateOnly.TryParse(str, out value);
	}

	public JsonData Create(DateOnly value) => (JsonData)value.ToString("yyyy-MM-dd");
}
```

For open-generic or family-of-type scenarios, implement `IJsonDataConverterFactory`:

```csharp
public class MyFactory : IJsonDataConverterFactory
{
	public bool CanConvert(Type type) => type.IsSubclassOf(typeof(MyBase));
	public IJsonDataConverter? CreateConverter(Type type) => ...;
}
```

---

## Typed Arrays

`JsonDataArray<T>` is a typed wrapper around a JSON array node. It provides strongly typed element
access using the converter system, and supports mutation (add, insert, remove, clear) when the
backing data is not readonly.

```csharp
// Get a typed array from a JsonData property
JsonDataArray<string> tags = data.GetArray<string>("tags");

// Typed indexer and enumeration
string? first = tags[0];
foreach (string tag in tags) { ... }

// Mutation (only when not readonly)
tags.Add("new-tag");
tags.RemoveAt(0);

// Create or retrieve an array, creating it if absent
JsonDataArray<string> tags = data.GetOrCreateArray<string>("tags");
```

For arrays of typed wrappers, use the element type `T` constrained to the key type:

```csharp
JsonDataArray<JsonData<TagItem>> tagItems = data.GetArray<JsonData<TagItem>>("tags");
foreach (JsonData<TagItem> tag in tagItems)
{
	Console.WriteLine(tag.Name);
}
```

`TryGetArray<T>` is available when the array may be absent:

```csharp
if (data.TryGetArray<string>("tags", out var tags))
{
	foreach (string tag in tags) { ... }
}
```

---

## Collections

`JsonDataCollection` wraps various sources as an `IEnumerable<JsonData>`:

```csharp
// From a JsonArray
var col = JsonDataCollection.Create(jsonArray);

// From a JsonDocument (optionally auto-dispose)
var col = JsonDataCollection.Create(document, autoDispose: true);

// From a raw JSON string
var col = JsonDataCollection.Create("[{...},{...}]");

// Project as typed
IEnumerable<JsonData<ArticleItem>> articles = col.AsJsonData<ArticleItem>();
```

`JsonDataCollection` implements `IDisposable`. Dispose it when created from a `JsonDocument`
or JSON string to release the underlying document memory.

---

## Serialization / JSON Integration

`JsonData` and `JsonData<T>` both carry `[JsonConverter]` attributes and serialize/deserialize
transparently when used in objects passed to `System.Text.Json`.

```csharp
public class MyResponse
{
	public JsonData<ArticleItem> Article { get; set; }
}

// Deserialized from JSON — Article will be Element-backed (readonly)
var response = JsonSerializer.Deserialize<MyResponse>(json);
```

---

## Mutability

By default:
- `JsonElement`-backed instances are **always readonly**
- `JsonNode`-backed instances from `JsonNode.Parse` are **writable**
- `JsonValue` nodes (leaf primitives) are **always readonly**

To work with editable data from an Element source:

```csharp
JsonData editable = data.ToEditableJsonData();   // converts Element → Node, writable
JsonData readOnly = data.AsReadOnly();           // forces readonly, no-op if already readonly
JsonData clone    = data.Clone();                // deep copy preserving source type and readonly state
```

---

## Equality

`DeepEquals` compares structural content regardless of source type:

```csharp
bool equal = elementBacked.DeepEquals(nodeBacked);   // true if content is identical
```

---

## File Structure

```
JsonDataWrappers/
├── JsonData.cs                    Core untyped wrapper struct
├── JsonData.Operators.cs          Implicit/explicit cast operators for primitives
├── JsonDataOfT.cs                 Typed wrapper struct JsonData<T>
├── IJsonData.cs                   Base interface — exposes JsonData.Json
├── IJsonDataWrapper.cs            Interface for typed wrappers (adds init)
├── IJsonDataKey.cs                Marker interface for T phantom type keys
├── JsonDataExtensions.cs          Core extension methods (null, state, convert, count)
├── TypedJsonDataExtensions.cs     AsJsonData<T> cast extensions
├── TypedJsonDataArray.cs          Typed array wrapper struct JsonDataArray<T>
├── TypedJsonDataArrayExtensions.cs  GetArray / TryGetArray / GetOrCreateArray / Set extensions
├── JsonDataObjectExtensions.cs    AsJsonDataObject<T> helper for wrapper construction
├── JsonDataEnumExtensions.cs      Enum read/write extensions
├── JsonDataSerializedExtensions.cs  Serialized value get/set extensions
├── JsonDataJsonObjectExtensions.cs  JSON object property helpers
├── JsonDataJsonArrayExtensions.cs   JSON array item helpers
├── JsonDataCollection.cs          Collection wrappers and AsJsonData<T> projection
├── JsonDataHelper.cs              Internal utilities (node tree, property/array enumeration)
├── Converters/
│   ├── IJsonDataConverter.cs      Converter contract
│   ├── IJsonDataConverterFactory.cs  Factory contract for open-generic converters
│   ├── JsonDataConverter.cs       Static entry point — resolves and caches converters
│   ├── JsonDataConverterProvider.cs  Converter resolution pipeline
│   ├── CustomJsonDataConverters.cs   Assembly-scanned custom converter registry
│   ├── JsonDataEnumValueConverter.cs Enum ↔ JSON string conversion
│   ├── TypedJsonDataJsonConverter.cs System.Text.Json converter for JsonData<T>
│   └── JsonDataConverterProvider.*.cs  Partial files per converter category
└── Internal/
	├── JsonDataEquality.cs        Deep equality across all source combinations
	├── JsonDataExceptionHelper.cs Centralised exception factory
	└── Constants.cs               Shared constants (Guid format etc.)
```
