# JsonDataWrappers — Work Package Implementation Guide

> **Renamed** from `JsonDataWrappers_Implementation_2026-07-22-1.md` to match the standard
> `WorkPackage`/`WorkPackage_Implementation`/`WorkPackage_Outcome` naming family (see
> `DocumentationMethodology.md` §7).
>
> **Paired with:** `JsonDataWrappers_WorkPackage_2026-07-22-1.md` (§3, Code tasks). Same
> date-N so the two are visibly linked. This document covers the *how*; the Work Package
> tracks the *what/when-done* checklist — check items off there as each part below lands.
>
> **Design context for this package:** this Work Package's code tasks span nearly the whole
> of the library's design (null model, converters, renames, diff, merge) — broad enough that
> extracting only the relevant slice would either be lossy or end up reproducing most of
> Design anyway. Per `DocumentationMethodology.md` §7a, **the current
> `JsonDataWrappers_Design.md` is bundled alongside this Implementation guide** rather than
> extracted piecemeal — see the Work Package's own "Bundle contents" section for the full
> list of what travels together. `JsonDataWrappers_Decisions.md` is *not* bundled; the one
> piece of reasoning that matters for implementation (the `DeepEquals`/`DeepSemanticEquals`
> split, §8a below) is already extracted and restated here in full.
>
> **Source inspected:** `DigitalBusiness_JsonDataWrappers.zip`, project
> `DigitalBusiness.JsonDataWrappers` (net10.0, C# with the block-scoped `extension(...)`
> member syntax — new members below should follow the same pattern already used throughout
> the codebase, not classic `this`-parameter extension methods). All file paths below are
> relative to `DigitalBusiness.JsonDataWrappers/JsonDataWrappers/`.
>
> Scoped to this session's confirmed items only (F1–F10, Q1–Q6). Does **not** re-litigate
> anything already shipped and working in the current source — only what the Work Package
> lists as pending.

---

## Before starting

Everything below modifies **existing, working, reviewed source** — this is not a rewrite.
Read each "Current state" subsection against the actual file before changing it; line numbers
will drift from what's quoted here as the file changes, so match on the method signature, not
the line number.

---

## 1. Null model (§4.1, F1/Q1)

**The bug, precisely:** `Set` and "remove" are currently conflated for **object properties**
only — arrays already do the right thing.

### 1a. `Converters`-unrelated root cause — `JsonDataHelper.GetNodeToAdd`

`JsonDataHelper.cs`:

```csharp
public static JsonNode? GetNodeToAdd(in JsonData addValue, JsonNode addToNode)
{
    if (addValue.IsNull) return default;   // ← returns null both for "explicit JSON null"
                                            //   and for "no value" — these must not collapse
    ...
}
```

`JsonData.IsNull` is deliberately true for *both* "uninitialized wrapper" and "wraps an
explicit JSON null value" (documented behaviour, not itself a bug — `IsUnset`, added below,
is what distinguishes them where the source allows it). The bug is that **`Set` then can't
tell these two cases apart either**, because it asks the same question `GetNodeToAdd` already
collapsed.

**This method needs no change.** It's correct as a "what JsonNode should I attach" helper —
returning `null` for a JSON-null-valued input is exactly right, *because a JSON null **is**
represented as a null `JsonNode` reference in `System.Text.Json.Nodes`*. The bug is entirely
in what the caller does with that `null` result.

### 1b. The actual bug — `JsonDataJsonObjectExtensions.Set(string, JsonData?)`

`JsonDataJsonObjectExtensions.cs`, current:

```csharp
public void Set(string key, JsonData? value)
{
    jsonData.ThrowIfReadOnly();
    jsonData.ThrowIfNotObject();

    var addNode = value.HasValue ? JsonDataHelper.GetNodeToAdd(value.Value, jsonData.Node!) : null;
    if (addNode is not null)
    {
        jsonData.Node![key] = addNode;
    }
    else
    {
        jsonData.Remove(key);   // ← wrong: also hit when value.Value.IsNull is true
    }
}
```

Because `addNode` is `null` in *both* the "no value supplied" case and the "value is an
explicit JSON null" case, both currently fall into the `Remove` branch. Per §4.1: **`Set`
must always write, never remove; removal is `Remove`/`RemoveAt` only.**

**Fix — change the signature to take a non-nullable `JsonData` and always write:**

```csharp
public void Set(string key, JsonData value)
{
    jsonData.ThrowIfReadOnly();
    jsonData.ThrowIfNotObject();

    var addNode = JsonDataHelper.GetNodeToAdd(value, jsonData.Node!);
    jsonData.Node![key] = addNode;   // null addNode writes a JSON null property — that's correct
}
```

