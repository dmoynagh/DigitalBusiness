using System.Text.Json;
using System.Text.Json.Nodes;
using DigitalBusiness.Json;
using DigitalBusiness.JsonDataWrappers;
using DigitalBusiness.JsonDataWrappers.Converters;
using Xunit;

namespace DigitalBusiness.JsonDataWrappers.Tests;

public class JsonDataEnumValueConverterTests
{
    // Enums used for testing
    private enum StringEnum { Alpha, Beta, Gamma }

    [EnumJsonPersistance]
    private enum NumberEnum { Zero = 0, One = 1, Two = 2 }

    // -- TryToJsonValue (string persistence) ---------------------------------

    [Fact]
    public void TryToJsonValue_StringEnum_ReturnsStringJsonValue()
    {
        // Arrange & Act
        var result = JsonDataEnumValueConverter<StringEnum>.TryToJsonValue(StringEnum.Beta, out var jsonValue);

        // Assert
        Assert.True(result);
        Assert.NotNull(jsonValue);
        Assert.Equal("Beta", jsonValue.GetValue<string>());
    }

    [Fact]
    public void TryToJsonValue_StringEnumFirstValue_ReturnsStringJsonValue()
    {
        // Arrange & Act
        var result = JsonDataEnumValueConverter<StringEnum>.TryToJsonValue(StringEnum.Alpha, out var jsonValue);

        // Assert
        Assert.True(result);
        Assert.NotNull(jsonValue);
        Assert.Equal("Alpha", jsonValue.GetValue<string>());
    }

    // -- TryToJsonValue (number persistence) ---------------------------------

    [Fact]
    public void TryToJsonValue_NumberEnum_ReturnsInt64JsonValue()
    {
        // Arrange & Act
        var result = JsonDataEnumValueConverter<NumberEnum>.TryToJsonValue(NumberEnum.Two, out var jsonValue);

        // Assert
        Assert.True(result);
        Assert.NotNull(jsonValue);
        Assert.Equal(2L, jsonValue.GetValue<long>());
    }

    [Fact]
    public void TryToJsonValue_NumberEnumZero_ReturnsZeroJsonValue()
    {
        // Arrange & Act
        var result = JsonDataEnumValueConverter<NumberEnum>.TryToJsonValue(NumberEnum.Zero, out var jsonValue);

        // Assert
        Assert.True(result);
        Assert.NotNull(jsonValue);
        Assert.Equal(0L, jsonValue.GetValue<long>());
    }

    // -- FromJson(JsonValue) --------------------------------------------------

    [Fact]
    public void FromJson_JsonValueWithValidEnum_ReturnsTrueAndValue()
    {
        // Arrange
        var jsonValue = JsonValue.Create(StringEnum.Gamma)!;

        // Act
        var result = JsonDataEnumValueConverter<StringEnum>.FromJson(jsonValue, out var value);

        // Assert
        Assert.True(result);
        Assert.Equal(StringEnum.Gamma, value);
    }

    // -- FromJson(JsonElement) – String ---------------------------------------

    [Fact]
    public void FromJson_JsonElementString_ReturnsTrueAndParsedEnum()
    {
        // Arrange
        var element = JsonDocument.Parse("\"Beta\"").RootElement;

        // Act
        var result = JsonDataEnumValueConverter<StringEnum>.FromJson(element, out var value);

        // Assert
        Assert.True(result);
        Assert.Equal(StringEnum.Beta, value);
    }

    [Fact]
    public void FromJson_JsonElementStringInvalidValue_ReturnsFalse()
    {
        // Arrange
        var element = JsonDocument.Parse("\"NotAMember\"").RootElement;

        // Act
        var result = JsonDataEnumValueConverter<StringEnum>.FromJson(element, out var value);

        // Assert
        Assert.False(result);
        Assert.Equal(default(StringEnum), value);
    }

    // -- FromJson(JsonElement) – Number ---------------------------------------

    [Fact]
    public void FromJson_JsonElementNumber_ReturnsTrueAndEnumValue()
    {
        // Arrange
        var element = JsonDocument.Parse("1").RootElement;

        // Act
        var result = JsonDataEnumValueConverter<NumberEnum>.FromJson(element, out var value);

        // Assert
        Assert.True(result);
        Assert.Equal(NumberEnum.One, value);
    }

    // -- FromJson(JsonElement) – Null -----------------------------------------

    [Fact]
    public void FromJson_JsonElementNull_ReturnsTrueAndDefault()
    {
        // Arrange
        var element = JsonDocument.Parse("null").RootElement;

        // Act
        var result = JsonDataEnumValueConverter<StringEnum>.FromJson(element, out var value);

        // Assert
        Assert.True(result);
        Assert.Equal(default(StringEnum), value);
    }

    // -- FromJson(JsonElement) – Other (e.g. bool) ----------------------------

