using DigitalBusiness.Json.JsonPaths;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Nodes;

namespace DigitalBusiness.Json.JsonNodes
{
    public static class JsonNodePathExtensions
    {
        extension<T>(T node) where T : JsonNode
        { 
            /// <summary>
            /// Navigates to the node at <paramref name="path"/>. Throws if any segment is missing or the wrong kind.
            /// </summary>
            public JsonNode Get(JsonPath path)
            {
                if (node.TryGet(path, out var result))
                    return result!;
                throw new KeyNotFoundException($"Path '{path}' not found.");
            }

            /// <summary>
            /// Attempts to navigate to the node at <paramref name="path"/>.
            /// Returns null if any segment along the path is missing.
            /// </summary>
            public JsonNode? TryGet(JsonPath path)
            {
                node.TryGet(path, out var result);
                return result;
            }

            /// <summary>
            /// Attempts to navigate to the node at <paramref name="path"/>.
            /// Returns false if any segment is missing or the wrong kind.
            /// </summary>
            public bool TryGet(JsonPath path, out JsonNode? result)
            {
                JsonNode? current = node;

                foreach (var segment in path)
                {
                    if (current is null)
                    {
                        result = null;
                        return false;
                    }

                    if (segment.IsIndex)
                    {
                        if (current is not JsonArray arr || segment.Index >= arr.Count)
                        {
                            result = null;
                            return false;
                        }
                        current = arr[segment.Index];
                    }
                    else
                    {
                        if (current is not JsonObject obj || !obj.TryGetPropertyValue(segment.Property, out var child))
                        {
                            result = null;
                            return false;
                        }
                        current = child;
                    }
                }

                result = current;
                return true;
            }
        }
    }
}