`jsonData.Node![key] = null` is exactly how `System.Text.Json.Nodes.JsonObject` represents an
explicit null property value — no special-casing needed once the signature stops accepting
"no value" as a distinct, removal-triggering state.

**Ripple effects to check/update at every call site of this overload** (grep for
`.Set(` calls passing a `JsonData?`-typed local across the whole project once this lands —
the compiler will catch most, but check these specifically since they're the ones actually
touched):

- `JsonDataJsonObjectExtensions.GetOrCreateObject(string)` / `GetOrCreateArray(string)` —
  call `jsonData.Set(name, newValue)` where `newValue` is already a non-nullable `JsonData`
  (from `JsonData.CreateObject()`/`CreateArray()`). **No change needed** — already compatible.
- `JsonDataTypedExtensions.Set<T>(string name, T? value)` — currently:
  ```csharp
  public void Set<T>(string name, T? value)
  {
      if (value == null) { jsonData.Remove(name); return; }
      JsonData? newNode = JsonData.Create<T>(value);
      jsonData.Set(name, newNode);
  }
  ```
  Change the local to non-nullable (`JsonData.Create<T>(value)` already returns a
  non-nullable `JsonData` — the `JsonData?` local was only ever a stylistic mismatch, never
  actually null): `JsonData newNode = JsonData.Create<T>(value); jsonData.Set(name, newNode);`.
  **The `value == null` → `Remove` behaviour here is intentional and stays** — this is the
  *typed* setter, where a caller passing a C# `null` for `T?` means "I have no value," a
  different and legitimate convention from the untyped `JsonData` case. Q1/F1 only targets
  the untyped `JsonData?` overloads.
- `TypedJsonDataArrayExtensions.Set<T>(string name, JsonDataArray<T>? array)` — already calls
  `jsonData.Set(name, newNode)` with `newNode` typed as non-nullable `JsonData` (from
  `array.Value.Json`). **No change needed.**
- `JsonDataEnumExtensions.SetEnum<TEnum>(string name, TEnum value)` — calls
  `jsonData.Set(name, CreateFromEnum(value))`; `CreateFromEnum` returns non-nullable
  `JsonData`. **No change needed.** Check the `TEnum?` (nullable enum) overload similarly —
  if it currently short-circuits on a `null` enum value before calling `Set`, that's the same
  legitimate typed-null-means-remove pattern as above and stays as-is.
- `JsonDataPathExtensions.Set(JsonPath path, JsonData? value)` — currently delegates to
  `parent.Set(last.Property, value)`. Change this signature to `JsonData value` too (same
  reasoning, same fix), and confirm its own callers pass non-nullable values the same way.

### 1c. Array side — no fix needed, confirm only

`JsonDataJsonArrayExtensions.Set(int index, JsonData? value)` **already does the right
thing** — `addNode` (including a `null` from an explicit-JSON-null `value`) is written
directly into the array (`jsonArray[index] = addNode`), which is exactly how a JSON array
represents a null element. Only `RemoveAt` removes. **No code change required here** — this
was already confirmed correct in review; just don't let the object-side fix bleed into
"fixing" something on the array side that isn't broken. Worth a quick unit test asserting this
explicitly, so a future refactor doesn't accidentally "fix" it into the same bug.

### 1d. `IsUnset`

Add alongside the existing `IsNull` in `JsonDataExtensions.cs`:

```csharp
/// <summary>
/// True if there is no source at all — distinct from an explicit JSON null.
/// Only meaningful for Element-backed instances: a missing JsonElement.HasValue is
/// distinguishable from JsonValueKind.Null. Node-backed instances cannot distinguish
/// this (JsonNode uses a null reference to represent both states) — always returns
/// false for Node-backed instances; documented limitation, not a bug to fix here.
/// </summary>
public bool IsUnset => !jsonData.Element.HasValue && jsonData.Node is null;
```

Place it next to `IsNull` (same `extension(in JsonData jsonData)` block in
`JsonDataExtensions.cs`) so the two are read together; add an XML-doc cross-reference from
each to the other.

---

## 2. Converter registration (§4.2, F2/Q2)

### 2a. Rename and re-scope `CustomJsonDataConverters` → `JsonDataConverters`

Current file: `Converters/CustomJsonDataConverters.cs`. This is the file to change — it's
already the registration/lookup class the design is describing, just under the old name and
with the rejected ambient-scan behaviour still in it.

**Remove:**
```csharp
static CustomJsonDataConverters()
{
    ScanAssemblies(AppDomain.CurrentDomain.GetAssemblies());
    AppDomain.CurrentDomain.AssemblyLoad += (_, args) => ScanAssemblies([args.LoadedAssembly]);
}
```
Delete this static constructor entirely — no ambient scan at type-touch, no
`AssemblyLoad` hook. `ScanAssemblies` (private) becomes the body of a new public
`RegisterFromAssembly`, called explicitly and only when asked.

