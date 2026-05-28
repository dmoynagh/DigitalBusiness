using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Nodes;
using DigitalBusiness.JsonDataWrappers;
using Xunit;

namespace DigitalBusiness.JsonDataWrappers.Tests;

public class JsonDataJsonObjectExtensionsTests
{
    // -- IsObject -------------------------------------------------------------

    [Fact]
    public void IsObject_WithJsonObjectNode_ReturnsTrue()
    {
        var data = new JsonData(new JsonObject());
        Assert.True(data.IsObject);
    }

    [Fact]
    public void IsObject_WithJsonArrayNode_ReturnsFalse()
    {
        var data = new JsonData(new JsonArray());
        Assert.False(data.IsObject);
    }

    [Fact]
    public void IsObject_WithJsonValueNode_ReturnsFalse()
    {
        var data = new JsonData(JsonValue.Create(42));
        Assert.False(data.IsObject);
    }

    [Fact]
    public void IsObject_WithNullNode_ReturnsFalse()
    {
        var data = new JsonData((JsonNode?)null);
        Assert.False(data.IsObject);
    }

    [Fact]
    public void IsObject_WithObjectJsonElement_ReturnsTrue()
    {
        var element = JsonDocument.Parse("{}").RootElement;
        var data = new JsonData(element);
        Assert.True(data.IsObject);
    }

    [Fact]
    public void IsObject_WithArrayJsonElement_ReturnsFalse()
    {
        var element = JsonDocument.Parse("[]").RootElement;
        var data = new JsonData(element);
        Assert.False(data.IsObject);
    }

    [Fact]
    public void IsObject_WithNumberJsonElement_ReturnsFalse()
    {
        var element = JsonDocument.Parse("42").RootElement;
        var data = new JsonData(element);
        Assert.False(data.IsObject);
    }

    [Fact]
    public void IsObject_WithDefaultJsonData_ReturnsFalse()
    {
        var data = new JsonData();
        Assert.False(data.IsObject);
    }

    // -- ThrowIfNotObject ------------------------------------------------------

    [Fact]
    public void ThrowIfNotObject_WhenIsObject_DoesNotThrow()
    {
        var data = new JsonData(new JsonObject());
        data.ThrowIfNotObject(); // should not throw
    }

    [Fact]
    public void ThrowIfNotObject_WhenNotObject_ThrowsInvalidOperationException()
    {
        var data = new JsonData(new JsonArray());
        Assert.Throws<InvalidOperationException>(() => data.ThrowIfNotObject());
    }

    [Fact]
    public void ThrowIfNotObject_WithDefault_ThrowsInvalidOperationException()
    {
        var data = new JsonData();
        Assert.Throws<InvalidOperationException>(() => data.ThrowIfNotObject());
    }

    // -- CreateObject ----------------------------------------------------------

    [Fact]
    public void CreateObject_ReturnsJsonDataThatIsObject()
    {
        var data = JsonDataJsonObjectExtensions.CreateObject();
        Assert.True(data.IsObject);
    }

    [Fact]
    public void CreateObject_ReturnsNewInstanceEachTime()
    {
        var a = JsonDataJsonObjectExtensions.CreateObject();
        var b = JsonDataJsonObjectExtensions.CreateObject();
        Assert.NotSame(a.Node, b.Node);
    }

    // -- AsObject --------------------------------------------------------------

    [Fact]
    public void AsObject_WhenIsObject_ReturnsSameJsonData()
    {
        var data = new JsonData(new JsonObject());
        var result = data.AsObject();
        Assert.Equal(data, result);
    }

    [Fact]
    public void AsObject_WhenNotObject_ThrowsInvalidOperationException()
    {
        var data = new JsonData(new JsonArray());
        Assert.Throws<InvalidOperationException>(() => data.AsObject());
    }

    [Fact]
    public void AsObject_WithDefault_ThrowsInvalidOperationException()
    {
        var data = new JsonData();
        Assert.Throws<InvalidOperationException>(() => data.AsObject());
    }

    // -- TryAsObject -----------------------------------------------------------

    [Fact]
    public void TryAsObject_WhenIsObject_ReturnsJsonData()
    {
        var data = new JsonData(new JsonObject());
        var result = data.TryAsObject();
        Assert.NotNull(result);
        Assert.Equal(data, result.Value);
    }

