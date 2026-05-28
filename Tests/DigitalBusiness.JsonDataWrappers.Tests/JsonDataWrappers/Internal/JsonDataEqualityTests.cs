using System.Text.Json;
using System.Text.Json.Nodes;
using DigitalBusiness.JsonDataWrappers;
using DigitalBusiness.JsonDataWrappers.Internal;

namespace DigitalBusiness.JsonDataWrappers.Tests;

public class JsonDataEqualityTests
{
    // --- Helpers ---

    private static JsonData NullJsonData() => new JsonData((JsonNode?)null);
    private static JsonData IntJsonData(int value) => new JsonData(JsonValue.Create(value));
    private static JsonData StringJsonData(string value) => new JsonData(JsonValue.Create(value));

    private static JsonData ElementJsonData(string json)
    {
        var doc = JsonDocument.Parse(json);
        return new JsonData(doc.RootElement);
    }

    // --- Equals(JsonData, JsonData) ---

    [Fact]
    public void Equals_BothNull_ReturnsTrue()
    {
        var a = NullJsonData();
        var b = NullJsonData();
        Assert.True(JsonDataEquality.Equals(in a, in b));
    }

    [Fact]
    public void Equals_BothElementSameValue_ReturnsTrue()
    {
        var a = ElementJsonData("42");
        var b = ElementJsonData("42");
        Assert.True(JsonDataEquality.Equals(in a, in b));
    }

    [Fact]
    public void Equals_BothElementDifferentValue_ReturnsFalse()
    {
        var a = ElementJsonData("42");
        var b = ElementJsonData("99");
        Assert.False(JsonDataEquality.Equals(in a, in b));
    }

    [Fact]
    public void Equals_BothNodeSameValue_ReturnsTrue()
    {
        var a = IntJsonData(42);
        var b = IntJsonData(42);
        Assert.True(JsonDataEquality.Equals(in a, in b));
    }

    [Fact]
    public void Equals_BothNodeDifferentValue_ReturnsFalse()
    {
        var a = IntJsonData(1);
        var b = IntJsonData(2);
        Assert.False(JsonDataEquality.Equals(in a, in b));
    }

    [Fact]
    public void Equals_ElementAndNodeSameValue_ReturnsTrue()
    {
        var a = ElementJsonData("42");
        var b = IntJsonData(42);
        Assert.True(JsonDataEquality.Equals(in a, in b));
    }

    [Fact]
    public void Equals_ElementAndNodeDifferentValue_ReturnsFalse()
    {
        var a = ElementJsonData("42");
        var b = IntJsonData(99);
        Assert.False(JsonDataEquality.Equals(in a, in b));
    }

    [Fact]
    public void Equals_NodeAndElementSameValue_ReturnsTrue()
    {
        var a = IntJsonData(42);
        var b = ElementJsonData("42");
        Assert.True(JsonDataEquality.Equals(in a, in b));
    }

    [Fact]
    public void Equals_NodeAndElementDifferentValue_ReturnsFalse()
    {
        var a = IntJsonData(42);
        var b = ElementJsonData("99");
        Assert.False(JsonDataEquality.Equals(in a, in b));
    }

    [Fact]
    public void Equals_OneNullOneNonNull_ReturnsFalse()
    {
        var a = NullJsonData();
        var b = IntJsonData(1);
        Assert.False(JsonDataEquality.Equals(in a, in b));
    }

    // --- Equals(JsonData, JsonData?) ---

    [Fact]
    public void Equals_NullableB_NoValue_AIsNull_ReturnsTrue()
    {
        var a = NullJsonData();
        JsonData? b = null;
        Assert.True(JsonDataEquality.Equals(in a, in b));
    }

    [Fact]
    public void Equals_NullableB_NoValue_ANotNull_ReturnsFalse()
    {
        var a = IntJsonData(1);
        JsonData? b = null;
        Assert.False(JsonDataEquality.Equals(in a, in b));
    }

    [Fact]
    public void Equals_NullableB_HasValue_DelegatesToEquals()
    {
        var a = IntJsonData(42);
        JsonData? b = IntJsonData(42);
        Assert.True(JsonDataEquality.Equals(in a, in b));
    }

    // --- Equals(JsonData?, JsonData) ---

    [Fact]
    public void Equals_NullableA_NoValue_BIsNull_ReturnsTrue()
    {
        JsonData? a = null;
        var b = NullJsonData();
        Assert.True(JsonDataEquality.Equals(in a, in b));
    }

