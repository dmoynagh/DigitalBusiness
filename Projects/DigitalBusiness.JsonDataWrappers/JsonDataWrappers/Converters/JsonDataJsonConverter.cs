using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace DigitalBusiness.JsonDataWrappers.Converters
{
    //Json serializer for JsonData struct, which can handle both JsonNode and JsonElement representations of JSON data.
    //It ensures that the correct type is deserialized based on the input JSON structure and provides a way to serialize JsonData back to JSON format.
    public class JsonDataJsonConverter: JsonConverter<JsonData>
    {
        public override JsonData Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if(typeToConvert != typeof(JsonData))
                throw new JsonException($"Unexpected type to convert: {typeToConvert.FullName}. Expected: {typeof(JsonData).FullName}");
            
            var element = JsonSerializer.Deserialize<JsonElement>(ref reader, options);
            return new JsonData(element);
        }
       
        public override void Write(Utf8JsonWriter writer, JsonData value, JsonSerializerOptions options)
        {
            if(value.IsNode)
            {
                JsonSerializer.Serialize(writer, value.Node, options);
            }
            else if(value.IsElement)
            {
                JsonSerializer.Serialize(writer, value.Element, options);
            }
            else
            {
                writer.WriteNullValue();
            }
        }

    }
}
