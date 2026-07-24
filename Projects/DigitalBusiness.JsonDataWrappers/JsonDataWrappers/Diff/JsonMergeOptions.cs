using DigitalBusiness.Json.JsonPaths;

namespace DigitalBusiness.JsonDataWrappers.Diff
{
    /// <summary>Options controlling <see cref="JsonMerge.Apply"/>/<see cref="JsonMerge.ApplyInPlace"/>.</summary>
    public sealed class JsonMergeOptions
    {
        /// <summary>If set, any patch path that does not lie on-or-under this path is silently ignored —
        /// no error, no effect on the result.</summary>
        public JsonPath? Scope { get; init; }
    }
}