**Rename the class** `CustomJsonDataConverters` → `JsonDataConverters` (update the one
reference in `JsonDataConverterProvider.cs`'s `GetConverter<T>()` chain,
`CustomJsonDataConverters.GetConverter<T>()` → `JsonDataConverters.GetConverter<T>()`).

**Add the explicit registration surface:**

```csharp
public static class JsonDataConverters
{
    private static readonly ConcurrentDictionary<Type, IJsonDataConverter> _converters = new();
    private static readonly List<IJsonDataConverterFactory> _factories = new();
    private static readonly object _lock = new();
    private static readonly HashSet<Assembly> _scannedAssemblies = new();
    private static volatile bool _frozen;

    public static void Register<T>(IJsonDataConverter<T> converter)
    {
        ThrowIfFrozen();
        if (!_converters.TryAdd(typeof(T), converter))
            throw new InvalidOperationException($"A converter for {typeof(T).FullName} is already registered.");
    }

    public static void Register(IJsonDataConverterFactory factory)
    {
        ThrowIfFrozen();
        lock (_lock) { _factories.Add(factory); }
    }

    public static void RegisterFromAssembly(Assembly assembly)
    {
        ThrowIfFrozen();
        ScanAssemblies([assembly]);   // existing private method body, reused as-is
    }

    public static void Freeze() => _frozen = true;

    private static void ThrowIfFrozen()
    {
        if (_frozen) throw new InvalidOperationException(
            "JsonDataConverters is frozen — Register/RegisterFromAssembly can no longer be called.");
    }

    // GetConverter<T>, ScanAssemblies: keep existing bodies, but ScanAssemblies'
    // Activator.CreateInstance(type) + _converters.TryAdd(...) path for a duplicate found
    // during a *scan* should NOT throw (a scan finding two converters for the same type is
    // a "first one found wins, ambiguous" situation the design doesn't ask to make fatal —
    // only *explicit* Register<T> duplicate calls throw, per "Duplicate registration for the
    // same T throws — no silent first-wins" in §4.2, which is about the explicit API).
    // Confirm this distinction against the design doc's exact wording before finalizing —
    // flagged here as a judgment call the design text doesn't fully disambiguate.
}
```

**Note the flagged judgment call above explicitly** in code review: the design's "no silent
first-wins" sentence is about the *explicit* `Register<T>` call throwing on a duplicate. It's
ambiguous whether an assembly *scan* finding two candidate types for the same `T` should also
throw, or keep the scan's existing first-wins tolerance (since a scan is inherently
best-effort bootstrap sugar, per §4.2's own framing: "keeps scanning available as a
deliberate bootstrap act"). Recommend: scan keeps first-wins (don't throw), only explicit
`Register<T>` throws on duplicate. Confirm this reading with David before shipping if it
matters for the CMS's own bootstrap sequence.

### 2b. `JsonDataConverter<T>` — defer resolution to first use, not type touch

Current, `Converters/JsonDataConverter.cs`:

```csharp
public static class JsonDataConverter<T>
{
    private static readonly IJsonDataConverter<T> _converter = JsonDataConverterProvider.GetConverter<T>();
    // eager — runs at first *touch* of JsonDataConverter<T>, which can happen before
    // application bootstrap has finished calling Register<T>
```

Change to lazy, resolved on first actual conversion call:

```csharp
public static class JsonDataConverter<T>
{
    private static readonly Lazy<IJsonDataConverter<T>> _converter =
        new(() => JsonDataConverterProvider.GetConverter<T>(), isThreadSafe: true);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T Get(in JsonData jsonData) => _converter.Value.TryGet(jsonData, out var result)
        ? result : throw new InvalidOperationException($"Cannot convert JsonData to {typeof(T).FullName}.");

    // TryGet/Create overloads: same pattern, `_converter.Value` in place of `_converter`
}
```

**Debug-build "resolving a type a later registration would have served" check** (§4.2's
stated debug-only diagnostic): this needs `JsonDataConverters` to track, per resolved type,
whether resolution happened *before* a subsequent `Register<T>` call for that same type —
i.e. `GetConverter<T>` should record first-resolution order, and `Register<T>` should check
`#if DEBUG` whether the type was already resolved and, if so, throw with a message naming the
type and pointing at the "register before first use" contract. This is a debug-only
diagnostic addition to `Register<T>`, not a change to the `Get`/`TryGet` hot path.

### 2c. `[JsonConverter(typeof(...))]` attribute fallback

