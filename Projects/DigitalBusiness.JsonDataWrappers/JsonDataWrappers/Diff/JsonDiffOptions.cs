using System.Collections.Generic;

namespace DigitalBusiness.JsonDataWrappers.Diff
{
    /// <summary>Controls how <see cref="JsonDiff"/> and <see cref="JsonData.EnumerateLeaves"/> compare and traverse content.</summary>
    public sealed class JsonDiffOptions
    {
        /// <summary>How numeric values are compared. Defaults to <see cref="NumberComparisonMode.Numeric"/> —
        /// numbers are compared by parsed value (e.g. <c>1</c> and <c>1.0</c> are equal). Use
        /// <see cref="NumberComparisonMode.Structural"/> for BCL-matching exact-text comparison.</summary>
        public NumberComparisonMode NumberComparison { get; init; } = NumberComparisonMode.Numeric;

        /// <summary>Path prefixes to exclude entirely from traversal/diffing — no entries are emitted for
        /// anything under an excluded prefix. Compared against each path's string form via
        /// <see cref="string.StartsWith(string)"/>.</summary>
        public IReadOnlyList<string> ExcludedPathPrefixes { get; init; } = [];
    }

    /// <summary>The mode used to compare JSON numeric values for equality.</summary>
    public enum NumberComparisonMode
    {
        /// <summary>Numbers are compared by parsed decimal value — <c>1</c> and <c>1.0</c> are equal.</summary>
        Numeric,

        /// <summary>Numbers are compared by exact raw text, matching <see cref="JsonData.DeepEquals"/>/BCL semantics.</summary>
        Structural
    }
}
