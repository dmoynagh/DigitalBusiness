using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace DigitalBusiness.Json.JsonPaths
{
    /// <summary>
    /// An immutable, reusable sequence of <see cref="JsonPathSegment"/> values describing a navigation
    /// path through a JSON structure. Parse once, store as a <see langword="static"/> field, reuse freely.
    /// <para>
    /// Supports property steps (<c>prop1.prop2</c>), array index steps (<c>[0]</c>),
    /// and combinations (<c>prop1[2].name</c>, <c>[0].name</c>).
    /// </para>
    /// </summary>
    public sealed class JsonPath : IEnumerable<JsonPathSegment>, IEquatable<JsonPath>
    {
        private readonly JsonPathSegment[] _segments;

        private JsonPath(JsonPathSegment[] segments)
        {
            _segments = segments;
        }

        /// <summary>The number of segments in this path.</summary>
        public int Length => _segments.Length;

        /// <summary>Returns the segment at the given position.</summary>
        public JsonPathSegment this[int index] => _segments[index];

        /// <summary>
        /// Constructs a path from a span of segments. Implicit operators on <see cref="JsonPathSegment"/>
        /// allow mixed <see langword="string"/>/<see langword="int"/> call sites with no boxing:
        /// <code>JsonPath.From("prop1", 2, "name")</code>
        /// </summary>
        public static JsonPath From(params ReadOnlySpan<JsonPathSegment> segments)
        {
            if (segments.IsEmpty) throw new ArgumentException("A path must contain at least one segment.", nameof(segments));
            return new JsonPath(segments.ToArray());
        }

        /// <summary>
        /// Parses a path string into a <see cref="JsonPath"/>. Supported syntax:
        /// <list type="bullet">
        ///   <item><c>prop1.prop2</c> — chained property names</item>
        ///   <item><c>[0]</c> — array index (also valid at the start of a path)</item>
        ///   <item><c>prop1[2].name</c> — mixed</item>
        ///   <item><c>[0].name</c> — index-first</item>
        /// </list>
        /// </summary>
        public static JsonPath Parse(string path) => JsonPathParser.Parse(path);

        /// <summary>
        /// Returns a new path with the given segment appended.
        /// Useful for dynamically building paths from a known base.
        /// </summary>
        public JsonPath Append(JsonPathSegment segment)
        {
            var next = new JsonPathSegment[_segments.Length + 1];
            _segments.CopyTo(next, 0);
            next[^1] = segment;
            return new JsonPath(next);
        }

        /// <summary>Returns a new path with the given segments appended.</summary>
        public JsonPath Append(params ReadOnlySpan<JsonPathSegment> segments)
        {
            if (segments.IsEmpty) return this;
            var next = new JsonPathSegment[_segments.Length + segments.Length];
            _segments.CopyTo(next, 0);
            segments.CopyTo(next.AsSpan(_segments.Length));
            return new JsonPath(next);
        }

        /// <summary>Returns a sub-path starting at <paramref name="start"/>, optionally limited to <paramref name="length"/> segments.</summary>
        public JsonPath Slice(int start, int length = -1)
        {
            if (start < 0 || start >= _segments.Length) throw new ArgumentOutOfRangeException(nameof(start));
            int count = length < 0 ? _segments.Length - start : length;
            return new JsonPath(_segments.AsSpan(start, count).ToArray());
        }

        /// <inheritdoc/>
        public IEnumerator<JsonPathSegment> GetEnumerator() => ((IEnumerable<JsonPathSegment>)_segments).GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => _segments.GetEnumerator();

        /// <inheritdoc/>
        public bool Equals(JsonPath? other)
        {
            if (other is null) return false;
            if (ReferenceEquals(this, other)) return true;
            if (_segments.Length != other._segments.Length) return false;
            for (int i = 0; i < _segments.Length; i++)
                if (_segments[i] != other._segments[i]) return false;
            return true;
        }

        /// <inheritdoc/>
        public override bool Equals(object? obj) => obj is JsonPath other && Equals(other);

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            var hc = new HashCode();
            foreach (var s in _segments) hc.Add(s);
            return hc.ToHashCode();
        }

        /// <summary>Reconstructs the path string from its segments.</summary>
        public override string ToString()
        {
            var sb = new StringBuilder();
            foreach (var segment in _segments)
            {
                if (segment.IsIndex)
                    sb.Append($"[{segment.Index}]");
                else
                {
                    if (sb.Length > 0 && !EndsWithIndex(sb)) sb.Append('.');
                    sb.Append(segment.Property);
                }
            }
            return sb.ToString();

            static bool EndsWithIndex(StringBuilder sb) => sb.Length > 0 && sb[^1] == ']';
        }

        public static bool operator ==(JsonPath? left, JsonPath? right) => left?.Equals(right) ?? right is null;
        public static bool operator !=(JsonPath? left, JsonPath? right) => !(left == right);
    }
}