using System.Text.Json.Nodes;
using DigitalBusiness.JsonDataWrappers;
using Xunit;

namespace DigitalBusiness.JsonDataWrappers.Tests;

public class JsonDataOfTTests
{
    private sealed class TestKey : IJsonDataKey { }

    // ── Constructor ───────────────────────────────────────────────────────────

    [Fact]
    public void Constructor_WithDefaultJsonData_StoresJson()
    {
        // Arrange
        var json = new JsonData();

        // Act
        var typed = new JsonData<TestKey>(json);

        // Assert
        Assert.Equal(json, typed.Json);
    }

    [Fact]
    public void Constructor_WithNodeJsonData_StoresJson()
    {
        // Arrange
        var node = JsonNode.Parse("{\"x\":1}");
        var json = new JsonData(node);

        // Act
        var typed = new JsonData<TestKey>(json);

        // Assert
        Assert.Equal(json, typed.Json);
    }

    // ── From ──────────────────────────────────────────────────────────────────

    [Fact]
    public void From_WithDefaultJsonData_ReturnsTypedWrapperWithSameJson()
    {
        // Arrange
        var json = new JsonData();

        // Act
        var typed = JsonData<TestKey>.From(json);

        // Assert
        Assert.Equal(json, typed.Json);
    }

    [Fact]
    public void From_WithNodeJsonData_ReturnsTypedWrapperWithSameJson()
    {
        // Arrange
        var node = JsonNode.Parse("[1,2,3]");
        var json = new JsonData(node);

        // Act
        var typed = JsonData<TestKey>.From(json);

        // Assert
        Assert.Equal(json, typed.Json);
    }

    [Fact]
    public void From_ReturnsSameAsConstructor()
    {
        // Arrange
        var node = JsonNode.Parse("{\"a\":\"b\"}");
        var json = new JsonData(node);

        // Act
        var fromFactory = JsonData<TestKey>.From(json);
        var fromCtor = new JsonData<TestKey>(json);

        // Assert
        Assert.Equal(fromCtor.Json, fromFactory.Json);
    }
}
