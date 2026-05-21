using DigitalBusiness.JsonDataWrappers.Internal;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace DigitalBusiness.JsonDataWrappers
{
    public static class TypedJsonDataArrayExtensions
    {
        extension(IJsonDataWrapper wrapper)
        {
            public JsonDataArray<T> AsJsonDataArray<T>() => new JsonDataArray<T> { Json = wrapper.Json };

            public JsonData AsJsonData() => wrapper.Json;

        }

        extension(JsonData jsonData)
        {
            public JsonDataArray<T> AsJsonDataArray<T>() => new JsonDataArray<T> { Json = jsonData };


            public JsonDataArray<T> GetArray<T>() => jsonData.TryGetArray<T>() ?? throw JsonDataExceptionHelper.GetTypedValueException<JsonDataArray<T>>(jsonData);

            public JsonDataArray<T>? TryGetArray<T>() => jsonData.TryGetArray<T>(out var array) ? array : null;

            public bool TryGetArray<T>([MaybeNullWhen(false)] out JsonDataArray<T> array)
            {
                if (jsonData.IsArray)
                {
                    array = new JsonDataArray<T> { Json = jsonData };
                    return true;
                }
                else
                {
                    array = default;
                    return false;
                }
            }




            public JsonDataArray<T> GetArray<T>(string name) => jsonData.TryGetArray<T>(name) ?? throw JsonDataExceptionHelper.GetTypedPropertyException<JsonDataArray<T>>(name, jsonData);
            public JsonDataArray<T>? TryGetArray<T>(string name) => jsonData.TryGetArray<T>(name, out var array) ? array : null;

            public bool TryGetArray<T>(string name, [MaybeNullWhen(false)] out JsonDataArray<T> array)
            {
                if (jsonData.TryGet(name, out var result) && result.IsArray)
                {
                    array = new JsonDataArray<T> { Json = result };
                    return true;
                }
                else
                {
                    array = default;
                    return false;
                }
            }

            public void Set<T>(string name, JsonDataArray<T>? array)
            {
                if (array == null) { jsonData.Remove(name); return; }
                var newNode = array.Value.Json;
                jsonData.Set(name, newNode);
            }

            public JsonDataArray<T> GetOrCreateArray<T>(string name)
            {
                if (jsonData.TryGetArray<T>(name, out var array))
                {
                    return array;
                }
                else
                {
                    if (jsonData.ReadOnly) throw JsonDataExceptionHelper.ReadOnlyException();
                    if (jsonData.PropertyHasValue(name)) throw JsonDataExceptionHelper.PropertyExistsAndNotArrayException(name);

                    var newJsonData = JsonData.CreateArray();
                    jsonData.Set(name, newJsonData);

                    return newJsonData.AsJsonDataArray<T>();
                }
            }


            public JsonDataArray<T> GetArray<T>(int index) => jsonData.TryGetArray<T>(index) ?? throw JsonDataExceptionHelper.GetTypedIndexException<JsonDataArray<T>>(index, jsonData);
            public JsonDataArray<T>? TryGetArray<T>(int index) => jsonData.TryGetArray<T>(index, out var array) ? array : null;

            public bool TryGetArray<T>(int index, [MaybeNullWhen(false)] out JsonDataArray<T> array)
            {
                if (jsonData.TryGet(index, out var result) && result.IsArray)
                {
                    array = new JsonDataArray<T> { Json = result };
                    return true;
                }
                else
                {
                    array = default;
                    return false;
                }
            }

            public void Set<T>(int index, JsonDataArray<T>? array)
            {
                if (array == null) { jsonData.RemoveAt(index); return; }
                var newNode = array.Value.Json;
                jsonData.Set(index, newNode);
            }

            public JsonDataArray<T> EnsureArray<T>(int index)
            {
                if (jsonData.TryGetArray<T>(index, out var array))
                {
                    return array;
                }
                else
                {
                    if (jsonData.ReadOnly) throw JsonDataExceptionHelper.ReadOnlyException();
                    if (jsonData.IndexHasValue(index)) throw JsonDataExceptionHelper.IndexExistsAndNotArrayException(index);

                    var newJsonData = JsonData.CreateArray();
                    jsonData.Set(index, newJsonData);

                    return newJsonData.AsJsonDataArray<T>();
                }
            }

        }
    }
}