Not present in the current chain (`JsonDataConverterProvider.GetConverter<T>()`'s ordering:
primitive → nullable → JSON converter → defined → enum → JsonData converter → custom
(registered) → serialization fallback → `UndefinedConverter`). Add a new step **after**
`GetJsonConverter<T>()`'s existing check and **before** `CustomJsonDataConverters`/
`JsonDataConverters` in that chain — check `typeof(T).GetCustomAttribute<JsonConverterAttribute>()`
and, if present, wrap it as an `IJsonDataConverter<T>` adapter. Per §4.2: **explicit
registration via `Register<T>` always wins over the attribute fallback** — so the ordering
in `GetConverter<T>` must put the explicit `JsonDataConverters.GetConverter<T>()` lookup
*before* the new attribute-fallback step, not after. Reorder the existing chain accordingly
when adding this.

---

## 3. Committed-state integrity rule (§4.3, F3) — no code change

Add an XML-doc remark to `AsReadOnly()` in `JsonDataExtensions.cs`:

```csharp
/// <summary>Returns a readonly view. If already readonly, returns itself with no allocation.</summary>
/// <remarks>
/// This is a guard against accidental mutation, not a security boundary — a caller holding
/// the original writable JsonNode-backed instance can still mutate the underlying tree even
/// after another caller obtains a read-only view via this method (both views share the same
/// node graph). Anything diffed, audited, or persisted as a committed snapshot should be
/// Element-backed, not merely readonly-flagged Node-backed, to get a genuinely independent copy.
/// </remarks>
public JsonData AsReadOnly() => ...
```

No behavioural change — documentation only, as the Work Package states.

---

## 4. Renames (§4.4, F6/F9/Q6)

All four are pure renames, no behaviour change. `JsonDataExtensions.cs` for the first three,
`TypedJsonDataArrayExtensions.cs` for the fourth:

| File | Old | New |
|---|---|---|
| `JsonDataExtensions.cs` | `ToJsonElementJsonData()` | `ToElementBacked()` |
| `JsonDataExtensions.cs` | `ToJsonNodeJsonData(bool? readOnly = null)` | `ToNodeBacked(bool? readOnly = null)` |
| `JsonDataExtensions.cs` | `ToEditableJsonData()` | `ToEditable()` |
| `TypedJsonDataArrayExtensions.cs` | `EnsureArray<T>(int index)` | `GetOrCreateArray<T>(int index)` |

The fourth already has a same-named string-keyed sibling, `GetOrCreateArray<T>(string name)`,
in the same file — the rename brings the int-indexed overload in line with it (no signature
collision: differs by parameter type, and the non-generic `GetOrCreateArray(int index)` on
plain `JsonData` in `JsonDataJsonArrayExtensions.cs` is a different, already-distinct method
by generic arity).

Use your IDE's rename-symbol refactor (not find/replace) for all four — each has multiple
call sites across the test project and internal usages, and a symbol rename will update every
reference correctly including in XML-doc `<see cref>` tags.

---

## 5. Array enumeration (§4.4, Q5)

Current, `TypedJsonDataArray.cs`:

```csharp
public T? this[int index]
{
    get => Json.TryGet<T>(index);   // soft: returns default(T?) on null/mismatch
    set => Json.Set(index, value);
}

private IEnumerable<T> Items => Json.Items.Select(jsonDataItem => jsonDataItem.Get<T>());
// Get<T>() throws on null/mismatch — inconsistent with the indexer above
```

**Fix — make enumeration match the indexer's `TryGet` semantics:**

```csharp
private IEnumerable<T?> Items => Json.Items.Select(jsonDataItem => jsonDataItem.TryGet<T>());

IEnumerator<T> IEnumerable<T>.GetEnumerator() => Items.GetEnumerator()!;
// or, if the public contract should stay IEnumerable<T> (non-nullable T) rather than
// IEnumerable<T?>: keep Items as IEnumerable<T> but populate each slot with TryGet<T>()'s
// result (which is `default` on failure) rather than the throwing Get<T>() — the point is
// the *value produced on failure*, not the enumerable's declared nullability annotation.
// Confirm which of these two shapes matches the rest of the codebase's nullable-reference
// conventions before finalizing; both satisfy "default-on-null, length unchanged."
```

Confirm during implementation: `Json.Items` already iterates every array slot including
`null` ones (verified — `JsonDataHelper.GetArrayItems` yields a `JsonData` wrapper for every
element without skipping), so switching `.Get<T>()` to `.TryGet<T>()` in the `Select` is the
entire fix — enumerated count already always equals the source array length; only the
per-element failure behaviour needed to change from throw to default.

**Add the throwing form**, `GetRequired`, alongside the indexer:

