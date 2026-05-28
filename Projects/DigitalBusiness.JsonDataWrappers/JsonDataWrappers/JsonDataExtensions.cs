using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Nodes;
using DigitalBusiness.Json.JsonElements;
using DigitalBusiness.Json.JsonNodes;
using DigitalBusiness.JsonDataWrappers;
using DigitalBusiness.JsonDataWrappers.Internal;

namespace DigitalBusiness.JsonDataWrappers
{
    /// <summary>
    /// Core extension methods for <see cref="JsonData"/>.
    /// Covers null/state checks, read/write guards, source conversion, factory helpers, and collection access.
    /// </summary>
    public static class JsonDataExtensions
    {
        extension(in JsonData jsonData)
        {
            /// <summary>
            /// True if this represents a JSON null value, or if the wrapper is uninitialised (no source set).
            /// Note: when the source is a JsonNode, these two states are indistinguishable — JsonNode uses null
            /// to represent JSON null. Only JsonElement backed instances can distinguish explicit null from uninitialised.
            /// </summary>
            public bool IsNull => (jsonData.Node is null && !jsonData.Element.HasValue) ||
                (jsonData.Element.HasValue && jsonData.Element.Value.ValueKind == JsonValueKind.Null) ||
                (jsonData.Node is not null && jsonData.Node.GetValueKind() == JsonValueKind.Null);

            /// <summary>True if the value kind is <see cref="JsonValueKind.Undefined"/>.</summary>
            public bool IsUndefined => jsonData.ValueKind == JsonValueKind.Undefined;


            /// <summary>Throws if this instance is readonly. Call before any mutating operation.</summary>
            public void ThrowIfReadOnly() { if (jsonData.ReadOnly) throw JsonDataExceptionHelper.ReadOnlyException(); }

            /// <summary>Throws if this instance is null or uninitialized.</summary>
            public void ThrowIfNull()
            {
                if (jsonData.IsNull) throw new InvalidOperationException("JNode value is null.");
            }


            /// <summary>Returns a readonly view. If already readonly, returns itself with no allocation.</summary>
            public JsonData AsReadOnly() => jsonData.ReadOnly ? jsonData : new JsonData(jsonData.Node, true);

            /// <summary>The raw underlying source: the boxed <see cref="JsonElement"/> or the <see cref="JsonNode"/>. Null for uninitialized instances.</summary>
            public object? Source => jsonData.Element.HasValue ? jsonData.Element.Value : jsonData.Node;



            /// <summary>Creates an uninitialized (null) readonly instance.</summary>
            public static JsonData Create() => new JsonData();
            /// <summary>Creates a Node-backed instance. Defaults to writable unless <paramref name="readOnly"/> is true.</summary>
            public static JsonData Create(JsonNode? source, bool readOnly=false) => new JsonData(source,readOnly);            /// <summary>Creates an Element-backed (always readonly) instance.</summary>
            public static JsonData Create(JsonElement source) => new JsonData(source);
            /// <summary>Creates an Element-backed (always readonly) instance from a nullable element.</summary>
            public static JsonData Create(JsonElement? source) => new JsonData(source);


            /// <summary>
            /// Returns an Element-backed copy of this instance.
            /// Converts Node-backed data by serializing to a JsonElement. Uninitialized returns a null element.
            /// </summary>
            public JsonData ToJsonElementJsonData()
            {
                if (jsonData.IsElement) 
                {
                    return JsonData.Create(jsonData.Element.Value.Clone()); 
                }
                else if (jsonData.IsNode)
                {
                    return JsonData.Create(jsonData.Node.ToJsonElement());
                }
                else
                {
                    return JsonData.Create(JsonElement.CreateNullElement());
                }
            }

            /// <summary>
            /// Returns a Node-backed copy of this instance.
            /// Converts Element-backed data by deserializing to a JsonNode.
            /// <paramref name="readOnly"/> overrides the readonly state; if null, preserves the original state.
            /// </summary>
            public JsonData ToJsonNodeJsonData(bool? readOnly = null)
            {
                if (jsonData.IsNode)
                {
                    return JsonData.Create(jsonData.Node.DeepClone(),readOnly.HasValue ? readOnly.Value : jsonData.ReadOnly);
                }
                else if (jsonData.IsElement)
                {
                    bool ro = readOnly.GetValueOrDefault(false);
                    return new JsonData(jsonData.Element.Value.ToJsonNode(), ro, false);
                }
                else
                {
                    bool ro = readOnly.GetValueOrDefault(false);
                    var nullNode = JsonNode.Parse("null") ?? (JsonNode)JsonValue.Create<int?>(null)!;
                    return new JsonData(nullNode, ro, false);
                }
            }

            /// <summary>Returns a writable Node-backed copy of this instance. Shorthand for <c>ToJsonNodeJsonData(readOnly: false)</c>.</summary>
            public JsonData ToEditableJsonData() => jsonData.ToJsonNodeJsonData(false);

        




            /// <summary>Number of items in an array, or properties in an object. Returns 0 for all other value kinds.</summary>
            public int Count
            {
                get
                {
                    if (jsonData.Element.HasValue)
                    {
                        return jsonData.Element.Value.ValueKind switch
                        {
                            JsonValueKind.Array => jsonData.Element.Value.GetArrayLength(),
                            JsonValueKind.Object => jsonData.Element.Value.EnumerateObject().Count(),
                            _ => 0
                        };
                    }
                    else if (jsonData.Node != null)
                    {
                        return jsonData.Node switch
                        {
                            JsonArray jsonArray => jsonArray.Count,
                            JsonObject jsonObject => jsonObject.Count,
                            _ => 0
                        };
                    }
                    return 0;
                }
            }

            /// <summary>Removes all items from an array or all properties from an object. Requires a writable Node-backed instance.</summary>
            public void Clear()
            {
                jsonData.ThrowIfReadOnly();
                
                if (jsonData.Node is JsonArray jsonArray) jsonArray.Clear();
                else if (jsonData.Node is JsonObject jsonObject) jsonObject.Clear();
                else throw new InvalidOperationException("JNode is not an array or object.");
            }


            /// <summary>True if this instance wraps a primitive JSON value (string, number, bool, null).</summary>
           public bool IsValue =>
           (jsonData.Node is not null && jsonData.Node is JsonValue) ||
           (jsonData.Element.HasValue && jsonData.Element.Value.ValueKind switch
           {
               JsonValueKind.String => true,
               JsonValueKind.Number => true,
               JsonValueKind.True => true,
               JsonValueKind.False => true,
               JsonValueKind.Null => true,
               _ => false
           });

            /// <summary>Throws if this instance is not a primitive value. Used as a guard before value extraction.</summary>
            public bool ThrowIfNotValue() { if (!jsonData.IsValue) throw new InvalidOperationException("Node is not a value."); else return true; }

        }

      
    }
}
