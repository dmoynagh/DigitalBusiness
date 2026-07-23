using DigitalBusiness.Json.JsonPaths;

namespace DigitalBusiness.JsonDataWrappers.Diff
{
    /// <summary>Options controlling <see cref="JsonMerge.Apply"/>/<see cref="JsonMerge.ApplyInPlace"/>.</summary>
    public sealed class JsonMergeOptions
    {
        /// <summary>The merge semantics version to apply. Defaults to <see cref="JsonMergeSemanticsV2.Instance"/>.</summary>
        public IJsonMergeSemantics Semantics { get; init; } = JsonMergeSemanticsV2.Instance;

        /// <summary>If set, any patch path that does not lie on-or-under this path is silently ignored —
        /// no error, no effect on the result.</summary>
        public JsonPath? Scope { get; init; }
    }
}
