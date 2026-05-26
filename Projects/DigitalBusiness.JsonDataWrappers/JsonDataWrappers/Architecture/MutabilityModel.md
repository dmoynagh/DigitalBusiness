# Mutability Model

## Two Sources, Two Behaviours

`JsonData` wraps one of two BCL JSON types, each with different mutability characteristics:

| Source | Type | Mutable? | When to use |
|---|---|---|---|
| `JsonElement` | `System.Text.Json.JsonElement` | Never | Reading from `JsonDocument`, deserialization, API responses |
| `JsonNode` | `System.Text.Json.Nodes.JsonNode` | Configurable | Building, editing, or assembling JSON data |

These are fundamentally different BCL types with no common base. `JsonData` unifies them.

---

## ReadOnly Rules

`ReadOnly` is always true when:
- The source is a `JsonElement` (immutable by design in the BCL)
- The source is a `JsonValue` (leaf primitive — no children to mutate)
- There is no source (default/uninitialized struct)

`ReadOnly` is configurable when:
- The source is a `JsonObject` or `JsonArray` node
- The constructor is called with `readOnly: true` to lock a node-backed instance

```csharp
// Always readonly
JsonData fromElement = JsonDocument.Parse(json).RootElement;
JsonData fromValue   = JsonNode.Parse("\"hello\"");   // JsonValue — always readonly

// Writable by default
JsonData fromObject = JsonNode.Parse("{\"title\":\"Hello\"}");
JsonData fromArray  = JsonNode.Parse("[1,2,3]");

// Force readonly on a node
JsonData locked = new JsonData(jsonObject, readOnly: true);
JsonData locked = fromObject.AsReadOnly();
```

---

## Default Struct Safety

A `default(JsonData)` or uninitialized `JsonData<T>` has no source. In this state:
- `ReadOnly` returns `true` — prevents accidental mutation
- `IsNull` returns `true` — treated as null content
- `ValueKind` returns `JsonValueKind.Null`

This is a deliberate safety measure. An uninitialized wrapper behaves as a readonly null,
not as a writable empty structure.

---

## Converting Between Sources

Sometimes you need to move between source types. The extension methods handle this:

```csharp
// Convert to Element-backed (readonly copy)
JsonData readOnly = data.ToJsonElementJsonData();

// Convert to Node-backed (default: preserve current readonly state)
JsonData asNode = data.ToJsonNodeJsonData();

// Convert to Node-backed and force writable
JsonData editable = data.ToEditableJsonData();
// equivalent to:
JsonData editable = data.ToJsonNodeJsonData(readOnly: false);

// Clone (deep copy, same source type, same readonly state)
JsonData copy = data.Clone();
```

> **Note:** Converting from Element to Node involves a parse step. Do this once and cache the
> result if you need to make multiple edits. Do not call `ToEditableJsonData` in a loop.

---

## Mutation Guard

Any write operation checks `ReadOnly` first. Attempting to mutate a readonly instance throws:

```csharp
jsonData.ThrowIfReadOnly();   // call explicitly before any mutating logic
jsonData["title"] = "New";    // throws InvalidOperationException if ReadOnly
```

The setter on the `JsonData` indexer calls this guard internally.

---

## JsonNode Ownership Rules

`JsonNode` enforces single-parent ownership — a node cannot exist in two trees simultaneously.
Attempting to add a node that already has a parent to another tree throws at the BCL level.

`JsonDataHelper.GetNodeToAdd` handles this transparently:
- If the node has no parent — use it directly
- If it shares the same root — use it directly (already in the same tree)
- If it has a different root — deep clone it first

This is used internally by the set/add operations and should be considered when writing
custom mutation code that works directly with nodes.

---

## Summary: Which Source to Use

| Scenario | Recommended source |
|---|---|
| Reading API response or file | `JsonElement` — zero allocation, readonly by design |
| Building new JSON to send | `JsonNode` — writable, composable |
| Editing existing JSON | Convert to `JsonNode` via `ToEditableJsonData()` |
| Passing to code that only reads | Either — use `JsonData` untyped, cast will work |
| Storing in a long-lived cache | `JsonElement` from a cloned `JsonDocument`, or Node with `AsReadOnly()` |
