using DigitalBusiness.Json;
using DigitalBusiness.JsonDataWrappers;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace DigitalBusiness.JsonDataWrappers.Converters
{
    public static partial class JsonDataConverterProvider
    {

        private static IJsonDataConverter<T>? GetEnumConverter<T>()
        {
            if (typeof(T).IsEnum)
            {
                if (EnumJsonPersistanceAttribute.PersistAsNumber<T>())
                {
                    var converterType = typeof(JsonDataEnumStringConverter<>).MakeGenericType(typeof(T));
                    return (IJsonDataConverter<T>)Activator.CreateInstance(converterType)!;                    
                }
                else
                {
                    var converterType = typeof(JsonDataEnumNumberConverter<>).MakeGenericType(typeof(T));
                    return (IJsonDataConverter<T>)Activator.CreateInstance(converterType)!;                    
                }
            }
            return null;
        }


        private class JsonDataEnumStringConverter<T> : IJsonDataConverter<T> where T : struct, Enum
        {
            public JsonData Create(T value)
            {
                var strValue = value.ToString();
                return new JsonData(JsonValue.Create(strValue));
            }

            public bool TryGet(in JsonData jsonData, out T value)
            {
                if (!jsonData.IsNull)
                {
                    if (jsonData.Element is not null)
                    {
                        switch (jsonData.Element.Value.ValueKind)
                        {
                            case JsonValueKind.String:
                                var strValue = jsonData.Element.Value.GetString();
                                if (!string.IsNullOrEmpty(strValue) && Enum.TryParse(strValue, out T result))
                                {
                                    value = result;
                                    return true;
                                }
                                break;
                            case JsonValueKind.Number:
                                try
                                {
                                    var numValue = jsonData.Element.Value.GetInt64();
                                    value = (T)Enum.ToObject(typeof(T), numValue);
                                    return true;
                                }
                                catch { }
                                break;
                        }
                    }
                    else if (jsonData.IsNode && jsonData.Node is JsonValue jsonValue && jsonValue != null)
                    {
                        switch (jsonValue.GetValueKind())
                        {
                            case JsonValueKind.String:
                                var strValue = jsonValue.GetValue<string>();
                                if (!string.IsNullOrEmpty(strValue) && Enum.TryParse(strValue, out T result))
                                {
                                    value = result;
                                    return true;
                                }
                                break;
                            case JsonValueKind.Number:
                                try
                                {
                                    var numValue = Convert.ToInt64(jsonValue.GetValue<object>());
                                    value = (T)Enum.ToObject(typeof(T), numValue);
                                    return true;
                                }
                                catch { }
                                break;
                        }
                    }
                }
                value = default!;
                return false;
            }


        }

        private class JsonDataEnumNumberConverter<T> : IJsonDataConverter<T> where T : struct, Enum
        {
            public JsonData Create(T value)
            {
                var numValue = Convert.ToInt64(value);
                return new JsonData(JsonValue.Create(numValue));
            }

            public bool TryGet(in JsonData jsonData, [MaybeNullWhen(false)] out T value)
            {
                if (!jsonData.IsNull)
                {
                    if (jsonData.Element is not null)
                    {
                        switch (jsonData.Element.Value.ValueKind)
                        {
                            case JsonValueKind.Number:
                                try
                                {
                                    var numValue = jsonData.Element.Value.GetInt64();
                                    value = (T)Enum.ToObject(typeof(T), numValue);
                                    return true;
                                }
                                catch { }
                                break;
                            case JsonValueKind.String:
                                var strValue = jsonData.Element.Value.GetString();
                                if (!string.IsNullOrEmpty(strValue) && Enum.TryParse(strValue, out T result))
                                {
                                    value = result;
                                    return true;
                                }
                                break;
                        }
                    }
                    else if (jsonData.IsNode && jsonData.Node is JsonValue jsonValue && jsonValue != null)
                    {
                        switch (jsonValue.GetValueKind())
                        {
                            case JsonValueKind.Number:
                                try
                                {
                                    var numValue = Convert.ToInt64(jsonValue.GetValue<object>());
                                    value = (T)Enum.ToObject(typeof(T), numValue);
                                    return true;
                                }
                                catch { }
                                break;
                            case JsonValueKind.String:
                                var strValue = jsonValue.GetValue<string>();
                                if (!string.IsNullOrEmpty(strValue) && Enum.TryParse(strValue, out T result))
                                {
                                    value = result;
                                    return true;
                                }
                                break;
                        }
                    }
                }
                value = default!;
                return false;                
            }
        }




        private static IJsonDataConverter<T>? GetSerializationConverter<T>()
        {
            var type = typeof(T);
            // Only use serialization for user-defined class/struct types, not BCL types
            if ((type.IsClass || (type.IsValueType && !type.IsEnum && !type.IsPrimitive))
                && type.Assembly != typeof(string).Assembly
                && type.Assembly != typeof(Uri).Assembly)
            {
                return new JsonDataSerializationConverter<T>();
            }
            return null;
        }

        private class JsonDataSerializationConverter<T> : IJsonDataConverter<T>
        {
            public bool TryGet(in JsonData jsonData, out T value)
            {
                try
                {
                    if (!jsonData.IsNull)
                    {
                        if (jsonData.IsNode)
                        {
                            var result = jsonData.Node!.Deserialize<T>();
                            if (result != null)
                            {
                                value = result;
                                return true;
                            }
                        }
                        else if (jsonData.IsElement)
                        {
                            var result = jsonData.Element.Value!.Deserialize<T>();
                            if (result != null)
                            {
                                value = result;
                                return true;
                            }
                        }
                    }
                }
                catch
                {
                    // Ignore deserialization errors and return false
                }
                value = default!;
                return false;
            }
            public JsonData Create(T value)
            {
                var json = JsonSerializer.Serialize(value);
                var jsonNode = JsonNode.Parse(json);
                return new JsonData(jsonNode);
            }
        }



        private class UndefinedConverter<T> : IJsonDataConverter<T>
        {
            public bool TryGet(in JsonData jsonData, out T value) => throw new NotSupportedException($"Cannot get value from JNode for type {typeof(T).FullName}. No converter defined.");
            public JsonData Create(T value) => throw new NotSupportedException($"Cannot create JNode from type {typeof(T).FullName}. No converter defined.");


        }

    }
}