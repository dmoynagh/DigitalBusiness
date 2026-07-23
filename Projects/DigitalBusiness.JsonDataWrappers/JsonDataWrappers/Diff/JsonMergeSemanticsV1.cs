using System.Text.Json;
using System.Text.Json.Nodes;

namespace DigitalBusiness.JsonDataWrappers.Diff
{
    /// <summary>
    /// Merge semantics version 1: object patch values merge, array patch values replace wholesale,
    /// and either the <c>"$$delete"</c> sentinel string or an explicit JSON <c>null</c> deletes the
    /// property at that path. There is no way in v1 to express "set to explicit null" separately from
    /// "delete" — this is a documented, accepted limitation (see the round-trip law exception in
    /// <see cref="JsonMerge"/>'s remarks).
    /// </summary>
    public sealed class JsonMergeSemanticsV1 : IJsonMergeSemantics
    {
        /// <summary>The shared singleton instance.</summary>
        public static readonly JsonMergeSemanticsV1 Instance = new();

        private JsonMergeSemanticsV1() { }

        /// <inheritdoc/>
        public string Version => "1";

        /// <inheritdoc/>
        public bool IsDelete(in JsonData patchValue) =>
            patchValue.IsNull || (patchValue.ValueKind == JsonValueKind.String && patchValue.Get<string>() == "$$delete");

        /// <inheritdoc/>
        public bool IsSetNull(in JsonData patchValue) => false;

        /// <inheritdoc/>
        public MergeBehaviour ForKind(JsonValueKind patchValueKind) => patchValueKind switch
        {
            JsonValueKind.Object => MergeBehaviour.Merge,
            JsonValueKind.Array => MergeBehaviour.Replace,
            _ => MergeBehaviour.Replace
        };

        /// <inheritdoc/>
        public JsonData CreateDeleteMarker() => new JsonData((JsonNode?)JsonValue.Create("$$delete"));

        /// <inheritdoc/>
        public JsonData CreateSetNullMarker() => CreateDeleteMarker();
    }
}
