using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Nodes;
using DigitalBusiness.Json.JsonElements;
using DigitalBusiness.Json.JsonNodes;
using DigitalBusiness.JsonDataWrappers;
using DigitalBusiness.JsonDataWrappers.Internal;

namespace DigitalBusiness.JsonDataWrappers
{
    public static class JsonDataExtensions
    {
        extension(in JsonData jsonData)
        {
            public bool IsNull => (jsonData.Node is null && !jsonData.Element.HasValue) ||
                (jsonData.Element.HasValue && jsonData.Element.Value.ValueKind == JsonValueKind.Null) ||
                (jsonData.Node is not null && jsonData.Node.GetValueKind() == JsonValueKind.Null);

            public bool IsUndefined => jsonData.ValueKind == JsonValueKind.Undefined;
            //public bool IsEmpty => !node.Element.HasValue && node.Node is null;


            public void ThrowIfReadOnly() { if (jsonData.ReadOnly) throw JsonDataExceptionHelper.ReadOnlyException(); }

            //public void EnsureEditable() { if (jsonData.ReadOnly) throw JNodeExceptionHelper.ReadOnlyException(); }

            public void ThrowIfNull()
            {
                if (jsonData.IsNull) throw new InvalidOperationException("JNode value is null.");
            }


            public JsonData AsReadOnly() => jsonData.ReadOnly ? jsonData : new JsonData(jsonData.Node, true);

            public object? Source => jsonData.Element.HasValue ? jsonData.Element.Value : jsonData.Node;



            public static JsonData Create() => new JsonData();
            public static JsonData Create(JsonNode? source, bool readOnly=false) => new JsonData(source,readOnly);
            public static JsonData Create(JsonElement source) => new JsonData(source);
            public static JsonData Create(JsonElement? source) => new JsonData(source);


            public JsonData ToJsonElementJsonData()
            {
                if (jsonData.IsElement) 
                {
                    return JsonData.Create(jsonData.Element.Value.Clone()); 
                }
                else if (jsonData.IsNode)
                {
                    return JsonData.Create(jsonData.Node.ToJsonElement());
                }
                else
                {
                    return JsonData.Create(JsonElement.CreateNullElement());
                }
            }

            public JsonData ToJsonNodeJsonData(bool? readOnly = null)
            {
                if (jsonData.IsNode)
                {
                    return JsonData.Create(jsonData.Node.DeepClone(),readOnly.HasValue ? readOnly.Value : jsonData.ReadOnly);
                }
                else if (jsonData.IsElement)
                {
                    return JsonData.Create(jsonData.Element.Value.ToJsonNode(), readOnly.GetValueOrDefault(false));
                }
                else
                {
                    return JsonData.Create(JsonValue.Create((string?)null), readOnly.GetValueOrDefault(false));
                }
            }

            public JsonData ToEditableJsonData() => jsonData.ToJsonNodeJsonData(true);

        




            public int Count
            {
                get
                {
                    if (jsonData.Element.HasValue)
                    {
                        return jsonData.Element.Value.ValueKind switch
                        {
                            JsonValueKind.Array => jsonData.Element.Value.GetArrayLength(),
                            JsonValueKind.Object => jsonData.Element.Value.EnumerateObject().Count(),
                            _ => 0
                        };
                    }
                    else if (jsonData.Node != null)
                    {
                        return jsonData.Node switch
                        {
                            JsonArray jsonArray => jsonArray.Count,
                            JsonObject jsonObject => jsonObject.Count,
                            _ => 0
                        };
                    }
                    return 0;
                }
            }

            public void Clear()
            {
                jsonData.ThrowIfReadOnly();
                
                if (jsonData.Node is JsonArray jsonArray) jsonArray.Clear();
                else if (jsonData.Node is JsonObject jsonObject) jsonObject.Clear();
                else throw new InvalidOperationException("JNode is not an array or object.");
            }


           public bool IsValue =>
           (jsonData.Node is not null && jsonData.Node is JsonValue) ||
           (jsonData.Element.HasValue && jsonData.Element.Value.ValueKind switch
           {
               JsonValueKind.String => true,
               JsonValueKind.Number => true,
               JsonValueKind.True => true,
               JsonValueKind.False => true,
               JsonValueKind.Null => true,
               _ => false
           });

            public bool ThrowIfNotValue() { if (!jsonData.IsValue) throw new InvalidOperationException("Node is not a value."); else return true; }

        }

      
    }
}
