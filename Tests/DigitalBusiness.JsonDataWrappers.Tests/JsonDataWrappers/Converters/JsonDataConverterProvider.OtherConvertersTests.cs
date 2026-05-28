using System.Text.Json;
using System.Text.Json.Nodes;
using DigitalBusiness.Json;
using DigitalBusiness.JsonDataWrappers;
using DigitalBusiness.JsonDataWrappers.Converters;
using Xunit;

namespace DigitalBusiness.JsonDataWrappers.Tests;

public class JsonDataConverterProviderOtherConvertersTests
{
    // -----------------------------------------------------------------------
    // Test enums
    // EnumJsonPersistanceAttribute ? PersistAsNumber returns true ? JsonDataEnumStringConverter
    // No attribute ? PersistAsNumber returns false ? JsonDataEnumNumberConverter
    // -----------------------------------------------------------------------

    [EnumJsonPersistance]
    public enum StringPersistedColor { Red = 0, Green = 1, Blue = 2 }

    public enum NumberPersistedColor { Red = 0, Green = 1, Blue = 2 }

    // -----------------------------------------------------------------------
    // Helpers
    // -----------------------------------------------------------------------

    private static JsonData FromElement(string json)
    {
        using var doc = JsonDocument.Parse(json);
        return new JsonData(doc.RootElement.Clone());
    }

    private static JsonData FromNode(JsonNode? node) => new JsonData(node);

    private static JsonData NullData() => new JsonData();

    // -----------------------------------------------------------------------
    // JsonDataEnumStringConverter.Create
    // -----------------------------------------------------------------------

    [Fact]
    public void EnumStringConverter_Create_ReturnsJsonDataWithStringValue()
    {
        var converter = JsonDataConverterProvider.GetConverter<StringPersistedColor>();

        var result = converter.Create(StringPersistedColor.Green);

        Assert.True(result.IsNode);
        Assert.Equal(JsonValueKind.String, result.ValueKind);
        Assert.Equal("Green", result.Node!.GetValue<string>());
    }

    [Fact]
    public void EnumStringConverter_Create_FirstEnumValue_ReturnsStringRepresentation()
    {
        var converter = JsonDataConverterProvider.GetConverter<StringPersistedColor>();

        var result = converter.Create(StringPersistedColor.Red);

        Assert.Equal("Red", result.Node!.GetValue<string>());
    }

    // -----------------------------------------------------------------------
    // JsonDataEnumStringConverter.TryGet — Element-backed
    // -----------------------------------------------------------------------

    [Fact]
    public void EnumStringConverter_TryGet_ElementStringValid_ReturnsTrueAndValue()
    {
        var converter = JsonDataConverterProvider.GetConverter<StringPersistedColor>();
        var jsonData = FromElement("\"Green\"");

        var result = converter.TryGet(in jsonData, out var value);

        Assert.True(result);
        Assert.Equal(StringPersistedColor.Green, value);
    }

    [Fact]
    public void EnumStringConverter_TryGet_ElementStringEmpty_ReturnsFalse()
    {
        var converter = JsonDataConverterProvider.GetConverter<StringPersistedColor>();
        var jsonData = FromElement("\"\"");

        var result = converter.TryGet(in jsonData, out var value);

        Assert.False(result);
    }

    [Fact]
    public void EnumStringConverter_TryGet_ElementStringInvalid_ReturnsFalse()
    {
        var converter = JsonDataConverterProvider.GetConverter<StringPersistedColor>();
        var jsonData = FromElement("\"Purple\"");

        var result = converter.TryGet(in jsonData, out var value);

        Assert.False(result);
    }

    [Fact]
    public void EnumStringConverter_TryGet_ElementNumber_ReturnsTrueWithCastedValue()
    {
        var converter = JsonDataConverterProvider.GetConverter<StringPersistedColor>();
        var jsonData = FromElement("1");

        var result = converter.TryGet(in jsonData, out var value);

        Assert.True(result);
        Assert.Equal(StringPersistedColor.Green, value);
    }

    [Fact]
    public void EnumStringConverter_TryGet_ElementNull_ReturnsFalse()
    {
        var converter = JsonDataConverterProvider.GetConverter<StringPersistedColor>();
        var jsonData = NullData();

        var result = converter.TryGet(in jsonData, out var value);

        Assert.False(result);
    }

    [Fact]
    public void EnumStringConverter_TryGet_ElementBool_ReturnsFalse()
    {
        var converter = JsonDataConverterProvider.GetConverter<StringPersistedColor>();
        var jsonData = FromElement("true");

        var result = converter.TryGet(in jsonData, out var value);

        Assert.False(result);
    }

    // -----------------------------------------------------------------------
    // JsonDataEnumStringConverter.TryGet — Node-backed
    // -----------------------------------------------------------------------

    [Fact]
    public void EnumStringConverter_TryGet_NodeStringValid_ReturnsTrueAndValue()
    {
        var converter = JsonDataConverterProvider.GetConverter<StringPersistedColor>();
        var jsonData = FromNode(JsonValue.Create("Blue"));

        var result = converter.TryGet(in jsonData, out var value);

        Assert.True(result);
        Assert.Equal(StringPersistedColor.Blue, value);
    }