    [Fact]
    public void Equals_NullableA_NoValue_BNotNull_ReturnsFalse()
    {
        JsonData? a = null;
        var b = IntJsonData(1);
        Assert.False(JsonDataEquality.Equals(in a, in b));
    }

    [Fact]
    public void Equals_NullableA_HasValue_DelegatesToEquals()
    {
        JsonData? a = IntJsonData(42);
        var b = IntJsonData(42);
        Assert.True(JsonDataEquality.Equals(in a, in b));
    }

    // --- Equals(JsonData?, JsonData?) ---

    [Fact]
    public void Equals_BothNullable_NeitherHasValue_ReturnsTrue()
    {
        JsonData? a = null;
        JsonData? b = null;
        Assert.True(JsonDataEquality.Equals(in a, in b));
    }

    [Fact]
    public void Equals_BothNullable_ANoValue_BExplicitNull_ReturnsTrue()
    {
        JsonData? a = null;
        JsonData? b = NullJsonData();
        Assert.True(JsonDataEquality.Equals(in a, in b));
    }

    [Fact]
    public void Equals_BothNullable_ANoValue_BHasValue_ReturnsFalse()
    {
        JsonData? a = null;
        JsonData? b = IntJsonData(1);
        Assert.False(JsonDataEquality.Equals(in a, in b));
    }

    [Fact]
    public void Equals_BothNullable_BNoValue_AExplicitNull_ReturnsTrue()
    {
        JsonData? a = NullJsonData();
        JsonData? b = null;
        Assert.True(JsonDataEquality.Equals(in a, in b));
    }

    [Fact]
    public void Equals_BothNullable_BNoValue_AHasValue_ReturnsFalse()
    {
        JsonData? a = IntJsonData(1);
        JsonData? b = null;
        Assert.False(JsonDataEquality.Equals(in a, in b));
    }

    [Fact]
    public void Equals_BothNullable_BothHaveValue_SameContent_ReturnsTrue()
    {
        JsonData? a = IntJsonData(5);
        JsonData? b = IntJsonData(5);
        Assert.True(JsonDataEquality.Equals(in a, in b));
    }

    [Fact]
    public void Equals_BothNullable_BothHaveValue_DifferentContent_ReturnsFalse()
    {
        JsonData? a = IntJsonData(5);
        JsonData? b = IntJsonData(9);
        Assert.False(JsonDataEquality.Equals(in a, in b));
    }

    // --- Equals(JsonElement, JsonElement) ---

    [Fact]
    public void Equals_BothElements_SameValue_ReturnsTrue()
    {
        var doc1 = JsonDocument.Parse("42");
        var doc2 = JsonDocument.Parse("42");
        Assert.True(JsonDataEquality.Equals(doc1.RootElement, doc2.RootElement));
    }

    [Fact]
    public void Equals_BothElements_DifferentValue_ReturnsFalse()
    {
        var doc1 = JsonDocument.Parse("42");
        var doc2 = JsonDocument.Parse("99");
        Assert.False(JsonDataEquality.Equals(doc1.RootElement, doc2.RootElement));
    }

    [Fact]
    public void Equals_BothElements_SameObject_ReturnsTrue()
    {
        var doc = JsonDocument.Parse("{\"x\":1}");
        var doc2 = JsonDocument.Parse("{\"x\":1}");
        Assert.True(JsonDataEquality.Equals(doc.RootElement, doc2.RootElement));
    }

    [Fact]
    public void Equals_BothElements_DifferentObject_ReturnsFalse()
    {
        var doc1 = JsonDocument.Parse("{\"x\":1}");
        var doc2 = JsonDocument.Parse("{\"x\":2}");
        Assert.False(JsonDataEquality.Equals(doc1.RootElement, doc2.RootElement));
    }

    // --- Equals(JsonElement?, JsonElement?) ---

    [Fact]
    public void Equals_NullableElements_BothNoValue_ReturnsTrue()
    {
        JsonElement? e1 = null;
        JsonElement? e2 = null;
        Assert.True(JsonDataEquality.Equals(in e1, in e2));
    }

    [Fact]
    public void Equals_NullableElements_E1NoValue_E2ExplicitNull_ReturnsTrue()
    {
        JsonElement? e1 = null;
        JsonElement? e2 = JsonDocument.Parse("null").RootElement;
        Assert.True(JsonDataEquality.Equals(in e1, in e2));
    }

