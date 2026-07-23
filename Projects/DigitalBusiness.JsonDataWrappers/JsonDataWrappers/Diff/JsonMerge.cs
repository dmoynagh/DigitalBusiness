using DigitalBusiness.Json.JsonPaths;
using System.Text.Json;

namespace DigitalBusiness.JsonDataWrappers.Diff
{
    /// <summary>
    /// Applies a <see cref="JsonPatch"/> (produced by <see cref="JsonDiffResult.ToPatch"/>, or built by
    /// hand) onto a base <see cref="JsonData"/> document, honouring the versioned semantics in
    /// <see cref="IJsonMergeSemantics"/> (v1 or v2) for deletion and explicit-null markers.
    /// <para>
    /// Round-trip law: for a given base and target, <c>JsonMerge.Apply(base, JsonDiff.Diff(base, target).ToPatch(semantics), options)</c>
    /// reproduces <c>target</c> — with the accepted exception that v1 cannot distinguish "delete" from
    /// "set explicit null", so if <c>target</c> contains an explicit null where <c>base</c> had a value,
    /// applying a v1 patch removes the property instead of nulling it.
    /// </para>
    /// </summary>
    public static class JsonMerge
    {
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
            var semantics = options.Semantics;

            if (semantics.IsDelete(patchValue))
            {
                parentObject.Remove(propertyName);
                return;
            }

            if (semantics.IsSetNull(patchValue))
            {
                parentObject.Set(propertyName, JsonData.CreateNull());
                return;
            }

            var behaviour = semantics.ForKind(patchValue.ValueKind);
            if (behaviour == MergeBehaviour.Merge && parentObject.TryGet(propertyName, out var existing) && existing.IsObject)
            {
                ApplyObject(existing, patchValue, childPath, options);
            }
            else
            {
                parentObject.Set(propertyName, patchValue.Clone());
            }
        }

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