    [Fact]
    public void EnumStringConverter_TryGet_NodeStringEmpty_ReturnsFalse()
    {
        var converter = JsonDataConverterProvider.GetConverter<StringPersistedColor>();
        var jsonData = FromNode(JsonValue.Create(string.Empty));

        var result = converter.TryGet(in jsonData, out var value);

        Assert.False(result);
    }

    [Fact]
    public void EnumStringConverter_TryGet_NodeStringInvalid_ReturnsFalse()
    {
        var converter = JsonDataConverterProvider.GetConverter<StringPersistedColor>();
        var jsonData = FromNode(JsonValue.Create("NotAColor"));

        var result = converter.TryGet(in jsonData, out var value);

        Assert.False(result);
    }

    [Fact]
    public void EnumStringConverter_TryGet_NodeNumber_ReturnsTrueWithCastedValue()
    {
        var converter = JsonDataConverterProvider.GetConverter<StringPersistedColor>();
        var jsonData = FromNode(JsonValue.Create(2L));

        var result = converter.TryGet(in jsonData, out var value);

        Assert.True(result);
        Assert.Equal(StringPersistedColor.Blue, value);
    }

    [Fact]
    public void EnumStringConverter_TryGet_NodeNull_ReturnsFalse()
    {
        var converter = JsonDataConverterProvider.GetConverter<StringPersistedColor>();
        var jsonData = FromNode(null);

        var result = converter.TryGet(in jsonData, out var value);

        Assert.False(result);
    }

    // -----------------------------------------------------------------------
    // JsonDataEnumNumberConverter.Create
    // -----------------------------------------------------------------------

    [Fact]
    public void EnumNumberConverter_Create_ReturnsJsonDataWithNumericValue()
    {
        var converter = JsonDataConverterProvider.GetConverter<NumberPersistedColor>();

        var result = converter.Create(NumberPersistedColor.Green);

        Assert.True(result.IsNode);
        Assert.Equal(JsonValueKind.Number, result.ValueKind);
        Assert.Equal(1L, result.Node!.GetValue<long>());
    }

    [Fact]
    public void EnumNumberConverter_Create_FirstValue_ReturnsZero()
    {
        var converter = JsonDataConverterProvider.GetConverter<NumberPersistedColor>();

        var result = converter.Create(NumberPersistedColor.Red);

        Assert.Equal(0L, result.Node!.GetValue<long>());
    }

    // -----------------------------------------------------------------------
    // JsonDataEnumNumberConverter.TryGet — Element-backed
    // -----------------------------------------------------------------------

    [Fact]
    public void EnumNumberConverter_TryGet_ElementNumber_ReturnsTrueAndValue()
    {
        var converter = JsonDataConverterProvider.GetConverter<NumberPersistedColor>();
        var jsonData = FromElement("1");

        var result = converter.TryGet(in jsonData, out var value);

        Assert.True(result);
        Assert.Equal(NumberPersistedColor.Green, value);
    }

    [Fact]
    public void EnumNumberConverter_TryGet_ElementStringValid_ReturnsTrueAndValue()
    {
        var converter = JsonDataConverterProvider.GetConverter<NumberPersistedColor>();
        var jsonData = FromElement("\"Blue\"");

        var result = converter.TryGet(in jsonData, out var value);

        Assert.True(result);
        Assert.Equal(NumberPersistedColor.Blue, value);
    }

    [Fact]
    public void EnumNumberConverter_TryGet_ElementStringInvalid_ReturnsFalse()
    {
        var converter = JsonDataConverterProvider.GetConverter<NumberPersistedColor>();
        var jsonData = FromElement("\"Purple\"");

        var result = converter.TryGet(in jsonData, out var value);

        Assert.False(result);
    }

    [Fact]
    public void EnumNumberConverter_TryGet_ElementStringEmpty_ReturnsFalse()
    {
        var converter = JsonDataConverterProvider.GetConverter<NumberPersistedColor>();
        var jsonData = FromElement("\"\"");

        var result = converter.TryGet(in jsonData, out var value);

        Assert.False(result);
    }

    [Fact]
    public void EnumNumberConverter_TryGet_ElementNull_ReturnsFalse()
    {
        var converter = JsonDataConverterProvider.GetConverter<NumberPersistedColor>();
        var jsonData = NullData();

        var result = converter.TryGet(in jsonData, out var value);

        Assert.False(result);
    }

    [Fact]
    public void EnumNumberConverter_TryGet_ElementBool_ReturnsFalse()
    {
        var converter = JsonDataConverterProvider.GetConverter<NumberPersistedColor>();
        var jsonData = FromElement("true");

        var result = converter.TryGet(in jsonData, out var value);

        Assert.False(result);
    }

    // -----------------------------------------------------------------------
    // JsonDataEnumNumberConverter.TryGet — Node-backed
    // -----------------------------------------------------------------------

