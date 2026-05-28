using System.Text.Json;
using System.Text.Json.Nodes;
using DigitalBusiness.JsonDataWrappers;
using DigitalBusiness.JsonDataWrappers.Converters;

namespace DigitalBusiness.JsonDataWrappers.Tests;

public class JsonDataConverterProviderJsonConvertersTests
{
    // --------------- JsonElementConverter.TryGet ---------------

    [Fact]
    public void JsonElementConverter_TryGet_ThrowsNotImplementedException()
    {
        var converter = JsonDataConverterProvider.GetConverter<JsonElement>();
        var jsonData = new JsonData(JsonValue.Create(1));

        Assert.Throws<NotImplementedException>(() =>
        {
            converter.TryGet(in jsonData, out _);
        });
    }

    // --------------- JsonElementConverter.Create ---------------

    [Fact]
    public void JsonElementConverter_Create_ThrowsNotImplementedException()
    {
        var converter = JsonDataConverterProvider.GetConverter<JsonElement>();
        var element = JsonDocument.Parse("1").RootElement;

        Assert.Throws<NotImplementedException>(() => converter.Create(element));
    }

    // --------------- JsonDocumentConverter.TryGet ---------------

    [Fact]
    public void JsonDocumentConverter_TryGet_ThrowsNotImplementedException()
    {
        var converter = JsonDataConverterProvider.GetConverter<JsonDocument>();
        var jsonData = new JsonData(JsonValue.Create(1));

        Assert.Throws<NotImplementedException>(() =>
        {
            converter.TryGet(in jsonData, out _);
        });
    }

    // --------------- JsonDocumentConverter.Create ---------------

    [Fact]
    public void JsonDocumentConverter_Create_ThrowsNotImplementedException()
    {
        var converter = JsonDataConverterProvider.GetConverter<JsonDocument>();
        using var doc = JsonDocument.Parse("{}");

        Assert.Throws<NotImplementedException>(() => converter.Create(doc));
    }

    // --------------- JsonNodeConverter.TryGet ---------------

    [Fact]
    public void JsonNodeConverter_TryGet_ThrowsNotImplementedException()
    {
        var converter = JsonDataConverterProvider.GetConverter<JsonNode>();
        var jsonData = new JsonData(JsonValue.Create(1));

        Assert.Throws<NotImplementedException>(() =>
        {
            converter.TryGet(in jsonData, out _);
        });
    }

    // --------------- JsonNodeConverter.Create ---------------

    [Fact]
    public void JsonNodeConverter_Create_ThrowsNotImplementedException()
    {
        var converter = JsonDataConverterProvider.GetConverter<JsonNode>();
        JsonNode node = JsonValue.Create(42)!;

        Assert.Throws<NotImplementedException>(() => converter.Create(node));
    }

    // --------------- JsonObjectConverter.TryGet ---------------

    [Fact]
    public void JsonObjectConverter_TryGet_ThrowsNotImplementedException()
    {
        var converter = JsonDataConverterProvider.GetConverter<JsonObject>();
        var jsonData = new JsonData(JsonValue.Create(1));

        Assert.Throws<NotImplementedException>(() =>
        {
            converter.TryGet(in jsonData, out _);
        });
    }

    // --------------- JsonObjectConverter.Create ---------------

    [Fact]
    public void JsonObjectConverter_Create_ThrowsNotImplementedException()
    {
        var converter = JsonDataConverterProvider.GetConverter<JsonObject>();
        var obj = new JsonObject();

        Assert.Throws<NotImplementedException>(() => converter.Create(obj));
    }

    // --------------- JsonArrayConverter.TryGet ---------------

    [Fact]
    public void JsonArrayConverter_TryGet_ThrowsNotImplementedException()
    {
        var converter = JsonDataConverterProvider.GetConverter<JsonArray>();
        var jsonData = new JsonData(JsonValue.Create(1));

        Assert.Throws<NotImplementedException>(() =>
        {
            converter.TryGet(in jsonData, out _);
        });
    }

    // --------------- JsonArrayConverter.Create ---------------

    [Fact]
    public void JsonArrayConverter_Create_ThrowsNotImplementedException()
    {
        var converter = JsonDataConverterProvider.GetConverter<JsonArray>();
        var arr = new JsonArray();

        Assert.Throws<NotImplementedException>(() => converter.Create(arr));
    }

    // --------------- JsonValueConverter.TryGet ---------------

    [Fact]
    public void JsonValueConverter_TryGet_ThrowsNotImplementedException()
    {
        var converter = JsonDataConverterProvider.GetConverter<JsonValue>();
        var jsonData = new JsonData(JsonValue.Create(1));

        Assert.Throws<NotImplementedException>(() =>
        {
            converter.TryGet(in jsonData, out _);
        });
    }

    // --------------- JsonValueConverter.Create ---------------

    [Fact]
    public void JsonValueConverter_Create_ThrowsNotImplementedException()
    {
        var converter = JsonDataConverterProvider.GetConverter<JsonValue>();
        JsonValue value = JsonValue.Create(42)!;

        Assert.Throws<NotImplementedException>(() => converter.Create(value));
    }
}
