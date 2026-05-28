using System.Text.Json;
using System.Text.Json.Nodes;
using DigitalBusiness.JsonDataWrappers;
using Xunit;

namespace DigitalBusiness.JsonDataWrappers.Tests;

public class JsonDataTests
{
    // -- Default constructor ---------------------------------------------------

    [Fact]
    public void DefaultConstructor_SetsReadOnlyTrue()
    {
        var sut = new JsonData();

        Assert.True(sut.ReadOnly);
    }

    [Fact]
    public void DefaultConstructor_NodeIsNull()
    {
        var sut = new JsonData();

        Assert.Null(sut.Node);
    }

    [Fact]
    public void DefaultConstructor_ElementIsNull()
    {
        var sut = new JsonData();

        Assert.Null(sut.Element);
    }

    // -- JsonData(JsonNode?) constructor ---------------------------------------

    [Fact]
    public void ConstructorNode_WithNullNode_IsReadOnly()
    {
        var sut = new JsonData((JsonNode?)null);

        Assert.True(sut.ReadOnly);
    }

    [Fact]
    public void ConstructorNode_WithNullNode_NodeIsNull()
    {
        var sut = new JsonData((JsonNode?)null);

        Assert.Null(sut.Node);
    }

    [Fact]
    public void ConstructorNode_WithJsonValue_IsReadOnly()
    {
        JsonNode node = JsonValue.Create(42)!;

        var sut = new JsonData(node);

        Assert.True(sut.ReadOnly);
    }

    [Fact]
    public void ConstructorNode_WithJsonObject_IsNotReadOnly()
    {
        JsonNode node = new JsonObject();

        var sut = new JsonData(node);

        Assert.False(sut.ReadOnly);
    }

    [Fact]
    public void ConstructorNode_WithJsonArray_IsNotReadOnly()
    {
        JsonNode node = new JsonArray();

        var sut = new JsonData(node);

        Assert.False(sut.ReadOnly);
    }

    [Fact]
    public void ConstructorNode_WithJsonObject_SetsNode()
    {
        JsonNode node = new JsonObject();

        var sut = new JsonData(node);

        Assert.Same(node, sut.Node);
    }

    // -- JsonData.CreateReadOnly factory --------------------------------------

    [Fact]
    public void CreateReadOnly_WithNullNode_IsReadOnly()
    {
        var sut = JsonData.CreateReadOnly(null);

        Assert.True(sut.ReadOnly);
    }

    [Fact]
    public void CreateReadOnly_WithJsonObject_IsReadOnly()
    {
        JsonNode node = new JsonObject();

        var sut = JsonData.CreateReadOnly(node);

        Assert.True(sut.ReadOnly);
    }

    [Fact]
    public void CreateReadOnly_WithJsonArray_IsReadOnly()
    {
        JsonNode node = new JsonArray();

        var sut = JsonData.CreateReadOnly(node);

        Assert.True(sut.ReadOnly);
    }

    [Fact]
    public void CreateReadOnly_WithJsonObject_SetsNode()
    {
        JsonNode node = new JsonObject();

        var sut = JsonData.CreateReadOnly(node);

        Assert.Same(node, sut.Node);
    }

    // -- JsonData(JsonElement) constructor -------------------------------------

    [Fact]
    public void ConstructorElement_IsReadOnly()
    {
        using var doc = JsonDocument.Parse("{\"a\":1}");
        var element = doc.RootElement;

        var sut = new JsonData(element);

        Assert.True(sut.ReadOnly);
    }

    [Fact]
    public void ConstructorElement_SetsElement()
    {
        using var doc = JsonDocument.Parse("{\"a\":1}");
        var element = doc.RootElement;

        var sut = new JsonData(element);

        Assert.True(sut.Element.HasValue);
        Assert.Equal(JsonValueKind.Object, sut.Element!.Value.ValueKind);
    }

    [Fact]
    public void ConstructorElement_NodeIsNull()
    {
        using var doc = JsonDocument.Parse("42");
        var element = doc.RootElement;

        var sut = new JsonData(element);

        Assert.Null(sut.Node);
    }

    // -- IJsonData.Json property -----------------------------------------------

    [Fact]
    public void Json_ExplicitInterface_ReturnsSelf()
    {
        var sut = new JsonData(new JsonObject());

        IJsonData iface = sut;

        Assert.Equal(sut, iface.Json);
    }

    [Fact]
    public void Json_DefaultInstance_ReturnsSelf()
    {
        var sut = new JsonData();

        IJsonData iface = sut;

        Assert.Equal(sut, iface.Json);
    }

