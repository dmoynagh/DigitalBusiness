using System.Text.Json.Nodes;
using DigitalBusiness.JsonDataWrappers;
using Moq;
using Xunit;

namespace DigitalBusiness.JsonDataWrappers.Tests;

public class TypedJsonDataExtensionsTests
{
    // A minimal IJsonDataKey for phantom-type tests
    private sealed class TestKey : IJsonDataKey { }

    // -- AsJsonData<T> on IJsonDataWrapper --------------------------------------

    [Fact]
    public void AsJsonData_IJsonDataWrapper_WithJsonNodeBacked_ReturnsJsonDataTWithSameJson()
    {
        // Arrange
        var node = JsonNode.Parse("{\"a\":1}");
        var inner = new JsonData(node);
        var mock = new Mock<IJsonDataWrapper>();
        mock.SetupGet(w => w.Json).Returns(inner);

        // Act
        JsonData<TestKey> result = mock.Object.AsJsonData<TestKey>();

        // Assert
        Assert.Equal(inner, result.Json);
    }

    [Fact]
    public void AsJsonData_IJsonDataWrapper_WithDefaultJson_ReturnsDefaultJsonDataT()
    {
        // Arrange
        var mock = new Mock<IJsonDataWrapper>();
        mock.SetupGet(w => w.Json).Returns(default(JsonData));

        // Act
        JsonData<TestKey> result = mock.Object.AsJsonData<TestKey>();

        // Assert
        Assert.Equal(default, result.Json);
    }

    // -- AsJsonData<T> on IJsonData ---------------------------------------------

    [Fact]
    public void AsJsonData_IJsonData_WithJsonNodeBacked_ReturnsJsonDataTWithSameJson()
    {
        // Arrange
        var node = JsonNode.Parse("{\"b\":2}");
        var inner = new JsonData(node);
        var mock = new Mock<IJsonData>();
        mock.SetupGet(d => d.Json).Returns(inner);

        // Act
        JsonData<TestKey> result = mock.Object.AsJsonData<TestKey>();

        // Assert
        Assert.Equal(inner, result.Json);
    }

    [Fact]
    public void AsJsonData_IJsonData_WithDefaultJson_ReturnsDefaultJsonDataT()
    {
        // Arrange
        var mock = new Mock<IJsonData>();
        mock.SetupGet(d => d.Json).Returns(default(JsonData));

        // Act
        JsonData<TestKey> result = mock.Object.AsJsonData<TestKey>();

        // Assert
        Assert.Equal(default, result.Json);
    }

    // -- AsJsonData<T> on JsonData (struct) ------------------------------------

    [Fact]
    public void AsJsonData_JsonData_WithJsonNodeBacked_WrapsItself()
    {
        // Arrange
        var node = JsonNode.Parse("{\"c\":3}");
        var jsonData = new JsonData(node);

        // Act
        JsonData<TestKey> result = jsonData.AsJsonData<TestKey>();

        // Assert
        Assert.Equal(jsonData, result.Json);
    }

    [Fact]
    public void AsJsonData_JsonData_WithDefault_WrapsDefault()
    {
        // Arrange
        var jsonData = default(JsonData);

        // Act
        JsonData<TestKey> result = jsonData.AsJsonData<TestKey>();

        // Assert
        Assert.Equal(default, result.Json);
    }

    [Fact]
    public void AsJsonData_JsonData_ResultIsJsonDataWrapper()
    {
        // Arrange
        var jsonData = new JsonData(new JsonObject());

        // Act
        JsonData<TestKey> result = jsonData.AsJsonData<TestKey>();

        // Assert
        Assert.IsAssignableFrom<IJsonDataWrapper>(result);
    }

    // -- GetOrCreateJsonData<T> on JsonData ------------------------------------

    [Fact]
    public void GetOrCreateJsonData_WhenPropertyMissing_CreatesObjectAndWraps()
    {
        // Arrange
        var jsonData = new JsonData(new JsonObject());

        // Act
        JsonData<TestKey> result = jsonData.GetOrCreateJsonData<TestKey>("child");

        // Assert
        Assert.True(result.Json.IsObject);
    }

    [Fact]
    public void GetOrCreateJsonData_WhenPropertyExists_ReturnsExistingObjectWrapped()
    {
        // Arrange
        var child = new JsonObject { ["x"] = 42 };
        var parent = new JsonData(new JsonObject { ["child"] = child });

        // Act
        JsonData<TestKey> result = parent.GetOrCreateJsonData<TestKey>("child");

        // Assert
        Assert.True(result.Json.IsObject);
        Assert.Equal(42, result.Json.Get<int>("x"));
    }

    [Fact]
    public void GetOrCreateJsonData_WhenReadOnly_ThrowsInvalidOperationException()
    {
        // Arrange
        var jsonData = JsonData.CreateReadOnly(new JsonObject());

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => jsonData.GetOrCreateJsonData<TestKey>("child"));
    }

    // -- GetOrCreateJsonDataArray<T> on JsonData -------------------------------

    [Fact]
    public void GetOrCreateJsonDataArray_WhenPropertyMissing_CreatesArrayAndWraps()
    {
        // Arrange
        var jsonData = new JsonData(new JsonObject());

        // Act
        JsonData<TestKey> result = jsonData.GetOrCreateJsonDataArray<TestKey>("items");

        // Assert
        Assert.True(result.Json.IsArray);
    }

    [Fact]
    public void GetOrCreateJsonDataArray_WhenPropertyExistsAndIsArray_ReturnsExistingArrayWrapped()
    {
        // Arrange
        var arr = new JsonArray(JsonValue.Create(1), JsonValue.Create(2));
        var jsonData = new JsonData(new JsonObject { ["items"] = arr });

        // Act
        JsonData<TestKey> result = jsonData.GetOrCreateJsonDataArray<TestKey>("items");

        // Assert
        Assert.True(result.Json.IsArray);
    }

    [Fact]
    public void GetOrCreateJsonDataArray_WhenReadOnly_ThrowsInvalidOperationException()
    {
        // Arrange
        var jsonData = JsonData.CreateReadOnly(new JsonObject());

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => jsonData.GetOrCreateJsonDataArray<TestKey>("items"));
    }

    [Fact]
    public void GetOrCreateJsonDataArray_WhenPropertyExistsButNotArray_ThrowsInvalidOperationException()
    {
        // Arrange
        var jsonData = new JsonData(new JsonObject { ["items"] = new JsonObject() });

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => jsonData.GetOrCreateJsonDataArray<TestKey>("items"));
    }
}
