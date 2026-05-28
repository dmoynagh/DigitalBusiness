
using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Linq;
using System.Diagnostics.CodeAnalysis;
using DigitalBusiness.JsonDataWrappers;


namespace DigitalBusiness.JsonDataWrappers
{
    public delegate bool JsonDataPredicate(in JsonData jsonData);

    public static class JsonDataJsonArrayExtensions
    {
        extension(in JsonData jsonData)
        {
            /// <summary>True if this instance represents a JSON array, regardless of source type.</summary>
            public bool IsArray => (jsonData.Element.HasValue && jsonData.Element.Value.ValueKind == JsonValueKind.Array) || (jsonData.Node != null && jsonData.Node is JsonArray);

            /// <summary>Throws if this instance is not a JSON array. Returns true if it is (allows use in guard expressions).</summary>
            public bool ThrowIfNotArray() { if (!jsonData.IsArray) throw new InvalidOperationException("Node is not an array."); else return true; }

            /// <summary>Returns this instance asserted as an array, or throws if it is not.</summary>
            public JsonData EnsureArray() => jsonData.IsArray ? jsonData : throw new InvalidOperationException("Node is not an array.");

            /// <summary>Creates a new writable Node-backed JSON array instance.</summary>
            public static JsonData CreateArray() => new JsonData(new JsonArray());

            /// <summary>Returns this instance asserted as an array, or throws if it is not.</summary>
            public JsonData AsArray() => jsonData.IsArray ? jsonData : throw new InvalidOperationException("Node is not an array.");

            /// <summary>Returns this instance as an array, or null if it is not an array.</summary>
            public JsonData? TryAsArray() => jsonData.IsArray ? jsonData : (JsonData?)null;


            //public int Count => jsonData.IsArray ?
            //    (jsonData.Element.HasValue ? jsonData.Element.Value.GetArrayLength() :
            //    (jsonData.Node is JsonArray jsonArray ? jsonArray.Count : 0)) : throw new InvalidOperationException("Node is not an array.");


            /// <summary>Gets the item at the given index. Throws if out of range or not an array.</summary>
            public JsonData Get(int index)
            {
                if (jsonData.TryGet(index, out var result))
                {
                    return result;
                }
                jsonData.ThrowIfNotArray();
                throw new IndexOutOfRangeException("Index was out of range.");
            }

            /// <summary>Gets the item at the given index, or null if out of range.</summary>
            public JsonData? TryGet(int index) => jsonData.TryGet(index, out JsonData? result) ? result : default;

            /// <summary>Gets the item at the given index. Returns false if out of range; child inherits parent readonly state for Node-backed sources.</summary>
            public bool TryGet(int index, [MaybeNullWhen(false)] out JsonData result)
            {

                if (jsonData.Element.HasValue && jsonData.Element.Value.ValueKind == JsonValueKind.Array)
                {
                    if (index > -1 && index < jsonData.Element.Value.GetArrayLength())
                    {
                        result = new JsonData(jsonData.Element.Value[index]);
                        return true;
                    }
                }
                else if (jsonData.Node != null && jsonData.Node is JsonArray jsonArray)
                {
                    if (index > -1 && index < jsonArray.Count)
                    {
                        result = new JsonData(jsonArray[index]!, jsonData.ReadOnly);
                        return true;
                    }
                }

                result = default;
                return false;
            }


            /// <summary>Gets the item at the given index as an object, creating it if absent. Requires a writable instance.</summary>
            public JsonData GetOrCreateObject(int index)
            {
                if (jsonData.TryGet(index, out var result))
                {
                    result.ThrowIfNotObject();
                    return result;
                }
                else
                {
                    var newNode = JsonData.CreateObject();
                    jsonData.Add(newNode);
                    return newNode;
                }
            }

            /// <summary>Gets the item at the given index as an array, creating it if absent. Requires a writable instance.</summary>
            public JsonData GetOrCreateArray(int index)
            {
                if (jsonData.TryGet(index, out var result))
                {
                    result.ThrowIfNotArray();
                    return result;
                }
                else
                {
                    var newNode = JsonData.CreateArray();
                    jsonData.Add(newNode);
                    return newNode;
                }
            }



            /// <summary>Returns true if any item in the array satisfies the predicate.</summary>
            public bool Contains(JsonDataPredicate predicate) => jsonData.IndexOf(predicate) > -1;