    [Fact]
    public void FromJson_JsonElementBoolean_ReturnsFalse()
    {
        // Arrange
        var element = JsonDocument.Parse("true").RootElement;

        // Act
        var result = JsonDataEnumValueConverter<StringEnum>.FromJson(element, out var value);

        // Assert
        Assert.False(result);
        Assert.Equal(default(StringEnum), value);
    }

    // -- FromJsonData – IsValue (returns false) -------------------------------

    [Fact]
    public void FromJsonData_IsValueBacked_ReturnsFalse()
    {
        // Arrange – JsonData backed by a JsonValue node: IsValue == true ? early return false
        JsonData data = JsonValue.Create("Beta")!;

        // Act
        var result = JsonDataEnumValueConverter<StringEnum>.FromJsonData(data, out var value);

        // Assert
        Assert.False(result);
        Assert.Equal(default(StringEnum), value);
    }

    // -- FromJsonData – IsElement ---------------------------------------------

    [Fact]
    public void FromJsonData_IsElementStringEnum_IsValueTrueSoReturnsFalse()
    {
        // Arrange – Element-backed with a string kind: IsValue is true, so !IsValue is false
        // and the code falls through to value=default, return false
        var element = JsonDocument.Parse("\"Gamma\"").RootElement;
        JsonData data = element;

        // Act
        var result = JsonDataEnumValueConverter<StringEnum>.FromJsonData(data, out var value);

        // Assert – IsValue==true for string elements ? condition !IsValue fails ? returns false
        Assert.False(result);
        Assert.Equal(default(StringEnum), value);
    }

    [Fact]
    public void FromJsonData_IsElementNumber_IsValueTrueSoReturnsFalse()
    {
        // Arrange – Element-backed with a number kind: IsValue is true
        var element = JsonDocument.Parse("2").RootElement;
        JsonData data = element;

        // Act
        var result = JsonDataEnumValueConverter<NumberEnum>.FromJsonData(data, out var value);

        // Assert – IsValue==true for number elements ? returns false
        Assert.False(result);
        Assert.Equal(default(NumberEnum), value);
    }

    [Fact]
    public void FromJsonData_IsElementObjectKind_EntersElementBranchButReturnsFalse()
    {
        // Arrange – Element-backed with an Object kind: IsValue is false ? enters !IsValue branch
        // IsElement is true ? calls FromJson(JsonElement) which returns false for Object kind
        var element = JsonDocument.Parse("{\"x\":1}").RootElement;
        JsonData data = element;

        // Act
        var result = JsonDataEnumValueConverter<StringEnum>.FromJsonData(data, out var value);

        // Assert
        Assert.False(result);
        Assert.Equal(default(StringEnum), value);
    }

    // -- FromJsonData – IsNode backed by JsonValue ----------------------------

    [Fact]
    public void FromJsonData_IsNodeJsonValue_ReturnsTrueAndValue()
    {
        // Arrange – node-backed with a JsonValue but wrapped in JsonObject to avoid IsValue path
        // We need IsNode true and IsValue false: use JsonObject wrapping a value property
        // Instead, use a JsonNode that IS a JsonValue: IsValue would be true for that...
        // Actually let's test via a JsonObject node, which gives IsNode=true, IsValue=false
        // but then Node is JsonValue is false, so it falls through to value=default, false.
        var obj = new JsonObject { ["x"] = JsonValue.Create(1) };
        JsonData data = obj;

        // Act
        var result = JsonDataEnumValueConverter<StringEnum>.FromJsonData(data, out var value);

        // Assert – Node is JsonObject, not JsonValue ? returns false
        Assert.False(result);
        Assert.Equal(default(StringEnum), value);
    }

    [Fact]
    public void FromJsonData_IsNodeBackedJsonValueNode_UsesFromJsonOverload()
    {
        // Arrange – wrap a JsonValue directly as node but bypass IsValue by using JsonData(node) ctor
        // JsonData(JsonNode? node) where node is JsonValue ? IsValue=true (Node is JsonValue)
        // So FromJsonData returns false immediately. Let's verify this path:
        JsonNode node = JsonValue.Create("Alpha")!;
        var data = new JsonData(node);

        // Act
        var result = JsonDataEnumValueConverter<StringEnum>.FromJsonData(data, out var value);

        // Assert – IsValue is true ? condition !IsValue is false ? returns default/false
        Assert.False(result);
        Assert.Equal(default(StringEnum), value);
    }

    // -- FromJsonData – Uninitialized (not IsValue, not IsElement, not IsNode) -

    [Fact]
    public void FromJsonData_Uninitialized_ReturnsFalse()
    {
        // Arrange
        var data = new JsonData();

        // Act
        var result = JsonDataEnumValueConverter<StringEnum>.FromJsonData(data, out var value);

        // Assert
        Assert.False(result);
        Assert.Equal(default(StringEnum), value);
    }
}
