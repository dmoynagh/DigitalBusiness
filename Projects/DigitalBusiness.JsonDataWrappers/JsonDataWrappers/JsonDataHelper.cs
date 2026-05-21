using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;

namespace DigitalBusiness.JsonDataWrappers
{
    public static class JsonDataHelper
    {
        public static JsonNode? CreateNodeFromElement(in JsonElement element) =>  JsonValue.Create(element);

        public static JsonNode GetRootNode(JsonNode node)
        {
            JsonNode current = node;
            while (current.Parent is not null)
            {
                current = current.Parent;
            }
            return current;
        }

        public static bool HasCommonRoot(JsonNode nodeA, JsonNode nodeB)=> GetRootNode(nodeA) == GetRootNode(nodeB);
        

        public static JsonNode GetNodeToAdd(JsonNode addNode, JsonNode parentNode)
        {            
            if(addNode.Parent is null) return addNode;

            if (HasCommonRoot(addNode,parentNode)) return addNode;
            return addNode.DeepClone();
        }

        public static JsonNode? GetNodeToAdd(in JsonData addValue, JsonNode addToNode)
        {
            if (addValue.IsNull) return default;
            
            if (addValue.Element.HasValue) CreateNodeFromElement(addValue.Element.Value);

            if(addValue.Node is not null) return GetNodeToAdd(addValue.Node, addToNode);
            throw new InvalidOperationException("JsonData does not contain a valid node or element.");
        }

        public static IEnumerable<string> GetPropertyNames(JsonData jsonData)
        {
            if (jsonData.Element.HasValue && jsonData.Element.Value.ValueKind == JsonValueKind.Object)
            {
                foreach (var property in jsonData.Element.Value.EnumerateObject())
                {
                    yield return property.Name;
                }
            }
            else if (jsonData.Node != null && jsonData.Node is JsonObject jsonObj)
            {
                foreach (var item in jsonObj)
                {
                    yield return item.Key;
                }
            }
        }

        public static IEnumerable<JsonData> GetArrayItems(JsonData jsonData)
        {
            if (jsonData.Element.HasValue && jsonData.Element.Value.ValueKind == JsonValueKind.Array)
            {
                foreach (var element in jsonData.Element.Value.EnumerateArray())
                {
                    yield return new JsonData(element);
                }
            }
            else if (jsonData.Node != null && jsonData.Node is JsonArray jsonArray)
            {
                foreach (var item in jsonArray)
                {
                    yield return new JsonData(item);
                }
            }
        }


        //public static JsonNode? GetNodeToAdd(JsonData? addValue, JsonData? addTo)
        //{
        //    if (addValue is null) return null;
        //    if (addValue.Value.Node is not null) return GetNodeToAdd(addValue.Value.Node, addTo?.Node);
        //    if (addValue.Element.HasValue) return GetNodeToAdd(CreateNodeFromElement(addValue.Element.Value), addTo?.Node);
        //    return null;
        //}

    }
}
