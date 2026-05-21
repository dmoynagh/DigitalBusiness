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
            public  bool IsValue()
            {
                return element.ValueKind == JsonValueKind.String ||
                    element.ValueKind == JsonValueKind.Number ||
                    element.ValueKind == JsonValueKind.True ||
                    element.ValueKind == JsonValueKind.False ||
                    element.ValueKind == JsonValueKind.Null;
            }

            public  bool IsNull()
            {
                return element.ValueKind == JsonValueKind.Null;
            }

            public  bool IsUndefined()
            {
                return element.ValueKind == JsonValueKind.Undefined;
            }

            public  bool IsArray()
            {
                return element.ValueKind == JsonValueKind.Array;
            }

            public  bool IsObject()
            {
                return element.ValueKind == JsonValueKind.Object;
            }

            public static JsonElement CreateNullElement()
            {
                using var doc = JsonDocument.Parse("null");
                return doc.RootElement.Clone();
            }


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
