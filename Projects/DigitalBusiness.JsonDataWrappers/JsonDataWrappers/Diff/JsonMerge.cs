using DigitalBusiness.Json.JsonPaths;
using System.Text.Json;

namespace DigitalBusiness.JsonDataWrappers.Diff
{
    /// <summary>
    /// Applies a <see cref="JsonPatch"/> (produced by <see cref="JsonDiffResult.ToPatch"/>, or built by
    /// hand) onto a base <see cref="JsonData"/> document.
    /// <para>
    /// A property whose patch value is the string marker <c>"$$delete"</c> is removed from the target.
    /// A property whose patch value is a literal JSON null or the string marker <c>"$$null"</c> is set to
    /// an explicit null. Object-valued properties are merged recursively; all other values (including
    /// arrays) replace the existing value wholesale.
    /// </para>
    /// <para>
    /// Round-trip law: for a given base and target, <c>JsonMerge.Apply(base, JsonDiff.Diff(base, target).ToPatch(), options)</c>
    /// reproduces <c>target</c>.
    /// </para>
    /// </summary>
    public static class JsonMerge
    {
        /// <summary>The marker value used to represent property deletion in a patch document.</summary>
        public const string DeleteMarker = "$$delete";

        /// <summary>The marker value used to represent an explicit null in a patch document.</summary>
        public const string SetNullMarker = "$$null";
        /// <summary>
        /// Applies <paramref name="patch"/> onto a fresh clone of <paramref name="baseDocument"/> and
        /// returns the result. <paramref name="baseDocument"/> is never mutated.
        /// </summary>
        public static JsonData Apply(in JsonData baseDocument, in JsonPatch patch, JsonMergeOptions? options = null)
        {
            var clone = baseDocument.Clone();
            ApplyInPlace(clone, patch, options);
            return clone;
        }

        /// <summary>
        /// Applies <paramref name="patch"/> directly onto <paramref name="baseDocument"/>, mutating it
        /// in place. Requires a writable Node-backed instance.
        /// </summary>
        public static void ApplyInPlace(in JsonData baseDocument, in JsonPatch patch, JsonMergeOptions? options = null)
        {
            options ??= new JsonMergeOptions();
            baseDocument.ThrowIfNotObject();
            ApplyObject(baseDocument, patch.Json, path: null, options);
        }

        private static void ApplyObject(in JsonData target, in JsonData patchObject, JsonPath? path, JsonMergeOptions options)
        {
            foreach (var (name, patchValue) in patchObject.Properties)
            {
                var childPath = path is null ? JsonPath.From(name) : path.Append(name);
                if (IsOutOfScope(childPath, options.Scope)) continue;

                ApplyValue(target, name, patchValue, childPath, options);
            }
        }

        private static void ApplyValue(in JsonData parentObject, string propertyName, in JsonData patchValue, JsonPath childPath, JsonMergeOptions options)
        {
            if (IsDeleteMarker(patchValue))
            {
                parentObject.Remove(propertyName);
                return;
            }

            if (IsSetNullMarker(patchValue))
            {
                parentObject.Set(propertyName, JsonData.CreateNull());
                return;
            }

            if (patchValue.IsObject && parentObject.TryGet(propertyName, out var existing) && existing.IsObject)
            {
                ApplyObject(existing, patchValue, childPath, options);
            }
            else
            {
                parentObject.Set(propertyName, patchValue.Clone());
            }
        }

        private static bool IsDeleteMarker(in JsonData value) =>
            value.ValueKind == JsonValueKind.String && value.Get<string>() == DeleteMarker;

        private static bool IsSetNullMarker(in JsonData value) =>
            value.IsNull || (value.ValueKind == JsonValueKind.String && value.Get<string>() == SetNullMarker);

        private static bool IsOutOfScope(JsonPath path, JsonPath? scope)
        {
            if (scope is null) return false;
            if (path.Length < scope.Length) return true;
            for (int i = 0; i < scope.Length; i++)
            {
                if (path[i] != scope[i]) return true;
            }
            return false;
        }
    }
}
