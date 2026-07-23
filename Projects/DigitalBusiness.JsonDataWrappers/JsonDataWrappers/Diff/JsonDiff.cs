using DigitalBusiness.Json.JsonPaths;
using System.Collections.Generic;
using System.Text.Json;

namespace DigitalBusiness.JsonDataWrappers.Diff
{
    /// <summary>
    /// Computes a structural diff between two <see cref="JsonData"/> instances, regardless of source
    /// type (Element or Node, in any combination). Both inputs are treated as read-only throughout —
    /// no mutation is ever performed on either.
    /// </summary>
    public static class JsonDiff
    {
        /// <summary>
        /// Compares <paramref name="baseline"/> against <paramref name="target"/> and returns every
        /// difference found. A kind-change at an intermediate path (e.g. object to array) is reported
        /// as a single <see cref="JsonDiffKind.Changed"/> entry at that path rather than a remove+add
        /// pair. Subtrees whose path starts with an entry in <paramref name="options"/>'s
        /// <see cref="JsonDiffOptions.ExcludedPathPrefixes"/> are skipped entirely.
        /// </summary>
        public static JsonDiffResult Diff(in JsonData baseline, in JsonData target, JsonDiffOptions? options = null)
        {
            options ??= new JsonDiffOptions();
            var entries = new List<JsonDiffEntry>();
            WalkPresent(baseline, target, path: null, options, entries);
            return new JsonDiffResult(entries, target);
        }

        private static void WalkPresent(JsonData baseline, JsonData target, JsonPath? path, JsonDiffOptions options, List<JsonDiffEntry> entries)
        {
            if (path is not null && IsExcluded(path, options.ExcludedPathPrefixes))
                return;

            var baselineKind = baseline.ValueKind;
            var targetKind = target.ValueKind;

            if (baselineKind == JsonValueKind.Object && targetKind == JsonValueKind.Object)
            {
                WalkObject(baseline, target, path, options, entries);
                return;
            }

            if (baselineKind == JsonValueKind.Array && targetKind == JsonValueKind.Array)
            {
                WalkArray(baseline, target, path, options, entries);
                return;
            }

            // Kind change (including object<->array, or a plain value kind change) or same-kind leaf compare.
            bool equal = options.NumberComparison == NumberComparisonMode.Numeric
                ? baseline.DeepSemanticEquals(target)
                : baseline.DeepEquals(target);

            if (!equal)
                entries.Add(new JsonDiffEntry(path ?? RootPath, JsonDiffKind.Changed, baseline, target));
        }

        private static void WalkObject(JsonData baseline, JsonData target, JsonPath? path, JsonDiffOptions options, List<JsonDiffEntry> entries)
        {
            var seen = new HashSet<string>();

            foreach (var (name, baselineValue) in baseline.Properties)
            {
                seen.Add(name);
                var childPath = Append(path, name);
                if (path is not null && IsExcluded(childPath, options.ExcludedPathPrefixes)) continue;

                if (target.TryGet(name, out var targetValue))
                    WalkPresent(baselineValue, targetValue, childPath, options, entries);
                else
                    entries.Add(new JsonDiffEntry(childPath, JsonDiffKind.Removed, baselineValue, null));
            }

            foreach (var (name, targetValue) in target.Properties)
            {
                if (seen.Contains(name)) continue;
                var childPath = Append(path, name);
                if (IsExcluded(childPath, options.ExcludedPathPrefixes)) continue;

                entries.Add(new JsonDiffEntry(childPath, JsonDiffKind.Added, null, targetValue));
            }
        }

        private static void WalkArray(JsonData baseline, JsonData target, JsonPath? path, JsonDiffOptions options, List<JsonDiffEntry> entries)
        {
            using var baselineEnumerator = baseline.Items.GetEnumerator();
            using var targetEnumerator = target.Items.GetEnumerator();

            int index = 0;
            while (true)
            {
                bool hasBaseline = baselineEnumerator.MoveNext();
                bool hasTarget = targetEnumerator.MoveNext();
                if (!hasBaseline && !hasTarget) break;

                var childPath = Append(path, index);
                if (IsExcluded(childPath, options.ExcludedPathPrefixes)) { index++; continue; }

                if (hasBaseline && hasTarget)
                {
                    WalkPresent(baselineEnumerator.Current, targetEnumerator.Current, childPath, options, entries);
                }
                else if (hasBaseline)
                {
                    entries.Add(new JsonDiffEntry(childPath, JsonDiffKind.Removed, baselineEnumerator.Current, null));
                }
                else
                {
                    entries.Add(new JsonDiffEntry(childPath, JsonDiffKind.Added, null, targetEnumerator.Current));
                }

                index++;
            }
        }

        private static bool IsExcluded(JsonPath path, IReadOnlyList<string> excludedPrefixes)
        {
            if (excludedPrefixes.Count == 0) return false;
            var pathString = path.ToString();
            foreach (var prefix in excludedPrefixes)
            {
                if (pathString.StartsWith(prefix)) return true;
            }
            return false;
        }

        private static JsonPath Append(JsonPath? path, string name) => path is null ? JsonPath.From(name) : path.Append(name);
        private static JsonPath Append(JsonPath? path, int index) => path is null ? JsonPath.From(index) : path.Append(index);

        private static readonly JsonPath RootPath = JsonPath.From("$");
    }
}
