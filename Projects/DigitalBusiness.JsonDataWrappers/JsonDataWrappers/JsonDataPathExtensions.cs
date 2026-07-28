using DigitalBusiness.Json.JsonPaths;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace DigitalBusiness.JsonDataWrappers
{
    /// <summary>
    /// Path-based navigation and mutation extensions for <see cref="JsonData"/>.
    /// Overloads <c>Get</c>, <c>TryGet</c>, <c>Set</c>, <c>Remove</c> with <see cref="JsonPath"/> so
    /// call sites can use the same method names regardless of whether they navigate by a single
    /// key or a full path.
    /// <para>
    /// Two mutation families exist:
    /// <list type="bullet">
    ///   <item><b>Set / Remove</b> — parent must already exist; throws if any intermediate segment is missing.</item>
    ///   <item><b>SetDeep / GetOrCreateObjectDeep / GetOrCreateArrayDeep</b> — creates missing intermediate
    ///     nodes, inferring type (array vs object) from the following segment.</item>
    /// </list>
    /// </para>
    /// </summary>
    public static class JsonDataPathExtensions
    {
        extension(in JsonData jsonData)
        {
            // ----------------------------------------------------------------
            // Existence checks
            // ----------------------------------------------------------------

            /// <summary>
            /// Returns true if <paramref name="path"/> resolves to any node, including null/undefined values.
            /// </summary>
            public bool Contains(JsonPath path) => jsonData.TryGet(path, out _);

            /// <summary>
            /// Returns true if <paramref name="path"/> resolves to a node whose value is not null or undefined.
            /// </summary>
            public bool HasValue(JsonPath path) =>
                jsonData.TryGet(path, out var node) && !node.IsNull && !node.IsUndefined;

            // ----------------------------------------------------------------
            // Navigation
            // ----------------------------------------------------------------

            /// <summary>
            /// Navigates to the value at <paramref name="path"/>. Throws if any segment along
            /// the path is missing or an intermediate node is the wrong kind.
            /// </summary>
            public JsonData Get(JsonPath path)
            {
                if (jsonData.TryGet(path, out var result))
                    return result;
                throw new KeyNotFoundException($"Path '{path}' not found in JsonData.");
            }

            /// <summary>
            /// Navigates to the value at <paramref name="path"/>.
            /// Returns null if any segment along the path is missing.
            /// </summary>
            public JsonData? TryGet(JsonPath path) =>
                jsonData.TryGet(path, out var result) ? result : (JsonData?)null;

            /// <summary>
            /// Attempts to navigate to the value at <paramref name="path"/>.
            /// Returns false if any segment is missing or the wrong kind.
            /// Child instances inherit the parent's readonly state for Node-backed sources.
            /// </summary>
            public bool TryGet(JsonPath path, [MaybeNullWhen(false)] out JsonData value)
            {
                JsonData current = jsonData;

                foreach (var segment in path)
                {
                    bool found = segment.IsIndex
                        ? current.TryGet(segment.Index, out current)
                        : current.TryGet(segment.Property, out current);

                    if (!found)
                    {
                        value = default;
                        return false;
                    }
                }

                value = current;
                return true;
            }

            // ----------------------------------------------------------------
            // Mutation — parent must exist
            // ----------------------------------------------------------------

            /// <summary>
            /// Navigates to the parent of the final segment in <paramref name="path"/>, then sets
            /// the final property or index to <paramref name="value"/>.
            /// Throws if any parent segment is missing. Use <see cref="SetDeep"/> to create missing intermediates.
            /// </summary>
            public void Set(JsonPath path, JsonData value)
            {
                (JsonData parent, JsonPathSegment last) = jsonData.ResolveParent(path);
                if (last.IsIndex)
                    parent.Set(last.Index, value);
                else
                    parent.Set(last.Property, value);
            }

            /// <summary>
            /// Navigates to the parent of the final segment in <paramref name="path"/>, then removes
            /// the final property or index. Requires a writable Node-backed instance.
            /// <list type="bullet">
            ///   <item>Property segment — delegates to <c>Remove(string)</c>; returns false if the property did not exist.</item>
            ///   <item>Index segment — delegates to <c>RemoveAt(int)</c>; always returns true if the index is in range.</item>
            /// </list>
            /// </summary>
            public bool Remove(JsonPath path)
            {
                (JsonData parent, JsonPathSegment last) = jsonData.ResolveParent(path);
                if (last.IsIndex)
                {
                    parent.RemoveAt(last.Index);
                    return true;
                }
                return parent.Remove(last.Property);
            }

            // ----------------------------------------------------------------
            // Deep mutation — creates missing intermediates
            // ----------------------------------------------------------------

            /// <summary>
            /// Sets the value at <paramref name="path"/>, creating any missing intermediate nodes.
            /// The type of each missing intermediate is inferred from the following segment:
            /// an index segment implies a <see cref="System.Text.Json.Nodes.JsonArray"/>,
            /// a property segment implies a <see cref="System.Text.Json.Nodes.JsonObject"/>.
            /// <para>
            /// Throws if an intermediate already exists but is the wrong kind, or if the root is
            /// readonly or Element-backed (deep mutation requires a writable Node-backed root).
            /// </para>
            /// </summary>
            public void SetDeep(JsonPath path, JsonData? value) => SetDeepCore(jsonData, path, value);

            /// <summary>
            /// Navigates to <paramref name="path"/>, creating any missing intermediate nodes,
            /// and returns the node at the final segment as a JSON object — creating it if absent.
            /// Throws if any node along the path already exists but is the wrong kind.
            /// </summary>
            public JsonData GetOrCreateObjectDeep(JsonPath path)
            {
                JsonData parent = ResolveOrCreateParent(jsonData, path);
                JsonPathSegment last = path[path.Length - 1];
                return last.IsIndex
                    ? parent.GetOrCreateObject(last.Index)
                    : parent.GetOrCreateObject(last.Property);
            }

            /// <summary>
            /// Navigates to <paramref name="path"/>, creating any missing intermediate nodes,
            /// and returns the node at the final segment as a JSON array — creating it if absent.
            /// Throws if any node along the path already exists but is the wrong kind.
            /// </summary>
            public JsonData GetOrCreateArrayDeep(JsonPath path)
            {
                JsonData parent = ResolveOrCreateParent(jsonData, path);
                JsonPathSegment last = path[path.Length - 1];
                return last.IsIndex
                    ? parent.GetOrCreateArray(last.Index)
                    : parent.GetOrCreateArray(last.Property);
            }

            // ----------------------------------------------------------------
            // Internal helpers — shared by typed extensions
            // ----------------------------------------------------------------

            internal (JsonData Parent, JsonPathSegment LastSegment) ResolveParent(JsonPath path)
            {
                if (path.Length == 1)
                    return (jsonData, path[0]);

                JsonData parent = jsonData.Get(path.Slice(0, path.Length - 1));
                return (parent, path[path.Length - 1]);
            }
        }

        // ----------------------------------------------------------------
        // Internal helper — plain static method (not inside the extension
        // block) so JsonDataTypedPathExtensions.SetDeep<T> can invoke it by
        // fully-qualified name instead of the ambiguous unqualified
        // `jsonData.SetDeep(...)`, which would otherwise bind back to that
        // class's own generic overload (T inferred as JsonData, since
        // JsonData is a readonly struct) and recurse infinitely.
        // ----------------------------------------------------------------

        internal static void SetDeepCore(JsonData jsonData, JsonPath path, JsonData? value)
        {
            JsonData parent = ResolveOrCreateParent(jsonData, path);
            JsonPathSegment last = path[path.Length - 1];
            if (last.IsIndex)
            {
                parent.Set(last.Index, value);
            }
            else if (value.HasValue)
            {
                parent.Set(last.Property, value.Value);
            }
            else
            {
                parent.Remove(last.Property);
            }
        }

        // ----------------------------------------------------------------
        // Private walker — core of all deep operations
        // Used as a static method (not inside extension block) so that
        // `current` can be reassigned in the loop without `in` restrictions.
        // ----------------------------------------------------------------

        private static JsonData ResolveOrCreateParent(JsonData root, JsonPath path)
        {
            if (path.Length == 1) return root;

            root.ThrowIfReadOnly();

            JsonData current = root;

            for (int i = 0; i < path.Length - 1; i++)
            {
                JsonPathSegment segment     = path[i];
                JsonPathSegment nextSegment = path[i + 1];

                if (segment.IsIndex)
                {
                    if (!current.IsArray)
                        throw new InvalidOperationException(
                            $"Path segment [{segment.Index}] at position {i} expects an array but found {current.ValueKind}.");

                    if (current.TryGet(segment.Index, out JsonData existing))
                    {
                        ThrowIfWrongKind(existing, nextSegment, i);
                        current = existing;
                    }
                    else
                    {
                        JsonData newNode = nextSegment.IsIndex
                            ? JsonData.CreateArray()
                            : JsonData.CreateObject();

                        current.Set(segment.Index, newNode);
                        current = newNode;
                    }
                }
                else
                {
                    if (!current.IsObject)
                        throw new InvalidOperationException(
                            $"Path segment '{segment.Property}' at position {i} expects an object but found {current.ValueKind}.");

                    if (current.TryGet(segment.Property, out JsonData existing))
                    {
                        ThrowIfWrongKind(existing, nextSegment, i);
                        current = existing;
                    }
                    else
                    {
                        JsonData newNode = nextSegment.IsIndex
                            ? JsonData.CreateArray()
                            : JsonData.CreateObject();

                        current.Set(segment.Property, newNode);
                        current = newNode;
                    }
                }
            }

            return current;
        }

        private static void ThrowIfWrongKind(JsonData existing, JsonPathSegment nextSegment, int segmentIndex)
        {
            if (nextSegment.IsIndex && !existing.IsArray)
                throw new InvalidOperationException(
                    $"Intermediate node at segment position {segmentIndex} must be an array " +
                    $"(next segment is [{nextSegment.Index}]) but found {existing.ValueKind}.");

            if (nextSegment.IsProperty && !existing.IsObject)
                throw new InvalidOperationException(
                    $"Intermediate node at segment position {segmentIndex} must be an object " +
                    $"(next segment is '{nextSegment.Property}') but found {existing.ValueKind}.");
        }
    }
}
