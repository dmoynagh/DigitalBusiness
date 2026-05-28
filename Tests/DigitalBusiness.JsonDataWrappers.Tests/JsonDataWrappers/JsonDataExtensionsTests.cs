using System;
using System.Text.Json;
using System.Text.Json.Nodes;
using DigitalBusiness.JsonDataWrappers;
using Xunit;

namespace DigitalBusiness.JsonDataWrappers.Tests;

public class JsonDataExtensionsTests
{
    // -- IsNull ----------------------------------------------------------------

    [Fact]
    public void IsNull_Uninitialized_ReturnsTrue()
    {
        // Arrange
        var data = new JsonData();

        // Act & Assert
        Assert.True(data.IsNull);
    }

    [Fact]
    public void IsNull_NullNodeNoElement_ReturnsTrue()
    {
        // Arrange
        var data = new JsonData((JsonNode?)null);

        // Act & Assert
        Assert.True(data.IsNull);
    }

    [Fact]
    public void IsNull_ElementBackedNullKind_ReturnsTrue()
    {
        // Arrange
        var element = JsonDocument.Parse("null").RootElement;
        var data = new JsonData(element);

        // Act & Assert
        Assert.True(data.IsNull);
    }

    [Fact]
    public void IsNull_ElementBackedNonNullKind_ReturnsFalse()
    {
        // Arrange
        var element = JsonDocument.Parse("42").RootElement;
        var data = new JsonData(element);

        // Act & Assert
        Assert.False(data.IsNull);
    }

    [Fact]
    public void IsNull_NodeBackedWithValue_ReturnsFalse()
    {
        // Arrange
        var node = JsonValue.Create(42)!;
        var data = new JsonData(node);

        // Act & Assert
        Assert.False(data.IsNull);
    }

    [Fact]
    public void IsNull_NodeBackedString_ReturnsFalse()
    {
        // Arrange
        var node = JsonValue.Create("hello")!;
        var data = new JsonData(node);

        // Act & Assert
        Assert.False(data.IsNull);
    }

    [Fact]
    public void IsNull_NullableElementWithValue_ReturnsFalse()
    {
        // Arrange
        JsonElement? element = JsonDocument.Parse("true").RootElement;
        var data = new JsonData(element);

        // Act & Assert
        Assert.False(data.IsNull);
    }

    [Fact]
    public void IsNull_NullableElementNull_ReturnsTrue()
    {
        // Arrange
        JsonElement? element = null;
        var data = new JsonData(element);

        // Act & Assert
        Assert.True(data.IsNull);
    }

    // -- IsUndefined -----------------------------------------------------------

    [Fact]
    public void IsUndefined_UninitializedElement_ReturnsFalse()
    {
        // Arrange – default JsonData has no node and no element; ValueKind returns Null
        var data = new JsonData();

        // Act & Assert
        Assert.False(data.IsUndefined);
    }

    [Fact]
    public void IsUndefined_DefaultJsonElement_ReturnsTrue()
    {
        // Arrange – default JsonElement has ValueKind == Undefined
        var data = new JsonData(default(JsonElement));

        // Act & Assert
        Assert.True(data.IsUndefined);
    }

    [Fact]
    public void IsUndefined_NodeBackedValue_ReturnsFalse()
    {
        // Arrange
        var node = JsonValue.Create(1)!;
        var data = new JsonData(node);

        // Act & Assert
        Assert.False(data.IsUndefined);
    }

    [Fact]
    public void IsUndefined_ElementBackedNumber_ReturnsFalse()
    {
        // Arrange
        var element = JsonDocument.Parse("1").RootElement;
        var data = new JsonData(element);

        // Act & Assert
        Assert.False(data.IsUndefined);
    }

    // -- ThrowIfReadOnly -------------------------------------------------------