    [Fact]
    public void TryAsObject_WhenNotObject_ReturnsNull()
    {
        var data = new JsonData(new JsonArray());
        var result = data.TryAsObject();
        Assert.Null(result);
    }

    [Fact]
    public void TryAsObject_WithDefault_ReturnsNull()
    {
        var data = new JsonData();
        var result = data.TryAsObject();
        Assert.Null(result);
    }

    // -- ContainsProperty ------------------------------------------------------

    [Fact]
    public void ContainsProperty_NullKey_ThrowsArgumentException()
    {
        var data = new JsonData(new JsonObject());
        Assert.Throws<ArgumentNullException>(() => data.ContainsProperty(null!));
    }

    [Fact]
    public void ContainsProperty_WhitespaceKey_ThrowsArgumentException()
    {
        var data = new JsonData(new JsonObject());
        Assert.Throws<ArgumentException>(() => data.ContainsProperty("   "));
    }

    [Fact]
    public void ContainsProperty_JsonObjectNodeWithExistingKey_ReturnsTrue()
    {
        var node = new JsonObject { ["foo"] = JsonValue.Create(1) };
        var data = new JsonData(node);
        Assert.True(data.ContainsProperty("foo"));
    }

    [Fact]
    public void ContainsProperty_JsonObjectNodeWithMissingKey_ReturnsFalse()
    {
        var node = new JsonObject { ["foo"] = JsonValue.Create(1) };
        var data = new JsonData(node);
        Assert.False(data.ContainsProperty("bar"));
    }

    [Fact]
    public void ContainsProperty_JsonElementObjectWithExistingKey_ReturnsTrue()
    {
        var element = JsonDocument.Parse("{\"foo\":1}").RootElement;
        var data = new JsonData(element);
        Assert.True(data.ContainsProperty("foo"));
    }

    [Fact]
    public void ContainsProperty_JsonElementObjectWithMissingKey_ReturnsFalse()
    {
        var element = JsonDocument.Parse("{\"foo\":1}").RootElement;
        var data = new JsonData(element);
        Assert.False(data.ContainsProperty("bar"));
    }

    [Fact]
    public void ContainsProperty_JsonElementNotObject_ReturnsFalse()
    {
        var element = JsonDocument.Parse("42").RootElement;
        var data = new JsonData(element);
        Assert.False(data.ContainsProperty("foo"));
    }

    [Fact]
    public void ContainsProperty_DefaultJsonData_ReturnsFalse()
    {
        var data = new JsonData();
        Assert.False(data.ContainsProperty("foo"));
    }

    [Fact]
    public void ContainsProperty_NullNodeData_ReturnsFalse()
    {
        var data = new JsonData((JsonNode?)null);
        Assert.False(data.ContainsProperty("foo"));
    }

    [Fact]
    public void ContainsProperty_JsonArrayNode_ReturnsFalse()
    {
        var data = new JsonData(new JsonArray());
        Assert.False(data.ContainsProperty("foo"));
    }

    // -- HasProperty -----------------------------------------------------------

    [Fact]
    public void HasProperty_ExistingKey_ReturnsTrue()
    {
        var node = new JsonObject { ["x"] = JsonValue.Create("hello") };
        var data = new JsonData(node);
        Assert.True(data.HasProperty("x"));
    }

    [Fact]
    public void HasProperty_MissingKey_ReturnsFalse()
    {
        var node = new JsonObject { ["x"] = JsonValue.Create("hello") };
        var data = new JsonData(node);
        Assert.False(data.HasProperty("y"));
    }

    // -- PropertyHasValue ------------------------------------------------------

    [Fact]
    public void PropertyHasValue_NullKey_ThrowsArgumentException()
    {
        var data = new JsonData(new JsonObject());
        Assert.Throws<ArgumentNullException>(() => data.PropertyHasValue(null!));
    }

    [Fact]
    public void PropertyHasValue_WhitespaceKey_ThrowsArgumentException()
    {
        var data = new JsonData(new JsonObject());
        Assert.Throws<ArgumentException>(() => data.PropertyHasValue("   "));
    }