    [Fact]
    public void Equals_NullableElements_E2NoValue_E1ExplicitNull_ReturnsTrue()
    {
        JsonElement? e1 = JsonDocument.Parse("null").RootElement;
        JsonElement? e2 = null;
        Assert.True(JsonDataEquality.Equals(in e1, in e2));
    }

    [Fact]
    public void Equals_NullableElements_E1NoValue_E2NonNull_ReturnsFalse()
    {
        JsonElement? e1 = null;
        JsonElement? e2 = JsonDocument.Parse("42").RootElement;
        Assert.False(JsonDataEquality.Equals(in e1, in e2));
    }

    [Fact]
    public void Equals_NullableElements_E2NoValue_E1NonNull_ReturnsFalse()
    {
        JsonElement? e1 = JsonDocument.Parse("42").RootElement;
        JsonElement? e2 = null;
        Assert.False(JsonDataEquality.Equals(in e1, in e2));
    }

    [Fact]
    public void Equals_NullableElements_BothHaveValue_SameContent_ReturnsTrue()
    {
        JsonElement? e1 = JsonDocument.Parse("42").RootElement;
        JsonElement? e2 = JsonDocument.Parse("42").RootElement;
        Assert.True(JsonDataEquality.Equals(in e1, in e2));
    }

    [Fact]
    public void Equals_NullableElements_BothHaveValue_DifferentContent_ReturnsFalse()
    {
        JsonElement? e1 = JsonDocument.Parse("42").RootElement;
        JsonElement? e2 = JsonDocument.Parse("99").RootElement;
        Assert.False(JsonDataEquality.Equals(in e1, in e2));
    }

    // --- Equals(JsonNode?, JsonNode?) ---

    [Fact]
    public void Equals_Nodes_BothNull_ReturnsTrue()
    {
        Assert.True(JsonDataEquality.Equals((JsonNode?)null, (JsonNode?)null));
    }

    [Fact]
    public void Equals_Nodes_SameValue_ReturnsTrue()
    {
        JsonNode? a = JsonValue.Create(42);
        JsonNode? b = JsonValue.Create(42);
        Assert.True(JsonDataEquality.Equals(a, b));
    }

    [Fact]
    public void Equals_Nodes_DifferentValue_ReturnsFalse()
    {
        JsonNode? a = JsonValue.Create(1);
        JsonNode? b = JsonValue.Create(2);
        Assert.False(JsonDataEquality.Equals(a, b));
    }

    [Fact]
    public void Equals_Nodes_OneNull_ReturnsFalse()
    {
        JsonNode? a = JsonValue.Create(1);
        Assert.False(JsonDataEquality.Equals(a, (JsonNode?)null));
    }

    // --- Equals(JsonNode?, JsonElement?) ---

    [Fact]
    public void Equals_NodeNullElementNull_ReturnsTrue()
    {
        JsonElement? e = null;
        Assert.True(JsonDataEquality.Equals((JsonNode?)null, in e));
    }

    [Fact]
    public void Equals_NodeNullElementExplicitNull_ReturnsTrue()
    {
        JsonElement? e = JsonDocument.Parse("null").RootElement;
        Assert.True(JsonDataEquality.Equals((JsonNode?)null, in e));
    }

    [Fact]
    public void Equals_NodeNullElementNonNull_ReturnsFalse()
    {
        JsonElement? e = JsonDocument.Parse("42").RootElement;
        Assert.False(JsonDataEquality.Equals((JsonNode?)null, in e));
    }

    [Fact]
    public void Equals_NodeValueElementMatchingValue_ReturnsTrue()
    {
        JsonNode? node = JsonValue.Create(42);
        JsonElement? e = JsonDocument.Parse("42").RootElement;
        Assert.True(JsonDataEquality.Equals(node, in e));
    }

    [Fact]
    public void Equals_NodeValueElementDifferentValue_ReturnsFalse()
    {
        JsonNode? node = JsonValue.Create("hello");
        JsonElement? e = JsonDocument.Parse("\"world\"").RootElement;
        Assert.False(JsonDataEquality.Equals(node, in e));
    }

    // --- Equals(JsonElement?, JsonNode?) ---

    [Fact]
    public void Equals_ElementNullableNoValue_NodeNull_ReturnsTrue()
    {
        JsonElement? e = null;
        Assert.True(JsonDataEquality.Equals(in e, (JsonNode?)null));
    }

    [Fact]
    public void Equals_ElementNullableNoValue_NodeExplicitNull_ReturnsTrue()
    {
        JsonElement? e = null;
        JsonNode? node = JsonValue.Create<object?>(null);
        Assert.True(JsonDataEquality.Equals(in e, node));
    }