    [Fact]
    public void Json_ElementInstance_ReturnsSelf()
    {
        using var doc = JsonDocument.Parse("true");
        var sut = new JsonData(doc.RootElement);

        IJsonData iface = sut;

        Assert.Equal(sut, iface.Json);
    }

    // -- JsonData(JsonElement?) constructor ------------------------------------

    [Fact]
    public void ConstructorNullableElement_WithNull_IsReadOnly()
    {
        var sut = new JsonData((JsonElement?)null);

        Assert.True(sut.ReadOnly);
    }

    [Fact]
    public void ConstructorNullableElement_WithNull_ElementIsNull()
    {
        var sut = new JsonData((JsonElement?)null);

        Assert.False(sut.Element.HasValue);
    }

    [Fact]
    public void ConstructorNullableElement_WithNull_NodeIsNull()
    {
        var sut = new JsonData((JsonElement?)null);

        Assert.Null(sut.Node);
    }

    [Fact]
    public void ConstructorNullableElement_WithValue_IsReadOnly()
    {
        using var doc = JsonDocument.Parse("\"hello\"");
        JsonElement? element = doc.RootElement;

        var sut = new JsonData(element);

        Assert.True(sut.ReadOnly);
    }

    [Fact]
    public void ConstructorNullableElement_WithValue_SetsElement()
    {
        using var doc = JsonDocument.Parse("\"hello\"");
        JsonElement? element = doc.RootElement;

        var sut = new JsonData(element);

        Assert.True(sut.Element.HasValue);
        Assert.Equal(JsonValueKind.String, sut.Element!.Value.ValueKind);
    }

    [Fact]
    public void ConstructorNullableElement_WithValue_NodeIsNull()
    {
        using var doc = JsonDocument.Parse("true");
        JsonElement? element = doc.RootElement;

        var sut = new JsonData(element);

        Assert.Null(sut.Node);
    }

    // -- CreateNull ------------------------------------------------------------

    [Fact]
    public void CreateNull_IsReadOnly()
    {
        var sut = JsonData.CreateNull();

        Assert.True(sut.ReadOnly);
    }

    [Fact]
    public void CreateNull_NodeIsNull()
    {
        var sut = JsonData.CreateNull();

        Assert.Null(sut.Node);
    }

    [Fact]
    public void CreateNull_ElementIsNull()
    {
        var sut = JsonData.CreateNull();

        Assert.False(sut.Element.HasValue);
    }

    [Fact]
    public void CreateNull_ValueKindIsNull()
    {
        var sut = JsonData.CreateNull();

        Assert.Equal(JsonValueKind.Null, sut.ValueKind);
    }

    // -- ReadOnly property -----------------------------------------------------

    [Fact]
    public void ReadOnly_DefaultInstance_IsTrue()
    {
        var sut = new JsonData();

        Assert.True(sut.ReadOnly);
    }

    [Fact]
    public void ReadOnly_ElementBacked_IsTrue()
    {
        using var doc = JsonDocument.Parse("1");
        var sut = new JsonData(doc.RootElement);

        Assert.True(sut.ReadOnly);
    }

    [Fact]
    public void ReadOnly_NullableElementBacked_IsTrue()
    {
        using var doc = JsonDocument.Parse("[]");
        JsonElement? el = doc.RootElement;
        var sut = new JsonData(el);

        Assert.True(sut.ReadOnly);
    }

    [Fact]
    public void ReadOnly_NodeBackedJsonObject_WhenNotForcedReadOnly_IsFalse()
    {
        var sut = new JsonData(new JsonObject());

        Assert.False(sut.ReadOnly);
    }

    [Fact]
    public void ReadOnly_NodeBackedJsonObject_WhenCreatedReadOnly_IsTrue()
    {
        var sut = JsonData.CreateReadOnly(new JsonObject());

        Assert.True(sut.ReadOnly);
    }

    [Fact]
    public void ReadOnly_NodeBackedJsonValue_IsAlwaysTrue()
    {
        var sut = new JsonData(JsonValue.Create(99)!);

        Assert.True(sut.ReadOnly);
    }

    // -- ValueKind property ----------------------------------------------------

    [Fact]
    public void ValueKind_Uninitialized_ReturnsNull()
    {
        var sut = new JsonData();

        Assert.Equal(JsonValueKind.Null, sut.ValueKind);
    }

    [Fact]
    public void ValueKind_ElementBackedObject_ReturnsObject()
    {
        using var doc = JsonDocument.Parse("{\"x\":1}");
        var sut = new JsonData(doc.RootElement);

        Assert.Equal(JsonValueKind.Object, sut.ValueKind);
    }

