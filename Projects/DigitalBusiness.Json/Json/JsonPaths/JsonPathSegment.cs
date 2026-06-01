using System;

namespace DigitalBusiness.Json.JsonPaths
{
    /// <summary>
    /// A single step in a <see cref="JsonPath"/>: either a property name or an array index.
    /// A readonly struct — zero heap allocation.
    /// </summary>
    public readonly struct JsonPathSegment : IEquatable<JsonPathSegment>
    {
        private readonly string? _property;
        private readonly int _index;

        /// <summary>Creates a property segment.</summary>
        private JsonPathSegment(string property)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(property);
            _property = property;
            _index = 0;
            IsIndex = false;
        }

        /// <summary>Creates an index segment.</summary>
        private JsonPathSegment(int index)
        {
            if (index < 0) throw new ArgumentOutOfRangeException(nameof(index), "Array index must be non-negative.");
            _index = index;
            _property = null;
            IsIndex = true;
        }

        /// <summary>True if this segment represents an array index; false if it is a property name.</summary>
        public bool IsIndex { get; }

        /// <summary>True if this segment represents a property name.</summary>
        public bool IsProperty => !IsIndex;

        /// <summary>The property name. Throws if this is an index segment.</summary>
        public string Property => !IsIndex ? _property! : throw new InvalidOperationException("This segment is an index, not a property.");

        /// <summary>The array index. Throws if this is a property segment.</summary>
        public int Index => IsIndex ? _index : throw new InvalidOperationException("This segment is a property, not an index.");

        /// <summary>Creates a property segment.</summary>
        public static JsonPathSegment FromProperty(string property) => new(property);

        /// <summary>Creates an index segment.</summary>
        public static JsonPathSegment FromIndex(int index) => new(index);

        /// <summary>Implicit conversion from <see langword="string"/> — enables <c>JsonPath.From("prop1", 2)</c> syntax.</summary>
        public static implicit operator JsonPathSegment(string property) => FromProperty(property);

        /// <summary>Implicit conversion from <see langword="int"/> — enables <c>JsonPath.From("prop1", 2)</c> syntax.</summary>
        public static implicit operator JsonPathSegment(int index) => FromIndex(index);

        /// <inheritdoc/>
        public bool Equals(JsonPathSegment other) =>
            IsIndex == other.IsIndex &&
            (IsIndex ? _index == other._index : string.Equals(_property, other._property, StringComparison.Ordinal));

        /// <inheritdoc/>
        public override bool Equals(object? obj) => obj is JsonPathSegment other && Equals(other);

        /// <inheritdoc/>
        public override int GetHashCode() => IsIndex ? HashCode.Combine(true, _index) : HashCode.Combine(false, _property);

        /// <inheritdoc/>
        public override string ToString() => IsIndex ? $"[{_index}]" : _property!;

        public static bool operator ==(JsonPathSegment left, JsonPathSegment right) => left.Equals(right);
        public static bool operator !=(JsonPathSegment left, JsonPathSegment right) => !left.Equals(right);
    }
}