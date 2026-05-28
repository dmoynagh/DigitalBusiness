using System.Text.Json.Nodes;
using DigitalBusiness.JsonDataWrappers;
using Xunit;

namespace DigitalBusiness.JsonDataWrappers.Tests;

public class JsonDataObjectExtensionsTests
{
    // A minimal concrete IJsonDataWrapper for testing
    private struct TestWrapper : IJsonDataWrapper
    {
        public JsonData Json { get; init; }
    }

    // -- AsJsonDataObject ------------------------------------------------------

    [Fact]
    public void AsJsonDataObject_WithJsonNodeBacked_CopiesJsonProperty()
    {
        // Arrange
        var node = JsonNode.Parse("{\"key\":\"value\"}");
        var source = new JsonData(node);

        // Act
        var result = source.AsJsonDataObject<TestWrapper>();

        // Assert
        Assert.Equal(((IJsonData)source).Json, result.Json);
    }

    [Fact]
    public void AsJsonDataObject_WithDefaultJsonData_CopiesDefaultJson()
    {
        // Arrange
        var source = new JsonData();

        // Act
        var result = source.AsJsonDataObject<TestWrapper>();

        // Assert
        Assert.Equal(((IJsonData)source).Json, result.Json);
    }

    [Fact]
    public void AsJsonDataObject_ReturnedInstanceJsonMatchesSourceJson()
    {
        // Arrange
        var node = JsonNode.Parse("42");
        var source = new JsonData(node);

        // Act
        var result = source.AsJsonDataObject<TestWrapper>();

        // Assert
        Assert.Equal(((IJsonData)source).Json, result.Json);
    }

    [Fact]
    public void AsJsonDataObject_ReturnsNewInstanceEachCall()
    {
        // Arrange
        var node = JsonNode.Parse("{\"a\":1}");
        var source = new JsonData(node);

        // Act
        var result1 = source.AsJsonDataObject<TestWrapper>();
        var result2 = source.AsJsonDataObject<TestWrapper>();

        // Assert – both should carry the same Json
        Assert.Equal(result1.Json, result2.Json);
    }
}
