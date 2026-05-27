using System.Text.Json;
using System.Text.Json.Nodes;
using DigitalBusiness.JsonDataWrappers;
using DigitalBusiness.JsonDataWrappers.Converters;
using Xunit;

namespace DigitalBusiness.JsonDataWrappers.Tests;

public class TypedJsonDataJsonConverterTests
{
    // Minimal IJsonDataKey implementation for testing
    private sealed class TestKey : IJsonDataKey { }

    private static JsonSerializerOptions CreateOptions()
    {
        var options = new JsonSerializerOptions();
        options.Converters.Add(new TypedJsonDataJsonConverter<TestKey>());
        return options;
    }

    // ── Read ──────────────────────────────────────────────────────────────────

    [Fact]
    public void Read_SimpleObject_ReturnsJsonDataWithElementBacked()
    {
        // Arrange
        const string json = """{"foo":"bar"}""";
        var options = CreateOptions();

        // Act
        var result = JsonSerializer.Deserialize<JsonData<TestKey>>(json, options);

        // Assert
        Assert.True(result.Json.IsElement);
        Assert.False(result.Json.IsNode);
    }

    [Fact]
    public void Read_SimpleObject_PreservesValues()
    {
        // Arrange
        const string json = """{"name":"Alice","age":30}""";
        var options = CreateOptions();

        // Act
        var result = JsonSerializer.Deserialize<JsonData<TestKey>>(json, options);

        // Assert
        Assert.Equal(JsonValueKind.Object, result.Json.Element!.Value.ValueKind);
        Assert.Equal("Alice", result.Json.Element.Value.GetProperty("name").GetString());
        Assert.Equal(30, result.Json.Element.Value.GetProperty("age").GetInt32());
    }

    [Fact]
    public void Read_Array_ReturnsJsonDataArray()
    {
        // Arrange
        const string json = "[1,2,3]";
        var options = CreateOptions();

        // Act
        var result = JsonSerializer.Deserialize<JsonData<TestKey>>(json, options);

        // Assert
        Assert.True(result.Json.IsElement);
        Assert.Equal(JsonValueKind.Array, result.Json.Element!.Value.ValueKind);
    }

    [Fact]
    public void Read_NullJson_ReturnsJsonDataWithNullElement()
    {
        // Arrange
        const string json = "null";
        var options = CreateOptions();

        // Act
        var result = JsonSerializer.Deserialize<JsonData<TestKey>>(json, options);

        // Assert
        Assert.True(result.Json.IsElement);
        Assert.Equal(JsonValueKind.Null, result.Json.Element!.Value.ValueKind);
    }

    [Fact]
    public void Read_StringValue_ReturnsJsonDataWithStringElement()
    {
        // Arrange
        const string json = "\"hello\"";
        var options = CreateOptions();

        // Act
        var result = JsonSerializer.Deserialize<JsonData<TestKey>>(json, options);

        // Assert
        Assert.True(result.Json.IsElement);
        Assert.Equal(JsonValueKind.String, result.Json.Element!.Value.ValueKind);
    }

    // ── Write ─────────────────────────────────────────────────────────────────

    [Fact]
    public void Write_ElementBacked_RoundTripsCorrectly()
    {
        // Arrange
        const string original = """{"x":1}""";
        var options = CreateOptions();
        var deserialized = JsonSerializer.Deserialize<JsonData<TestKey>>(original, options);

        // Act
        var serialized = JsonSerializer.Serialize(deserialized, options);

        // Assert
        Assert.Equal(original, serialized);
    }

    [Fact]
    public void Write_NodeBacked_SerializesNode()
    {
        // Arrange
        var node = JsonNode.Parse("""{"key":"value"}""")!;
        var jsonData = new JsonData<TestKey>(new JsonData(node));
        var options = CreateOptions();

        // Act
        var result = JsonSerializer.Serialize(jsonData, options);

        // Assert
        using var doc = JsonDocument.Parse(result);
        Assert.Equal("value", doc.RootElement.GetProperty("key").GetString());
    }

    [Fact]
    public void Write_Uninitialized_WritesNullValue()
    {
        // Arrange
        var jsonData = new JsonData<TestKey>(new JsonData());
        var options = CreateOptions();

        // Act
        var result = JsonSerializer.Serialize(jsonData, options);

        // Assert
        Assert.Equal("null", result);
    }

    [Fact]
    public void Write_NodeBacked_Array_SerializesArray()
    {
        // Arrange
        var node = JsonNode.Parse("[10,20,30]")!;
        var jsonData = new JsonData<TestKey>(new JsonData(node));
        var options = CreateOptions();

        // Act
        var result = JsonSerializer.Serialize(jsonData, options);

        // Assert
        Assert.Equal("[10,20,30]", result);
    }

    [Fact]
    public void Write_ElementBacked_NullElement_WritesNullValue()
    {
        // Arrange
        var element = JsonDocument.Parse("null").RootElement;
        var jsonData = new JsonData<TestKey>(new JsonData(element));
        var options = CreateOptions();

        // Act
        var result = JsonSerializer.Serialize(jsonData, options);

        // Assert
        Assert.Equal("null", result);
    }
}
