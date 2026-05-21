using DigitalBusiness.JsonDataWrappers;
using DigitalBusiness.JsonDataWrappers.Converters;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace DigitalBusiness.JsonDataWrappers.Converters
{
    public static partial class JsonDataConverterProvider
    {


        private static IJsonDataConverter<T>? GetJsonDataConverter<T>()
        {
            var type = typeof(T);

            //covers JsonData
            if(typeof(JsonData).IsAssignableFrom(type))
            {
                return new JsonDataConverterProvider.JsonDataConverter() as IJsonDataConverter<T> ?? throw new InvalidOperationException("Failed to create JsonDataConverter.");
            }
            //covers IJsonDataWrapper, JsonData<T>, IJsonDataObject
            else if (typeof(IJsonDataWrapper).IsAssignableFrom(type))
            {
                var converterType = typeof(JsonDataWrapperConverter<>).MakeGenericType(type);
                return (IJsonDataConverter<T>)Activator.CreateInstance(converterType)!;
            }
           
            return null;
        }

        private class JsonDataConverter : IJsonDataConverter<JsonData>
        {
            public bool TryGet(in JsonData jsonData, out JsonData value) { value = jsonData; return true; }
            public JsonData Create(JsonData value) => value;

        }

        private class JsonDataWrapperConverter<T> : IJsonDataConverter<T> where T : IJsonDataWrapper, new()
        {
            public JsonData Create(T value)=>value.Json;

            public bool TryGet(in JsonData jsonData, [MaybeNullWhen(false)] out T value){ value = new T() { Json = jsonData }; return true; }                      

        }



        //public class TypedJsonDataWrapperConverter<T> : IJsonDataConverter<JsonData<T>>
        //{

        //    public bool TryGet(in JsonData jsonData, [MaybeNullWhen(false)] out JsonData<T> value)
        //    {
        //        if (!jsonData.IsNull)
        //        {
        //            value = jsonData.AsJsonData<T>();
        //            return true;
        //        }
        //        value = default; return false;
        //    }

        //    public JsonData Create(JsonData<T> value) => value.Json;

        //}

        //private static IJsonDataConverter<T>? GetTypedJsonDataArrayWrapperConverter<T>()
        //{
        //    var type = typeof(T);

        //    if (!type.IsGenericType) return null;
        //    var genericArgs = type.GetGenericArguments();
        //    if (genericArgs.Length != 1) return null;

        //    var genericType = type.GetGenericTypeDefinition();
        //    if (genericType == typeof(JsonDataArray<>))
        //    {
        //        var elementType = genericArgs[0];
        //        var converterType = typeof(TypedJsonDataArrayWrsapperConverter<>).MakeGenericType(elementType);
        //        return (IJsonDataConverter<T>)Activator.CreateInstance(converterType)!;
        //    }
        //    return null;
        //}


        //public class TypedJsonDataArrayWrsapperConverter<T> : IJsonDataConverter<JsonDataArray<T>>
        //{
        //    public bool TryGet(in JsonData jsonData, [MaybeNullWhen(false)] out JsonDataArray<T> value)
        //    {
        //        if (!jsonData.IsNull && jsonData.IsArray)
        //        {
        //            value = jsonData.AsJsonDataArray<T>();
        //            return true;
        //        }
        //        value = default; return false;
        //    }

        //    public JsonData Create(JsonDataArray<T> value) => value.Json;
        //}
    }
}