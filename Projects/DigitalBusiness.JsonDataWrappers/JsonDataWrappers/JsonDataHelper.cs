using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;

namespace DigitalBusiness.JsonDataWrappers
{
    /// <summary>
    /// Internal utility methods for working with <see cref="JsonData"/> internals.
    /// Handles JsonNode tree navigation and cross-source operations not suited to extension methods.
    /// </summary>
    public static class JsonDataHelper
    {
        /// <summary>Wraps a <see cref="JsonElement"/> as a <see cref="JsonNode"/> (as a JsonValue).</summary>
        public static JsonNode? CreateNodeFromElement(in JsonElement element) =>  JsonValue.Create(element);

        /// <summary>Walks up the parent chain to return the root node of a node tree.</summary>
        public static JsonNode GetRootNode(JsonNode node)
        {
            JsonNode current = node;
            while (current.Parent is not null)
            {
                current = current.Parent;
            }
            return current;
        }

        /// <summary>Returns true if both nodes share the same root — i.e. they are part of the same node tree.</summary>
        public static bool HasCommonRoot(JsonNode nodeA, JsonNode nodeB)=> GetRootNode(nodeA) == GetRootNode(nodeB);


        /// <summary>
        /// Returns the node safe to add to another tree. If the node already has a parent in a different tree,
        /// returns a deep clone to avoid cross-tree ownership violations enforced by <see cref="JsonNode"/>.
        /// </summary>
        public static JsonNode GetNodeToAdd(JsonNode addNode, JsonNode parentNode)
        {            
            if(addNode.Parent is null) return addNode;

            if (HasCommonRoot(addNode,parentNode)) return addNode;
            return addNode.DeepClone();
        }

        /// <summary>Resolves the correct node to add from a <see cref="JsonData"/> value, handling null, Element, and Node sources.</summary>
        public static JsonNode? GetNodeToAdd(in JsonData addValue, JsonNode addToNode)
        {
            if (addValue.IsNull) return default;
            
            if (addValue.Element.HasValue) CreateNodeFromElement(addValue.Element.Value);

            if(addValue.Node is not null) return GetNodeToAdd(addValue.Node, addToNode);
            throw new InvalidOperationException("JsonData does not contain a valid node or element.");
        }

        /// <summary>Enumerates property names from an object-kind <see cref="JsonData"/>, regardless of source type.</summary>
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

        /// <summary>Enumerates array items from an array-kind <see cref="JsonData"/>, regardless of source type.</summary>
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
