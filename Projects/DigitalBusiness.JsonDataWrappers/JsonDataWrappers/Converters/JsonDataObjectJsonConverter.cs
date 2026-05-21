using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace DigitalBusiness.JsonDataWrappers.Converters
{
    using DigitalBusiness.JsonDataWrappers;
    using System;
    using System.Reflection;
    using System.Text.Json;
    using System.Text.Json.Serialization;

    public class JsonDataWrapperJsonConverterFactory : JsonConverterFactory
    {
        public override bool CanConvert(Type typeToConvert)
        {
            // Check if the type is or derives from JsonDataWrapper
            return typeof(JsonDataObject).IsAssignableFrom(typeToConvert) && !typeToConvert.IsAbstract;
        }

        public override JsonConverter? CreateConverter(Type typeToConvert, JsonSerializerOptions options)
        {
            // Create JsonDataWrapperJsonConverter<TWrapper> at runtime
            JsonConverter converter = (JsonConverter)Activator.CreateInstance(
                typeof(JsonDataWrapperJsonConverter<>).MakeGenericType(typeToConvert),
                BindingFlags.Instance | BindingFlags.Public,
                binder: null,
                args: null,
                culture: null)!;

            return converter;
        }
    }

    public class JsonDataWrapperJsonConverter<TWrapper> : JsonConverter<TWrapper>
        where TWrapper : JsonDataObject, IJsonDataWrapper, new()
    {
        public override TWrapper Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            // Deserialize as JsonData
            var jsonData = JsonSerializer.Deserialize<JsonData>(ref reader, options);

            // Create instance of the wrapper type and set Json property via interface
            var wrapper = new TWrapper() { Json = jsonData };

            return wrapper;
        }

        public override void Write(Utf8JsonWriter writer, TWrapper value, JsonSerializerOptions options)
        {
            // Get the Json property through the interface
            var jsonData = value.Json;

            // Serialize the JsonData
            JsonSerializer.Serialize(writer, jsonData, options);
        }
    }    
}
