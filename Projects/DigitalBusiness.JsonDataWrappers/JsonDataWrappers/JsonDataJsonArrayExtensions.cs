
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Linq;
using System.Diagnostics.CodeAnalysis;
using System.Reflection.Metadata;
using DigitalBusiness.JsonDataWrappers;


namespace DigitalBusiness.JsonDataWrappers
{
    public delegate bool JsonDataPredicate(in JsonData jsonData);

    public static class JsonDataJsonArrayExtensions
    {
        extension(in JsonData jsonData)
        {
            public bool IsArray => (jsonData.Element.HasValue && jsonData.Element.Value.ValueKind == JsonValueKind.Array) || (jsonData.Node != null && jsonData.Node is JsonArray);

            public bool ThrowIfNotArray() { if (!jsonData.IsArray) throw new InvalidOperationException("Node is not an array."); else return true; }
            public JsonData EnsureArray() => jsonData.IsArray ? jsonData : throw new InvalidOperationException("Node is not an array.");

            public static JsonData CreateArray() => new JsonData(new JsonArray());

            public JsonData AsArray() => jsonData.IsArray ? jsonData : throw new InvalidOperationException("Node is not an array.");
            public JsonData? TryAsArray() => jsonData.IsArray ? jsonData : (JsonData?)null;


            //public int Count => jsonData.IsArray ?
            //    (jsonData.Element.HasValue ? jsonData.Element.Value.GetArrayLength() :
            //    (jsonData.Node is JsonArray jsonArray ? jsonArray.Count : 0)) : throw new InvalidOperationException("Node is not an array.");


            public JsonData Get(int index)
            {
                if (jsonData.TryGet(index, out var result))
                {
                    return result;
                }
                jsonData.ThrowIfNotArray();
                throw new IndexOutOfRangeException("Index was out of range.");
            }

            public JsonData? TryGet(int index) => jsonData.TryGet(index, out JsonData? result) ? result : default;

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
                        result = new JsonData(jsonArray[index]!);
                        return true;
                    }
                }

                result = default;
                return false;
            }


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



            public bool Contains(JsonDataPredicate predicate) => jsonData.IndexOf(predicate) > -1;

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
                        var jsonDataItem = new JsonData(item!);
                        if (predicate(in jsonDataItem)) return index;
                        index++;
                    }
                }
                return -1;
            }

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



            public IEnumerable<JsonData> Items => JsonDataHelper.GetArrayItems(jsonData);

                   
         

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

        //extension(in JsonData? node)
        //{
        //    public bool IsArray => !node.HasValue || node.Value.IsArray;

        //    public JsonData RequireArray() => node.HasValue ? node.Value.EnsureArray() : throw JNodeExceptionHelper.NullException();

        //    public JsonData? Get(int index) => node.HasValue ? node.Value.TryGet(index) : default;

            
        //}
    }
}