            /// <summary>Returns the index of the first item satisfying the predicate, or -1 if not found.
            /// Child items inherit parent readonly state for Node-backed sources.</summary>
            public int IndexOf(JsonDataPredicate predicate)
            {
                if (jsonData.Element.HasValue && jsonData.Element!.Value.ValueKind == JsonValueKind.Array)
                {
                    int index = 0;
                    foreach (var item in jsonData.Element.Value.EnumerateArray())
                    {                        
                        var jsonDataItem = new JsonData(item);
                        if (predicate(in jsonDataItem)) return index;
                        index++;
                    }
                }
                else if(jsonData.Node is not null && jsonData.Node is JsonArray jsonArray)
                {
                    int index = 0;
                    foreach (var item in jsonArray)
                    {
                        var jsonDataItem = new JsonData(item!, jsonData.ReadOnly);
                        if (predicate(in jsonDataItem)) return index;
                        index++;
                    }
                }
                return -1;
            }

            /// <summary>Returns true if the index is within range and the value at that index is not null or undefined.</summary>
            public bool IndexHasValue(int index)
            {
                if(index < 0) return false;
                if (jsonData.Element.HasValue)
                {
                    if(jsonData.Element.Value.ValueKind != JsonValueKind.Array) return false;
                    if(index>= jsonData.Element.Value.GetArrayLength()) return false;

                    var indexValue = jsonData.Element.Value[index];
                    return indexValue.ValueKind != JsonValueKind.Null && indexValue.ValueKind != JsonValueKind.Undefined;
                }
                else if (jsonData.Node != null && jsonData.Node is JsonArray jsonArray)
                {
                    if(index >= jsonArray.Count) return false;
                    var indexValue = jsonArray[index];
                    return indexValue is not null && indexValue.GetValueKind() != JsonValueKind.Null && indexValue.GetValueKind() != JsonValueKind.Undefined;
                }
                return false;
            }



            /// <summary>Enumerates all items in the array. Child items inherit parent readonly state for Node-backed sources.</summary>
            public IEnumerable<JsonData> Items => JsonDataHelper.GetArrayItems(jsonData);




            /// <summary>Sets the item at the given index. A null value sets a JSON null at that position.
            /// Extends the array with nulls if the index is beyond the current length. Requires a writable instance.</summary>
            public void Set(int index, JsonData? value)
            {
                jsonData.ThrowIfNotArray();
                jsonData.ThrowIfReadOnly();

                var addNode = value.HasValue ? JsonDataHelper.GetNodeToAdd(value.Value, jsonData.Node!) : null;
                if (jsonData.Node is JsonArray jsonArray)
                {
                    // Fill with nulls if index is beyond current length
                    while (jsonArray.Count <= index)
                        jsonArray.Add((JsonNode?)null);
                    jsonArray[index] = addNode;
                }
                else
                {
                    jsonData.Node![index] = addNode;
                }
            }

      
            /// <summary>Appends an item to the end of the array. Requires a writable instance.</summary>
            public void Add(in JsonData? value)
            {
                jsonData.ThrowIfNotArray();
                jsonData.ThrowIfReadOnly();

                var addNode = value.HasValue ? JsonDataHelper.GetNodeToAdd(value.Value, jsonData.Node!) : null;
                if(jsonData.Node is JsonArray jsonArray)
                {
                    jsonArray.Add(addNode);

                }
                else throw new InvalidOperationException("Node is not an array.");
            }

            /// <summary>Inserts an item at the given index, shifting subsequent items. Requires a writable instance.</summary>
            public void Insert(int index, JsonData? value)
            {
                jsonData.ThrowIfNotArray();
                jsonData.ThrowIfReadOnly();

                var addNode = value.HasValue ? JsonDataHelper.GetNodeToAdd(value.Value, jsonData.Node!) : null;
                if (jsonData.Node is JsonArray jsonArray)
                {
                    jsonArray.Insert(index, addNode);

                }
                else throw new InvalidOperationException("Node is not an array.");              
            }           

            /// <summary>Removes the item at the given index. Requires a writable instance.</summary>
            public void RemoveAt(int index)
            {
                jsonData.ThrowIfReadOnly();
                jsonData.ThrowIfNotArray();

                if (jsonData.Node is JsonArray jsonArray)
                {
                    jsonArray.RemoveAt(index);

                }
                else throw new InvalidOperationException("Node is not an array.");
            }

        }

    }
}