    [Fact]
    public void EnumNumberConverter_TryGet_NodeNumber_ReturnsTrueAndValue()
    {
        var converter = JsonDataConverterProvider.GetConverter<NumberPersistedColor>();
        var jsonData = FromNode(JsonValue.Create(2L));

        var result = converter.TryGet(in jsonData, out var value);

        Assert.True(result);
        Assert.Equal(NumberPersistedColor.Blue, value);
    }

    [Fact]
    public void EnumNumberConverter_TryGet_NodeStringValid_ReturnsTrueAndValue()
    {
        var converter = JsonDataConverterProvider.GetConverter<NumberPersistedColor>();
        var jsonData = FromNode(JsonValue.Create("Red"));

        var result = converter.TryGet(in jsonData, out var value);

        Assert.True(result);
        Assert.Equal(NumberPersistedColor.Red, value);
    }

    [Fact]
    public void EnumNumberConverter_TryGet_NodeStringInvalid_ReturnsFalse()
    {
        var converter = JsonDataConverterProvider.GetConverter<NumberPersistedColor>();
        var jsonData = FromNode(JsonValue.Create("NoSuchColor"));

        var result = converter.TryGet(in jsonData, out var value);

        Assert.False(result);
    }

    [Fact]
    public void EnumNumberConverter_TryGet_NodeNull_ReturnsFalse()
    {
        var converter = JsonDataConverterProvider.GetConverter<NumberPersistedColor>();
        var jsonData = FromNode(null);

        var result = converter.TryGet(in jsonData, out var value);

        Assert.False(result);
    }

    // -----------------------------------------------------------------------
    // JsonDataSerializationConverter.TryGet — Node-backed
    // -----------------------------------------------------------------------

    private record SimpleRecord(string Name, int Age);

    [Fact]
    public void SerializationConverter_TryGet_NodeWithValidObject_ReturnsTrueAndDeserializedValue()
    {
        var converter = JsonDataConverterProvider.GetConverter<SimpleRecord>();
        var node = JsonNode.Parse("{\"Name\":\"Alice\",\"Age\":30}");
        var jsonData = FromNode(node);

        var result = converter.TryGet(in jsonData, out var value);

        Assert.True(result);
        Assert.Equal("Alice", value?.Name);
        Assert.Equal(30, value?.Age);
    }

    [Fact]
    public void SerializationConverter_TryGet_NodeNull_ReturnsFalse()
    {
        var converter = JsonDataConverterProvider.GetConverter<SimpleRecord>();
        var jsonData = NullData();

        var result = converter.TryGet(in jsonData, out var value);

        Assert.False(result);
    }

    [Fact]
    public void SerializationConverter_TryGet_ElementWithValidObject_ReturnsTrueAndDeserializedValue()
    {
        var converter = JsonDataConverterProvider.GetConverter<SimpleRecord>();
        var jsonData = FromElement("{\"Name\":\"Bob\",\"Age\":25}");

        var result = converter.TryGet(in jsonData, out var value);

        Assert.True(result);
        Assert.Equal("Bob", value?.Name);
        Assert.Equal(25, value?.Age);
    }

    [Fact]
    public void SerializationConverter_TryGet_ElementNull_ReturnsFalse()
    {
        var converter = JsonDataConverterProvider.GetConverter<SimpleRecord>();
        var jsonData = NullData();

        var result = converter.TryGet(in jsonData, out var value);

        Assert.False(result);
    }

    [Fact]
    public void SerializationConverter_TryGet_InvalidJson_ReturnsFalse()
    {
        var converter = JsonDataConverterProvider.GetConverter<SimpleRecord>();
        // A number is not deserializable to SimpleRecord
        var jsonData = FromElement("42");

        var result = converter.TryGet(in jsonData, out var value);

        Assert.False(result);
    }

    [Fact]
    public void SerializationConverter_TryGet_NodeObjectDeserializesToNull_ReturnsFalse()
    {
        var converter = JsonDataConverterProvider.GetConverter<SimpleRecord>();
        var jsonData = FromElement("null");

        var result = converter.TryGet(in jsonData, out var value);

        Assert.False(result);
    }

    // -----------------------------------------------------------------------
    // JsonDataSerializationConverter.Create
    // -----------------------------------------------------------------------

    [Fact]
    public void SerializationConverter_Create_SimpleObject_ReturnsJsonDataWithSerializedNode()
    {
        var converter = JsonDataConverterProvider.GetConverter<SimpleRecord>();
        var input = new SimpleRecord("Alice", 30);

        var result = converter.Create(input);

        Assert.True(result.IsNode);
        Assert.Equal("Alice", result.Node!["Name"]!.GetValue<string>());
        Assert.Equal(30, result.Node!["Age"]!.GetValue<int>());
    }

    [Fact]
    public void SerializationConverter_Create_NullValue_ReturnsJsonDataWithNullNode()
    {
        var converter = JsonDataConverterProvider.GetConverter<SimpleRecord?>();

        var result = converter.Create(null);

        Assert.True(result.IsNull);
    }

}
