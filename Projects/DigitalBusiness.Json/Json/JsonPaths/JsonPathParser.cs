using System;
using System.Collections.Generic;

namespace DigitalBusiness.Json.JsonPaths
{
    /// <summary>
    /// Parses a path string into a <see cref="JsonPath"/>.
    /// <para>
    /// Supported syntax:
    /// <list type="bullet">
    ///   <item><c>prop</c> — single property</item>
    ///   <item><c>prop1.prop2</c> — chained properties</item>
    ///   <item><c>[0]</c> — array index, also valid at path start</item>
    ///   <item><c>prop1[2].name</c> — mixed</item>
    ///   <item><c>[0].name</c> — index-first</item>
    /// </list>
    /// </para>
    /// </summary>
    internal static class JsonPathParser
    {
        internal static JsonPath Parse(string path)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(path);

            var segments = new List<JsonPathSegment>();
            var span = path.AsSpan();
            int pos = 0;

            while (pos < span.Length)
            {
                char c = span[pos];

                if (c == '[')
                {
                    // Array index segment: [N]
                    int close = span.Slice(pos).IndexOf(']');
                    if (close < 0) throw new FormatException($"Unclosed '[' in path '{path}' at position {pos}.");

                    var indexSlice = span.Slice(pos + 1, close - 1);
                    if (!int.TryParse(indexSlice, out int index) || index < 0)
                        throw new FormatException($"Invalid array index '{indexSlice.ToString()}' in path '{path}'.");

                    segments.Add(JsonPathSegment.FromIndex(index));
                    pos += close + 1; // move past ']'

                    // consume optional '.' separator after ']'
                    if (pos < span.Length && span[pos] == '.') pos++;
                }
                else if (c == '.')
                {
                    // Leading or double dot — skip (treat as separator)
                    pos++;
                }
                else
                {
                    // Property name: read until '.', '[', or end
                    int start = pos;
                    while (pos < span.Length && span[pos] != '.' && span[pos] != '[')
                        pos++;

                    var name = span.Slice(start, pos - start).ToString();
                    if (string.IsNullOrWhiteSpace(name))
                        throw new FormatException($"Empty property name in path '{path}' at position {start}.");

                    segments.Add(JsonPathSegment.FromProperty(name));

                    // consume optional '.' separator
                    if (pos < span.Length && span[pos] == '.') pos++;
                }
            }

            if (segments.Count == 0)
                throw new FormatException($"Path '{path}' produced no segments.");

            return JsonPath.From(System.Runtime.InteropServices.CollectionsMarshal.AsSpan(segments));
        }
    }
}