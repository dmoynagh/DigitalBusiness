namespace DigitalBusiness.JsonDataWrappers.Diff
{
    /// <summary>
    /// A patch document produced by <see cref="JsonDiffResult.ToPatch"/> and consumed by
    /// <see cref="JsonMerge.Apply"/>/<see cref="JsonMerge.ApplyInPlace"/>. Wraps a JSON object whose
    /// shape mirrors the target document, using <see cref="JsonMerge.DeleteMarker"/>/
    /// <see cref="JsonMerge.SetNullMarker"/> to represent deletions and explicit nulls.
    /// </summary>
    public readonly struct JsonPatch
    {
        /// <summary>The underlying patch document, always an object.</summary>
        public JsonData Json { get; }

        /// <summary>Wraps an existing <see cref="JsonData"/> object as a patch document.</summary>
        public JsonPatch(JsonData json)
        {
            json.ThrowIfNotObject();
            Json = json;
        }
    }
}