```csharp
/// <summary>Gets the item at the given index. Throws if the index is out of range or the
/// element cannot be converted to <typeparamref name="T"/>. Use the indexer for
/// default-on-failure semantics.</summary>
public T GetRequired(int index) => Json.Get<T>(index);
```

`Json.Get<T>(int index)` (in `JsonDataTypedExtensions.cs`) already throws correctly — this is
a thin, correctly-named wrapper, not new logic.

---

## 6. Dead code (F8)

- Delete `JsonDataPrimativeExtensions.cs` from disk.
- Remove the now-unneeded exclusion from `DigitalBusiness.JsonDataWrappers.csproj`:
  ```xml
  <ItemGroup>
    <Compile Remove="JsonDataWrappers\JsonDataPrimativeExtensions.cs" />
  </ItemGroup>
  ```
  (Confirmed already compile-excluded, so this file is a genuine no-op today — deleting it
  and its csproj entry is risk-free with respect to build behaviour.)

---

## 7. New surface (§4.5)

### 7a. `Properties`

Add to `JsonDataJsonObjectExtensions.cs`, alongside the existing `PropertyNames`:

```csharp
/// <summary>Enumerates all (name, value) pairs in this JSON object. Child values inherit
/// this instance's readonly state for Node-backed sources, matching PropertyNames/Items.</summary>
public IEnumerable<(string Name, JsonData Value)> Properties
{
    get
    {
        if (jsonData.Element.HasValue && jsonData.Element.Value.ValueKind == JsonValueKind.Object)
        {
            foreach (var property in jsonData.Element.Value.EnumerateObject())
                yield return (property.Name, new JsonData(property.Value));
        }
        else if (jsonData.Node is JsonObject jsonObject)
        {
            foreach (var kvp in jsonObject)
                yield return (kvp.Key, new JsonData(kvp.Value, jsonData.ReadOnly));
        }
    }
}
```

Mirrors `JsonDataHelper.GetPropertyNames`/`GetArrayItems`'s existing dual-source pattern
exactly — consider adding a `JsonDataHelper.GetProperties(JsonData)` and having this delegate
to it, for consistency with how `PropertyNames`/`Items` are implemented.

### 7b. `EnumerateLeaves(JsonDiffOptions? options = null)`

This depends on §8 below for `JsonDiffOptions`'s shape (specifically the path-exclusion
list), so implement it alongside the diff subsystem, not before. Depth-first traversal:
recurse into objects (via the new `Properties` above) and arrays (via existing `Items`),
yielding a `(JsonPath Path, JsonData Value)` for every non-object, non-array node reached,
skipping any subtree whose path matches an exclusion in `options.ExcludedPaths` (the
`cmsSystem` rule). Shared by the diff engine, branch snapshot capture, and text search per
the design — implement it as a standalone public method on `JsonData` (in a new
`JsonDataDiffExtensions.cs` or similar, per your existing one-concern-per-file pattern) so
all three call the same code rather than each rolling their own traversal.

---

## 8. Diff subsystem — new (§5, F5/R5)

Entirely new code — no existing file to modify. Suggested new file:
`JsonDataWrappers/Diff/JsonDiff.cs` (+ `JsonDiffOptions.cs`, `JsonDiffResult.cs`,
`JsonDiffEntry.cs` alongside it), following the existing one-concern-per-file convention.

### Shape

```csharp
public static class JsonDiff
{
    public static JsonDiffResult Diff(in JsonData baseline, in JsonData target, JsonDiffOptions? options = null);
}

public sealed class JsonDiffOptions
{
    public NumberComparisonMode NumberComparison { get; init; } = NumberComparisonMode.Numeric;  // §8, Q3
    public IReadOnlyList<string> ExcludedPathPrefixes { get; init; } = [];                          // cmsSystem rule
}

public enum NumberComparisonMode { Numeric, Structural }

public sealed class JsonDiffResult
{
    public IReadOnlyList<JsonDiffEntry> Entries { get; }
    public IReadOnlyList<JsonPath> ChangedPaths { get; }
    public bool IsEmpty => Entries.Count == 0;
    public JsonPatch ToPatch(IJsonMergeSemantics semantics);   // see §9 — feeds JsonMerge
}

public enum JsonDiffKind { Added, Removed, Changed }

public readonly record struct JsonDiffEntry(JsonPath Path, JsonDiffKind Kind, JsonData? OldValue, JsonData? NewValue);
```

### Algorithm outline

1. Walk both `baseline` and `target` in lockstep using the same traversal shape as
   `EnumerateLeaves` (§7b), but structural (recursing into objects/arrays rather than
   flattening to leaves only) so kind-changes at an intermediate path (object → array, etc.)
   are caught as a single `Changed` entry at that path rather than a remove+add pair — this
   is the "kind-change handling as a single Changed entry" requirement.
