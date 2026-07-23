using System.Text.Json;

namespace DigitalBusiness.JsonDataWrappers.Diff
{
    /// <summary>The behaviour a patch value at a given path implies during a merge.</summary>
    public enum MergeBehaviour
    {
        /// <summary>Recurse into the patch value and merge it property-by-property into the base object.</summary>
        Merge,
        /// <summary>Replace the base value wholesale with the patch value (used for arrays and scalars).</summary>
        Replace,
        /// <summary>Remove the property/index at this path from the base.</summary>
        Delete,
        /// <summary>Set the property/index at this path to an explicit JSON null.</summary>
        SetNull
    }

    /// <summary>
    /// Defines how <see cref="JsonMerge"/> interprets patch values — the versioned contract distinguishing
    /// merge semantics v1 from v2 (see <see cref="JsonMergeSemanticsV1"/>/<see cref="JsonMergeSemanticsV2"/>).
    /// </summary>
    public interface IJsonMergeSemantics
    {
        /// <summary>The semantics version, e.g. "1" or "2".</summary>
        string Version { get; }

        /// <summary>True if <paramref name="patchValue"/> is this version's delete marker.</summary>
        bool IsDelete(in JsonData patchValue);

        /// <summary>True if <paramref name="patchValue"/> is this version's explicit-set-null marker.
        /// Always false for v1, which has no way to express this separately from delete.</summary>
        bool IsSetNull(in JsonData patchValue);

        /// <summary>Returns the merge behaviour that applies to a patch value of the given JSON value kind,
        /// before <see cref="IsDelete"/>/<see cref="IsSetNull"/> are consulted for markers.</summary>
        MergeBehaviour ForKind(JsonValueKind patchValueKind);

        /// <summary>Creates the patch value this version's <see cref="ToPatch"/> emits for a deleted path.</summary>
        JsonData CreateDeleteMarker();

        /// <summary>Creates the patch value this version's <see cref="ToPatch"/> emits for a path whose
        /// target value is explicit JSON null.</summary>
        JsonData CreateSetNullMarker();
    }
}
