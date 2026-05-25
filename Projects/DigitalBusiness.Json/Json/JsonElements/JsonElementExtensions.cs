using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading.Tasks;

namespace DigitalBusiness.Json.JsonElements
{
    public static class JsonElementExtensions
    {
        extension(JsonElement element)
        {

            // Checks if the JsonElement is a value (string, number, boolean, or null). This is useful for determining if you can get a value from the element.
            public bool IsValue()
            {
                return element.ValueKind == JsonValueKind.String ||
                    element.ValueKind == JsonValueKind.Number ||
                    element.ValueKind == JsonValueKind.True ||
                    element.ValueKind == JsonValueKind.False ||
                    element.ValueKind == JsonValueKind.Null;
            }

            // Checks if the JsonElement is null. This is useful for determining if the element represents a null value in JSON.
            public bool IsNull()
            {
                return element.ValueKind == JsonValueKind.Null;
            }

            // Checks if the JsonElement is undefined. This is useful for determining if the element was not set or is missing in the JSON.
            public bool IsUndefined()
            {
                return element.ValueKind == JsonValueKind.Undefined;
            }

            // Checks if the JsonElement is an array. This is useful for determining if you can enumerate over the element.
            public bool IsArray()
            {
                return element.ValueKind == JsonValueKind.Array;
            }

            // Checks if the JsonElement is an object. This is useful for determining if you can access properties on the element.
            public bool IsObject()
            {
                return element.ValueKind == JsonValueKind.Object;
            }


            // Creates a JsonElement that represents null. This is useful for returning a null value in a method that returns a JsonElement.
            public static JsonElement CreateNullElement()
            {
                using var doc = JsonDocument.Parse("null");
                return doc.RootElement.Clone();
            }

            // Converts the JsonElement to a JsonNode. This is useful for working with the JsonNode API when you have a JsonElement.
            public JsonNode? ToJsonNode() 
            {
                return element.ValueKind switch
                {
                    JsonValueKind.Null => null,
                    JsonValueKind.Array => JsonArray.Create(element),
                    JsonValueKind.Object => JsonObject.Create(element),
                    _ => JsonValue.Create(element) // Handles strings, numbers, booleans
                };             
            }
        }



    }
}
