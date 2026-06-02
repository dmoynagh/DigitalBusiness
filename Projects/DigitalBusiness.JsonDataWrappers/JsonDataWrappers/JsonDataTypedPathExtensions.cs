using DigitalBusiness.Json.JsonPaths;
using DigitalBusiness.JsonDataWrappers.Internal;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace DigitalBusiness.JsonDataWrappers
{
    /// <summary>
    /// Typed path-based navigation and mutation extensions for <see cref="JsonData"/>.
    /// Mirrors the typed overloads in <see cref="JsonDataTypedExtensions"/> and
    /// <see cref="TypedJsonDataArrayExtensions"/>, replacing the <c>string</c>/<c>int</c>
    /// parameter with a <see cref="JsonPath"/> for multi-step navigation.
    /// <para>
    /// Store paths as <see langword="static readonly"/> fields to avoid repeated parsing:
    /// <code>static readonly JsonPath NamePath = JsonPath.Parse("user.profile[0].name");</code>
    /// </para>
    /// </summary>
    public static class JsonDataTypedPathExtensions
    {
        extension(in JsonData jsonData)
        {
            // ----------------------------------------------------------------
            // Typed get by path
            // ----------------------------------------------------------------

            /// <summary>
            /// Navigates to <paramref name="path"/> then deserializes the result as <typeparamref name="T"/>.
            /// Throws if the path is not found or the value cannot be converted.
            /// </summary>
            public T Get<T>(JsonPath path) => jsonData.Get(path).Get<T>();

            /// <summary>
            /// Navigates to <paramref name="path"/> then attempts to deserialize as <typeparamref name="T"/>.
            /// Returns <see langword="default"/> if the path is not found or conversion fails.
            /// </summary>
            public T? TryGet<T>(JsonPath path) =>
                jsonData.TryGet<T>(path, out var result) ? result : default;

            /// <summary>
            /// Navigates to <paramref name="path"/> then attempts to deserialize as <typeparamref name="T"/>.
            /// Returns false if the path is not found or the value cannot be converted.
            /// </summary>
            public bool TryGet<T>(JsonPath path, [MaybeNullWhen(false)] out T? value)
            {
                if (jsonData.TryGet(path, out var node))
                    return node.TryGet<T>(out value);

                value = default;
                return false;
            }

            // ----------------------------------------------------------------
            // Typed set by path — parent must exist
            // ----------------------------------------------------------------

            /// <summary>
            /// Navigates to the parent of the final segment in <paramref name="path"/>, then sets
            /// the final property or index to <paramref name="value"/>.
            /// Throws if any parent segment is missing. Use <see cref="SetDeep{T}"/> to create missing intermediates.
            /// A <see langword="null"/> value removes the target property or sets a JSON null at the index.
            /// </summary>
            public void Set<T>(JsonPath path, T? value)
            {
                (JsonData parent, JsonPathSegment last) = jsonData.ResolveParent(path);
                JsonData? newNode = value is null ? (JsonData?)null : JsonData.Create<T>(value);
                if (last.IsIndex)
                    parent.Set(last.Index, newNode);
                else
                    parent.Set(last.Property, newNode);
            }

            // ----------------------------------------------------------------
            // Typed set by path — creates missing intermediates
            // ----------------------------------------------------------------

            /// <summary>
            /// Sets the value at <paramref name="path"/>, creating any missing intermediate nodes.
            /// The type of each missing intermediate is inferred from the following segment.
            /// Throws if an intermediate exists but is the wrong kind, or if the root is readonly.
            /// A <see langword="null"/> value removes the target or sets JSON null at the position.
            /// </summary>
            public void SetDeep<T>(JsonPath path, T? value)
            {
                JsonData? newNode = value is null ? (JsonData?)null : JsonData.Create<T>(value);
                jsonData.SetDeep(path, newNode);
            }

            // ----------------------------------------------------------------
            // Typed ensure by path
            // ----------------------------------------------------------------

            /// <summary>
            /// Navigates to the parent of the final segment in <paramref name="path"/>, then
            /// returns the existing typed value if present, or sets and returns the result of
            /// <paramref name="defaultFactory"/> if absent. Parent segments must exist.
            /// </summary>
            public T Ensure<T>(JsonPath path, Func<T> defaultFactory)
            {
                (JsonData parent, JsonPathSegment last) = jsonData.ResolveParent(path);

                if (last.IsProperty)
                    return parent.Ensure<T>(last.Property, defaultFactory);

                // Index segment — no dedicated Ensure(int) overload, so implement inline
                if (parent.TryGet<T>(last.Index, out var existing) && existing is not null)
                    return existing;

                T created = defaultFactory();
                parent.Set<T>(last.Index, created);
                return created;
            }

            /// <summary>Overload accepting a value directly rather than a factory.</summary>
            public T Ensure<T>(JsonPath path, T defaultValue) =>
                jsonData.Ensure<T>(path, () => defaultValue);

            // ----------------------------------------------------------------
            // Typed array mutation by path
            // ----------------------------------------------------------------

            /// <summary>
            /// Navigates to the array at <paramref name="path"/> and appends <paramref name="value"/>.
            /// Throws if the path is not found or the target is not a writable array.
            /// </summary>
            public void Add<T>(JsonPath path, T value)
            {
                JsonData array = jsonData.Get(path);
                array.ThrowIfNotArray();
                array.Add<T>(value);
            }

            /// <summary>
            /// Navigates to the array at <paramref name="path"/> and inserts <paramref name="value"/>
            /// at <paramref name="index"/>, shifting subsequent items.
            /// Throws if the path is not found or the target is not a writable array.
            /// </summary>
            public void Insert<T>(JsonPath path, int index, T value)
            {
                JsonData array = jsonData.Get(path);
                array.ThrowIfNotArray();
                array.Insert<T>(index, value);
            }

            // ----------------------------------------------------------------
            // Typed JsonDataArray<T> by path
            // ----------------------------------------------------------------

            /// <summary>
            /// Navigates to <paramref name="path"/> and returns the result as a <see cref="JsonDataArray{T}"/>.
            /// Throws if the path is not found or the value is not an array.
            /// </summary>
            public JsonDataArray<T> GetArray<T>(JsonPath path) =>
                jsonData.TryGetArray<T>(path, out var array)
                    ? array
                    : throw JsonDataExceptionHelper.GetTypedValueException<JsonDataArray<T>>(jsonData.Get(path));

            /// <summary>
            /// Navigates to <paramref name="path"/> and returns the result as a <see cref="JsonDataArray{T}"/>,
            /// or <see langword="null"/> if the path is not found or the value is not an array.
            /// </summary>
            public JsonDataArray<T>? TryGetArray<T>(JsonPath path) =>
                jsonData.TryGetArray<T>(path, out var array) ? array : null;

            /// <summary>
            /// Attempts to navigate to <paramref name="path"/> and wrap the result as a <see cref="JsonDataArray{T}"/>.
            /// Returns false if the path is not found or the node is not an array.
            /// </summary>
            public bool TryGetArray<T>(JsonPath path, [MaybeNullWhen(false)] out JsonDataArray<T> array)
            {
                if (jsonData.TryGet(path, out var node) && node.IsArray)
                {
                    array = new JsonDataArray<T> { Json = node };
                    return true;
                }
                array = default;
                return false;
            }
        }
    }
}
