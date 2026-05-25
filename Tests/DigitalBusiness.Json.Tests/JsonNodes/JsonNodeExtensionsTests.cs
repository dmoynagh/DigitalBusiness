using System.Text.Json;
using System.Text.Json.Nodes;
using DigitalBusiness.Json.JsonNodes;
using FluentAssertions;

namespace DigitalBusiness.Json.Tests.JsonNodes;

public class JsonNodeExtensionsTests
{
    // ── helpers ──────────────────────────────────────────────────────────────

    private static JsonNode ParseNode(string json)
        => JsonNode.Parse(json)!;

    // ── ToJsonElement (via JsonValue) ─────────────────────────────────────

    [Fact]
    public void ToJsonElement_JsonValueString_ReturnsStringKind()
    {
        JsonValue node = JsonValue.Create("hello")!;
        var element = node.ToJsonElement();
        element.ValueKind.Should().Be(JsonValueKind.String);
        element.GetString().Should().Be("hello");
    }

    [Fact]
    public void ToJsonElement_JsonValueNumber_ReturnsNumberKind()
    {
        JsonValue node = JsonValue.Create(42)!;
        var element = node.ToJsonElement();
        element.ValueKind.Should().Be(JsonValueKind.Number);
        element.GetInt32().Should().Be(42);
    }

    [Fact]
    public void ToJsonElement_JsonValueBoolean_ReturnsBoolKind()
    {
        JsonValue node = JsonValue.Create(true)!;
        var element = node.ToJsonElement();
        element.ValueKind.Should().BeOneOf(JsonValueKind.True, JsonValueKind.False);
    }

    // ── ToJsonElement (via JsonObject) ────────────────────────────────────

    [Fact]
    public void ToJsonElement_JsonObject_ReturnsObjectKind()
    {
        var node = ParseNode(@"{""key"":""value""}").AsObject();
        var element = node.ToJsonElement();
        element.ValueKind.Should().Be(JsonValueKind.Object);
    }

    [Fact]
    public void ToJsonElement_JsonObject_PreservesProperties()
    {
        var node = ParseNode(@"{""name"":""test"",""count"":5}").AsObject();
        var element = node.ToJsonElement();
        element.GetProperty("name").GetString().Should().Be("test");
        element.GetProperty("count").GetInt32().Should().Be(5);
    }

    [Fact]
    public void ToJsonElement_EmptyJsonObject_ReturnsEmptyObject()
    {
        var node = new JsonObject();
        var element = node.ToJsonElement();
        element.ValueKind.Should().Be(JsonValueKind.Object);
        element.EnumerateObject().Should().BeEmpty();
    }

    // ── ToJsonElement (via JsonArray) ─────────────────────────────────────

    [Fact]
    public void ToJsonElement_JsonArray_ReturnsArrayKind()
    {
        var node = ParseNode("[1,2,3]").AsArray();
        var element = node.ToJsonElement();
        element.ValueKind.Should().Be(JsonValueKind.Array);
    }

    [Fact]
    public void ToJsonElement_JsonArray_PreservesElements()
    {
        var node = ParseNode("[10,20,30]").AsArray();
        var element = node.ToJsonElement();
        element.GetArrayLength().Should().Be(3);
        element.EnumerateArray().Select(e => e.GetInt32())
            .Should().ContainInOrder(10, 20, 30);
    }

    [Fact]
    public void ToJsonElement_EmptyJsonArray_ReturnsEmptyArray()
    {
        var node = new JsonArray();
        var element = node.ToJsonElement();
        element.ValueKind.Should().Be(JsonValueKind.Array);
        element.GetArrayLength().Should().Be(0);
    }

    // ── Round-trip ────────────────────────────────────────────────────────

    [Fact]
    public void ToJsonElement_RoundTrip_JsonObjectToElementAndBack()
    {
        var original = ParseNode(@"{""x"":1}").AsObject();
        var element = original.ToJsonElement();
        var restored = JsonObject.Create(element);
        restored.Should().NotBeNull();
        restored!["x"]!.GetValue<int>().Should().Be(1);
    }

    [Fact]
    public void ToJsonElement_RoundTrip_JsonArrayToElementAndBack()
    {
        var original = ParseNode("[true,false]").AsArray();
        var element = original.ToJsonElement();
        var restored = JsonArray.Create(element);
        restored.Should().NotBeNull();
        restored!.Count.Should().Be(2);
    }

    [Fact]
    public void ToJsonElement_NestedObject_PreservesStructure()
    {
        var node = ParseNode(@"{""outer"":{""inner"":42}}").AsObject();
        var element = node.ToJsonElement();
        element.GetProperty("outer")
               .GetProperty("inner")
               .GetInt32()
               .Should().Be(42);
    }
}
