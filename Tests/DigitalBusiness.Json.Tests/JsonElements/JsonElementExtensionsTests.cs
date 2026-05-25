using System.Text.Json;
using System.Text.Json.Nodes;
using DigitalBusiness.Json.JsonElements;
using FluentAssertions;

namespace DigitalBusiness.Json.Tests.JsonElements;

public class JsonElementExtensionsTests
{
    // ── helpers ──────────────────────────────────────────────────────────────

    private static JsonElement Parse(string json)
    {
        using var doc = JsonDocument.Parse(json);
        return doc.RootElement.Clone();
    }

    // ── IsValue ──────────────────────────────────────────────────────────────

    [Fact]
    public void IsValue_String_ReturnsTrue()
        => Parse(@"""hello""").IsValue().Should().BeTrue();

    [Fact]
    public void IsValue_Number_ReturnsTrue()
        => Parse("42").IsValue().Should().BeTrue();

    [Fact]
    public void IsValue_True_ReturnsTrue()
        => Parse("true").IsValue().Should().BeTrue();

    [Fact]
    public void IsValue_False_ReturnsTrue()
        => Parse("false").IsValue().Should().BeTrue();

    [Fact]
    public void IsValue_Null_ReturnsTrue()
        => Parse("null").IsValue().Should().BeTrue();

    [Fact]
    public void IsValue_Object_ReturnsFalse()
        => Parse("{}").IsValue().Should().BeFalse();

    [Fact]
    public void IsValue_Array_ReturnsFalse()
        => Parse("[]").IsValue().Should().BeFalse();

    // ── IsNull ───────────────────────────────────────────────────────────────

    [Fact]
    public void IsNull_NullJson_ReturnsTrue()
        => Parse("null").IsNull().Should().BeTrue();

    [Fact]
    public void IsNull_String_ReturnsFalse()
        => Parse(@"""text""").IsNull().Should().BeFalse();

    [Fact]
    public void IsNull_Number_ReturnsFalse()
        => Parse("0").IsNull().Should().BeFalse();

    [Fact]
    public void IsNull_Object_ReturnsFalse()
        => Parse("{}").IsNull().Should().BeFalse();

    // ── IsUndefined ──────────────────────────────────────────────────────────

    [Fact]
    public void IsUndefined_DefaultElement_ReturnsTrue()
    {
        var element = default(JsonElement);
        element.IsUndefined().Should().BeTrue();
    }

    [Fact]
    public void IsUndefined_ParsedElement_ReturnsFalse()
        => Parse("null").IsUndefined().Should().BeFalse();

    // ── IsArray ──────────────────────────────────────────────────────────────

    [Fact]
    public void IsArray_EmptyArray_ReturnsTrue()
        => Parse("[]").IsArray().Should().BeTrue();

    [Fact]
    public void IsArray_PopulatedArray_ReturnsTrue()
        => Parse("[1,2,3]").IsArray().Should().BeTrue();

    [Fact]
    public void IsArray_Object_ReturnsFalse()
        => Parse("{}").IsArray().Should().BeFalse();

    [Fact]
    public void IsArray_String_ReturnsFalse()
        => Parse(@"""text""").IsArray().Should().BeFalse();

    // ── IsObject ─────────────────────────────────────────────────────────────

    [Fact]
    public void IsObject_EmptyObject_ReturnsTrue()
        => Parse("{}").IsObject().Should().BeTrue();

    [Fact]
    public void IsObject_PopulatedObject_ReturnsTrue()
        => Parse(@"{""key"":""value""}").IsObject().Should().BeTrue();

    [Fact]
    public void IsObject_Array_ReturnsFalse()
        => Parse("[]").IsObject().Should().BeFalse();

    [Fact]
    public void IsObject_Null_ReturnsFalse()
        => Parse("null").IsObject().Should().BeFalse();

    // ── CreateNullElement ────────────────────────────────────────────────────

    [Fact]
    public void CreateNullElement_ReturnsElementWithNullKind()
    {
        var element = JsonElementExtensions.CreateNullElement();
        element.ValueKind.Should().Be(JsonValueKind.Null);
    }

    [Fact]
    public void CreateNullElement_IsNullReturnsTrueOnResult()
    {
        var element = JsonElementExtensions.CreateNullElement();
        element.IsNull().Should().BeTrue();
    }

    [Fact]
    public void CreateNullElement_CalledTwice_ReturnsSeparateClones()
    {
        var a = JsonElementExtensions.CreateNullElement();
        var b = JsonElementExtensions.CreateNullElement();
        a.ValueKind.Should().Be(b.ValueKind);
    }

    // ── ToJsonNode ───────────────────────────────────────────────────────────

    [Fact]
    public void ToJsonNode_NullElement_ReturnsNull()
        => Parse("null").ToJsonNode().Should().BeNull();

    [Fact]
    public void ToJsonNode_ArrayElement_ReturnsJsonArray()
        => Parse("[1,2]").ToJsonNode().Should().BeOfType<JsonArray>();

    [Fact]
    public void ToJsonNode_ObjectElement_ReturnsJsonObject()
        => Parse(@"{""a"":1}").ToJsonNode().Should().BeOfType<JsonObject>();

    [Fact]
    public void ToJsonNode_StringElement_ReturnsJsonValue()
    {
        var node = Parse(@"""hello""").ToJsonNode();
        node.Should().NotBeNull();
        node.Should().BeAssignableTo<JsonValue>();
        node!.GetValue<JsonElement>().GetString().Should().Be("hello");
    }

    [Fact]
    public void ToJsonNode_NumberElement_ReturnsJsonValue()
    {
        var node = Parse("99").ToJsonNode();
        node.Should().NotBeNull();
        node.Should().BeAssignableTo<JsonValue>();
        node!.GetValue<JsonElement>().GetInt32().Should().Be(99);
    }

    [Fact]
    public void ToJsonNode_BooleanTrueElement_ReturnsJsonValue()
    {
        var node = Parse("true").ToJsonNode();
        node.Should().BeAssignableTo<JsonValue>();
    }

    [Fact]
    public void ToJsonNode_ObjectPreservesProperties()
    {
        var node = Parse(@"{""name"":""test"",""value"":42}").ToJsonNode() as JsonObject;
        node.Should().NotBeNull();
        node!["name"]!.GetValue<string>().Should().Be("test");
        node["value"]!.GetValue<int>().Should().Be(42);
    }

    [Fact]
    public void ToJsonNode_ArrayPreservesElements()
    {
        var node = Parse("[10,20,30]").ToJsonNode() as JsonArray;
        node.Should().NotBeNull();
        node!.Count.Should().Be(3);
    }
}
