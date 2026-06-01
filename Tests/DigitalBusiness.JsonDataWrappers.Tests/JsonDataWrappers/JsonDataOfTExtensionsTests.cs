using System;
using System.Text.Json.Nodes;
using DigitalBusiness.JsonDataWrappers;
using Xunit;

namespace DigitalBusiness.JsonDataWrappers.Tests;

public class JsonDataOfTExtensionsTests
{
    private sealed class TestKey : IJsonDataKey { }

    [Fact]
    public void With_ActionIsInvoked_ReturnsSameInstance()
    {
        // Arrange
        var json = new JsonData(JsonNode.Parse("{\"a\":1}"));
        var typed = new JsonData<TestKey>(json);
        var actionInvoked = false;

        // Act
        var result = typed.With(_ => actionInvoked = true);

        // Assert
        Assert.True(actionInvoked);
        Assert.Equal(typed, result);
    }

    [Fact]
    public void With_ActionReceivesCorrectInstance()
    {
        // Arrange
        var json = new JsonData(JsonNode.Parse("{\"a\":1}"));
        var typed = new JsonData<TestKey>(json);
        JsonData<TestKey> received = default;

        // Act
        typed.With(d => received = d);

        // Assert
        Assert.Equal(typed, received);
    }

    [Fact]
    public void With_ChainedCalls_AllActionsAreInvoked()
    {
        // Arrange
        var json = new JsonData(JsonNode.Parse("{}"));
        var typed = new JsonData<TestKey>(json);
        var count = 0;

        // Act
        var result = typed
            .With(_ => count++)
            .With(_ => count++)
            .With(_ => count++);

        // Assert
        Assert.Equal(3, count);
        Assert.Equal(typed, result);
    }

    [Fact]
    public void With_NullAction_ThrowsNullReferenceException()
    {
        // Arrange
        var json = new JsonData(JsonNode.Parse("{}"));
        var typed = new JsonData<TestKey>(json);
        Action<JsonData<TestKey>> action = null!;

        // Act & Assert
        Assert.Throws<NullReferenceException>(() => typed.With(action));
    }
}