    [Fact]
    public void Equals_ElementNullableNoValue_NodeNonNull_ReturnsFalse()
    {
        JsonElement? e = null;
        JsonNode? node = JsonValue.Create(42);
        Assert.False(JsonDataEquality.Equals(in e, node));
    }

    [Fact]
    public void Equals_ElementNullableHasValue_NodeMatches_ReturnsTrue()
    {
        JsonElement? e = JsonDocument.Parse("42").RootElement;
        JsonNode? node = JsonValue.Create(42);
        Assert.True(JsonDataEquality.Equals(in e, node));
    }

    [Fact]
    public void Equals_ElementNullableHasValue_NodeDiffers_ReturnsFalse()
    {
        JsonElement? e = JsonDocument.Parse("42").RootElement;
        JsonNode? node = JsonValue.Create(99);
        Assert.False(JsonDataEquality.Equals(in e, node));
    }

    [Fact]
    public void Equals_ElementNullableHasStringValue_NodeMatches_ReturnsTrue()
    {
        JsonElement? e = JsonDocument.Parse("\"hello\"").RootElement;
        JsonNode? node = JsonValue.Create("hello");
        Assert.True(JsonDataEquality.Equals(in e, node));
    }

    [Fact]
    public void Equals_ElementNullableHasStringValue_NodeDiffers_ReturnsFalse()
    {
        JsonElement? e = JsonDocument.Parse("\"hello\"").RootElement;
        JsonNode? node = JsonValue.Create("world");
        Assert.False(JsonDataEquality.Equals(in e, node));
    }

    // --- Equals(JsonElement, JsonNode?) ---

    [Fact]
    public void Equals_ElementNonNullable_NodeNull_ReturnsFalse()
    {
        var e = JsonDocument.Parse("42").RootElement;
        Assert.False(JsonDataEquality.Equals(in e, (JsonNode?)null));
    }

    [Fact]
    public void Equals_ElementNonNullable_NodeNullExplicit_ReturnsFalse()
    {
        var e = JsonDocument.Parse("42").RootElement;
        JsonNode? node = JsonValue.Create(99);
        Assert.False(JsonDataEquality.Equals(in e, node));
    }

    [Fact]
    public void Equals_ElementNonNullable_NodeMatches_ReturnsTrue()
    {
        var e = JsonDocument.Parse("42").RootElement;
        JsonNode? node = JsonValue.Create(42);
        Assert.True(JsonDataEquality.Equals(in e, node));
    }

    [Fact]
    public void Equals_ElementNonNullableString_NodeDiffers_ReturnsFalse()
    {
        var e = JsonDocument.Parse("\"hello\"").RootElement;
        JsonNode? node = JsonValue.Create("world");
        Assert.False(JsonDataEquality.Equals(in e, node));
    }

    [Fact]
    public void Equals_ElementNonNullableString_NodeMatches_ReturnsTrue()
    {
        var e = JsonDocument.Parse("\"hello\"").RootElement;
        JsonNode? node = JsonValue.Create("hello");
        Assert.True(JsonDataEquality.Equals(in e, node));
    }

    [Fact]
    public void Equals_ElementNonNullableNull_NodeNull_ReturnsTrue()
    {
        var e = JsonDocument.Parse("null").RootElement;
        Assert.True(JsonDataEquality.Equals(in e, (JsonNode?)null));
    }

    // --- Equals(JsonNode?, in JsonElement) ---

    [Fact]
    public void Equals_NodeNull_ElementNull_ReturnsTrue()
    {
        var element = JsonDocument.Parse("null").RootElement;
        Assert.True(JsonDataEquality.Equals((JsonNode?)null, in element));
    }

    [Fact]
    public void Equals_NodeNull_ElementNonNull_ReturnsFalse()
    {
        var element = JsonDocument.Parse("42").RootElement;
        Assert.False(JsonDataEquality.Equals((JsonNode?)null, in element));
    }

    [Fact]
    public void Equals_NodeJsonObject_ElementObject_ReturnsTrue()
    {
        JsonNode node = JsonNode.Parse("{\"a\":1}")!;
        var element = JsonDocument.Parse("{\"a\":1}").RootElement;
        Assert.True(JsonDataEquality.Equals(node, in element));
    }

    [Fact]
    public void Equals_NodeJsonObject_ElementObject_ReturnsFalse()
    {
        JsonNode node = JsonNode.Parse("{\"a\":1}")!;
        var element = JsonDocument.Parse("{\"a\":2}").RootElement;
        Assert.False(JsonDataEquality.Equals(node, in element));
    }