    [Fact]
    public void PropertyHasValue_JsonObjectNodeWithNonNullValue_ReturnsTrue()
    {
        var node = new JsonObject { ["a"] = JsonValue.Create(42) };
        var data = new JsonData(node);
        Assert.True(data.PropertyHasValue("a"));
    }

    [Fact]
    public void PropertyHasValue_JsonObjectNodeWithNullValue_ReturnsFalse()
    {
        var node = new JsonObject { ["a"] = null };
        var data = new JsonData(node);
        Assert.False(data.PropertyHasValue("a"));
    }

    [Fact]
    public void PropertyHasValue_JsonObjectNodeMissingKey_ReturnsFalse()
    {
        var node = new JsonObject { ["a"] = JsonValue.Create(1) };
        var data = new JsonData(node);
        Assert.False(data.PropertyHasValue("missing"));
    }

    [Fact]
    public void PropertyHasValue_JsonElementObjectWithNonNullValue_ReturnsTrue()
    {
        var element = JsonDocument.Parse("{\"a\":42}").RootElement;
        var data = new JsonData(element);
        Assert.True(data.PropertyHasValue("a"));
    }

    [Fact]
    public void PropertyHasValue_JsonElementObjectWithNullValue_ReturnsFalse()
    {
        var element = JsonDocument.Parse("{\"a\":null}").RootElement;
        var data = new JsonData(element);
        Assert.False(data.PropertyHasValue("a"));
    }

    [Fact]
    public void PropertyHasValue_JsonElementObjectMissingKey_ReturnsFalse()
    {
        var element = JsonDocument.Parse("{\"a\":1}").RootElement;
        var data = new JsonData(element);
        Assert.False(data.PropertyHasValue("missing"));
    }

    [Fact]
    public void PropertyHasValue_JsonElementNotObject_ReturnsFalse()
    {
        var element = JsonDocument.Parse("42").RootElement;
        var data = new JsonData(element);
        Assert.False(data.PropertyHasValue("foo"));
    }

    [Fact]
    public void PropertyHasValue_DefaultJsonData_ReturnsFalse()
    {
        var data = new JsonData();
        Assert.False(data.PropertyHasValue("foo"));
    }

    // -- Get -------------------------------------------------------------------

    [Fact]
    public void Get_NullName_ThrowsArgumentException()
    {
        var data = new JsonData(new JsonObject());
        Assert.Throws<ArgumentNullException>(() => data.Get(null!));
    }

    [Fact]
    public void Get_WhitespaceName_ThrowsArgumentException()
    {
        var data = new JsonData(new JsonObject());
        Assert.Throws<ArgumentException>(() => data.Get("  "));
    }

    [Fact]
    public void Get_JsonObjectNodeExistingKey_ReturnsJsonData()
    {
        var node = new JsonObject { ["key"] = JsonValue.Create("val") };
        var data = new JsonData(node);
        var result = data.Get("key");
        Assert.Equal("val", result.Node!.GetValue<string>());
    }

    [Fact]
    public void Get_JsonObjectNodeMissingKey_ThrowsException()
    {
        var node = new JsonObject();
        var data = new JsonData(node);
        Assert.ThrowsAny<Exception>(() => data.Get("missing"));
    }

    [Fact]
    public void Get_JsonElementObjectExistingKey_ReturnsJsonData()
    {
        var element = JsonDocument.Parse("{\"key\":99}").RootElement;
        var data = new JsonData(element);
        var result = data.Get("key");
        Assert.Equal(99, result.Element!.Value.GetInt32());
    }

    [Fact]
    public void Get_JsonElementObjectMissingKey_ThrowsException()
    {
        var element = JsonDocument.Parse("{}").RootElement;
        var data = new JsonData(element);
        Assert.ThrowsAny<Exception>(() => data.Get("missing"));
    }

    [Fact]
    public void Get_NotAnObject_ThrowsException()
    {
        var data = new JsonData(new JsonArray());
        Assert.ThrowsAny<Exception>(() => data.Get("foo"));
    }

    // -- TryGet (string key) ? JsonData? --------------------------------------

