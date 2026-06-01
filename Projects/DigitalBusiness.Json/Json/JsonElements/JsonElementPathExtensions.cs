using DigitalBusiness.Json.JsonPaths;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;

namespace DigitalBusiness.Json.JsonElements
{
    public static class JsonElementPathExtensions
    {
        extension(JsonElement element)
        { 
            /// <summary>
            /// Navigates to the element at <paramref name="path"/>. Throws if any segment is missing or wrong kind.
            /// </summary>
            public JsonElement Get(JsonPath path)
            {
                if (element.TryGet(path, out var result))
                    return result;
                throw new KeyNotFoundException($"Path '{path}' not found.");
            }

            /// <summary>
            /// Attempts to navigate to the element at <paramref name="path"/>.
            /// Returns a null-kind element if any segment along the path is missing.
            /// </summary>
            public JsonElement? TryGet(JsonPath path)
            {
                return element.TryGet(path, out var result) ? result : null;
            }

            /// <summary>
            /// Attempts to navigate to the element at <paramref name="path"/>.
            /// Returns false if any segment is missing or the wrong kind.
            /// </summary>
            public bool TryGet(JsonPath path, out JsonElement result)
            {
                JsonElement current = element;

                foreach (var segment in path)
                {
                    if (segment.IsIndex)
                    {
                        if (current.ValueKind != JsonValueKind.Array || segment.Index >= current.GetArrayLength())
                        {
                            result = default;
                            return false;
                        }
                        current = current[segment.Index];
                    }
                    else
                    {
                        if (current.ValueKind != JsonValueKind.Object ||
                            !current.TryGetProperty(segment.Property, out var child))
                        {
                            result = default;
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
