
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace DigitalBusiness.JsonDataWrappers.Internal
{
    /// <summary>
    /// Deep structural equality comparisons for <see cref="JsonData"/> instances.
    /// Handles all source combinations: Element/Element, Node/Node, and cross-source Element/Node.
    /// Cross-source comparison iterates structures manually since the BCL provides no built-in cross-type deep equals.
    /// </summary>
    public static class JsonDataEquality
    {
        /// <summary>Compares two <see cref="JsonData"/> instances for structural equality regardless of source type.</summary>
        public static bool Equals(in JsonData a, in JsonData b)
        {
            if (a.IsNull && b.IsNull) return true;
            if (a.Element.HasValue && b.Element.HasValue) return Equals(a.Element.Value, b.Element.Value);
            if (a.Node != null && b.Node != null) return Equals(a.Node, b.Node);
            if (a.Element.HasValue && b.Node != null) return Equals(a.Element.Value, b.Node);
            if (a.Node != null && b.Element.HasValue) return Equals(a.Node, b.Element.Value);

            return false;
        }
        /// <summary>Treats a null (no-value) <paramref name="b"/> as equal to a null <see cref="JsonData"/>.</summary>
        public static bool Equals(in JsonData a, in JsonData? b)
        {
            if (!b.HasValue) return a.IsNull; // If b is null, return true if a is empty or explicitly null
            return Equals(a, b.Value);
        }

        /// <summary>Treats a null (no-value) <paramref name="a"/> as equal to a null <see cref="JsonData"/>.</summary>
        public static bool Equals(in JsonData? a, in JsonData b)
        {
            if (!a.HasValue) return b.IsNull; // If a is null, return true if b is empty or explicitly null
            return Equals(a.Value, b);
        }

        /// <summary>Compares two nullable <see cref="JsonData"/> instances. Two no-value instances are equal.</summary>
        public static bool Equals(in JsonData? a, in JsonData? b)
        {
            if (!a.HasValue)
            {
                if (!b.HasValue) return true; // Both are null
                if (b.Value.IsNull) return true; // a is null and b is explicitly null
                return false; // a is null but b is not null
            }
            if (!b.HasValue)
            {
                return a.Value.IsNull; // b is null, return true if a is explicitly null, otherwise false
            }
            return Equals(a.Value, b.Value);
        }

        
        /// <summary>Delegates to <see cref="JsonElement.DeepEquals"/>.</summary>
        public static bool Equals(in  JsonElement element1, in JsonElement element2) => JsonElement.DeepEquals(element1, element2);
        /// <summary>Treats a missing element as equal to an explicit null element.</summary>
        public static bool Equals(in  JsonElement? element1, in JsonElement? element2)
        {
            if (!element1.HasValue && !element2.HasValue) return true;
            if (!element1.HasValue && element2!.Value.ValueKind == JsonValueKind.Null) return true; // One is null and the other is explicitly null
            if (!element2.HasValue && element1!.Value.ValueKind == JsonValueKind.Null) return true; // One is null and the other is explicitly null
            return Equals(element1!.Value, element2!.Value);
        }

        /// <summary>Delegates to <see cref="JsonNode.DeepEquals"/>.</summary>
        public static bool Equals(JsonNode? nodeA, JsonNode? nodeB)=>JsonNode.DeepEquals(nodeA, nodeB);

        // Cross-source comparisons (Node vs Element) — requires manual structural traversal
        public static bool Equals(JsonNode? node,in JsonElement? element) => Equals(in element, node);
        public static bool Equals(in JsonElement? element, JsonNode? node) => element.HasValue ? Equals(element.Value, node) : node is null || node.GetValueKind() == JsonValueKind.Null;
        public static bool Equals(in JsonElement element, JsonNode? node) => Equals(node,in element);      
        public static bool Equals(JsonNode? node, in JsonElement element)
        {
            if(node is null) return element.ValueKind == JsonValueKind.Null;
            if(node is JsonObject jsonObject) return JsonObjectEquals(jsonObject, element);
            else if(node is JsonArray jsonArray) return JsonArrayEquals(jsonArray, element);
            else if(node is JsonValue jsonValue) return JsonValueEquals(jsonValue, element);
            return false;
        }


        public static bool JsonArrayEquals(JsonArray arrayNode, JsonElement arrayElement)
        {
            if (arrayElement.ValueKind != JsonValueKind.Array) return false;

            using (var nodeEnumerator = arrayNode.GetEnumerator())            
            using (var elementEnumerator = arrayElement.EnumerateArray())
            {
                while (nodeEnumerator.MoveNext())
                {
                    if (!elementEnumerator.MoveNext()) return false;
                    if (nodeEnumerator.Current is null)
                    {
                        if (elementEnumerator.Current.ValueKind != JsonValueKind.Null)  return false;
                    }
                    else
                    {
                        if (!Equals(nodeEnumerator.Current, elementEnumerator.Current)) return false;
                    }
                    
                }

                if(elementEnumerator.MoveNext()) return false; // Check if element has more items

                return true;
            }
        }

        public static bool JsonObjectEquals(JsonObject objectNode, JsonElement objectElement)
        {
            var count = 0;
            var nodePropertyCount = objectNode.Count;
            using (var elementEnumerator = objectElement.EnumerateObject())
            {
                while (elementEnumerator.MoveNext())
                {
                    count++;
                    if(count>nodePropertyCount) return false; // Check if element has more properties than node

                    //get property node value
                    if (objectNode.TryGetPropertyValue(elementEnumerator.Current.Name, out var nodePropertyValue))
                    {
                        //if nodePropertyValue is null then element property value must be null, otherwise return false
                        if (nodePropertyValue is null)
                        {
                            if (elementEnumerator.Current.Value.ValueKind != JsonValueKind.Null) return false;
                        }
                        //compare values
                        else
                        {
                            if (!Equals(nodePropertyValue, elementEnumerator.Current.Value)) return false;
                        }
                    }
                    //false if not exists
                    else
                    {
                        return false;
                    }

                   
                }
            }
            return count == objectNode.Count;
          
        }

        public static bool JsonValueEquals(JsonValue valueNode, JsonElement valueElement)
        {
            var nodeKind = valueNode.GetValueKind();
            var elementKind = valueElement.ValueKind;   
            if(nodeKind != elementKind) return false;

            return valueElement.ValueKind switch
            {
                JsonValueKind.String => valueNode.GetValue<string>() == valueElement.GetString(),
                JsonValueKind.Number => valueNode.GetValue<decimal>() == valueElement.GetDecimal(),
                JsonValueKind.Null=> true,
                JsonValueKind.True => true,
                JsonValueKind.False => true,            
                JsonValueKind.Undefined => false,
                JsonValueKind.Object => throw new InvalidOperationException("Cannot compare a JsonValue of type Object to a JsonElement. Use JsonObjectEquals instead."),
                JsonValueKind.Array => throw new InvalidOperationException("Cannot compare a JsonValue of type Array to a JsonElement. Use JsonArrayEquals instead."),
                _ => false
            };
        }



    }
}