    [Fact]
    public void TryGet_JsonObjectNodeExistingKey_ReturnsJsonData()
    {
        var node = new JsonObject { ["k"] = JsonValue.Create(7) };
        var data = new JsonData(node);
        var result = data.TryGet("k");
        Assert.NotNull(result);
    }

    [Fact]
    public void TryGet_JsonObjectNodeMissingKey_ReturnsNull()
    {
        var node = new JsonObject();
        var data = new JsonData(node);
        var result = data.TryGet("missing");
        Assert.Null(result);
    }

    [Fact]
    public void TryGet_JsonElementObjectExistingKey_ReturnsJsonData()
    {
        var element = JsonDocument.Parse("{\"k\":5}").RootElement;
        var data = new JsonData(element);
        var result = data.TryGet("k");
        Assert.NotNull(result);
    }

    [Fact]
    public void TryGet_JsonElementObjectMissingKey_ReturnsNull()
    {
        var element = JsonDocument.Parse("{}").RootElement;
        var data = new JsonData(element);
        var result = data.TryGet("missing");
        Assert.Null(result);
    }

    [Fact]
    public void TryGet_DefaultJsonData_ReturnsNull()
    {
        var data = new JsonData();
        var result = data.TryGet("foo");
        Assert.Null(result);
    }

    [Fact]
    public void TryGet_NotAnObject_ReturnsNull()
    {
        var data = new JsonData(new JsonArray());
        var result = data.TryGet("foo");
        Assert.Null(result);
    }

    // -- TryGet (string key, out JsonData) -------------------------------------

    [Fact]
    public void TryGet_Out_NullKey_ThrowsArgumentNullException()
    {
        var data = new JsonData(new JsonObject());
        Assert.Throws<ArgumentNullException>(() => data.TryGet(null!, out _));
    }

    [Fact]
    public void TryGet_Out_WhitespaceKey_ThrowsArgumentException()
    {
        var data = new JsonData(new JsonObject());
        Assert.Throws<ArgumentException>(() => data.TryGet("   ", out _));
    }

    [Fact]
    public void TryGet_Out_JsonElementObjectExistingKey_ReturnsTrueAndValue()
    {
        var element = JsonDocument.Parse("{\"x\":42}").RootElement;
        var data = new JsonData(element);
        var found = data.TryGet("x", out var value);
        Assert.True(found);
        Assert.Equal(42, value.Element!.Value.GetInt32());
    }

    [Fact]
    public void TryGet_Out_JsonElementObjectMissingKey_ReturnsFalseAndDefault()
    {
        var element = JsonDocument.Parse("{}").RootElement;
        var data = new JsonData(element);
        var found = data.TryGet("missing", out var value);
        Assert.False(found);
        Assert.Equal(default, value);
    }

    [Fact]
    public void TryGet_Out_JsonElementNotObject_ReturnsFalse()
    {
        var element = JsonDocument.Parse("42").RootElement;
        var data = new JsonData(element);
        var found = data.TryGet("foo", out _);
        Assert.False(found);
    }

    [Fact]
    public void TryGet_Out_JsonObjectNodeExistingKey_ReturnsTrueAndValue()
    {
        var node = new JsonObject { ["name"] = JsonValue.Create("hello") };
        var data = new JsonData(node);
        var found = data.TryGet("name", out var value);
        Assert.True(found);
        Assert.Equal("hello", value.Node!.GetValue<string>());
    }

    [Fact]
    public void TryGet_Out_JsonObjectNodeMissingKey_ReturnsFalse()
    {
        var node = new JsonObject();
        var data = new JsonData(node);
        var found = data.TryGet("absent", out var value);
        Assert.False(found);
        Assert.Equal(default, value);
    }

    [Fact]
    public void TryGet_Out_NullNodeData_ReturnsFalse()
    {
        var data = new JsonData((JsonNode?)null);
        var found = data.TryGet("foo", out _);
        Assert.False(found);
    }

    [Fact]
    public void TryGet_Out_DefaultJsonData_ReturnsFalse()
    {
        var data = new JsonData();
        var found = data.TryGet("foo", out _);
        Assert.False(found);
    }

    // -- GetOrCreateObject -----------------------------------------------------

