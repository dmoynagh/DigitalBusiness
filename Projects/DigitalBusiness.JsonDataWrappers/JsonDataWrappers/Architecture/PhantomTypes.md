# Phantom Types and the JsonData&lt;T&gt; Pattern

## What is a Phantom Type?

A phantom type is a generic type parameter that **exists only at compile time**.
It carries no runtime data — no fields, no properties, nothing. Its only job is to give
the compiler a token it can use to resolve different sets of methods for the same underlying type.

In this system, `JsonData<T>` is the same struct regardless of what `T` is at runtime.
Two variables of type `JsonData<ArticleItem>` and `JsonData<ProductItem>` hold identical data.
The `T` is erased at runtime by the JIT. What `T` enables is **compile-time method dispatch**.

---

## Why Use This Instead of Inheritance?

The obvious alternative would be subclassing:

```csharp
public class ArticleData : JsonData { ... }  // won't work — JsonData is a struct
```

Structs cannot be inherited in C#. This rules out the standard OO approach entirely.

The phantom type + extension method pattern is the struct-compatible equivalent:

```csharp
// T acts as the "type" without any struct inheritance
public readonly struct JsonData<T> : IJsonDataWrapper where T : IJsonDataKey
{
	public JsonData Json { get; init; }
}
```

---

## How the Dispatch Works

Extension methods in C# can be constrained to a specific generic argument:

```csharp
// Only available on JsonData<ArticleItem> and subtypes
extension(JsonData<ArticleItem> article)
{
	public string? Title => (string?)article.Json["title"];
}

// Only available on JsonData<ProductItem>
extension(JsonData<ProductItem> product)
{
	public decimal Price => (decimal)product.Json["price"];
}
```

The compiler resolves which set of extension methods applies based on the type of `T`.
There is no runtime cost — this is equivalent to calling a static method.

---

## Key Inheritance for Method Layering

Key types can inherit from other key types. Extension methods defined for a base key
are accessible on all derived keys:

```csharp
public class ContentItem : IJsonDataKey { }         // base key
public class ArticleItem : ContentItem { }           // inherits ContentItem extensions
public class FeaturedArticleItem : ArticleItem { }   // inherits ArticleItem + ContentItem extensions
```

This allows a layered extension model:

```csharp
// ContentItem extensions — available on all content types
extension(JsonData<ContentItem> item)
{
	public string? Title => (string?)item.Json["title"];
	public DateTime? Created => (DateTime?)item.Json["created"];
}

// ArticleItem extensions — adds article-specific members
extension(JsonData<ArticleItem> article)
{
	public string? Body => (string?)article.Json["body"];
}

// FeaturedArticleItem inherits both
JsonData<FeaturedArticleItem> featured = ...;
string? title = featured.Title;   // from ContentItem extensions
string? body  = featured.Body;    // from ArticleItem extensions
```

---

## Implicit Cast to JsonData

`JsonData<T>` implicitly casts to `JsonData`. This means typed instances can be passed
anywhere an untyped `JsonData` is accepted, without explicit casting:

```csharp
void Save(JsonData data) { ... }

JsonData<ArticleItem> article = ...;
Save(article);   // implicit — T is dropped, raw data passes through
```

The reverse (untyped → typed) is explicit, requiring deliberate intent:

```csharp
JsonData raw = ...;
JsonData<ArticleItem> article = JsonData<ArticleItem>.From(raw);   // explicit wrapping
```

---

## What T Is and Is Not

| T **is** | T **is not** |
|---|---|
| A compile-time dispatch key | A runtime type token |
| A marker that selects extension methods | A class with data or behaviour |
| Inheritable for method layering | A base class for the JSON data |
| Enforced by `where T : IJsonDataKey` | Visible in the debugger by default (use `[DebuggerDisplay]`) |

---

## Implications for Code Consumers

- **Do not** put logic inside the key class. It should be empty.
- **Do** put all domain logic in extension methods scoped to the key.
- **Do** use key inheritance to share common extensions across related types.
- **Do not** rely on `typeof(T)` or reflection on `T` for runtime decisions — use the JSON
  content itself (e.g. `ValueKind`, property checks) for that.
