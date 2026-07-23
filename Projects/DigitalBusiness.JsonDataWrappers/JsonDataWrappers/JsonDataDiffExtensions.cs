using DigitalBusiness.Json.JsonPaths;
using DigitalBusiness.JsonDataWrappers.Diff;
using System.Collections.Generic;
using System.Linq;

namespace DigitalBusiness.JsonDataWrappers
{
    /// <summary>
    /// Traversal extensions for <see cref="JsonData"/> shared by the diff engine, branch snapshot
    /// capture, and text search.
    /// </summary>
    public static class JsonDataDiffExtensions
    {
        extension(in JsonData jsonData)
        {
            /// <summary>
            /// Depth-first traversal yielding a <c>(Path, Value)</c> pair for every non-object, non-array
            /// node reached from this instance. Objects and arrays are recursed into (via <see cref="JsonDataJsonObjectExtensions.Properties"/>
            /// and <see cref="JsonDataJsonArrayExtensions.Items"/>) but never yielded themselves. Any subtree
            /// whose path starts with an entry in <paramref name="options"/>'s <see cref="JsonDiffOptions.ExcludedPathPrefixes"/>
            /// is skipped entirely.
            /// </summary>
            public IEnumerable<(JsonPath Path, JsonData Value)> EnumerateLeaves(JsonDiffOptions? options = null)
            {
                var excludedPrefixes = options?.ExcludedPathPrefixes;
                return EnumerateLeavesCore(jsonData, path: null, excludedPrefixes);
            }
        }

        private static IEnumerable<(JsonPath Path, JsonData Value)> EnumerateLeavesCore(
            JsonData jsonData, JsonPath? path, IReadOnlyList<string>? excludedPrefixes)
        {
            if (path is not null && IsExcluded(path, excludedPrefixes))
                yield break;

            if (jsonData.IsObject)
            {
                foreach (var (name, value) in jsonData.Properties)
                {
                    var childPath = AppendProperty(path, name);
                    foreach (var leaf in EnumerateLeavesCore(value, childPath, excludedPrefixes))
                        yield return leaf;
                }
            }
            else if (jsonData.IsArray)
            {
                int index = 0;
                foreach (var value in jsonData.Items)
                {
                    var childPath = AppendIndex(path, index);
                    foreach (var leaf in EnumerateLeavesCore(value, childPath, excludedPrefixes))
                        yield return leaf;
                    index++;
                }
            }
            else if (path is not null)
            {
                yield return (path, jsonData);
            }
        }

        private static bool IsExcluded(JsonPath path, IReadOnlyList<string>? excludedPrefixes)
        {
            if (excludedPrefixes is null || excludedPrefixes.Count == 0) return false;
            var pathString = path.ToString();
            return excludedPrefixes.Any(prefix => pathString.StartsWith(prefix));
        }

        private static JsonPath AppendProperty(JsonPath? path, string name) =>
            path is null ? JsonPath.From(name) : path.Append(name);

        private static JsonPath AppendIndex(JsonPath? path, int index) =>
            path is null ? JsonPath.From(index) : path.Append(index);
    }
}