    [Fact]
    public void GetOrCreateObject_NullName_ThrowsArgumentNullException()
    {
        var data = new JsonData(new JsonObject());
        Assert.Throws<ArgumentNullException>(() => data.GetOrCreateObject(null!));
    }

    [Fact]
    public void GetOrCreateObject_WhitespaceName_ThrowsArgumentException()
    {
        var data = new JsonData(new JsonObject());
        Assert.Throws<ArgumentException>(() => data.GetOrCreateObject("  "));
    }

    [Fact]
    public void GetOrCreateObject_KeyDoesNotExist_CreatesAndSetsObject()
    {
        var data = new JsonData(new JsonObject());
        var result = data.GetOrCreateObject("child");
        Assert.True(result.IsObject);
        Assert.True(data.ContainsProperty("child"));
    }

    [Fact]
    public void GetOrCreateObject_KeyExistsAsObject_ReturnsExistingObject()
    {
        var inner = new JsonObject { ["x"] = JsonValue.Create(1) };
        var node = new JsonObject { ["child"] = inner };
        var data = new JsonData(node);
        var result = data.GetOrCreateObject("child");
        Assert.True(result.IsObject);
        Assert.True(result.ContainsProperty("x"));
    }

    [Fact]
    public void GetOrCreateObject_KeyExistsAsNonObject_ThrowsInvalidOperationException()
    {
        var node = new JsonObject { ["child"] = JsonValue.Create(99) };
        var data = new JsonData(node);
        Assert.Throws<InvalidOperationException>(() => data.GetOrCreateObject("child"));
    }

    // -- GetOrCreateArray ------------------------------------------------------

    [Fact]
    public void GetOrCreateArray_NullName_ThrowsArgumentNullException()
    {
        var data = new JsonData(new JsonObject());
        Assert.Throws<ArgumentNullException>(() => data.GetOrCreateArray(null!));
    }

    [Fact]
    public void GetOrCreateArray_WhitespaceName_ThrowsArgumentException()
    {
        var data = new JsonData(new JsonObject());
        Assert.Throws<ArgumentException>(() => data.GetOrCreateArray("  "));
    }

    [Fact]
    public void GetOrCreateArray_KeyDoesNotExist_CreatesAndSetsArray()
    {
        var data = new JsonData(new JsonObject());
        var result = data.GetOrCreateArray("items");
        Assert.True(result.IsArray);
        Assert.True(data.ContainsProperty("items"));
    }

    [Fact]
    public void GetOrCreateArray_KeyExistsAsNonArray_ThrowsInvalidOperationException()
    {
        // GetOrCreateArray calls ThrowIfNotObject on the existing value, which will throw for non-object
        var node = new JsonObject { ["items"] = JsonValue.Create(1) };
        var data = new JsonData(node);
        Assert.Throws<InvalidOperationException>(() => data.GetOrCreateArray("items"));
    }

    // -- Set -------------------------------------------------------------------

    [Fact]
    public void Set_ReadOnlyData_ThrowsInvalidOperationException()
    {
        var data = JsonData.CreateReadOnly(new JsonObject());
        Assert.Throws<InvalidOperationException>(() => data.Set("key", new JsonData(JsonValue.Create(1))));
    }

    [Fact]
    public void Set_NotAnObject_ThrowsInvalidOperationException()
    {
        var data = new JsonData(new JsonArray());
        Assert.Throws<InvalidOperationException>(() => data.Set("key", new JsonData(JsonValue.Create(1))));
    }

    [Fact]
    public void Set_ValidKey_SetsProperty()
    {
        var data = new JsonData(new JsonObject());
        data.Set("name", new JsonData(JsonValue.Create("Alice")));
        Assert.True(data.ContainsProperty("name"));
    }

    [Fact]
    public void Set_NullValue_RemovesKey()
    {
        var node = new JsonObject { ["name"] = JsonValue.Create("Bob") };
        var data = new JsonData(node);
        data.Set("name", null);
        Assert.False(data.ContainsProperty("name"));
    }

    [Fact]
    public void Set_OverwritesExistingKey()
    {
        var node = new JsonObject { ["n"] = JsonValue.Create(1) };
        var data = new JsonData(node);
        data.Set("n", new JsonData(JsonValue.Create(2)));
        Assert.True(data.TryGet("n", out var v));
        Assert.Equal(2, v.Node!.GetValue<int>());
    }

