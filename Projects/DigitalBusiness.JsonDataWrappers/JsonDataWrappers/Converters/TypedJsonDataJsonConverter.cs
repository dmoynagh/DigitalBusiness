using System.Text.Json;
using System.Text.Json.Serialization;

namespace DigitalBusiness.JsonDataWrappers.Converters
{
    /// <summary>
    /// JSON converter for <see cref="JsonData{T}"/>.
    /// Deserializes as a JsonElement (readonly). Serializes from either JsonNode or JsonElement source.
    /// </summary>
    public class TypedJsonDataJsonConverter<T>: JsonConverter<JsonData<T>> where T : IJsonDataKey
    {
        public override JsonData<T> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            
            var element = JsonSerializer.Deserialize<JsonElement>(ref reader, options);
            return new JsonData<T>(new JsonData(element));
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
