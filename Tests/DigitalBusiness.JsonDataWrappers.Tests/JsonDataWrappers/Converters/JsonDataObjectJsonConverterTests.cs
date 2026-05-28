using System;
using System.Text.Json;
using System.Text.Json.Nodes;
using DigitalBusiness.JsonDataWrappers;
using DigitalBusiness.JsonDataWrappers.Converters;

namespace DigitalBusiness.JsonDataWrappers.Tests;

public class JsonDataObjectJsonConverterTests
{
    // Concrete non-abstract JsonDataObject for testing
    private class ConcreteJsonDataObject : JsonDataObject
    {
    }

    // A second concrete type
    private class AnotherJsonDataObject : JsonDataObject
    {
    }

    private static JsonSerializerOptions CreateOptions()
    {
        var options = new JsonSerializerOptions();
        options.Converters.Add(new JsonDataWrapperJsonConverterFactory());
        return options;
    }

    private readonly JsonDataWrapperJsonConverterFactory _factory = new();

    // --------------- CanConvert ---------------

    [Fact]
    public void CanConvert_ConcreteJsonDataObject_ReturnsTrue()
    {
        var result = _factory.CanConvert(typeof(ConcreteJsonDataObject));

        Assert.True(result);
    }

    [Fact]
    public void CanConvert_JsonDataObjectAbstractBase_ReturnsFalse()
    {
        // JsonDataObject itself is abstract
        var result = _factory.CanConvert(typeof(JsonDataObject));

        Assert.False(result);
    }

    [Fact]
    public void CanConvert_UnrelatedType_ReturnsFalse()
    {
        var result = _factory.CanConvert(typeof(string));

        Assert.False(result);
    }

    [Fact]
    public void CanConvert_IntType_ReturnsFalse()
    {
        var result = _factory.CanConvert(typeof(int));

        Assert.False(result);
    }

    [Fact]
    public void CanConvert_AnotherConcreteJsonDataObject_ReturnsTrue()
    {
        var result = _factory.CanConvert(typeof(AnotherJsonDataObject));

        Assert.True(result);
    }

    // --------------- CreateConverter ---------------

    [Fact]
    public void CreateConverter_ConcreteJsonDataObject_ReturnsNonNull()
    {
        var converter = _factory.CreateConverter(typeof(ConcreteJsonDataObject), new JsonSerializerOptions());

        Assert.NotNull(converter);
    }

    [Fact]
    public void CreateConverter_ConcreteJsonDataObject_ReturnsCorrectConverterType()
    {
        var converter = _factory.CreateConverter(typeof(ConcreteJsonDataObject), new JsonSerializerOptions());

        Assert.IsType<JsonDataWrapperJsonConverter<ConcreteJsonDataObject>>(converter);
    }

    [Fact]
    public void CreateConverter_DifferentConcreteType_ReturnsConverterForThatType()
    {
        var converter = _factory.CreateConverter(typeof(AnotherJsonDataObject), new JsonSerializerOptions());

        Assert.IsType<JsonDataWrapperJsonConverter<AnotherJsonDataObject>>(converter);
    }

    // --------------- Read ---------------

    [Fact]
    public void Read_ValidJsonObject_ReturnsNonNull()
    {
        var options = CreateOptions();

        var result = JsonSerializer.Deserialize<ConcreteJsonDataObject>("""{"key":"value"}""", options);

        Assert.NotNull(result);
    }

    [Fact]
    public void Read_ValidJsonObject_WrapperJsonContainsExpectedData()
    {
        var options = CreateOptions();

        var result = JsonSerializer.Deserialize<ConcreteJsonDataObject>("""{"key":"value"}""", options)!;
        var jsonData = ((IJsonDataWrapper)result).Json;

        Assert.Equal("value", jsonData["key"]!.Value.Get<string>());
    }

    [Fact]
    public void Read_JsonWithMultipleProperties_AllPropertiesAccessible()
    {
        var options = CreateOptions();

        var result = JsonSerializer.Deserialize<ConcreteJsonDataObject>("""{"a":1,"b":"hello"}""", options)!;
        var jsonData = ((IJsonDataWrapper)result).Json;

        Assert.Equal(1, jsonData["a"]!.Value.Get<int>());
        Assert.Equal("hello", jsonData["b"]!.Value.Get<string>());
    }

    // --------------- Write ---------------

    [Fact]
    public void Write_RoundTrip_ProducesOriginalJson()
    {
        var options = CreateOptions();
        var inputJson = """{"key":"value"}""";

        var wrapper = JsonSerializer.Deserialize<ConcreteJsonDataObject>(inputJson, options)!;
        var outputJson = JsonSerializer.Serialize(wrapper, options);

        using var doc = JsonDocument.Parse(outputJson);
        Assert.Equal("value", doc.RootElement.GetProperty("key").GetString());
    }

    [Fact]
    public void Write_RoundTrip_NumericValue_RoundTripsCorrectly()
    {
        var options = CreateOptions();
        var inputJson = """{"x":42}""";

        var wrapper = JsonSerializer.Deserialize<ConcreteJsonDataObject>(inputJson, options)!;
        var outputJson = JsonSerializer.Serialize(wrapper, options);

        using var doc = JsonDocument.Parse(outputJson);
        Assert.Equal(42, doc.RootElement.GetProperty("x").GetInt32());
    }

    [Fact]
    public void Write_NullWrapper_SerializesNull()
    {
        var options = CreateOptions();

        var wrapper = JsonSerializer.Deserialize<ConcreteJsonDataObject>("null", options)!;
        var outputJson = JsonSerializer.Serialize(wrapper, options);

        Assert.Equal("null", outputJson);
    }

    [Fact]
    public void Write_MultipleProperties_AllSerializedCorrectly()
    {
        var options = CreateOptions();
        var inputJson = """{"a":1,"b":"hello","c":true}""";

        var wrapper = JsonSerializer.Deserialize<ConcreteJsonDataObject>(inputJson, options)!;
        var outputJson = JsonSerializer.Serialize(wrapper, options);

        using var doc = JsonDocument.Parse(outputJson);
        Assert.Equal(1, doc.RootElement.GetProperty("a").GetInt32());
        Assert.Equal("hello", doc.RootElement.GetProperty("b").GetString());
        Assert.True(doc.RootElement.GetProperty("c").GetBoolean());
    }
}
