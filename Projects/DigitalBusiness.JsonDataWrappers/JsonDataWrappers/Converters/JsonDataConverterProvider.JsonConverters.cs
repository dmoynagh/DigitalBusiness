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
        private static IJsonDataConverter<T>? GetJsonConverter<T>()
        {
            if (typeof(T) == typeof(JsonElement))
                return new JsonElementConverter() as IJsonDataConverter<T>;
            if (typeof(T) == typeof(JsonDocument))
                return new JsonDocumentConverter() as IJsonDataConverter<T>;
            if (typeof(T) == typeof(JsonNode))
                return new JsonNodeConverter() as IJsonDataConverter<T>;
            if (typeof(T) == typeof(JsonObject))
                return new JsonObjectConverter() as IJsonDataConverter<T>;
            if (typeof(T) == typeof(JsonArray))
                return new JsonArrayConverter() as IJsonDataConverter<T>;
            if (typeof(T) == typeof(JsonValue))
                return new JsonValueConverter() as IJsonDataConverter<T>;

            return null;
        }


        private class JsonElementConverter : IJsonDataConverter<JsonElement>
        {
            public bool TryGet(in JsonData jsonData, out System.Text.Json.JsonElement value) => throw new NotImplementedException();
            public JsonData Create(System.Text.Json.JsonElement value) => throw new NotImplementedException();
        }

        private class JsonDocumentConverter : IJsonDataConverter<JsonDocument>
        {
            public bool TryGet(in JsonData jsonData, out JsonDocument value) => throw new NotImplementedException();

            public JsonData Create(JsonDocument value) => throw new NotImplementedException();

        }

        private class JsonNodeConverter : IJsonDataConverter<JsonNode>
        {
            public bool TryGet(in JsonData jsonData, out JsonNode value) => throw new NotImplementedException();

            public JsonData Create(JsonNode value) => throw new NotImplementedException();
        }

        private class JsonObjectConverter : IJsonDataConverter<JsonObject>
        {
            public bool TryGet(in JsonData jsonData, out JsonObject value) => throw new NotImplementedException();

            public JsonData Create(JsonObject value) => throw new NotImplementedException();
        }

        private class JsonArrayConverter : IJsonDataConverter<JsonArray>
        {
            public bool TryGet(in JsonData jsonData, out JsonArray value) => throw new NotImplementedException();

            public JsonData Create(JsonArray value) => throw new NotImplementedException();
        }

        private class JsonValueConverter : IJsonDataConverter<JsonValue>
        {
            public bool TryGet(in JsonData jsonData, out JsonValue value) => throw new NotImplementedException();
            public JsonData Create(JsonValue value) => throw new NotImplementedException();
        }





        //private static IJsonDataConverter<T>? GetTypedJsonDataWrapperConverter<T>()
        //{
        //    var type = typeof(T);

        //    if (!type.IsGenericType) return null;
        //    var genericArgs = type.GetGenericArguments();
        //    if (genericArgs.Length != 1) return null;

        //    var genericType = type.GetGenericTypeDefinition();
        //    if (genericType == typeof(JsonData<>))
        //    {
        //        var elementType = genericArgs[0];
        //        var converterType = typeof(TypedJsonDataWrapperConverter<>).MakeGenericType(elementType);
        //        return (IJsonDataConverter<T>)Activator.CreateInstance(converterType)!;
        //    }
        //    return null;
        //}

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