    [Fact]
    public void Equals_NodeJsonArray_ElementArray_ReturnsTrue()
    {
        JsonNode node = JsonNode.Parse("[1,2,3]")!;
        var element = JsonDocument.Parse("[1,2,3]").RootElement;
        Assert.True(JsonDataEquality.Equals(node, in element));
    }

    [Fact]
    public void Equals_NodeJsonArray_ElementArray_ReturnsFalse()
    {
        JsonNode node = JsonNode.Parse("[1,2]")!;
        var element = JsonDocument.Parse("[1,2,3]").RootElement;
        Assert.False(JsonDataEquality.Equals(node, in element));
    }

    [Fact]
    public void Equals_NodeJsonValue_ElementValue_ReturnsTrue()
    {
        JsonNode node = JsonValue.Create("hello")!;
        var element = JsonDocument.Parse("\"hello\"").RootElement;
        Assert.True(JsonDataEquality.Equals(node, in element));
    }

    [Fact]
    public void Equals_NodeJsonValue_ElementValue_ReturnsFalse()
    {
        JsonNode node = JsonValue.Create("hello")!;
        var element = JsonDocument.Parse("\"world\"").RootElement;
        Assert.False(JsonDataEquality.Equals(node, in element));
    }

    // --- JsonArrayEquals ---

    [Fact]
    public void JsonArrayEquals_ElementNotArray_ReturnsFalse()
    {
        var array = JsonNode.Parse("[1,2]")!.AsArray();
        var element = JsonDocument.Parse("{\"a\":1}").RootElement;
        Assert.False(JsonDataEquality.JsonArrayEquals(array, element));
    }

    [Fact]
    public void JsonArrayEquals_EqualArrays_ReturnsTrue()
    {
        var array = JsonNode.Parse("[1,2,3]")!.AsArray();
        var element = JsonDocument.Parse("[1,2,3]").RootElement;
        Assert.True(JsonDataEquality.JsonArrayEquals(array, element));
    }

    [Fact]
    public void JsonArrayEquals_NodeHasMoreItems_ReturnsFalse()
    {
        var array = JsonNode.Parse("[1,2,3,4]")!.AsArray();
        var element = JsonDocument.Parse("[1,2,3]").RootElement;
        Assert.False(JsonDataEquality.JsonArrayEquals(array, element));
    }

    [Fact]
    public void JsonArrayEquals_ElementHasMoreItems_ReturnsFalse()
    {
        var array = JsonNode.Parse("[1,2]")!.AsArray();
        var element = JsonDocument.Parse("[1,2,3]").RootElement;
        Assert.False(JsonDataEquality.JsonArrayEquals(array, element));
    }

    [Fact]
    public void JsonArrayEquals_EmptyArrays_ReturnsTrue()
    {
        var array = JsonNode.Parse("[]")!.AsArray();
        var element = JsonDocument.Parse("[]").RootElement;
        Assert.True(JsonDataEquality.JsonArrayEquals(array, element));
    }

    [Fact]
    public void JsonArrayEquals_NullItemMatching_ReturnsTrue()
    {
        var array = new JsonArray(new JsonNode?[] { null });
        var element = JsonDocument.Parse("[null]").RootElement;
        Assert.True(JsonDataEquality.JsonArrayEquals(array, element));
    }

    [Fact]
    public void JsonArrayEquals_NullItemNotMatching_ReturnsFalse()
    {
        var array = new JsonArray(new JsonNode?[] { null });
        var element = JsonDocument.Parse("[1]").RootElement;
        Assert.False(JsonDataEquality.JsonArrayEquals(array, element));
    }

    [Fact]
    public void JsonArrayEquals_DifferentValues_ReturnsFalse()
    {
        var array = JsonNode.Parse("[1,99]")!.AsArray();
        var element = JsonDocument.Parse("[1,2]").RootElement;
        Assert.False(JsonDataEquality.JsonArrayEquals(array, element));
    }

    // --- JsonObjectEquals ---

    [Fact]
    public void JsonObjectEquals_EqualObjects_ReturnsTrue()
    {
        var obj = JsonNode.Parse("{\"a\":1,\"b\":\"hello\"}")!.AsObject();
        var element = JsonDocument.Parse("{\"a\":1,\"b\":\"hello\"}").RootElement;
        Assert.True(JsonDataEquality.JsonObjectEquals(obj, element));
    }

