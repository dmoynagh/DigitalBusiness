using DigitalBusiness.Json.JsonPaths;

namespace DigitalBusiness.JsonDataWrappers.Diff
{
    /// <summary>The kind of change a <see cref="JsonDiffEntry"/> represents.</summary>
    public enum JsonDiffKind
    {
        /// <summary>Present in the target but not the baseline.</summary>
        Added,
        /// <summary>Present in the baseline but not the target.</summary>
        Removed,
        /// <summary>Present in both, but with a different value (including a value-kind change,
        /// e.g. object to array, reported as a single entry rather than a remove+add pair).</summary>
        Changed
    }

    /// <summary>A single difference found by <see cref="JsonDiff.Diff"/> at a specific path.</summary>
    public readonly record struct JsonDiffEntry(JsonPath Path, JsonDiffKind Kind, JsonData? OldValue, JsonData? NewValue);
}
