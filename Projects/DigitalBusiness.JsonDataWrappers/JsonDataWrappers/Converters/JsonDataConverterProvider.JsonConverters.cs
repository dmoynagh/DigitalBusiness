using DigitalBusiness.JsonDataWrappers.Converters;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Net.Http.Headers;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

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

        /// <summary>
        /// Adapts a <see cref="JsonConverterAttribute"/>-specified <see cref="JsonConverter{T}"/> as an
        /// <see cref="IJsonDataConverter{T}"/>. Only consulted when no explicit registration exists via
        /// <see cref="JsonDataConverters"/> — explicit registration always wins over this fallback.
        /// </summary>
        private static IJsonDataConverter<T>? GetJsonConverterAttributeConverter<T>()
        {
            var type = typeof(T);
            var attribute = type.GetCustomAttribute<JsonConverterAttribute>();
            if (attribute?.ConverterType is null) return null;

            if (Activator.CreateInstance(attribute.ConverterType) is not JsonConverter<T> converter)
                return null;

            return new JsonConverterAttributeAdapter<T>(converter);
        }

        private sealed class JsonConverterAttributeAdapter<T> : IJsonDataConverter<T>
        {
            private readonly JsonConverter<T> _converter;

            public JsonConverterAttributeAdapter(JsonConverter<T> converter) => _converter = converter;

            public bool TryGet(in JsonData jsonData, [MaybeNullWhen(false)] out T value)
            {
                try
                {
                    if (jsonData.IsNull) { value = default; return false; }

                    var json = jsonData.IsNode ? jsonData.Node!.ToJsonString() : jsonData.Element!.Value.GetRawText();
                    var reader = new Utf8JsonReader(System.Text.Encoding.UTF8.GetBytes(json));
                    reader.Read();
                    var result = _converter.Read(ref reader, typeof(T), JsonSerializerOptions.Default);
                    if (result is not null)
                    {
                        value = result;
                        return true;
                    }
                }
                catch
                {
                    // fall through to false
                }
                value = default;
                return false;
            }

            public JsonData Create(T value)
            {
                using var stream = new System.IO.MemoryStream();
                using (var writer = new Utf8JsonWriter(stream))
                {
                    _converter.Write(writer, value, JsonSerializerOptions.Default);
                }
                var element = JsonSerializer.Deserialize<JsonElement>(stream.ToArray());
                return new JsonData(element);
            }
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