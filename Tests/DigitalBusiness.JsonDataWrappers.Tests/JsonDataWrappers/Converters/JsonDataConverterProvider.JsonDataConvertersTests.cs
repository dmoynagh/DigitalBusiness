using System.Text.Json.Nodes;
using DigitalBusiness.JsonDataWrappers;
using DigitalBusiness.JsonDataWrappers.Converters;

namespace DigitalBusiness.JsonDataWrappers.Tests;

public class JsonDataConverterProviderJsonDataConvertersTests
{
    private sealed class TestKey : IJsonDataKey { }

    // --------------- JsonDataConverter.TryGet ---------------

    [Fact]
    public void JsonDataConverter_TryGet_ReturnsInputJsonDataAsValue()
    {
        var jsonData = new JsonData(JsonValue.Create(99));
        var converter = JsonDataConverterProvider.GetConverter<JsonData>();

        var result = converter.TryGet(in jsonData, out var value);

        Assert.True(result);
        Assert.Equal(jsonData, value);
    }

    [Fact]
    public void JsonDataConverter_TryGet_EmptyJsonData_ReturnsTrue()
    {
        var jsonData = new JsonData((JsonNode?)null);
        var converter = JsonDataConverterProvider.GetConverter<JsonData>();

        var result = converter.TryGet(in jsonData, out var value);

        Assert.True(result);
        Assert.Equal(jsonData, value);
    }

    // --------------- JsonDataConverter.Create ---------------

    [Fact]
    public void JsonDataConverter_Create_ReturnsInputValue()
    {
        var jsonData = new JsonData(JsonValue.Create("hello"));
        var converter = JsonDataConverterProvider.GetConverter<JsonData>();

        var result = converter.Create(jsonData);

        Assert.Equal(jsonData, result);
    }

    [Fact]
    public void JsonDataConverter_Create_EmptyJsonData_ReturnsEmptyJsonData()
    {
        var jsonData = new JsonData((JsonNode?)null);
        var converter = JsonDataConverterProvider.GetConverter<JsonData>();

        var result = converter.Create(jsonData);

        Assert.Equal(jsonData, result);
    }

    // --------------- JsonDataWrapperConverter.Create ---------------

    [Fact]
    public void JsonDataWrapperConverter_Create_ReturnsUnderlyingJson()
    {
        var innerJson = new JsonData(JsonValue.Create(42));
        var wrapper = new JsonData<TestKey>(innerJson);
        var converter = JsonDataConverterProvider.GetConverter<JsonData<TestKey>>();

        var result = converter.Create(wrapper);

        Assert.Equal(innerJson, result);
    }

    [Fact]
    public void JsonDataWrapperConverter_Create_EmptyWrapper_ReturnsEmptyJson()
    {
        var wrapper = default(JsonData<TestKey>);
        var converter = JsonDataConverterProvider.GetConverter<JsonData<TestKey>>();

        var result = converter.Create(wrapper);

        Assert.Equal(wrapper.Json, result);
    }

    // --------------- JsonDataWrapperConverter.TryGet ---------------

    [Fact]
    public void JsonDataWrapperConverter_TryGet_ReturnsWrapperWithCorrectJson()
    {
        var jsonData = new JsonData(JsonValue.Create("world"));
        var converter = JsonDataConverterProvider.GetConverter<JsonData<TestKey>>();

        var result = converter.TryGet(in jsonData, out var value);

        Assert.True(result);
        Assert.Equal(jsonData, value.Json);
    }

    [Fact]
    public void JsonDataWrapperConverter_TryGet_EmptyJsonData_ReturnsTrueAndDefaultWrapper()
    {
        var jsonData = new JsonData((JsonNode?)null);
        var converter = JsonDataConverterProvider.GetConverter<JsonData<TestKey>>();

        var result = converter.TryGet(in jsonData, out var value);

        Assert.True(result);
        Assert.Equal(jsonData, value.Json);
    }
}
