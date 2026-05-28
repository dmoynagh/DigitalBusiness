
using DigitalBusiness.JsonDataWrappers;
using DigitalBusiness.JsonDataWrappers.Converters;
using DigitalBusiness.JsonDataWrappers.Internal;
using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;


namespace DigitalBusiness.JsonDataWrappers
{
    /// <summary>
    /// A lightweight readonly struct that wraps a JSON value from either a <see cref="JsonElement"/> (always readonly)
    /// or a <see cref="JsonNode"/> (read or write). Provides a unified API over both sources.
    /// <para>
    /// Structs are used deliberately — instances are created and discarded frequently, avoiding heap allocations.
    /// Use <see cref="JsonData{T}"/> to attach a typed key and gain domain-specific extension methods.
    /// </para>
    /// </summary>
    [DebuggerDisplay("{DebugDisplay,nq}")]
    [JsonConverter(typeof(JsonDataJsonConverter))]
    public readonly partial struct JsonData : IJsonData
    {
        JsonData IJsonData.Json => this;

        /// <summary>Creates an uninitialized (null) readonly instance. Equivalent to <see cref="CreateNull"/>.</summary>
        public JsonData() {
            _readOnly = true;
        }

        /// <summary>
        /// Wraps a <see cref="JsonNode"/>. Null nodes and <see cref="JsonValue"/> nodes are always readonly
        /// since <see cref="JsonValue"/> is an immutable leaf.
        /// </summary>
        public JsonData(JsonNode? node) : this(node, false) { }

        /// <summary>Wraps a <see cref="JsonElement"/>. Always readonly — JsonElement is an immutable BCL type.</summary>
        public JsonData(JsonElement element)
        {
            _readOnly = true;
            Element = element;
        }

        /// <summary>Wraps a nullable <see cref="JsonElement"/>. A null element produces an uninitialized readonly instance.</summary>
        public JsonData(JsonElement? element)
        {
            _readOnly = true;
            Element = element;
        }

        /// <summary>
        /// Sets readonly state explicitly, with automatic promotion:
        /// null nodes and <see cref="JsonValue"/> leaf nodes are always readonly regardless of <paramref name="readOnly"/>.
        /// </summary>
        internal JsonData(JsonNode? node, bool readOnly)
        {
            Node = node;
            _readOnly = readOnly || node is null || node is JsonValue;
        }

        /// <summary>The underlying <see cref="JsonNode"/>, if this instance was created from one. Null for Element-backed or uninitialized instances.</summary>
        public readonly JsonNode? Node { get; }

        /// <summary>The underlying <see cref="JsonElement"/>, if this instance was created from one.</summary>
        public readonly JsonElement? Element { get; }

        private readonly bool _readOnly;

        /// <summary>
        /// Whether this instance is readonly. Always true for Element-backed and uninitialized instances.
        /// For Node-backed instances, reflects the value passed at construction.
        /// <para>Also true when there is no source at all (default/uninitialized), preventing accidental mutation.</para>
        /// </summary>
        public readonly bool ReadOnly => _readOnly;


        /// <summary>
        /// The JSON value kind of the underlying data.
        /// Returns <see cref="JsonValueKind.Null"/> for uninitialized instances (no source).
        /// </summary>
        public JsonValueKind ValueKind => Element.HasValue ? Element.Value.ValueKind : Node?.GetValueKind() ?? JsonValueKind.Null;


        /// <summary>Gets or sets a child item by array index. Set requires a writable Node-backed instance.</summary>
        public JsonData? this[int index]
        {
            get => this.Get(index);
            set => this.Set(index, value);
        }

        /// <summary>Gets or sets a child property by name. Set requires a writable Node-backed instance.</summary>
        public JsonData? this[string key]
        {
            get => this.Get(key);
            set => this.Set(key, value);
        }


        /// <summary>True if this instance is backed by a <see cref="JsonElement"/>.</summary>
        [MemberNotNullWhen(true, nameof(Element))]
        public bool IsElement => Element.HasValue;

        /// <summary>True if this instance is backed by a <see cref="JsonNode"/>.</summary>
        [MemberNotNullWhen(true, nameof(Node))]
        public bool IsNode => Node is not null;

        /// <summary>Compares the JSON content of two instances for structural equality, regardless of source type.</summary>
        public bool DeepEquals(in JsonData other)=> JsonDataEquality.Equals(this, other);

        /// <summary>Implicitly wraps a <see cref="JsonNode"/> — enables direct assignment without explicit construction.</summary>
        public static implicit operator JsonData(JsonNode? node) => new JsonData(node);
        /// <summary>Implicitly wraps a <see cref="JsonElement"/>.</summary>
        public static implicit operator JsonData(JsonElement element) => new JsonData(element);
        /// <summary>Implicitly wraps a nullable <see cref="JsonElement"/>.</summary>
        public static implicit operator JsonData(JsonElement? element) => new JsonData(element);

        /// <summary>Extracts the underlying <see cref="JsonNode"/>. Returns null if Element-backed or uninitialized.</summary>
        public static explicit operator JsonNode?(JsonData source) => source.Node;
        /// <summary>Extracts the underlying <see cref="JsonElement"/>. Throws if this instance is not Element-backed.</summary>
        public static explicit operator JsonElement(JsonData source) => source.Element ?? throw new InvalidOperationException("Cannot convert to JsonElement because the JNode does not contain an element.");
        /// <summary>Extracts the underlying <see cref="JsonElement"/> as nullable.</summary>
        public static explicit operator JsonElement?(JsonData source) => source.Element;     

        private string DebugDisplay =>
          $"[{(IsNode ? "Node" : IsElement ? "Element" : "Empty")}]" +
          $"[{(ReadOnly ? "RO" : "RW")}] " +
          ValueKind.ToString();

    }
  
}
