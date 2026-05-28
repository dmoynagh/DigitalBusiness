using System.Text.Json.Nodes;
using DigitalBusiness.JsonDataWrappers;
using Xunit;

namespace DigitalBusiness.JsonDataWrappers.Tests;

public class JsonDataObjectTests
{
    private sealed class ConcreteJsonDataObject : JsonDataObject
    {
        public ConcreteJsonDataObject() { }

        public ConcreteJsonDataObject(JsonData json)
        {
            Json = json;
        }
    }

    // -- IJsonDataWrapper.Json getter ------------------------------------------

    [Fact]
    public void IJsonDataWrapperJson_Get_ReturnsInitializedValue()
    {
        // Arrange
        var node = JsonValue.Create(42)!;
        var json = new JsonData(node);
        var obj = new ConcreteJsonDataObject(json);

        // Act
        var result = ((IJsonDataWrapper)obj).Json;

        // Assert
        Assert.Equal(json, result);
    }

    [Fact]
    public void IJsonDataWrapperJson_Get_DefaultWhenNotInitialized()
    {
        // Arrange
        var obj = new ConcreteJsonDataObject();

        // Act
        var result = ((IJsonDataWrapper)obj).Json;

        // Assert
        Assert.Equal(default(JsonData), result);
    }

    // -- IJsonDataWrapper.Json init --------------------------------------------

    [Fact]
    public void IJsonDataWrapperJson_Init_SetsJsonProperty()
    {
        // Arrange
        var node = JsonValue.Create("hello")!;
        var json = new JsonData(node);

        // Act
        var obj = new ConcreteJsonDataObject(json);

        // Assert
        Assert.Equal(json, ((IJsonDataWrapper)obj).Json);
    }

    // -- IJsonData.Json getter -------------------------------------------------

    [Fact]
    public void IJsonDataJson_Get_ReturnsSameValueAsIJsonDataWrapperJson()
    {
        // Arrange
        var node = JsonValue.Create(99)!;
        var json = new JsonData(node);
        var obj = new ConcreteJsonDataObject(json);

        // Act
        var wrapperJson = ((IJsonDataWrapper)obj).Json;
        var dataJson = ((IJsonData)obj).Json;

        // Assert
        Assert.Equal(wrapperJson, dataJson);
    }

    [Fact]
    public void IJsonDataJson_Get_DefaultWhenNotInitialized()
    {
        // Arrange
        var obj = new ConcreteJsonDataObject();

        // Act
        var result = ((IJsonData)obj).Json;

        // Assert
        Assert.Equal(default(JsonData), result);
    }

    [Fact]
    public void IJsonDataJson_Get_ReturnsInitializedValue()
    {
        // Arrange
        var node = JsonValue.Create(true)!;
        var json = new JsonData(node);
        var obj = new ConcreteJsonDataObject(json);

        // Act
        var result = ((IJsonData)obj).Json;

        // Assert
        Assert.Equal(json, result);
    }
}