    // -- Remove ----------------------------------------------------------------

    [Fact]
    public void Remove_ReadOnlyData_ThrowsInvalidOperationException()
    {
        var data = JsonData.CreateReadOnly(new JsonObject());
        Assert.Throws<InvalidOperationException>(() => data.Remove("key"));
    }

    [Fact]
    public void Remove_NotAnObject_ThrowsInvalidOperationException()
    {
        var data = new JsonData(new JsonArray());
        Assert.Throws<InvalidOperationException>(() => data.Remove("key"));
    }

    [Fact]
    public void Remove_ExistingKey_ReturnsTrueAndRemovesProperty()
    {
        var node = new JsonObject { ["k"] = JsonValue.Create(5) };
        var data = new JsonData(node);
        var result = data.Remove("k");
        Assert.True(result);
        Assert.False(data.ContainsProperty("k"));
    }

    [Fact]
    public void Remove_MissingKey_ReturnsFalse()
    {
        var data = new JsonData(new JsonObject());
        var result = data.Remove("nonexistent");
        Assert.False(result);
    }

    // -- PropertyNames ---------------------------------------------------------

    [Fact]
    public void PropertyNames_JsonObjectNodeWithProperties_ReturnsAllPropertyNames()
    {
        var node = new JsonObject { ["a"] = JsonValue.Create(1), ["b"] = JsonValue.Create(2) };
        var data = new JsonData(node);
        var names = data.PropertyNames.ToList();
        Assert.Contains("a", names);
        Assert.Contains("b", names);
        Assert.Equal(2, names.Count);
    }

    [Fact]
    public void PropertyNames_EmptyJsonObjectNode_ReturnsEmpty()
    {
        var data = new JsonData(new JsonObject());
        Assert.Empty(data.PropertyNames);
    }

    [Fact]
    public void PropertyNames_JsonElementObjectWithProperties_ReturnsAllPropertyNames()
    {
        var element = JsonDocument.Parse("{\"x\":1,\"y\":2}").RootElement;
        var data = new JsonData(element);
        var names = data.PropertyNames.ToList();
        Assert.Contains("x", names);
        Assert.Contains("y", names);
        Assert.Equal(2, names.Count);
    }

    [Fact]
    public void PropertyNames_EmptyJsonElementObject_ReturnsEmpty()
    {
        var element = JsonDocument.Parse("{}").RootElement;
        var data = new JsonData(element);
        Assert.Empty(data.PropertyNames);
    }

    [Fact]
    public void PropertyNames_JsonElementNotObject_ReturnsEmpty()
    {
        var element = JsonDocument.Parse("42").RootElement;
        var data = new JsonData(element);
        Assert.Empty(data.PropertyNames);
    }

    [Fact]
    public void PropertyNames_DefaultJsonData_ReturnsEmpty()
    {
        var data = new JsonData();
        Assert.Empty(data.PropertyNames);
    }

    [Fact]
    public void PropertyNames_NullNodeData_ReturnsEmpty()
    {
        var data = new JsonData((JsonNode?)null);
        Assert.Empty(data.PropertyNames);
    }

    [Fact]
    public void PropertyNames_JsonArrayNode_ReturnsEmpty()
    {
        var data = new JsonData(new JsonArray());
        Assert.Empty(data.PropertyNames);
    }

    // -- Readonly propagation -- TryGet(string) ---------------------------------

    [Fact]
    public void TryGet_ReadOnlyParent_ChildIsReadOnly()
    {
        var obj = new JsonObject { ["child"] = new JsonObject() };
        var parent = JsonData.CreateReadOnly(obj);

        var found = parent.TryGet("child", out var child);

        Assert.True(found);
        Assert.True(child.ReadOnly);
    }

    [Fact]
    public void TryGet_WritableParent_ChildIsWritable()
    {
        var obj = new JsonObject { ["child"] = new JsonObject() };
        var parent = new JsonData(obj);

        var found = parent.TryGet("child", out var child);

        Assert.True(found);
        Assert.False(child.ReadOnly);
    }
}