    [Fact]
    public void ThrowIfReadOnly_ReadOnlyInstance_Throws()
    {
        // Arrange
        var node = JsonValue.Create(1)!;
        var data = new JsonData(node, readOnly: true);

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => data.ThrowIfReadOnly());
    }

    [Fact]
    public void ThrowIfReadOnly_WritableJsonObjectInstance_DoesNotThrow()
    {
        // Arrange – JsonObject is not a JsonValue so it can be writable
        var node = new JsonObject();
        var data = new JsonData(node, readOnly: false);

        // Act & Assert – should not throw
        data.ThrowIfReadOnly();
    }

    [Fact]
    public void ThrowIfReadOnly_UninitializedInstance_Throws()
    {
        // Arrange – uninitialized is always readonly
        var data = new JsonData();

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => data.ThrowIfReadOnly());
    }

    // -- ThrowIfNull -----------------------------------------------------------

    [Fact]
    public void ThrowIfNull_NullInstance_Throws()
    {
        // Arrange
        var data = new JsonData();

        // Act & Assert
        var ex = Assert.Throws<InvalidOperationException>(() => data.ThrowIfNull());
        Assert.Equal("JNode value is null.", ex.Message);
    }

    [Fact]
    public void ThrowIfNull_ElementBackedNull_Throws()
    {
        // Arrange
        var element = JsonDocument.Parse("null").RootElement;
        var data = new JsonData(element);

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => data.ThrowIfNull());
    }

    [Fact]
    public void ThrowIfNull_NonNullInstance_DoesNotThrow()
    {
        // Arrange
        var node = JsonValue.Create(42)!;
        var data = new JsonData(node);

        // Act & Assert – should not throw
        data.ThrowIfNull();
    }

    [Fact]
    public void ThrowIfNull_ElementBackedNonNull_DoesNotThrow()
    {
        // Arrange
        var element = JsonDocument.Parse("\"text\"").RootElement;
        var data = new JsonData(element);

        // Act & Assert – should not throw
        data.ThrowIfNull();
    }

    // -- AsReadOnly ------------------------------------------------------------

    [Fact]
    public void AsReadOnly_AlreadyReadOnly_ReturnsSameInstance()
    {
        // Arrange
        var node = JsonValue.Create(1)!;
        var data = new JsonData(node, readOnly: true);

        // Act
        var result = data.AsReadOnly();

        // Assert
        Assert.Equal(data, result);
    }

    [Fact]
    public void AsReadOnly_Writable_ReturnsReadOnlyInstance()
    {
        // Arrange – JsonObject is writable; JsonValue is always readonly
        var node = new JsonObject();
        var data = new JsonData(node, readOnly: false);

        // Act
        var result = data.AsReadOnly();

        // Assert
        Assert.True(result.ReadOnly);
    }

    [Fact]
    public void AsReadOnly_Writable_ReturnsDifferentReadOnlyInstance()
    {
        // Arrange
        var node = new JsonObject();
        var data = new JsonData(node, readOnly: false);

        // Act
        var result = data.AsReadOnly();

        // Assert – original is writable, result is readonly
        Assert.False(data.ReadOnly);
        Assert.True(result.ReadOnly);
    }

    [Fact]
    public void AsReadOnly_Writable_PreservesNode()
    {
        // Arrange
        var node = new JsonObject();
        var data = new JsonData(node, readOnly: false);

        // Act
        var result = data.AsReadOnly();

        // Assert
        Assert.Same(node, result.Node);
    }

    [Fact]
    public void AsReadOnly_Uninitialized_ReturnsSameInstance()
    {
        // Arrange – uninitialized is already readonly
        var data = new JsonData();

        // Act
        var result = data.AsReadOnly();

        // Assert
        Assert.Equal(data, result);
    }

    // -- Source ----------------------------------------------------------------

    [Fact]
    public void Source_Uninitialized_ReturnsNull()
    {
        // Arrange
        var data = new JsonData();

        // Act & Assert
        Assert.Null(data.Source);
    }

    [Fact]
    public void Source_NodeBacked_ReturnsNode()
    {
        // Arrange
        var node = JsonValue.Create(42)!;
        var data = new JsonData(node);

        // Act
        var source = data.Source;

        // Assert
        Assert.Same(node, source);
    }

    [Fact]
    public void Source_NullNodeBacked_ReturnsNull()
    {
        // Arrange
        var data = new JsonData((JsonNode?)null);

        // Act & Assert
        Assert.Null(data.Source);
    }

    [Fact]
    public void Source_ElementBacked_ReturnsBoxedElement()
    {
        // Arrange
        var element = JsonDocument.Parse("99").RootElement;
        var data = new JsonData(element);

        // Act
        var source = data.Source;

        // Assert
        Assert.IsType<JsonElement>(source);
        Assert.Equal(99, ((JsonElement)source!).GetInt32());
    }

    [Fact]
    public void Source_NullableElementWithValue_ReturnsBoxedElement()
    {
        // Arrange
        JsonElement? element = JsonDocument.Parse("true").RootElement;
        var data = new JsonData(element);

        // Act
        var source = data.Source;

        // Assert
        Assert.IsType<JsonElement>(source);
        Assert.True(((JsonElement)source!).GetBoolean());
    }

    [Fact]
    public void Source_NullableElementNull_ReturnsNull()
    {
        // Arrange
        JsonElement? element = null;
        var data = new JsonData(element);

        // Act & Assert
        Assert.Null(data.Source);
    }

    // -- Create() -------------------------------------------------------------

    [Fact]
    public void Create_NoArgs_ReturnsUninitializedInstance()
    {
        // Act
        var data = JsonData.Create();

        // Assert
        Assert.True(data.IsNull);
    }

    [Fact]
    public void Create_NoArgs_ReturnsReadOnlyInstance()
    {
        // Act
        var data = JsonData.Create();

        // Assert
        Assert.True(data.ReadOnly);
    }

    // -- Create(JsonNode?, bool) -----------------------------------------------

    [Fact]
    public void Create_WithNullNode_ReturnsNullInstance()
    {
        // Act – explicit cast to resolve the correct overload
        var data = JsonData.Create(source: (JsonNode?)null);

        // Assert
        Assert.True(data.IsNull);
    }

    [Fact]
    public void Create_WithNode_ReturnsNodeBackedInstance()
    {
        // Arrange
        JsonNode node = new JsonObject();

        // Act – explicit cast to resolve the correct overload
        var data = JsonData.Create(source: (JsonNode?)node);

        // Assert
        Assert.Same(node, data.Node);
    }

    [Fact]
    public void Create_WithNodeDefaultReadOnly_IsWritable()
    {
        // Arrange
        JsonNode node = new JsonObject();

        // Act
        var data = JsonData.Create(source: (JsonNode?)node);

        // Assert
        Assert.False(data.ReadOnly);
    }

    [Fact]
    public void Create_WithNodeReadOnlyTrue_IsReadOnly()
    {
        // Arrange
        JsonNode node = new JsonObject();

        // Act
        var data = JsonData.Create(source: (JsonNode?)node, readOnly: true);

        // Assert
        Assert.True(data.ReadOnly);
    }

    [Fact]
    public void Create_WithNodeReadOnlyFalse_IsWritable()
    {
        // Arrange
        JsonNode node = new JsonObject();

        // Act
        var data = JsonData.Create(source: (JsonNode?)node, readOnly: false);

        // Assert
        Assert.False(data.ReadOnly);
    }

    // -- Create(JsonElement) ---------------------------------------------------

    [Fact]
    public void Create_WithElement_ReturnsElementBackedInstance()
    {
        // Arrange
        var element = JsonDocument.Parse("123").RootElement;

        // Act
        var data = JsonData.Create(element);

        // Assert
        Assert.True(data.IsElement);
        Assert.Equal(123, data.Element!.Value.GetInt32());
    }

    [Fact]
    public void Create_WithElement_IsReadOnly()
    {
        // Arrange
        var element = JsonDocument.Parse("1").RootElement;

        // Act
        var data = JsonData.Create(element);

        // Assert
        Assert.True(data.ReadOnly);
    }

    // -- Create(JsonElement?) --------------------------------------------------

    [Fact]
    public void Create_WithNullableElementHasValue_ReturnsElementBackedInstance()
    {
        // Arrange
        JsonElement? element = JsonDocument.Parse("\"hello\"").RootElement;

        // Act
        var data = JsonData.Create(element);

        // Assert
        Assert.True(data.IsElement);
        Assert.Equal("hello", data.Element!.Value.GetString());
    }

    [Fact]
    public void Create_WithNullableElementNull_ReturnsNullInstance()
    {
        // Act
        var data = JsonData.Create((JsonElement?)null);

        // Assert
        Assert.True(data.IsNull);
    }

    [Fact]
    public void Create_WithNullableElement_IsReadOnly()
    {
        // Arrange
        JsonElement? element = JsonDocument.Parse("false").RootElement;

        // Act
        var data = JsonData.Create(element);

        // Assert
        Assert.True(data.ReadOnly);
    }

    // -- ToJsonElementJsonData -------------------------------------------------

    [Fact]
    public void ToJsonElementJsonData_ElementBacked_ReturnsElementBackedClone()
    {
        // Arrange
        var element = JsonDocument.Parse("42").RootElement;
        var data = new JsonData(element);

        // Act
        var result = data.ToJsonElementJsonData();

        // Assert
        Assert.True(result.IsElement);
        Assert.Equal(42, result.Element!.Value.GetInt32());
    }

    [Fact]
    public void ToJsonElementJsonData_ElementBacked_IsReadOnly()
    {
        // Arrange
        var element = JsonDocument.Parse("\"hello\"").RootElement;
        var data = new JsonData(element);

        // Act
        var result = data.ToJsonElementJsonData();

        // Assert
        Assert.True(result.ReadOnly);
    }

    [Fact]
    public void ToJsonElementJsonData_NodeBacked_ReturnsElementBacked()
    {
        // Arrange
        var node = JsonNode.Parse("{\"x\":1}")!;
        var data = new JsonData(node);

        // Act
        var result = data.ToJsonElementJsonData();

        // Assert
        Assert.True(result.IsElement);
        Assert.Equal(JsonValueKind.Object, result.Element!.Value.ValueKind);
    }

    [Fact]
    public void ToJsonElementJsonData_Uninitialized_ReturnsNullElement()
    {
        // Arrange
        var data = new JsonData();

        // Act
        var result = data.ToJsonElementJsonData();

        // Assert
        Assert.True(result.IsElement);
        Assert.Equal(JsonValueKind.Null, result.Element!.Value.ValueKind);
    }

    // -- ToJsonNodeJsonData ----------------------------------------------------

    [Fact]
    public void ToJsonNodeJsonData_NodeBacked_ReturnsNodeBackedDeepClone()
    {
        // Arrange
        var node = JsonNode.Parse("{\"a\":1}")!;
        var data = new JsonData(node, readOnly: false);

        // Act
        var result = data.ToJsonNodeJsonData();

        // Assert
        Assert.True(result.IsNode);
        Assert.NotSame(node, result.Node);
        Assert.Equal(JsonValueKind.Object, result.Node!.GetValueKind());
    }

    [Fact]
    public void ToJsonNodeJsonData_NodeBacked_PreservesReadOnlyWhenNotOverridden()
    {
        // Arrange
        var node = new JsonObject();
        var data = new JsonData(node, readOnly: false);

        // Act
        var result = data.ToJsonNodeJsonData();

        // Assert
        Assert.False(result.ReadOnly);
    }

    [Fact]
    public void ToJsonNodeJsonData_NodeBacked_ReadOnlyOverrideTrue()
    {
        // Arrange
        var node = new JsonObject();
        var data = new JsonData(node, readOnly: false);

        // Act
        var result = data.ToJsonNodeJsonData(readOnly: true);

        // Assert
        Assert.True(result.ReadOnly);
    }

    [Fact]
    public void ToJsonNodeJsonData_NodeBacked_ReadOnlyOverrideFalse()
    {
        // Arrange
        var node = new JsonObject();
        var data = new JsonData(node, readOnly: true);

        // Act
        var result = data.ToJsonNodeJsonData(readOnly: false);

        // Assert
        Assert.False(result.ReadOnly);
    }

    [Fact]
    public void ToJsonNodeJsonData_ElementBacked_ReturnsNodeBacked()
    {
        // Arrange
        var element = JsonDocument.Parse("{\"b\":2}").RootElement;
        var data = new JsonData(element);

        // Act
        var result = data.ToJsonNodeJsonData();

        // Assert
        Assert.True(result.IsNode);
        Assert.Equal(JsonValueKind.Object, result.Node!.GetValueKind());
    }

    [Fact]
    public void ToJsonNodeJsonData_ElementBacked_DefaultReadOnlyFalse()
    {
        // Arrange
        var element = JsonDocument.Parse("1").RootElement;
        var data = new JsonData(element);

        // Act
        var result = data.ToJsonNodeJsonData();

        // Assert
        Assert.False(result.ReadOnly);
    }

    [Fact]
    public void ToJsonNodeJsonData_ElementBacked_ReadOnlyOverrideTrue()
    {
        // Arrange
        var element = JsonDocument.Parse("1").RootElement;
        var data = new JsonData(element);

        // Act
        var result = data.ToJsonNodeJsonData(readOnly: true);

        // Assert
        Assert.True(result.ReadOnly);
    }

    [Fact]
    public void ToJsonNodeJsonData_Uninitialized_ReturnsNullNodeBacked()
    {
        // Arrange
        var data = new JsonData();

        // Act
        var result = data.ToJsonNodeJsonData();

        // Assert
        Assert.True(result.IsNode || result.IsNull);
        Assert.False(result.ReadOnly);
    }

    [Fact]
    public void ToJsonNodeJsonData_Uninitialized_ReadOnlyOverrideTrue()
    {
        // Arrange
        var data = new JsonData();

        // Act
        var result = data.ToJsonNodeJsonData(readOnly: true);

        // Assert
        Assert.True(result.ReadOnly);
    }

    // -- ToEditableJsonData ----------------------------------------------------

    [Fact]
    public void ToEditableJsonData_NodeBacked_ReturnsWritableNodeBacked()
    {
        // Arrange
        var node = new JsonObject();
        var data = new JsonData(node, readOnly: true);

        // Act
        var result = data.ToEditableJsonData();

        // Assert
        Assert.True(result.IsNode);
        Assert.False(result.ReadOnly);
    }

    [Fact]
    public void ToEditableJsonData_ElementBacked_ReturnsWritableNodeBacked()
    {
        // Arrange
        var element = JsonDocument.Parse("99").RootElement;
        var data = new JsonData(element);

        // Act
        var result = data.ToEditableJsonData();

        // Assert
        Assert.True(result.IsNode);
        Assert.False(result.ReadOnly);
    }

    [Fact]
    public void ToEditableJsonData_Uninitialized_ReturnsWritable()
    {
        // Arrange
        var data = new JsonData();

        // Act
        var result = data.ToEditableJsonData();

        // Assert
        Assert.False(result.ReadOnly);
    }

    // -- Count -----------------------------------------------------------------

    [Fact]
    public void Count_ElementBackedArray_ReturnsArrayLength()
    {
        // Arrange
        var element = JsonDocument.Parse("[1,2,3]").RootElement;
        var data = new JsonData(element);

        // Act & Assert
        Assert.Equal(3, data.Count);
    }

    [Fact]
    public void Count_ElementBackedObject_ReturnsPropertyCount()
    {
        // Arrange
        var element = JsonDocument.Parse("{\"a\":1,\"b\":2}").RootElement;
        var data = new JsonData(element);

        // Act & Assert
        Assert.Equal(2, data.Count);
    }

    [Fact]
    public void Count_ElementBackedScalar_ReturnsZero()
    {
        // Arrange
        var element = JsonDocument.Parse("42").RootElement;
        var data = new JsonData(element);

        // Act & Assert
        Assert.Equal(0, data.Count);
    }

    [Fact]
    public void Count_NodeBackedJsonArray_ReturnsCount()
    {
        // Arrange
        var arr = JsonNode.Parse("[1,2,3,4]") as JsonArray;
        var data = new JsonData(arr!, readOnly: false);

        // Act & Assert
        Assert.Equal(4, data.Count);
    }

    [Fact]
    public void Count_NodeBackedJsonObject_ReturnsCount()
    {
        // Arrange
        var obj = JsonNode.Parse("{\"x\":1,\"y\":2,\"z\":3}") as JsonObject;
        var data = new JsonData(obj!, readOnly: false);

        // Act & Assert
        Assert.Equal(3, data.Count);
    }

    [Fact]
    public void Count_NodeBackedScalar_ReturnsZero()
    {
        // Arrange
        var node = JsonValue.Create(7)!;
        var data = new JsonData(node);

        // Act & Assert
        Assert.Equal(0, data.Count);
    }

    [Fact]
    public void Count_Uninitialized_ReturnsZero()
    {
        // Arrange
        var data = new JsonData();

        // Act & Assert
        Assert.Equal(0, data.Count);
    }

    // -- Clear -----------------------------------------------------------------

    [Fact]
    public void Clear_NodeBackedJsonArray_RemovesAllItems()
    {
        // Arrange
        var arr = JsonNode.Parse("[1,2,3]") as JsonArray;
        var data = new JsonData(arr!, readOnly: false);

        // Act
        data.Clear();

        // Assert
        Assert.Empty(arr!);
    }

    [Fact]
    public void Clear_NodeBackedJsonObject_RemovesAllProperties()
    {
        // Arrange
        var obj = JsonNode.Parse("{\"a\":1,\"b\":2}") as JsonObject;
        var data = new JsonData(obj!, readOnly: false);

        // Act
        data.Clear();

        // Assert
        Assert.Empty(obj!);
    }

    [Fact]
    public void Clear_ReadOnly_ThrowsInvalidOperationException()
    {
        // Arrange
        var arr = JsonNode.Parse("[1,2]") as JsonArray;
        var data = new JsonData(arr!, readOnly: true);

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => data.Clear());
    }

    [Fact]
    public void Clear_NodeBackedScalar_ThrowsInvalidOperationException()
    {
        // Arrange
        var node = JsonValue.Create(5)!;
        var data = new JsonData(node, readOnly: false);

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => data.Clear());
    }

    // -- IsValue ---------------------------------------------------------------

    [Fact]
    public void IsValue_NodeBackedJsonValue_ReturnsTrue()
    {
        // Arrange
        var node = JsonValue.Create(42)!;
        var data = new JsonData(node);

        // Act & Assert
        Assert.True(data.IsValue);
    }

    [Fact]
    public void IsValue_NodeBackedJsonObject_ReturnsFalse()
    {
        // Arrange
        var node = new JsonObject();
        var data = new JsonData(node);

        // Act & Assert
        Assert.False(data.IsValue);
    }

    [Fact]
    public void IsValue_NodeBackedJsonArray_ReturnsFalse()
    {
        // Arrange
        var node = new JsonArray();
        var data = new JsonData(node);

        // Act & Assert
        Assert.False(data.IsValue);
    }

    [Fact]
    public void IsValue_ElementBackedString_ReturnsTrue()
    {
        // Arrange
        var element = JsonDocument.Parse("\"hello\"").RootElement;
        var data = new JsonData(element);

        // Act & Assert
        Assert.True(data.IsValue);
    }

    [Fact]
    public void IsValue_ElementBackedNumber_ReturnsTrue()
    {
        // Arrange
        var element = JsonDocument.Parse("99").RootElement;
        var data = new JsonData(element);

        // Act & Assert
        Assert.True(data.IsValue);
    }

    [Fact]
    public void IsValue_ElementBackedTrue_ReturnsTrue()
    {
        // Arrange
        var element = JsonDocument.Parse("true").RootElement;
        var data = new JsonData(element);

        // Act & Assert
        Assert.True(data.IsValue);
    }

    [Fact]
    public void IsValue_ElementBackedFalse_ReturnsTrue()
    {
        // Arrange
        var element = JsonDocument.Parse("false").RootElement;
        var data = new JsonData(element);

        // Act & Assert
        Assert.True(data.IsValue);
    }

    [Fact]
    public void IsValue_ElementBackedNull_ReturnsTrue()
    {
        // Arrange
        var element = JsonDocument.Parse("null").RootElement;
        var data = new JsonData(element);

        // Act & Assert
        Assert.True(data.IsValue);
    }

    [Fact]
    public void IsValue_ElementBackedObject_ReturnsFalse()
    {
        // Arrange
        var element = JsonDocument.Parse("{\"a\":1}").RootElement;
        var data = new JsonData(element);

        // Act & Assert
        Assert.False(data.IsValue);
    }

    [Fact]
    public void IsValue_ElementBackedArray_ReturnsFalse()
    {
        // Arrange
        var element = JsonDocument.Parse("[1,2,3]").RootElement;
        var data = new JsonData(element);

        // Act & Assert
        Assert.False(data.IsValue);
    }

    [Fact]
    public void IsValue_Uninitialized_ReturnsFalse()
    {
        // Arrange
        var data = new JsonData();

        // Act & Assert
        Assert.False(data.IsValue);
    }

    [Fact]
    public void IsValue_NullNodeBacked_ReturnsFalse()
    {
        // Arrange
        var data = new JsonData((JsonNode?)null);

        // Act & Assert
        Assert.False(data.IsValue);
    }

    // -- ThrowIfNotValue -------------------------------------------------------

    [Fact]
    public void ThrowIfNotValue_NodeBackedJsonValue_ReturnsTrue()
    {
        // Arrange
        var node = JsonValue.Create("text")!;
        var data = new JsonData(node);

        // Act
        var result = data.ThrowIfNotValue();

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void ThrowIfNotValue_ElementBackedNumber_ReturnsTrue()
    {
        // Arrange
        var element = JsonDocument.Parse("1").RootElement;
        var data = new JsonData(element);

        // Act
        var result = data.ThrowIfNotValue();

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void ThrowIfNotValue_NodeBackedObject_ThrowsInvalidOperationException()
    {
        // Arrange
        var node = new JsonObject();
        var data = new JsonData(node);

        // Act & Assert
        var ex = Assert.Throws<InvalidOperationException>(() => data.ThrowIfNotValue());
        Assert.Equal("Node is not a value.", ex.Message);
    }

    [Fact]
    public void ThrowIfNotValue_ElementBackedArray_ThrowsInvalidOperationException()
    {
        // Arrange
        var element = JsonDocument.Parse("[1,2]").RootElement;
        var data = new JsonData(element);

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => data.ThrowIfNotValue());
    }

    [Fact]
    public void ThrowIfNotValue_Uninitialized_ThrowsInvalidOperationException()
    {
        // Arrange
        var data = new JsonData();

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => data.ThrowIfNotValue());
    }

    [Fact]
    public void ThrowIfNotValue_ElementBackedObject_ThrowsInvalidOperationException()
    {
        // Arrange
        var element = JsonDocument.Parse("{\"x\":1}").RootElement;
        var data = new JsonData(element);

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => data.ThrowIfNotValue());
    }
}
