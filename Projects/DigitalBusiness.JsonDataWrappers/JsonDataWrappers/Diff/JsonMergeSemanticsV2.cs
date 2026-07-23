using System.Text.Json;
using System.Text.Json.Nodes;

namespace DigitalBusiness.JsonDataWrappers.Diff
{
    /// <summary>
    /// Merge semantics version 2: object patch values merge, array patch values replace wholesale,
    /// the <c>"$$delete"</c> sentinel string deletes the property at that path, and an explicit JSON
    /// <c>null</c> — or the greppable <c>"$$null"</c> sentinel — sets the property to explicit JSON null.
    /// Unlike v1, a literal <c>null</c> is no longer overloaded to mean delete.
    /// </summary>
    public sealed class JsonMergeSemanticsV2 : IJsonMergeSemantics
    {
        /// <summary>The shared singleton instance.</summary>
        public static readonly JsonMergeSemanticsV2 Instance = new();

        private JsonMergeSemanticsV2() { }

        /// <inheritdoc/>
        public string Version => "2";

        /// <inheritdoc/>
        public bool IsDelete(in JsonData patchValue) =>
            patchValue.ValueKind == JsonValueKind.String && patchValue.Get<string>() == "$$delete";

        /// <inheritdoc/>
        public bool IsSetNull(in JsonData patchValue) =>
            patchValue.IsNull || (patchValue.ValueKind == JsonValueKind.String && patchValue.Get<string>() == "$$null");

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
        public JsonData CreateSetNullMarker() => new JsonData((JsonNode?)JsonValue.Create("$$null"));
    }
}