    [Fact]
    public void ValueKind_ElementBackedArray_ReturnsArray()
    {
        using var doc = JsonDocument.Parse("[1,2]");
        var sut = new JsonData(doc.RootElement);

        Assert.Equal(JsonValueKind.Array, sut.ValueKind);
    }

    [Fact]
    public void ValueKind_ElementBackedString_ReturnsString()
    {
        using var doc = JsonDocument.Parse("\"test\"");
        var sut = new JsonData(doc.RootElement);

        Assert.Equal(JsonValueKind.String, sut.ValueKind);
    }

    [Fact]
    public void ValueKind_ElementBackedNumber_ReturnsNumber()
    {
        using var doc = JsonDocument.Parse("42");
        var sut = new JsonData(doc.RootElement);

        Assert.Equal(JsonValueKind.Number, sut.ValueKind);
    }

    [Fact]
    public void ValueKind_ElementBackedTrue_ReturnsTrue()
    {
        using var doc = JsonDocument.Parse("true");
        var sut = new JsonData(doc.RootElement);

        Assert.Equal(JsonValueKind.True, sut.ValueKind);
    }

    [Fact]
    public void ValueKind_NullableElementNull_ReturnsNull()
    {
        var sut = new JsonData((JsonElement?)null);

        Assert.Equal(JsonValueKind.Null, sut.ValueKind);
    }

    [Fact]
    public void ValueKind_NodeBackedJsonObject_ReturnsObject()
    {
        var sut = new JsonData(new JsonObject());

        Assert.Equal(JsonValueKind.Object, sut.ValueKind);
    }

    [Fact]
    public void ValueKind_NodeBackedJsonArray_ReturnsArray()
    {
        var sut = new JsonData(new JsonArray());

        Assert.Equal(JsonValueKind.Array, sut.ValueKind);
    }

    [Fact]
    public void ValueKind_NodeBackedJsonValueNumber_ReturnsNumber()
    {
        var sut = new JsonData(JsonValue.Create(7)!);

        Assert.Equal(JsonValueKind.Number, sut.ValueKind);
    }

    [Fact]
    public void ValueKind_NullNode_ReturnsNull()
    {
        var sut = new JsonData((JsonNode?)null);

        Assert.Equal(JsonValueKind.Null, sut.ValueKind);
    }

    // -- IsElement property ----------------------------------------------------

    [Fact]
    public void IsElement_ElementBacked_IsTrue()
    {
        using var doc = JsonDocument.Parse("1");
        var sut = new JsonData(doc.RootElement);

        Assert.True(sut.IsElement);
    }

    [Fact]
    public void IsElement_NullableElementWithValue_IsTrue()
    {
        using var doc = JsonDocument.Parse("{}");
        JsonElement? el = doc.RootElement;
        var sut = new JsonData(el);

        Assert.True(sut.IsElement);
    }

    [Fact]
    public void IsElement_NullableElementNull_IsFalse()
    {
        var sut = new JsonData((JsonElement?)null);

        Assert.False(sut.IsElement);
    }

    [Fact]
    public void IsElement_NodeBacked_IsFalse()
    {
        var sut = new JsonData(new JsonObject());

        Assert.False(sut.IsElement);
    }

    [Fact]
    public void IsElement_DefaultInstance_IsFalse()
    {
        var sut = new JsonData();

        Assert.False(sut.IsElement);
    }

    // -- IsNode property -------------------------------------------------------

    [Fact]
    public void IsNode_NodeBackedJsonObject_IsTrue()
    {
        var sut = new JsonData(new JsonObject());

        Assert.True(sut.IsNode);
    }

    [Fact]
    public void IsNode_NodeBackedJsonArray_IsTrue()
    {
        var sut = new JsonData(new JsonArray());

        Assert.True(sut.IsNode);
    }

    [Fact]
    public void IsNode_NodeBackedJsonValue_IsTrue()
    {
        var sut = new JsonData(JsonValue.Create(42)!);

        Assert.True(sut.IsNode);
    }

    [Fact]
    public void IsNode_NullNode_IsFalse()
    {
        var sut = new JsonData((JsonNode?)null);

        Assert.False(sut.IsNode);
    }

    [Fact]
    public void IsNode_ElementBacked_IsFalse()
    {
        using var doc = JsonDocument.Parse("1");
        var sut = new JsonData(doc.RootElement);

        Assert.False(sut.IsNode);
    }

    [Fact]
    public void IsNode_DefaultInstance_IsFalse()
    {
        var sut = new JsonData();

        Assert.False(sut.IsNode);
    }

    // -- Clone method ----------------------------------------------------------

    [Fact]
    public void Clone_DefaultInstance_ReturnsNewDefaultInstance()
    {
        var sut = new JsonData();

        var clone = sut.Clone();

        Assert.False(clone.IsNode);
        Assert.False(clone.IsElement);
        Assert.True(clone.ReadOnly);
    }

