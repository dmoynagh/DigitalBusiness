using System;
using System.Collections.Generic;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace DigitalBusiness.Json.JsonNodes
{
    public static class JsonNodeExtensions
    {
        extension<T>(T node) where T : JsonNode
        {
            public JsonElement ToJsonElement() => node.Deserialize<JsonElement>();
        }
    }
}
