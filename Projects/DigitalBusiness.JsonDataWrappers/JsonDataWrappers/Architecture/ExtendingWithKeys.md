# Extending with Keys — Domain-Specific JsonData

## Overview

The key + extension method pattern is how domain logic is attached to `JsonData<T>` without
modifying the wrapper itself. This document explains how to define keys, write extensions,
and organise them for a CMS or similar application.

---

## Step 1 — Define a Key

Create a plain class implementing `IJsonDataKey`. No members are required.

```csharp
// In your domain or CMS project
public class ArticleItem : IJsonDataKey { }
```

The key is used only as a type token. It carries no data and has no methods.

---

## Step 2 — Write Extension Methods

Use C# 14 `extension` blocks (or classic static extension methods) scoped to `JsonData<YourKey>`.

```csharp
public static class ArticleItemExtensions
{
	extension(JsonData<ArticleItem> article)
	{
		// Read a string property
		public string? Title => (string?)article.Json["title"];

		// Read with a fallback
		public string Slug => (string?)article.Json["slug"] ?? string.Empty;

		// Read a nested object as another typed wrapper
		public JsonData<AuthorItem> Author =>
			article.Json["author"]?.AsJsonData<AuthorItem>() ?? default;

		// Computed property
		public bool IsPublished =>
			(bool?)article.Json["published"] ?? false;

		// Enum property
		public ArticleStatus Status =>
			article.Json.TryGetEnum<ArticleStatus>("status", out var s) ? s : ArticleStatus.Draft;
	}
}
```

---

## Step 3 — Use the Typed Wrapper

```csharp
JsonData raw = GetJsonFromCms();

JsonData<ArticleItem> article = JsonData<ArticleItem>.From(raw);
// or via extension:
JsonData<ArticleItem> article = raw.AsJsonData<ArticleItem>();

Console.WriteLine(article.Title);
Console.WriteLine(article.IsPublished);
Console.WriteLine(article.Author.Name);   // if AuthorItem extensions are defined
```

---

## Layering via Inheritance

Key types inherit each other to share extensions across related structures.

```csharp
// Base key — shared properties for all content
public class ContentItem : IJsonDataKey { }

// Article-specific key
public class ArticleItem : ContentItem { }

// Featured article adds to article
public class FeaturedArticleItem : ArticleItem { }
```

```csharp
// ContentItem extensions — available on all three
extension(JsonData<ContentItem> item)
{
	public string? Title    => (string?)item.Json["title"];
	public DateTime? Created => (DateTime?)item.Json["created"];
}

// ArticleItem extensions — available on ArticleItem and FeaturedArticleItem
extension(JsonData<ArticleItem> article)
{
	public string? Body => (string?)article.Json["body"];
}

// FeaturedArticleItem extensions — adds its own
extension(JsonData<FeaturedArticleItem> featured)
{
	public string? FeatureImageUrl => (string?)featured.Json["featureImage"];
}
```

A `JsonData<FeaturedArticleItem>` has access to all three sets.

---

## Working with Child Objects

Child objects should be accessed as typed wrappers themselves, not as raw `JsonData`:

```csharp
// Prefer this — typed child
public JsonData<AuthorItem> Author =>
	article.Json["author"]?.AsJsonData<AuthorItem>() ?? default;

// Not this — caller has to know what it is
public JsonData? Author => article.Json["author"];
```

The child wrapper can then have its own extension methods via its own key type.

---

## Working with Arrays

For typed arrays, use `JsonDataCollection` and `AsJsonData<T>`:

```csharp
extension(JsonData<ArticleItem> article)
{
	public IEnumerable<JsonData<TagItem>> Tags
	{
		get
		{
			var tagsNode = article.Json["tags"];
			if (tagsNode is null || tagsNode.Value.IsNull) return [];
			return JsonDataCollection.Create(tagsNode.Value.Node as JsonArray
				   ?? throw new InvalidOperationException("tags is not an array"))
				   .AsJsonData<TagItem>();
		}
	}
}
```

---

## Writable Extensions

For write operations, check `ReadOnly` first:

```csharp
extension(JsonData<ArticleItem> article)
{
	public void SetTitle(string title)
	{
		article.Json.ThrowIfReadOnly();
		article.Json["title"] = (JsonData)title;
	}
}
```

If you need to edit an Element-backed instance, convert first:

```csharp
JsonData<ArticleItem> editable =
	article.Json.ToEditableJsonData().AsJsonData<ArticleItem>();

editable.SetTitle("Updated Title");
```

---

## Organisation Conventions

| Concern | Where to put it |
|---|---|
| Key class | Domain project, near the feature that owns the data |
| Read extensions | Same file or folder as the key |
| Write extensions | Separate file if mutation logic is significant |
| Shared base extensions | Separate file named after the base key |
| Collection projections | Near the code that produces collections |

A typical file layout for a CMS domain:

```
Content/
├── ArticleItem.cs                  Key class
├── ArticleItemExtensions.cs        Read extensions
├── ArticleItemWriteExtensions.cs   Write extensions (if needed)
├── ContentItem.cs                  Base key
└── ContentItemExtensions.cs        Base extensions shared across content types
```

---

## What Not To Do

- **Do not** put state or logic in the key class — it is a compile-time marker only
- **Do not** try to cast `JsonData<A>` to `JsonData<B>` directly — unwrap to `JsonData` first, then re-wrap
- **Do not** store `JsonData<T>` in long-lived collections unless the source data outlives the collection
- **Do not** call `ToEditableJsonData()` in a loop — convert once, then mutate