    [Fact]
    public void Clone_ElementBacked_ReturnsElementBackedInstance()
    {
        using var doc = JsonDocument.Parse("{\"a\":1}");
        var sut = new JsonData(doc.RootElement);

        var clone = sut.Clone();

        Assert.True(clone.IsElement);
        Assert.Equal(JsonValueKind.Object, clone.ValueKind);
    }

    [Fact]
    public void Clone_ElementBacked_IsReadOnly()
    {
        using var doc = JsonDocument.Parse("42");
        var sut = new JsonData(doc.RootElement);

        var clone = sut.Clone();

        Assert.True(clone.ReadOnly);
    }

    [Fact]
    public void Clone_NodeBacked_ReturnsNodeBackedInstance()
    {
        var node = new JsonObject { ["x"] = 7 };
        var sut = new JsonData(node);

        var clone = sut.Clone();

        Assert.True(clone.IsNode);
    }

    [Fact]
    public void Clone_NodeBacked_ReturnsDistinctNode()
    {
        var node = new JsonObject { ["x"] = 7 };
        var sut = new JsonData(node);

        var clone = sut.Clone();

        Assert.NotSame(node, clone.Node);
    }

    [Fact]
    public void Clone_NodeBackedReadOnly_CloneIsReadOnly()
    {
        var node = new JsonObject();
        var sut = JsonData.CreateReadOnly(node);

        var clone = sut.Clone();

        Assert.True(clone.ReadOnly);
    }

    [Fact]
    public void Clone_NodeBackedWritable_CloneIsWritable()
    {
        var node = new JsonObject();
        var sut = new JsonData(node);

        var clone = sut.Clone();

        Assert.False(clone.ReadOnly);
    }

    [Fact]
    public void Clone_NodeBackedWithData_CloneHasSameData()
    {
        var node = new JsonObject { ["key"] = "value" };
        var sut = new JsonData(node);

        var clone = sut.Clone();

        Assert.Equal(JsonValueKind.Object, clone.ValueKind);
        Assert.Equal("value", clone.Node!["key"]!.GetValue<string>());
    }

    // -- DeepEquals method -----------------------------------------------------

    [Fact]
    public void DeepEquals_TwoDefaultInstances_ReturnsTrue()
    {
        var a = new JsonData();
        var b = new JsonData();

        Assert.True(a.DeepEquals(b));
    }

    [Fact]
    public void DeepEquals_TwoEqualElementBacked_ReturnsTrue()
    {
        using var docA = JsonDocument.Parse("{\"x\":1}");
        using var docB = JsonDocument.Parse("{\"x\":1}");
        var a = new JsonData(docA.RootElement);
        var b = new JsonData(docB.RootElement);

        Assert.True(a.DeepEquals(b));
    }

    [Fact]
    public void DeepEquals_DifferentElementBacked_ReturnsFalse()
    {
        using var docA = JsonDocument.Parse("{\"x\":1}");
        using var docB = JsonDocument.Parse("{\"x\":2}");
        var a = new JsonData(docA.RootElement);
        var b = new JsonData(docB.RootElement);

        Assert.False(a.DeepEquals(b));
    }

    [Fact]
    public void DeepEquals_TwoEqualNodeBacked_ReturnsTrue()
    {
        var a = new JsonData(new JsonObject { ["y"] = 5 });
        var b = new JsonData(new JsonObject { ["y"] = 5 });

        Assert.True(a.DeepEquals(b));
    }

    [Fact]
    public void DeepEquals_DifferentNodeBacked_ReturnsFalse()
    {
        var a = new JsonData(new JsonObject { ["y"] = 5 });
        var b = new JsonData(new JsonObject { ["y"] = 9 });

        Assert.False(a.DeepEquals(b));
    }

    [Fact]
    public void DeepEquals_ElementBackedAndEquivalentNodeBacked_ReturnsTrue()
    {
        using var doc = JsonDocument.Parse("{\"z\":3}");
        var a = new JsonData(doc.RootElement);
        var b = new JsonData(new JsonObject { ["z"] = 3 });

        Assert.True(a.DeepEquals(b));
    }

    [Fact]
    public void DeepEquals_SameInstanceReflexive_ReturnsTrue()
    {
        var a = new JsonData(new JsonObject { ["k"] = true });

        Assert.True(a.DeepEquals(a));
    }

    [Fact]
    public void DeepEquals_DefaultVsNonDefault_ReturnsFalse()
    {
        var a = new JsonData();
        var b = new JsonData(new JsonObject());

        Assert.False(a.DeepEquals(b));
    }
}
