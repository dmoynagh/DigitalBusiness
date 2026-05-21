using DigitalBusiness.JsonDataWrappers;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace DigitalBusiness.JsonDataWrappers.Converters
{
    //Json serializer for JsonData<T> struct, which can handle both JsonNode and JsonElement representations of JSON data.
    //It ensures that the correct type is deserialized based on the input JSON structure and provides a way to serialize JsonData<T> back to JSON format.
    public class TypedJsonDataJsonConverter<T>: JsonConverter<JsonData<T>>
    {
        public override JsonData<T> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if(typeToConvert != typeof(JsonData))
                throw new JsonException($"Unexpected type to convert: {typeToConvert.FullName}. Expected: {typeof(JsonData).FullName}");
            
            var element = JsonSerializer.Deserialize<JsonElement>(ref reader, options);
            return new JsonData<T> { Json = new JsonData(element) };
        }
       
        public override void Write(Utf8JsonWriter writer, JsonData<T> value, JsonSerializerOptions options)
        {
            var jsonData = value.Json;
            if (jsonData.IsNode)
            {
                JsonSerializer.Serialize(writer, jsonData.Node, options);
            }
            else if(jsonData.IsElement)
            {
                JsonSerializer.Serialize(writer, jsonData.Element, options);
            }
            else
            {
                writer.WriteNullValue();
            }
        }

    }
}
