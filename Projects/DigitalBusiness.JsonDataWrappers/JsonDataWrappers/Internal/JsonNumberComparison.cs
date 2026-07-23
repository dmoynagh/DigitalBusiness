using System;

namespace DigitalBusiness.JsonDataWrappers.Internal
{
    /// <summary>Shared numeric comparison helper for <c>Numeric</c>-mode JSON number comparisons.</summary>
    internal static class JsonNumberComparison
    {
        /// <summary>
        /// Compares two raw JSON number texts by parsed decimal value. Falls back to raw-text equality
        /// if either value cannot be parsed as a <see cref="decimal"/> (e.g. a number outside decimal's range).
        /// </summary>
        public static bool AreEqual(string rawA, string rawB)
        {
            if (decimal.TryParse(rawA, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out var a)
                && decimal.TryParse(rawB, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out var b))
            {
                return a == b;
            }
            return string.Equals(rawA, rawB, StringComparison.Ordinal);
        }
    }
}