2. **Objects:** union of both sides' property names; a name present only in `target` →
   `Added`; only in `baseline` → `Removed`; present in both → recurse, unless leaf-equal
   (via the number-comparison-aware equality below), in which case skip (no entry).
3. **Arrays:** compare index-wise for `Entries`/`ChangedPaths` (per-index `Added`/`Removed`/
   `Changed` as if each index were an object property keyed by its position) — but
   `ToPatch(semantics)` collapses any array with at least one changed/added/removed index
   into a single whole-array-replacement patch entry at the array's own path, per the merge
   model's "arrays replace entirely" semantics (§9) rather than emitting per-index patch
   operations that merge semantics doesn't support.
4. **Values:** compare via `DeepSemanticEquals` (numbers compared by parsed value) when
   `options.NumberComparison == Numeric` (the default), or `DeepEquals` (BCL-matching,
   exact-text) when `Structural` — see §8a below for both methods' shape. Don't reimplement
   number comparison locally inside the diff engine; call these shared methods so `JsonDiff`
   and any other caller agree on what "equal" means.
5. **Path exclusions:** before recursing into any subtree, check whether its path starts with
   any entry in `options.ExcludedPathPrefixes`; if so, skip it entirely (no entries emitted
   for anything under an excluded prefix) — this is the `cmsSystem` rule's mechanism.
6. Both inputs are read-only throughout — no `Set`/`Remove`/mutation calls anywhere in the
   diff engine, regardless of whether `baseline`/`target` happen to be writable Node-backed
   instances.

### Cross-source requirement

`baseline` and `target` may be any combination of Element-backed and Node-backed — the
existing `JsonDataEquality` internal class already has the cross-source comparison logic
(`JsonObjectEquals`, `JsonArrayEquals`, `JsonValueEquals` for Node-vs-Element) that the new
`SemanticEquals` (§8a below) extends with numeric-value comparison; `JsonDiff` should call
`DeepSemanticEquals`/`DeepEquals` (§8a) rather than duplicating any comparison logic itself.

### 8a. New method: `DeepSemanticEquals` — `DeepEquals` itself is unchanged (revised 2026-07-22)

This section originally instructed changing `DeepEquals`'s own default to `Numeric`. **That
instruction is withdrawn** — see `JsonDataWrappers_Decisions.md` D8 for the full reasoning.
In short: `JsonElement.DeepEquals`/`JsonNode.DeepEquals` are BCL methods with an established
meaning (`Structural`, exact-text number comparison); redefining that under the same name for
a library built directly on `System.Text.Json` risks real confusion, and doing so would have
required replacing the BCL's own trusted `DeepEquals` implementation with new hand-rolled
traversal for same-source pairs — meaningful implementation risk for a naming-consistency
benefit a new method name gets for free. **`JsonData.DeepEquals` needs no code change at
all** — leave `Internal/JsonDataEquality.cs`'s existing `Equals(...)` overloads exactly as
they are, BCL delegation and all.

Add a new, separate method instead: **`DeepSemanticEquals`** — same recursive/cross-source
contract as `DeepEquals`, differing only in comparing numbers by parsed value rather than raw
text. This is what `JsonDiff`'s leaf comparison (§8, item 4 above) should actually call —
replace every "route number comparison through `options.NumberComparison`" reference above
with a call to `DeepSemanticEquals` (for `Numeric`) or `DeepEquals` (for `Structural`,
opt-in) rather than a diff-engine-local number-comparison branch.

**Where to add it and how it's shaped:**

- New method on `JsonData`, alongside the existing `DeepEquals` in `JsonData.cs`:
  ```csharp
  /// <summary>Compares the JSON content of two instances for structural equality, treating
  /// numbers by parsed decimal value rather than exact text (e.g. 1 and 1.0 compare equal).
  /// Falls back to raw-text comparison for numbers outside decimal's range. For BCL-matching,
  /// exact-text comparison, use <see cref="DeepEquals"/> instead.</summary>
  public bool DeepSemanticEquals(in JsonData other) => JsonDataEquality.SemanticEquals(this, other);
  ```
