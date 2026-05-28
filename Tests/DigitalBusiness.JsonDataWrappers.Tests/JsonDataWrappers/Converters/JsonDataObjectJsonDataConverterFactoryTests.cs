using System;
using System.Text.Json.Nodes;
using DigitalBusiness.JsonDataWrappers;
using DigitalBusiness.JsonDataWrappers.Converters;
using Xunit;

namespace DigitalBusiness.JsonDataWrappers.Tests;

public class JsonDataObjectJsonDataConverterFactoryTests
{
    // A concrete IJsonDataObject for testing
    private sealed class TestJsonDataObject : IJsonDataObject
    {
        public JsonData Json { get; init; }
    }

    // An abstract IJsonDataObject — should NOT be convertible
    private abstract class AbstractJsonDataObject : IJsonDataObject
    {
        public JsonData Json { get; init; }
    }

    // A plain class that does NOT implement IJsonDataObject
    private sealed class NotAJsonDataObject { }

    private readonly JsonDataObjectJsonDataConverterFactory _factory = new();

    #region CanConvert

    [Fact]
    public void CanConvert_ConcreteIJsonDataObject_ReturnsTrue()
    {
        var result = _factory.CanConvert(typeof(TestJsonDataObject));
        Assert.True(result);
    }

    [Fact]
    public void CanConvert_AbstractIJsonDataObject_ReturnsFalse()
    {
        var result = _factory.CanConvert(typeof(AbstractJsonDataObject));
        Assert.False(result);
    }

    [Fact]
    public void CanConvert_InterfaceIJsonDataObject_ReturnsFalse()
    {
        // IJsonDataObject itself is abstract (interface)
        var result = _factory.CanConvert(typeof(IJsonDataObject));
        Assert.False(result);
    }

    [Fact]
    public void CanConvert_NonIJsonDataObjectType_ReturnsFalse()
    {
        var result = _factory.CanConvert(typeof(NotAJsonDataObject));
        Assert.False(result);
    }

    [Fact]
    public void CanConvert_PrimitiveType_ReturnsFalse()
    {
        var result = _factory.CanConvert(typeof(int));
        Assert.False(result);
    }

    #endregion

    #region CreateConverter

    [Fact]
    public void CreateConverter_ConcreteIJsonDataObject_ReturnsConverter()
    {
        var converter = _factory.CreateConverter(typeof(TestJsonDataObject));
        Assert.NotNull(converter);
        Assert.IsType<JsonDataObjectJsonDataConverter<TestJsonDataObject>>(converter);
    }

    [Fact]
    public void CreateConverter_ConcreteIJsonDataObject_ReturnsIJsonDataConverter()
    {
        var converter = _factory.CreateConverter(typeof(TestJsonDataObject));
        Assert.IsAssignableFrom<IJsonDataConverter>(converter);
    }

    #endregion

    #region JsonDataObjectJsonDataConverter<T>.Create

    [Fact]
    public void Create_ValidValue_ReturnsJsonFromValue()
    {
        var converter = new JsonDataObjectJsonDataConverter<TestJsonDataObject>();
        var json = new JsonData(JsonNode.Parse("{\"key\":\"val\"}"));
        var obj = new TestJsonDataObject { Json = json };

        var result = converter.Create(obj);

        Assert.Equal(json, result);
    }

    #endregion

    #region JsonDataObjectJsonDataConverter<T>.TryGet

    [Fact]
    public void TryGet_NonNullJsonData_ReturnsTrueAndSetsValue()
    {
        var converter = new JsonDataObjectJsonDataConverter<TestJsonDataObject>();
        var json = new JsonData(JsonNode.Parse("{\"key\":\"val\"}"));

        var success = converter.TryGet(json, out var value);

        Assert.True(success);
        Assert.NotNull(value);
        Assert.Equal(json, ((IJsonData)value).Json);
    }

    [Fact]
    public void TryGet_NullJsonData_ReturnsFalseAndDefaultValue()
    {
        var converter = new JsonDataObjectJsonDataConverter<TestJsonDataObject>();
        var nullJson = new JsonData(); // default — IsNull is true

        var success = converter.TryGet(nullJson, out var value);

        Assert.False(success);
        Assert.Equal(default, value);
    }

    #endregion
}
