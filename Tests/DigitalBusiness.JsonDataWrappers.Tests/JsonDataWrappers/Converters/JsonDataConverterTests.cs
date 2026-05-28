using System;
using System.Text.Json;
using System.Text.Json.Nodes;
using DigitalBusiness.JsonDataWrappers;
using DigitalBusiness.JsonDataWrappers.Converters;

namespace DigitalBusiness.JsonDataWrappers.Tests;

public class JsonDataConverterTests
{
    // --------------- Get<T> ---------------

    [Fact]
    public void Get_ValidIntJsonData_ReturnsInt()
    {
        var jsonData = new JsonData(JsonValue.Create(42));

        var result = JsonDataConverter<int>.Get(jsonData);

        Assert.Equal(42, result);
    }

    [Fact]
    public void Get_ValidStringJsonData_ReturnsString()
    {
        var jsonData = new JsonData(JsonValue.Create("hello"));

        var result = JsonDataConverter<string>.Get(jsonData);

        Assert.Equal("hello", result);
    }

    [Fact]
    public void Get_InvalidJsonData_ThrowsInvalidOperationException()
    {
        // Empty JsonData has no element/node, so int converter returns false
        var jsonData = new JsonData((JsonNode?)null);

        Assert.Throws<InvalidOperationException>(() => JsonDataConverter<int>.Get(jsonData));
    }

    [Fact]
    public void Get_InvalidJsonData_ExceptionMessageContainsTypeName()
    {
        var jsonData = new JsonData((JsonNode?)null);

        var ex = Assert.Throws<InvalidOperationException>(() => JsonDataConverter<int>.Get(jsonData));
        Assert.Contains(typeof(int).FullName!, ex.Message);
    }

    [Fact]
    public void Get_JsonDataFromElement_ReturnsInt()
    {
        var element = JsonSerializer.Deserialize<JsonElement>("99");
        var jsonData = new JsonData(element);

        var result = JsonDataConverter<int>.Get(jsonData);

        Assert.Equal(99, result);
    }

    // --------------- TryGet<T> (no out) ---------------

    [Fact]
    public void TryGetNullable_ValidIntJsonData_ReturnsInt()
    {
        var jsonData = new JsonData(JsonValue.Create(7));

        var result = JsonDataConverter<int>.TryGet(jsonData);

        Assert.Equal(7, result);
    }

    [Fact]
    public void TryGetNullable_ValidStringJsonData_ReturnsString()
    {
        var jsonData = new JsonData(JsonValue.Create("world"));

        var result = JsonDataConverter<string>.TryGet(jsonData);

        Assert.Equal("world", result);
    }

    [Fact]
    public void TryGetNullable_InvalidJsonData_ReturnsDefault()
    {
        var jsonData = new JsonData((JsonNode?)null);

        var result = JsonDataConverter<int>.TryGet(jsonData);

        Assert.Equal(default, result);
    }

    [Fact]
    public void TryGetNullable_InvalidStringJsonData_ReturnsNull()
    {
        var jsonData = new JsonData((JsonNode?)null);

        var result = JsonDataConverter<string>.TryGet(jsonData);

        Assert.Null(result);
    }

    // --------------- TryGet<T> (with out) ---------------

    [Fact]
    public void TryGetOut_ValidIntJsonData_ReturnsTrueAndValue()
    {
        var jsonData = new JsonData(JsonValue.Create(55));

        var success = JsonDataConverter<int>.TryGet(jsonData, out var value);

        Assert.True(success);
        Assert.Equal(55, value);
    }

    [Fact]
    public void TryGetOut_ValidStringJsonData_ReturnsTrueAndValue()
    {
        var jsonData = new JsonData(JsonValue.Create("test"));

        var success = JsonDataConverter<string>.TryGet(jsonData, out var value);

        Assert.True(success);
        Assert.Equal("test", value);
    }

    [Fact]
    public void TryGetOut_InvalidJsonData_ReturnsFalse()
    {
        var jsonData = new JsonData((JsonNode?)null);

        var success = JsonDataConverter<int>.TryGet(jsonData, out var value);

        Assert.False(success);
    }

    [Fact]
    public void TryGetOut_JsonDataFromElement_ReturnsTrueAndValue()
    {
        var element = JsonSerializer.Deserialize<JsonElement>("123");
        var jsonData = new JsonData(element);

        var success = JsonDataConverter<int>.TryGet(jsonData, out var value);

        Assert.True(success);
        Assert.Equal(123, value);
    }

    // --------------- Create<T> ---------------

    [Fact]
    public void Create_IntValue_ReturnsJsonDataWithInt()
    {
        var jsonData = JsonDataConverter<int>.Create(42);

        Assert.True(JsonDataConverter<int>.TryGet(jsonData, out var value));
        Assert.Equal(42, value);
    }

    [Fact]
    public void Create_StringValue_ReturnsJsonDataWithString()
    {
        var jsonData = JsonDataConverter<string>.Create("hello");

        Assert.True(JsonDataConverter<string>.TryGet(jsonData, out var value));
        Assert.Equal("hello", value);
    }

    [Fact]
    public void Create_ZeroInt_ReturnsJsonDataWithZero()
    {
        var jsonData = JsonDataConverter<int>.Create(0);

        var result = JsonDataConverter<int>.Get(jsonData);
        Assert.Equal(0, result);
    }

    [Fact]
    public void Create_NegativeInt_ReturnsJsonDataWithNegativeValue()
    {
        var jsonData = JsonDataConverter<int>.Create(-1);

        var result = JsonDataConverter<int>.Get(jsonData);
        Assert.Equal(-1, result);
    }

    [Fact]
    public void Create_JsonData_RoundTrips()
    {
        var inner = new JsonData(JsonValue.Create(100));
        var created = JsonDataConverter<JsonData>.Create(inner);

        var result = JsonDataConverter<JsonData>.Get(created);
        Assert.True(JsonDataConverter<int>.TryGet(result, out var intVal));
        Assert.Equal(100, intVal);
    }
}