- New method in `Internal/JsonDataEquality.cs`, `SemanticEquals(...)`, mirroring the existing
  `Equals(...)` overload family's shape (the same `JsonData`/`JsonData?` nullable-handling
  wrappers) but **not** delegating to the BCL for same-source pairs — same-source
  (`Element`/`Element`, `Node`/`Node`) numeric tolerance is only reachable via a manual
  structural walk, since the BCL's `DeepEquals` methods take no comparison-mode parameter.
  This manual walk can reuse the same shape as the existing cross-source
  `JsonObjectEquals`/`JsonArrayEquals`/`JsonValueEquals` methods (which already don't use the
  BCL, precisely because cross-source comparison never could) — extend those three (or add
  `Semantic`-suffixed siblings) to route `JsonValueEquals`'s number branch through a shared
  numeric-comparison helper (e.g. new `Internal/JsonNumberComparison.cs`,
  `AreEqual(string rawA, string rawB)`: try `decimal.TryParse` both sides, compare if both
  parse, else fall back to raw-text equality) instead of always doing raw-text comparison.
  `JsonValueEquals` already isolates the number case in one `switch` arm
  (`JsonValueKind.Number => valueNode.ToJsonString() == valueElement.GetRawText()`) — this is
  the one line that changes; call the new shared helper here instead of the raw string
  comparison, and have the equivalent same-source traversal (new code, since same-source
  currently bypasses this file's manual methods entirely via the BCL) call the same helper.
- This is genuinely new code (a full same-source structural walk didn't exist before, since
  same-source previously always deferred to the BCL) — size it accordingly; it's not a
  one-line change even though `DeepEquals` itself needs none.

**Do not** add a `NumberComparisonMode` parameter to `DeepEquals` itself, and don't rename or
touch its existing overloads — the whole point of this revision is that `DeepEquals`'s
contract stays exactly as documented and exactly as a `System.Text.Json` user would expect.

---

## 9. Merge subsystem — new (§6, F5/R6/R7)

Entirely new code. Suggested new file: `JsonDataWrappers/Diff/JsonMerge.cs` (+
`JsonMergeOptions.cs`, `IJsonMergeSemantics.cs`, `JsonMergeSemanticsV1.cs`,
`JsonMergeSemanticsV2.cs`).

### Shape

```csharp
public static class JsonMerge
{
    public static JsonData Apply(in JsonData baseline, JsonPatch patch, JsonMergeOptions? options = null);
    public static void ApplyInPlace(in JsonData baseline, JsonPatch patch, JsonMergeOptions? options = null);
}

public sealed class JsonMergeOptions
{
    public IJsonMergeSemantics Semantics { get; init; } = JsonMergeSemanticsV2.Instance;
    public JsonPath? Scope { get; init; }
}

public interface IJsonMergeSemantics
{
    string Version { get; }                           // "1" or "2"
    bool IsDelete(in JsonData patchValue);             // v1/v2: "$$delete"; v1 only: JSON null also deletes
    bool IsSetNull(in JsonData patchValue);            // v2 only: "$$null" sentinel; v1: always false
    MergeBehaviour ForKind(JsonValueKind patchValueKind); // Object=Merge, Array=Replace, Value=Replace (both versions)
}

public enum MergeBehaviour { Merge, Replace, Delete, SetNull }
```

### v1 semantics (`JsonMergeSemanticsV1`)

- Object patch values: merge into the base object (recurse per-property).
- Array patch values: replace the base array wholesale (no index-wise merging — matches the
  diff engine's `ToPatch` collapsing arrays to whole-array replacement).
- `"$$delete"` string sentinel, **or JSON `null`**: delete the property at that path.
- Absent (property not present in the patch): base value preserved untouched.

### v2 semantics (`JsonMergeSemanticsV2`) — confirmed this session, resolves Q4

- Same object-merge/array-replace behaviour as v1.
- `"$$delete"`: deletes (unchanged from v1).
- JSON `null` is **no longer** overloaded as delete — a literal `null` in a v2 patch means...
  actually, per the design: v2 exists specifically so a v2 patch can express "set to explicit
  null" via the new sentinel below; a literal JSON `null` appearing in a v2 patch should
  itself just mean "set this property to null" directly (since `null` isn't hijacked for
  delete anymore) — confirm this reading against the design text before implementing;
  functionally `null` and `"$$null"` may end up doing the same thing under v2, with
  `"$$null"` existing mainly so `ToPatch(V2)` has an unambiguous, greppable sentinel to emit
  rather than relying on JSON's own `null` (which some patch-generation or serialization
  paths might otherwise still special-case). Worth a clarifying pass with David before
  finalizing this one specific point — it's the one part of the merge spec that's slightly
  underspecified relative to the rest.
- `"$$null"` sentinel: sets the property to explicit JSON null (new in v2).

### `Apply` algorithm

1. If `options.Scope` is set, silently ignore any patch path that doesn't lie on-or-under
   the scope path — no error, no entry in any result, per the design's explicit "silently"
   requirement.
2. Recurse the patch object against the base: for each property/path in the patch,
   determine `semantics.ForKind(...)`/`IsDelete`/`IsSetNull` and apply Merge (recurse),
   Replace (whole-subtree swap — used for arrays), Delete (`Remove`), or SetNull (`Set` with
   an explicit null `JsonData`, using the now-fixed §1 `Set` behaviour) accordingly.
3. `Apply` returns a new `JsonData` (clone base first, mutate the clone); `ApplyInPlace`
   mutates `baseline` directly — both should share one internal recursive implementation
   differing only in whether the base is cloned first.

### Round-trip law (§6.4, R7) — the shared acceptance test for §8+§9 together

`JsonMerge.Apply(baseline, JsonDiff.Diff(baseline, target).ToPatch(semantics))` must
`DeepEquals` `target`:
- **Unconditionally under v2** (the whole reason v2 exists).
- **Under v1**, with one accepted, permanent exception: a `target` property holding explicit
  JSON null round-trips to *absent*, because a v1 patch has no way to express "set to null"
  separately from "delete." This exception is a documented, accepted v1 limitation, not a bug
  to chase — the test suite should assert the *exception itself* (a specific test case with
  an explicit-null property, asserting the v1 round-trip intentionally does *not* match, and
  the v2 round-trip does) rather than only testing the general-case round-trip.

---

## 10. Testing obligations (§8 of the design doc)

Suggested test project structure (mirrors the source layout): one test class per source
file/concern, cross-source cases parameterized rather than duplicated by hand where xUnit
`[Theory]`/`[InlineData]` (or your existing test framework's equivalent) can cover the
Element×Element / Node×Node / Element×Node / Node×Element matrix from one test body.

Priority order (build these alongside the corresponding code, not after):

1. **Null model** (§1 above): `Set` writes null (assert the underlying `JsonNode`/`JsonElement`
   directly, not just `IsNull`, to catch a regression back to removal); `Remove`/`RemoveAt`
   remove; a byte-faithful round-trip of a `publishFrom: null` shaped document through
   serialize → `Set` → re-serialize, confirming the key is present with a null value, not
   absent.
2. **Converter registration** (§2 above): duplicate `Register<T>` throws; `Register` after
   `Freeze()` throws; a converter type lacking a parameterless constructor fails
   `RegisterFromAssembly` with the type named in the exception, not inside an unrelated
   static constructor; a `CRef` round-trip test mirroring the project's existing
   `CRefJsonConverterTests` (search the test project for this to match its existing
   conventions rather than inventing new ones).
3. **Renames** (§4): compile-level only — once the IDE rename-refactor lands, existing tests
   referencing the old names simply won't compile until updated; no new test logic needed
   here, just confirm nothing was missed (a full-solution build is the check).
4. **Array enumeration** (§5): `foreach` over a `JsonDataArray<T>` containing a null element
   and a wrong-kind element both yield `default(T)` at that position with the enumerated
   count unchanged from the source array length; `GetRequired` throws for the same two cases.
5. **Diff + merge + round-trip law together** (§8+§9): build these tests as one connected
   suite, not two separate ones — the round-trip law is the shared acceptance criterion the
   Work Package calls out, and testing diff/merge in isolation from each other risks each
   looking correct individually while disagreeing on patch shape.
6. **`DeepSemanticEquals` numeric comparison, and `DeepEquals` non-regression** (§8a): a
   same-source (`Element`/`Element` and `Node`/`Node`) test asserting `1` and `1.0` compare
   equal via `DeepSemanticEquals`, and unequal via `DeepEquals` — the second half matters as
   much as the first: it's the regression test confirming `DeepEquals` was genuinely left
   untouched (still BCL-delegating, still exact-text) and didn't quietly pick up the new
   behaviour by accident.
7. **Serialization round-trips**: `JsonData`/`JsonData<T>` inside a containing DTO through a
   full `JsonSerializer.Serialize`/`Deserialize` cycle; `TypedJsonDataJsonConverter<>`
   directly; the serialized-value extensions in `JsonDataSerializedExtensions.cs` (flagged as
   F10 — not previously covered by any existing test).

---

## 11. Sequencing reminder (matches Work Package §4)

Null model and converter registration (§1, §2 above) have no dependency on anything else —
start there. Renames (§4) and array enumeration (§5) are independent and mechanical — do
these anytime, ideally via IDE refactor in one focused commit each so they're trivial to
review. Diff and merge (§8, §9) depend on each other and should be developed together against
the shared round-trip-law test suite (§10.5) as the acceptance gate — don't consider either
"done" until that suite passes. `DeepSemanticEquals` (§8a) is new code needed by the diff
work and should be built alongside it; `DeepEquals` itself needs no change at all — worth a
one-line note in the PR description confirming it was left untouched, precisely because
that's the kind of thing a reviewer might otherwise assume got bundled in.
