using System.Text.Json;
using System.Text.Json.Nodes;
using DigitalBusiness.JsonDataWrappers;
using DigitalBusiness.JsonDataWrappers.Converters;

namespace DigitalBusiness.JsonDataWrappers.Tests;

public class JsonDataSerializedConverterTests
{
    private static readonly JsonSerializerOptions DefaultOptions = new();

    // --------------- TryGet - Node-backed JsonData ---------------

    [Fact]
    public void TryGet_NodeBackedWithValidObject_ReturnsTrueAndValue()
    {
        var node = JsonSerializer.SerializeToNode(new SampleDto { Name = "Alice", Age = 30 });
        var jsonData = new JsonData(node);

        var result = JsonDataSerializedConverter<SampleDto>.TryGet(jsonData, out var value, DefaultOptions);

        Assert.True(result);
        Assert.NotNull(value);
        Assert.Equal("Alice", value.Name);
        Assert.Equal(30, value.Age);
    }

    [Fact]
    public void TryGet_NodeBackedWithString_ReturnsTrueAndValue()
    {
        var node = JsonValue.Create("hello");
        var jsonData = new JsonData(node);

        var result = JsonDataSerializedConverter<string>.TryGet(jsonData, out var value, DefaultOptions);

        Assert.True(result);
        Assert.Equal("hello", value);
    }

    [Fact]
    public void TryGet_NodeBackedWithInt_ReturnsTrueAndValue()
    {
        var node = JsonValue.Create(42);
        var jsonData = new JsonData(node);

        var result = JsonDataSerializedConverter<int>.TryGet(jsonData, out var value, DefaultOptions);

        Assert.True(result);
        Assert.Equal(42, value);
    }

    [Fact]
    public void TryGet_NodeBackedNullValue_ReturnsFalseAndDefault()
    {
        // A JsonNode that serializes to null (JsonValue.Create<string>(null) produces a null node)
        var jsonData = new JsonData((JsonNode?)null);

        var result = JsonDataSerializedConverter<SampleDto>.TryGet(jsonData, out var value, DefaultOptions);

        Assert.False(result);
        Assert.Null(value);
    }

    // --------------- TryGet - Element-backed JsonData ---------------

    [Fact]
    public void TryGet_ElementBackedWithValidObject_ReturnsTrueAndValue()
    {
        var json = JsonSerializer.Serialize(new SampleDto { Name = "Bob", Age = 25 });
        var element = JsonDocument.Parse(json).RootElement;
        var jsonData = new JsonData(element);

        var result = JsonDataSerializedConverter<SampleDto>.TryGet(jsonData, out var value, DefaultOptions);

        Assert.True(result);
        Assert.NotNull(value);
        Assert.Equal("Bob", value.Name);
        Assert.Equal(25, value.Age);
    }

    [Fact]
    public void TryGet_ElementBackedWithString_ReturnsTrueAndValue()
    {
        var element = JsonDocument.Parse("\"world\"").RootElement;
        var jsonData = new JsonData(element);

        var result = JsonDataSerializedConverter<string>.TryGet(jsonData, out var value, DefaultOptions);

        Assert.True(result);
        Assert.Equal("world", value);
    }

    [Fact]
    public void TryGet_ElementBackedWithInt_ReturnsTrueAndValue()
    {
        var element = JsonDocument.Parse("99").RootElement;
        var jsonData = new JsonData(element);

        var result = JsonDataSerializedConverter<int>.TryGet(jsonData, out var value, DefaultOptions);

        Assert.True(result);
        Assert.Equal(99, value);
    }

    [Fact]
    public void TryGet_ElementBackedNullElement_ReturnsFalseAndDefault()
    {
        var element = JsonDocument.Parse("null").RootElement;
        var jsonData = new JsonData(element);

        var result = JsonDataSerializedConverter<SampleDto>.TryGet(jsonData, out var value, DefaultOptions);

        Assert.False(result);
        Assert.Null(value);
    }

    // --------------- TryGet - Default/empty JsonData ---------------

    [Fact]
    public void TryGet_DefaultJsonData_ReturnsFalseAndDefault()
    {
        var jsonData = default(JsonData);

        var result = JsonDataSerializedConverter<SampleDto>.TryGet(jsonData, out var value, DefaultOptions);

        Assert.False(result);
        Assert.Null(value);
    }

    // --------------- TryGet - Type mismatch (exception swallowed) ---------------

    [Fact]
    public void TryGet_NodeBackedTypeMismatch_ReturnsFalseAndDefault()
    {
        var node = JsonValue.Create("not-an-int");
        var jsonData = new JsonData(node);

        var result = JsonDataSerializedConverter<int>.TryGet(jsonData, out var value, DefaultOptions);

        Assert.False(result);
        Assert.Equal(default, value);
    }

    [Fact]
    public void TryGet_ElementBackedTypeMismatch_ReturnsFalseAndDefault()
    {
        var element = JsonDocument.Parse("\"not-an-int\"").RootElement;
        var jsonData = new JsonData(element);

        var result = JsonDataSerializedConverter<int>.TryGet(jsonData, out var value, DefaultOptions);

        Assert.False(result);
        Assert.Equal(default, value);
    }

    // --------------- Create ---------------

    [Fact]
    public void Create_WithObject_ReturnsJsonDataWithNode()
    {
        var dto = new SampleDto { Name = "Charlie", Age = 40 };

        var jsonData = JsonDataSerializedConverter<SampleDto>.Create(dto, DefaultOptions);

        Assert.False(jsonData.IsNull);
        var roundTripped = JsonDataSerializedConverter<SampleDto>.TryGet(jsonData, out var value, DefaultOptions);
        Assert.True(roundTripped);
        Assert.NotNull(value);
        Assert.Equal("Charlie", value.Name);
        Assert.Equal(40, value.Age);
    }

    [Fact]
    public void Create_WithString_ReturnsJsonDataWithNode()
    {
        var jsonData = JsonDataSerializedConverter<string>.Create("test", DefaultOptions);

        Assert.False(jsonData.IsNull);
        var ok = JsonDataSerializedConverter<string>.TryGet(jsonData, out var value, DefaultOptions);
        Assert.True(ok);
        Assert.Equal("test", value);
    }

    [Fact]
    public void Create_WithInt_ReturnsJsonDataWithNode()
    {
        var jsonData = JsonDataSerializedConverter<int>.Create(7, DefaultOptions);

        Assert.False(jsonData.IsNull);
        var ok = JsonDataSerializedConverter<int>.TryGet(jsonData, out var value, DefaultOptions);
        Assert.True(ok);
        Assert.Equal(7, value);
    }

    [Fact]
    public void Create_WithNull_ReturnsNullJsonData()
    {
        var jsonData = JsonDataSerializedConverter<SampleDto?>.Create(null, DefaultOptions);

        Assert.True(jsonData.IsNull);
    }

    // --------------- Helpers ---------------

    private sealed class SampleDto
    {
        public string? Name { get; set; }
        public int Age { get; set; }
    }
}