    [Fact]
    public void JsonObjectEquals_DifferentPropertyValue_ReturnsFalse()
    {
        var obj = JsonNode.Parse("{\"a\":1}")!.AsObject();
        var element = JsonDocument.Parse("{\"a\":2}").RootElement;
        Assert.False(JsonDataEquality.JsonObjectEquals(obj, element));
    }

    [Fact]
    public void JsonObjectEquals_ElementHasMoreProperties_ReturnsFalse()
    {
        var obj = JsonNode.Parse("{\"a\":1}")!.AsObject();
        var element = JsonDocument.Parse("{\"a\":1,\"b\":2}").RootElement;
        Assert.False(JsonDataEquality.JsonObjectEquals(obj, element));
    }

    [Fact]
    public void JsonObjectEquals_NodeHasMoreProperties_ReturnsFalse()
    {
        var obj = JsonNode.Parse("{\"a\":1,\"b\":2}")!.AsObject();
        var element = JsonDocument.Parse("{\"a\":1}").RootElement;
        Assert.False(JsonDataEquality.JsonObjectEquals(obj, element));
    }

    [Fact]
    public void JsonObjectEquals_PropertyMissingInNode_ReturnsFalse()
    {
        var obj = JsonNode.Parse("{\"x\":1}")!.AsObject();
        var element = JsonDocument.Parse("{\"y\":1}").RootElement;
        Assert.False(JsonDataEquality.JsonObjectEquals(obj, element));
    }

    [Fact]
    public void JsonObjectEquals_NullPropertyValueMatching_ReturnsTrue()
    {
        var obj = new JsonObject { ["a"] = null };
        var element = JsonDocument.Parse("{\"a\":null}").RootElement;
        Assert.True(JsonDataEquality.JsonObjectEquals(obj, element));
    }

    [Fact]
    public void JsonObjectEquals_NullPropertyValueNotMatching_ReturnsFalse()
    {
        var obj = new JsonObject { ["a"] = null };
        var element = JsonDocument.Parse("{\"a\":1}").RootElement;
        Assert.False(JsonDataEquality.JsonObjectEquals(obj, element));
    }

    [Fact]
    public void JsonObjectEquals_EmptyObjects_ReturnsTrue()
    {
        var obj = new JsonObject();
        var element = JsonDocument.Parse("{}").RootElement;
        Assert.True(JsonDataEquality.JsonObjectEquals(obj, element));
    }

    // --- JsonValueEquals ---

    [Fact]
    public void JsonValueEquals_DifferentKinds_ReturnsFalse()
    {
        var value = JsonValue.Create("hello");
        var element = JsonDocument.Parse("42").RootElement;
        Assert.False(JsonDataEquality.JsonValueEquals(value, element));
    }

    [Fact]
    public void JsonValueEquals_StringEqual_ReturnsTrue()
    {
        var value = JsonValue.Create("hello");
        var element = JsonDocument.Parse("\"hello\"").RootElement;
        Assert.True(JsonDataEquality.JsonValueEquals(value, element));
    }

    [Fact]
    public void JsonValueEquals_StringNotEqual_ReturnsFalse()
    {
        var value = JsonValue.Create("hello");
        var element = JsonDocument.Parse("\"world\"").RootElement;
        Assert.False(JsonDataEquality.JsonValueEquals(value, element));
    }

    [Fact]
    public void JsonValueEquals_NumberEqual_ReturnsTrue()
    {
        var value = JsonValue.Create(42m);
        var element = JsonDocument.Parse("42").RootElement;
        Assert.True(JsonDataEquality.JsonValueEquals(value, element));
    }

    [Fact]
    public void JsonValueEquals_NumberNotEqual_ReturnsFalse()
    {
        var value = JsonValue.Create(42m);
        var element = JsonDocument.Parse("99").RootElement;
        Assert.False(JsonDataEquality.JsonValueEquals(value, element));
    }

    [Fact]
    public void JsonValueEquals_TrueValue_ReturnsTrue()
    {
        var value = JsonValue.Create(true);
        var element = JsonDocument.Parse("true").RootElement;
        Assert.True(JsonDataEquality.JsonValueEquals(value!, element));
    }

    [Fact]
    public void JsonValueEquals_FalseValue_ReturnsTrue()
    {
        var value = JsonValue.Create(false);
        var element = JsonDocument.Parse("false").RootElement;
        Assert.True(JsonDataEquality.JsonValueEquals(value!, element));
    }
}
