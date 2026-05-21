
using DigitalBusiness.JsonDataWrappers;
using DigitalBusiness.JsonDataWrappers.Converters;
using DigitalBusiness.JsonDataWrappers.Internal;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using System.Xml.Linq;


namespace DigitalBusiness.JsonDataWrappers
{

    [JsonConverter(typeof(JsonDataJsonConverter))]
    public readonly partial struct JsonData : IJsonData
    {
        JsonData IJsonData.Json => this;

        public JsonData() {
            ReadOnly = true;
        }

        public JsonData(JsonNode? node)
        {
            Node = node;
            ReadOnly = node is null || node is JsonValue;
        }

        public JsonData(JsonNode? node, bool readOnly)
        {
            Node = node;
            ReadOnly = readOnly || node is null || node is JsonValue;
        }
        public JsonData(JsonElement element)
        {
            ReadOnly = true;
            Element = element;
        }
        public JsonData(JsonElement? element)
        {
            ReadOnly = true;
            Element = element;
        }

        public static JsonData CreateNull() => new JsonData();

        public readonly JsonNode? Node { get; }
        public readonly JsonElement? Element { get; }
        public readonly bool ReadOnly { get; } = true;

        public JsonValueKind ValueKind => Element.HasValue ? Element.Value.ValueKind : Node?.GetValueKind() ?? JsonValueKind.Null;


        public JsonData? this[int index]
        {
            get => this.Get(index);
            set => this.Set(index, value);
        }
        
        public JsonData? this[string key]
        {
            get => this.Get(key);
            set => this.Set(key, value);
        }



        [MemberNotNullWhen(true, nameof(Element))]
        public bool IsElement => Element.HasValue;

        [MemberNotNullWhen(true, nameof(Node))]
        public bool IsNode => Node is not null;

       

        public JsonData Clone()
        {            
            if (IsElement) return new JsonData(Element.Value.Clone());
            if (IsNode) return new JsonData(Node.DeepClone(),ReadOnly);
            return new JsonData();
        }

        public bool DeepEquals(in JsonData other)=> JsonDataEquality.Equals(this, other);
        
        public static implicit operator JsonData(JsonNode? node) => new JsonData(node);
        public static implicit operator JsonData(JsonElement element) => new JsonData(element);
        public static implicit operator JsonData(JsonElement? element) => new JsonData(element);

        public static explicit operator JsonNode?(JsonData source) => source.Node;
        public static explicit operator JsonElement(JsonData source) => source.Element ?? throw new InvalidOperationException("Cannot convert to JsonElement because the JNode does not contain an element.");
        public static explicit operator JsonElement?(JsonData source) => source.Element;     

    }
  
}
