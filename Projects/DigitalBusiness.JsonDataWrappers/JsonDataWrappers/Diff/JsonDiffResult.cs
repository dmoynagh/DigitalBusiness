using DigitalBusiness.Json.JsonPaths;
using System.Collections.Generic;
using System.Linq;

namespace DigitalBusiness.JsonDataWrappers.Diff
{
    /// <summary>The result of a <see cref="JsonDiff.Diff"/> comparison.</summary>
    public sealed class JsonDiffResult
    {
        /// <summary>All differences found, in traversal order.</summary>
        public IReadOnlyList<JsonDiffEntry> Entries { get; }

        /// <summary>The paths at which a difference was found, in traversal order.</summary>
        public IReadOnlyList<JsonPath> ChangedPaths { get; }

        /// <summary>True if no differences were found.</summary>
        public bool IsEmpty => Entries.Count == 0;

        private readonly JsonData _target;

        internal JsonDiffResult(IReadOnlyList<JsonDiffEntry> entries, in JsonData target)
        {
            Entries = entries;
            ChangedPaths = entries.Select(e => e.Path).ToArray();
            _target = target;
        }

        /// <summary>
        /// Converts this diff into a <see cref="JsonPatch"/> suitable for <see cref="JsonMerge.Apply"/>,
        /// using the given merge <paramref name="semantics"/> (v1 or v2) to decide how deletions and
        /// explicit-null values are represented. Any array with at least one changed/added/removed index
        /// is collapsed into a single whole-array-replacement patch entry at the array's own path,
        /// matching the merge model's "arrays replace entirely" semantics.
        /// </summary>
        public JsonPatch ToPatch(IJsonMergeSemantics semantics)
        {
            var patchRoot = JsonData.CreateObject();
            var handledArrayPaths = new HashSet<string>();

            foreach (var entry in Entries)
            {
                var indexPosition = FindFirstIndexSegment(entry.Path);
                if (indexPosition >= 0)
                {
                    var arrayPath = entry.Path.Slice(0, indexPosition);
                    var arrayPathKey = arrayPath.ToString();
                    if (!handledArrayPaths.Add(arrayPathKey)) continue;

                    if (_target.TryGet(arrayPath, out var newArrayValue))
                        SetPatchValue(patchRoot, arrayPath, newArrayValue.Clone());
                    else
                        SetPatchValue(patchRoot, arrayPath, semantics.CreateDeleteMarker());

                    continue;
                }

                switch (entry.Kind)
                {
                    case JsonDiffKind.Removed:
                        SetPatchValue(patchRoot, entry.Path, semantics.CreateDeleteMarker());
                        break;
                    case JsonDiffKind.Added:
                    case JsonDiffKind.Changed:
                        var newValue = entry.NewValue!.Value;
                        SetPatchValue(patchRoot, entry.Path, newValue.IsNull ? semantics.CreateSetNullMarker() : newValue.Clone());
                        break;
                }
            }

            return new JsonPatch(patchRoot);
        }

        private static void SetPatchValue(in JsonData patchRoot, JsonPath path, JsonData value)
        {
            JsonData parent = path.Length == 1
                ? patchRoot
                : (path[0].IsIndex ? patchRoot.GetOrCreateArrayDeep(path.Slice(0, path.Length - 1)) : patchRoot.GetOrCreateObjectDeep(path.Slice(0, path.Length - 1)));

            var last = path[path.Length - 1];
            if (last.IsIndex)
                parent.Set(last.Index, value);
            else
                parent.Set(last.Property, value);
        }

        private static int FindFirstIndexSegment(JsonPath path)
        {
            for (int i = 0; i < path.Length; i++)
            {
                if (path[i].IsIndex) return i;
            }
            return -1;
        }
    }
}
