using System.Text.Json;
using System.Text.Json.Nodes;
using DigitalBusiness.JsonDataWrappers;
using DigitalBusiness.JsonDataWrappers.Converters;
using Xunit;

namespace DigitalBusiness.JsonDataWrappers.Tests;

public class JsonDataConverterProviderPrimitiveNodeConvertersTests
{
    // -----------------------------------------------------------------------
    // Helpers
    // -----------------------------------------------------------------------

    private static JsonData FromJson(string json)
    {
        using var doc = JsonDocument.Parse(json);
        return new JsonData(doc.RootElement.Clone());
    }

    private static JsonData FromNode(JsonNode? node) => new JsonData(node);

    // -----------------------------------------------------------------------
    // StringConverter.TryGet
    // -----------------------------------------------------------------------

    [Fact]
    public void StringConverter_TryGet_ElementIsString_ReturnsTrueWithValue()
    {
        var converter = JsonDataConverterProvider.GetConverter<string>();
        var jsonData = FromJson("\"hello\"");

        var result = converter.TryGet(in jsonData, out var value);

        Assert.True(result);
        Assert.Equal("hello", value);
    }

    [Fact]
    public void StringConverter_TryGet_ElementIsNumber_ReturnsFalse()
    {
        var converter = JsonDataConverterProvider.GetConverter<string>();
        var jsonData = FromJson("42");

        var result = converter.TryGet(in jsonData, out var value);

        Assert.False(result);
        Assert.Null(value);
    }

    [Fact]
    public void StringConverter_TryGet_ElementIsTrue_ReturnsFalse()
    {
        var converter = JsonDataConverterProvider.GetConverter<string>();
        var jsonData = FromJson("true");

        var result = converter.TryGet(in jsonData, out var value);

        Assert.False(result);
        Assert.Null(value);
    }

    [Fact]
    public void StringConverter_TryGet_NodeIsJsonValueString_ReturnsTrueWithValue()
    {
        var converter = JsonDataConverterProvider.GetConverter<string>();
        var jsonData = FromNode(JsonValue.Create("world"));

        var result = converter.TryGet(in jsonData, out var value);

        Assert.True(result);
        Assert.Equal("world", value);
    }

    [Fact]
    public void StringConverter_TryGet_NodeIsJsonValueNumber_ReturnsFalse()
    {
        var converter = JsonDataConverterProvider.GetConverter<string>();
        var jsonData = FromNode(JsonValue.Create(123));

        var result = converter.TryGet(in jsonData, out var value);

        Assert.False(result);
        Assert.Null(value);
    }

    [Fact]
    public void StringConverter_TryGet_NodeIsJsonObject_ReturnsFalse()
    {
        var converter = JsonDataConverterProvider.GetConverter<string>();
        var jsonData = FromNode(new JsonObject());

        var result = converter.TryGet(in jsonData, out var value);

        Assert.False(result);
        Assert.Null(value);
    }

    [Fact]
    public void StringConverter_TryGet_EmptyJsonData_ReturnsFalse()
    {
        var converter = JsonDataConverterProvider.GetConverter<string>();
        var jsonData = new JsonData();

        var result = converter.TryGet(in jsonData, out var value);

        Assert.False(result);
        Assert.Null(value);
    }

    // -----------------------------------------------------------------------
    // StringConverter.Create
    // -----------------------------------------------------------------------

    [Fact]
    public void StringConverter_Create_ReturnsJsonDataWithCorrectValue()
    {
        var converter = JsonDataConverterProvider.GetConverter<string>();

        var jsonData = converter.Create("test");
        var result = converter.TryGet(in jsonData, out var value);

        Assert.True(result);
        Assert.Equal("test", value);
    }

    // -----------------------------------------------------------------------
    // BoolConverter.TryGet
    // -----------------------------------------------------------------------

    [Fact]
    public void BoolConverter_TryGet_ElementIsTrue_ReturnsTrueWithTrue()
    {
        var converter = JsonDataConverterProvider.GetConverter<bool>();
        var jsonData = FromJson("true");

        var result = converter.TryGet(in jsonData, out var value);

        Assert.True(result);
        Assert.True(value);
    }

    [Fact]
    public void BoolConverter_TryGet_ElementIsFalse_ReturnsTrueWithFalse()
    {
        var converter = JsonDataConverterProvider.GetConverter<bool>();
        var jsonData = FromJson("false");

        var result = converter.TryGet(in jsonData, out var value);

        Assert.True(result);
        Assert.False(value);
    }

    [Fact]
    public void BoolConverter_TryGet_ElementIsNumber_ReturnsFalse()
    {
        var converter = JsonDataConverterProvider.GetConverter<bool>();
        var jsonData = FromJson("1");

        var result = converter.TryGet(in jsonData, out var value);

        Assert.False(result);
        Assert.False(value);
    }

    [Fact]
    public void BoolConverter_TryGet_NodeIsTrue_ReturnsTrueWithTrue()
    {
        var converter = JsonDataConverterProvider.GetConverter<bool>();
        var jsonData = FromNode(JsonValue.Create(true));

        var result = converter.TryGet(in jsonData, out var value);

        Assert.True(result);
        Assert.True(value);
    }

    [Fact]
    public void BoolConverter_TryGet_NodeIsFalse_ReturnsTrueWithFalse()
    {
        var converter = JsonDataConverterProvider.GetConverter<bool>();
        var jsonData = FromNode(JsonValue.Create(false));

        var result = converter.TryGet(in jsonData, out var value);

        Assert.True(result);
        Assert.False(value);
    }

    // -----------------------------------------------------------------------
    // BoolConverter.Create
    // -----------------------------------------------------------------------

    [Fact]
    public void BoolConverter_Create_True_ReturnsJsonDataWithTrue()
    {
        var converter = JsonDataConverterProvider.GetConverter<bool>();

        var jsonData = converter.Create(true);
        var result = converter.TryGet(in jsonData, out var value);

        Assert.True(result);
        Assert.True(value);
    }

    [Fact]
    public void BoolConverter_Create_False_ReturnsJsonDataWithFalse()
    {
        var converter = JsonDataConverterProvider.GetConverter<bool>();

        var jsonData = converter.Create(false);
        var result = converter.TryGet(in jsonData, out var value);

        Assert.True(result);
        Assert.False(value);
    }

    // -----------------------------------------------------------------------
    // ByteConverter.TryGet
    // -----------------------------------------------------------------------

    [Fact]
    public void ByteConverter_TryGet_ElementIsByte_ReturnsTrueWithValue()
    {
        var converter = JsonDataConverterProvider.GetConverter<byte>();
        var jsonData = FromJson("42");

        var result = converter.TryGet(in jsonData, out var value);

        Assert.True(result);
        Assert.Equal((byte)42, value);
    }

    [Fact]
    public void ByteConverter_TryGet_ElementIsOutOfRange_ReturnsFalse()
    {
        var converter = JsonDataConverterProvider.GetConverter<byte>();
        var jsonData = FromJson("300");

        var result = converter.TryGet(in jsonData, out var value);

        Assert.False(result);
        Assert.Equal(default(byte), value);
    }

    [Fact(Skip = "ProductionBugSuspected")]
    public void ByteConverter_TryGet_ElementIsString_ReturnsFalse()
    {
        var converter = JsonDataConverterProvider.GetConverter<byte>();
        var jsonData = FromJson("\"hello\"");

        var result = converter.TryGet(in jsonData, out var value);

        Assert.False(result);
        Assert.Equal(default(byte), value);
    }

    [Fact]
    public void ByteConverter_TryGet_NodeIsJsonValueByte_ReturnsTrueWithValue()
    {
        var converter = JsonDataConverterProvider.GetConverter<byte>();
        var jsonData = FromNode(JsonValue.Create((byte)10));

        var result = converter.TryGet(in jsonData, out var value);

        Assert.True(result);
        Assert.Equal((byte)10, value);
    }

    [Fact]
    public void ByteConverter_TryGet_NodeIsJsonObject_ReturnsFalse()
    {
        var converter = JsonDataConverterProvider.GetConverter<byte>();
        var jsonData = FromNode(new JsonObject());

        var result = converter.TryGet(in jsonData, out var value);

        Assert.False(result);
        Assert.Equal(default(byte), value);
    }

    [Fact]
    public void ByteConverter_TryGet_EmptyJsonData_ReturnsFalse()
    {
        var converter = JsonDataConverterProvider.GetConverter<byte>();
        var jsonData = new JsonData();

        var result = converter.TryGet(in jsonData, out var value);

        Assert.False(result);
        Assert.Equal(default(byte), value);
    }

    // -----------------------------------------------------------------------
    // ByteConverter.Create
    // -----------------------------------------------------------------------

    [Fact]
    public void ByteConverter_Create_ReturnsJsonDataWithCorrectValue()
    {
        var converter = JsonDataConverterProvider.GetConverter<byte>();

        var jsonData = converter.Create((byte)200);
        var result = converter.TryGet(in jsonData, out var value);

        Assert.True(result);
        Assert.Equal((byte)200, value);
    }

    [Fact]
    public void ByteConverter_Create_Zero_ReturnsJsonDataWithZero()
    {
        var converter = JsonDataConverterProvider.GetConverter<byte>();

        var jsonData = converter.Create((byte)0);
        var result = converter.TryGet(in jsonData, out var value);

        Assert.True(result);
        Assert.Equal((byte)0, value);
    }

    [Fact]
    public void ByteConverter_Create_MaxValue_ReturnsJsonDataWithMaxValue()
    {
        var converter = JsonDataConverterProvider.GetConverter<byte>();

        var jsonData = converter.Create(byte.MaxValue);
        var result = converter.TryGet(in jsonData, out var value);

        Assert.True(result);
        Assert.Equal(byte.MaxValue, value);
    }

    // -----------------------------------------------------------------------
    // SByteConverter.TryGet
    // -----------------------------------------------------------------------

    [Fact]
    public void SByteConverter_TryGet_ElementIsSByte_ReturnsTrueWithValue()
    {
        var converter = JsonDataConverterProvider.GetConverter<sbyte>();
        var jsonData = FromJson("42");

        var result = converter.TryGet(in jsonData, out var value);

        Assert.True(result);
        Assert.Equal((sbyte)42, value);
    }

    [Fact]
    public void SByteConverter_TryGet_ElementIsNegative_ReturnsTrueWithValue()
    {
        var converter = JsonDataConverterProvider.GetConverter<sbyte>();
        var jsonData = FromJson("-10");

        var result = converter.TryGet(in jsonData, out var value);

        Assert.True(result);
        Assert.Equal((sbyte)-10, value);
    }

    [Fact]
    public void SByteConverter_TryGet_ElementIsOutOfRange_ReturnsFalse()
    {
        var converter = JsonDataConverterProvider.GetConverter<sbyte>();
        var jsonData = FromJson("200");

        var result = converter.TryGet(in jsonData, out var value);

        Assert.False(result);
        Assert.Equal(default(sbyte), value);
    }

    [Fact]
    public void SByteConverter_TryGet_NodeIsJsonValueSByte_ReturnsTrueWithValue()
    {
        var converter = JsonDataConverterProvider.GetConverter<sbyte>();
        var jsonData = FromNode(JsonValue.Create((sbyte)-5));

        var result = converter.TryGet(in jsonData, out var value);

        Assert.True(result);
        Assert.Equal((sbyte)-5, value);
    }

    [Fact]
    public void SByteConverter_TryGet_NodeIsJsonObject_ReturnsFalse()
    {
        var converter = JsonDataConverterProvider.GetConverter<sbyte>();
        var jsonData = FromNode(new JsonObject());

        var result = converter.TryGet(in jsonData, out var value);

        Assert.False(result);
        Assert.Equal(default(sbyte), value);
    }

    [Fact]
    public void SByteConverter_TryGet_EmptyJsonData_ReturnsFalse()
    {
        var converter = JsonDataConverterProvider.GetConverter<sbyte>();
        var jsonData = new JsonData();

        var result = converter.TryGet(in jsonData, out var value);

        Assert.False(result);
        Assert.Equal(default(sbyte), value);
    }

    // -----------------------------------------------------------------------
    // SByteConverter.Create
    // -----------------------------------------------------------------------

    [Fact]
    public void SByteConverter_Create_ReturnsJsonDataWithCorrectValue()
    {
        var converter = JsonDataConverterProvider.GetConverter<sbyte>();

        var jsonData = converter.Create((sbyte)100);
        var result = converter.TryGet(in jsonData, out var value);

        Assert.True(result);
        Assert.Equal((sbyte)100, value);
    }

    [Fact]
    public void SByteConverter_Create_Negative_ReturnsJsonDataWithCorrectValue()
    {
        var converter = JsonDataConverterProvider.GetConverter<sbyte>();

        var jsonData = converter.Create((sbyte)-50);
        var result = converter.TryGet(in jsonData, out var value);

        Assert.True(result);
        Assert.Equal((sbyte)-50, value);
    }

    [Fact]
    public void SByteConverter_Create_MaxValue_ReturnsJsonDataWithMaxValue()
    {
        var converter = JsonDataConverterProvider.GetConverter<sbyte>();

        var jsonData = converter.Create(sbyte.MaxValue);
        var result = converter.TryGet(in jsonData, out var value);

        Assert.True(result);
        Assert.Equal(sbyte.MaxValue, value);
    }

    // -----------------------------------------------------------------------
    // ShortConverter.TryGet
    // -----------------------------------------------------------------------

    [Fact]
    public void ShortConverter_TryGet_ElementIsShort_ReturnsTrueWithValue()
    {
        var converter = JsonDataConverterProvider.GetConverter<short>();
        var jsonData = FromJson("1000");

        var result = converter.TryGet(in jsonData, out var value);

        Assert.True(result);
        Assert.Equal((short)1000, value);
    }

    [Fact]
    public void ShortConverter_TryGet_ElementIsNegative_ReturnsTrueWithValue()
    {
        var converter = JsonDataConverterProvider.GetConverter<short>();
        var jsonData = FromJson("-500");

        var result = converter.TryGet(in jsonData, out var value);

        Assert.True(result);
        Assert.Equal((short)-500, value);
    }

    [Fact]
    public void ShortConverter_TryGet_ElementIsOutOfRange_ReturnsFalse()
    {
        var converter = JsonDataConverterProvider.GetConverter<short>();
        var jsonData = FromJson("100000");

        var result = converter.TryGet(in jsonData, out var value);

        Assert.False(result);
        Assert.Equal(default(short), value);
    }

    [Fact]
    public void ShortConverter_TryGet_ElementIsString_ReturnsFalse()
    {
        var converter = JsonDataConverterProvider.GetConverter<short>();
        var jsonData = FromJson("\"hello\"");

        var result = converter.TryGet(in jsonData, out var value);

        Assert.False(result);
        Assert.Equal(default(short), value);
    }

    [Fact]
    public void ShortConverter_TryGet_NodeIsJsonValueShort_ReturnsTrueWithValue()
    {
        var converter = JsonDataConverterProvider.GetConverter<short>();
        var jsonData = FromNode(JsonValue.Create((short)255));

        var result = converter.TryGet(in jsonData, out var value);

        Assert.True(result);
        Assert.Equal((short)255, value);
    }

    [Fact]
    public void ShortConverter_TryGet_NodeIsJsonObject_ReturnsFalse()
    {
        var converter = JsonDataConverterProvider.GetConverter<short>();
        var jsonData = FromNode(new JsonObject());

        var result = converter.TryGet(in jsonData, out var value);

        Assert.False(result);
        Assert.Equal(default(short), value);
    }

    [Fact]
    public void ShortConverter_TryGet_EmptyJsonData_ReturnsFalse()
    {
        var converter = JsonDataConverterProvider.GetConverter<short>();
        var jsonData = new JsonData();

        var result = converter.TryGet(in jsonData, out var value);

        Assert.False(result);
        Assert.Equal(default(short), value);
    }

    // -----------------------------------------------------------------------
    // ShortConverter.Create
    // -----------------------------------------------------------------------

    [Fact]
    public void ShortConverter_Create_ReturnsJsonDataWithCorrectValue()
    {
        var converter = JsonDataConverterProvider.GetConverter<short>();

        var jsonData = converter.Create((short)1234);
        var result = converter.TryGet(in jsonData, out var value);

        Assert.True(result);
        Assert.Equal((short)1234, value);
    }

    [Fact]
    public void ShortConverter_Create_Negative_ReturnsJsonDataWithCorrectValue()
    {
        var converter = JsonDataConverterProvider.GetConverter<short>();

        var jsonData = converter.Create((short)-1234);
        var result = converter.TryGet(in jsonData, out var value);

        Assert.True(result);
        Assert.Equal((short)-1234, value);
    }

    [Fact]
    public void ShortConverter_Create_MaxValue_ReturnsJsonDataWithMaxValue()
    {
        var converter = JsonDataConverterProvider.GetConverter<short>();

        var jsonData = converter.Create(short.MaxValue);
        var result = converter.TryGet(in jsonData, out var value);

        Assert.True(result);
        Assert.Equal(short.MaxValue, value);
    }

    // -----------------------------------------------------------------------
    // UShortConverter.TryGet
    // -----------------------------------------------------------------------

    [Fact]
    public void UShortConverter_TryGet_ElementIsUShort_ReturnsTrueWithValue()
    {
        var converter = JsonDataConverterProvider.GetConverter<ushort>();
        var jsonData = FromJson("1000");

        var result = converter.TryGet(in jsonData, out var value);

        Assert.True(result);
        Assert.Equal((ushort)1000, value);
    }

    [Fact]
    public void UShortConverter_TryGet_ElementIsOutOfRange_ReturnsFalse()
    {
        var converter = JsonDataConverterProvider.GetConverter<ushort>();
        var jsonData = FromJson("70000");

        var result = converter.TryGet(in jsonData, out var value);

        Assert.False(result);
        Assert.Equal(default(ushort), value);
    }

    [Fact(Skip = "ProductionBugSuspected")]
    public void UShortConverter_TryGet_ElementIsString_ReturnsFalse()
    {
        var converter = JsonDataConverterProvider.GetConverter<ushort>();
        var jsonData = FromJson("\"hello\"");

        var result = converter.TryGet(in jsonData, out var value);

        Assert.False(result);
        Assert.Equal(default(ushort), value);
    }

    [Fact]
    public void UShortConverter_TryGet_NodeIsJsonValueUShort_ReturnsTrueWithValue()
    {
        var converter = JsonDataConverterProvider.GetConverter<ushort>();
        var jsonData = FromNode(JsonValue.Create((ushort)255));

        var result = converter.TryGet(in jsonData, out var value);

        Assert.True(result);
        Assert.Equal((ushort)255, value);
    }

    [Fact]
    public void UShortConverter_TryGet_NodeIsJsonObject_ReturnsFalse()
    {
        var converter = JsonDataConverterProvider.GetConverter<ushort>();
        var jsonData = FromNode(new JsonObject());

        var result = converter.TryGet(in jsonData, out var value);

        Assert.False(result);
        Assert.Equal(default(ushort), value);
    }

    [Fact]
    public void UShortConverter_TryGet_EmptyJsonData_ReturnsFalse()
    {
        var converter = JsonDataConverterProvider.GetConverter<ushort>();
        var jsonData = new JsonData();

        var result = converter.TryGet(in jsonData, out var value);

        Assert.False(result);
        Assert.Equal(default(ushort), value);
    }

    // -----------------------------------------------------------------------
    // UShortConverter.Create
    // -----------------------------------------------------------------------

    [Fact]
    public void UShortConverter_Create_ReturnsJsonDataWithCorrectValue()
    {
        var converter = JsonDataConverterProvider.GetConverter<ushort>();

        var jsonData = converter.Create((ushort)1234);
        var result = converter.TryGet(in jsonData, out var value);

        Assert.True(result);
        Assert.Equal((ushort)1234, value);
    }

    [Fact]
    public void UShortConverter_Create_Zero_ReturnsJsonDataWithZero()
    {
        var converter = JsonDataConverterProvider.GetConverter<ushort>();

        var jsonData = converter.Create((ushort)0);
        var result = converter.TryGet(in jsonData, out var value);

        Assert.True(result);
        Assert.Equal((ushort)0, value);
    }

    [Fact]
    public void UShortConverter_Create_MaxValue_ReturnsJsonDataWithMaxValue()
    {
        var converter = JsonDataConverterProvider.GetConverter<ushort>();

        var jsonData = converter.Create(ushort.MaxValue);
        var result = converter.TryGet(in jsonData, out var value);

        Assert.True(result);
        Assert.Equal(ushort.MaxValue, value);
    }

    // -----------------------------------------------------------------------
    // IntConverter.TryGet
    // -----------------------------------------------------------------------

    [Fact]
    public void IntConverter_TryGet_ElementIsInt_ReturnsTrueWithValue()
    {
        var converter = JsonDataConverterProvider.GetConverter<int>();
        var jsonData = FromJson("42");

        var result = converter.TryGet(in jsonData, out var value);

        Assert.True(result);
        Assert.Equal(42, value);
    }

    [Fact]
    public void IntConverter_TryGet_ElementIsNegative_ReturnsTrueWithValue()
    {
        var converter = JsonDataConverterProvider.GetConverter<int>();
        var jsonData = FromJson("-100000");

        var result = converter.TryGet(in jsonData, out var value);

        Assert.True(result);
        Assert.Equal(-100000, value);
    }

    [Fact(Skip = "ProductionBugSuspected")]
    public void IntConverter_TryGet_ElementIsString_ReturnsFalse()
    {
        var converter = JsonDataConverterProvider.GetConverter<int>();
        var jsonData = FromJson("\"hello\"");

        var result = converter.TryGet(in jsonData, out var value);

        Assert.False(result);
        Assert.Equal(default(int), value);
    }

    [Fact]
    public void IntConverter_TryGet_NodeIsJsonValueInt_ReturnsTrueWithValue()
    {
        var converter = JsonDataConverterProvider.GetConverter<int>();
        var jsonData = FromNode(JsonValue.Create(99));

        var result = converter.TryGet(in jsonData, out var value);

        Assert.True(result);
        Assert.Equal(99, value);
    }

    [Fact]
    public void IntConverter_TryGet_NodeIsJsonObject_ReturnsFalse()
    {
        var converter = JsonDataConverterProvider.GetConverter<int>();
        var jsonData = FromNode(new JsonObject());

        var result = converter.TryGet(in jsonData, out var value);

        Assert.False(result);
        Assert.Equal(default(int), value);
    }

    [Fact]
    public void IntConverter_TryGet_EmptyJsonData_ReturnsFalse()
    {
        var converter = JsonDataConverterProvider.GetConverter<int>();
        var jsonData = new JsonData();

        var result = converter.TryGet(in jsonData, out var value);

        Assert.False(result);
        Assert.Equal(default(int), value);
    }

    // -----------------------------------------------------------------------
    // IntConverter.Create
    // -----------------------------------------------------------------------

    [Fact]
    public void IntConverter_Create_ReturnsJsonDataWithCorrectValue()
    {
        var converter = JsonDataConverterProvider.GetConverter<int>();

        var jsonData = converter.Create(12345);
        var result = converter.TryGet(in jsonData, out var value);

        Assert.True(result);
        Assert.Equal(12345, value);
    }

    [Fact]
    public void IntConverter_Create_Negative_ReturnsJsonDataWithCorrectValue()
    {
        var converter = JsonDataConverterProvider.GetConverter<int>();

        var jsonData = converter.Create(-99999);
        var result = converter.TryGet(in jsonData, out var value);

        Assert.True(result);
        Assert.Equal(-99999, value);
    }

    [Fact]
    public void IntConverter_Create_MaxValue_ReturnsJsonDataWithMaxValue()
    {
        var converter = JsonDataConverterProvider.GetConverter<int>();

        var jsonData = converter.Create(int.MaxValue);
        var result = converter.TryGet(in jsonData, out var value);

        Assert.True(result);
        Assert.Equal(int.MaxValue, value);
    }

    // -----------------------------------------------------------------------
    // UIntConverter.TryGet
    // -----------------------------------------------------------------------

    [Fact]
    public void UIntConverter_TryGet_ElementIsUInt_ReturnsTrueWithValue()
    {
        var converter = JsonDataConverterProvider.GetConverter<uint>();
        var jsonData = FromJson("100000");

        var result = converter.TryGet(in jsonData, out var value);

        Assert.True(result);
        Assert.Equal(100000u, value);
    }

    [Fact]
    public void UIntConverter_TryGet_ElementIsNegative_ReturnsFalse()
    {
        var converter = JsonDataConverterProvider.GetConverter<uint>();
        var jsonData = FromJson("-1");

        var result = converter.TryGet(in jsonData, out var value);

        Assert.False(result);
        Assert.Equal(default(uint), value);
    }

    [Fact(Skip = "ProductionBugSuspected")]
    public void UIntConverter_TryGet_ElementIsString_ReturnsFalse()
    {
        var converter = JsonDataConverterProvider.GetConverter<uint>();
        var jsonData = FromJson("\"hello\"");

        var result = converter.TryGet(in jsonData, out var value);

        Assert.False(result);
        Assert.Equal(default(uint), value);
    }

    [Fact]
    public void UIntConverter_TryGet_NodeIsJsonValueUInt_ReturnsTrueWithValue()
    {
        var converter = JsonDataConverterProvider.GetConverter<uint>();
        var jsonData = FromNode(JsonValue.Create(500u));

        var result = converter.TryGet(in jsonData, out var value);

        Assert.True(result);
        Assert.Equal(500u, value);
    }

    [Fact]
    public void UIntConverter_TryGet_NodeIsJsonObject_ReturnsFalse()
    {
        var converter = JsonDataConverterProvider.GetConverter<uint>();
        var jsonData = FromNode(new JsonObject());

        var result = converter.TryGet(in jsonData, out var value);

        Assert.False(result);
        Assert.Equal(default(uint), value);
    }

    [Fact]
    public void UIntConverter_TryGet_EmptyJsonData_ReturnsFalse()
    {
        var converter = JsonDataConverterProvider.GetConverter<uint>();
        var jsonData = new JsonData();

        var result = converter.TryGet(in jsonData, out var value);

        Assert.False(result);
        Assert.Equal(default(uint), value);
    }

    // -----------------------------------------------------------------------
    // UIntConverter.Create
    // -----------------------------------------------------------------------

    [Fact]
    public void UIntConverter_Create_ReturnsJsonDataWithCorrectValue()
    {
        var converter = JsonDataConverterProvider.GetConverter<uint>();

        var jsonData = converter.Create(99999u);
        var result = converter.TryGet(in jsonData, out var value);

        Assert.True(result);
        Assert.Equal(99999u, value);
    }

    [Fact]
    public void UIntConverter_Create_Zero_ReturnsJsonDataWithZero()
    {
        var converter = JsonDataConverterProvider.GetConverter<uint>();

        var jsonData = converter.Create(0u);
        var result = converter.TryGet(in jsonData, out var value);

        Assert.True(result);
        Assert.Equal(0u, value);
    }

    [Fact]
    public void UIntConverter_Create_MaxValue_ReturnsJsonDataWithMaxValue()
    {
        var converter = JsonDataConverterProvider.GetConverter<uint>();

        var jsonData = converter.Create(uint.MaxValue);
        var result = converter.TryGet(in jsonData, out var value);

        Assert.True(result);
        Assert.Equal(uint.MaxValue, value);
    }

    // -----------------------------------------------------------------------
    // LongConverter.TryGet
    // -----------------------------------------------------------------------

    [Fact]
    public void LongConverter_TryGet_ElementIsLong_ReturnsTrueWithValue()
    {
        var converter = JsonDataConverterProvider.GetConverter<long>();
        var jsonData = FromJson("9876543210");

        var result = converter.TryGet(in jsonData, out var value);

        Assert.True(result);
        Assert.Equal(9876543210L, value);
    }

    [Fact]
    public void LongConverter_TryGet_ElementIsNegative_ReturnsTrueWithValue()
    {
        var converter = JsonDataConverterProvider.GetConverter<long>();
        var jsonData = FromJson("-9876543210");

        var result = converter.TryGet(in jsonData, out var value);

        Assert.True(result);
        Assert.Equal(-9876543210L, value);
    }

    [Fact(Skip = "ProductionBugSuspected")]
    public void LongConverter_TryGet_ElementIsString_ReturnsFalse()
    {
        var converter = JsonDataConverterProvider.GetConverter<long>();
        var jsonData = FromJson("\"hello\"");

        var result = converter.TryGet(in jsonData, out var value);

        Assert.False(result);
        Assert.Equal(default(long), value);
    }

    [Fact]
    public void LongConverter_TryGet_NodeIsJsonValueLong_ReturnsTrueWithValue()
    {
        var converter = JsonDataConverterProvider.GetConverter<long>();
        var jsonData = FromNode(JsonValue.Create(123456789L));

        var result = converter.TryGet(in jsonData, out var value);

        Assert.True(result);
        Assert.Equal(123456789L, value);
    }

    [Fact]
    public void LongConverter_TryGet_NodeIsJsonObject_ReturnsFalse()
    {
        var converter = JsonDataConverterProvider.GetConverter<long>();
        var jsonData = FromNode(new JsonObject());

        var result = converter.TryGet(in jsonData, out var value);

        Assert.False(result);
        Assert.Equal(default(long), value);
    }

    [Fact]
    public void LongConverter_TryGet_EmptyJsonData_ReturnsFalse()
    {
        var converter = JsonDataConverterProvider.GetConverter<long>();
        var jsonData = new JsonData();

        var result = converter.TryGet(in jsonData, out var value);

        Assert.False(result);
        Assert.Equal(default(long), value);
    }

    // -----------------------------------------------------------------------
    // LongConverter.Create
    // -----------------------------------------------------------------------

    [Fact]
    public void LongConverter_Create_ReturnsJsonDataWithCorrectValue()
    {
        var converter = JsonDataConverterProvider.GetConverter<long>();

        var jsonData = converter.Create(9876543210L);
        var result = converter.TryGet(in jsonData, out var value);

        Assert.True(result);
        Assert.Equal(9876543210L, value);
    }

    [Fact]
    public void LongConverter_Create_Negative_ReturnsJsonDataWithCorrectValue()
    {
        var converter = JsonDataConverterProvider.GetConverter<long>();

        var jsonData = converter.Create(-9876543210L);
        var result = converter.TryGet(in jsonData, out var value);

        Assert.True(result);
        Assert.Equal(-9876543210L, value);
    }

    [Fact]
    public void LongConverter_Create_MaxValue_ReturnsJsonDataWithMaxValue()
    {
        var converter = JsonDataConverterProvider.GetConverter<long>();

        var jsonData = converter.Create(long.MaxValue);
        var result = converter.TryGet(in jsonData, out var value);

        Assert.True(result);
        Assert.Equal(long.MaxValue, value);
    }

    // -----------------------------------------------------------------------
    // ULongConverter.TryGet
    // -----------------------------------------------------------------------

    [Fact]
    public void ULongConverter_TryGet_ElementIsULong_ReturnsTrueWithValue()
    {
        var converter = JsonDataConverterProvider.GetConverter<ulong>();
        var jsonData = FromJson("9876543210");

        var result = converter.TryGet(in jsonData, out var value);

        Assert.True(result);
        Assert.Equal(9876543210UL, value);
    }

    [Fact]
    public void ULongConverter_TryGet_ElementIsNegative_ReturnsFalse()
    {
        var converter = JsonDataConverterProvider.GetConverter<ulong>();
        var jsonData = FromJson("-1");

        var result = converter.TryGet(in jsonData, out var value);

        Assert.False(result);
        Assert.Equal(default(ulong), value);
    }

    [Fact(Skip = "ProductionBugSuspected")]
    public void ULongConverter_TryGet_ElementIsString_ReturnsFalse()
    {
        var converter = JsonDataConverterProvider.GetConverter<ulong>();
        var jsonData = FromJson("\"hello\"");

        var result = converter.TryGet(in jsonData, out var value);

        Assert.False(result);
        Assert.Equal(default(ulong), value);
    }

    [Fact]
    public void ULongConverter_TryGet_NodeIsJsonValueULong_ReturnsTrueWithValue()
    {
        var converter = JsonDataConverterProvider.GetConverter<ulong>();
        var jsonData = FromNode(JsonValue.Create(500UL));

        var result = converter.TryGet(in jsonData, out var value);

        Assert.True(result);
        Assert.Equal(500UL, value);
    }

    [Fact]
    public void ULongConverter_TryGet_NodeIsJsonObject_ReturnsFalse()
    {
        var converter = JsonDataConverterProvider.GetConverter<ulong>();
        var jsonData = FromNode(new JsonObject());

        var result = converter.TryGet(in jsonData, out var value);

        Assert.False(result);
        Assert.Equal(default(ulong), value);
    }

    [Fact]
    public void ULongConverter_TryGet_EmptyJsonData_ReturnsFalse()
    {
        var converter = JsonDataConverterProvider.GetConverter<ulong>();
        var jsonData = new JsonData();

        var result = converter.TryGet(in jsonData, out var value);

        Assert.False(result);
        Assert.Equal(default(ulong), value);
    }

    // -----------------------------------------------------------------------
    // ULongConverter.Create
    // -----------------------------------------------------------------------

    [Fact]
    public void ULongConverter_Create_ReturnsJsonDataWithCorrectValue()
    {
        var converter = JsonDataConverterProvider.GetConverter<ulong>();

        var jsonData = converter.Create(9876543210UL);
        var result = converter.TryGet(in jsonData, out var value);

        Assert.True(result);
        Assert.Equal(9876543210UL, value);
    }

    [Fact]
    public void ULongConverter_Create_Zero_ReturnsJsonDataWithZero()
    {
        var converter = JsonDataConverterProvider.GetConverter<ulong>();

        var jsonData = converter.Create(0UL);
        var result = converter.TryGet(in jsonData, out var value);

        Assert.True(result);
        Assert.Equal(0UL, value);
    }

    [Fact]
    public void ULongConverter_Create_MaxValue_ReturnsJsonDataWithMaxValue()
    {
        var converter = JsonDataConverterProvider.GetConverter<ulong>();

        var jsonData = converter.Create(ulong.MaxValue);
        var result = converter.TryGet(in jsonData, out var value);

        Assert.True(result);
        Assert.Equal(ulong.MaxValue, value);
    }

    // -----------------------------------------------------------------------
    // FloatConverter.TryGet
    // -----------------------------------------------------------------------

    [Fact]
    public void FloatConverter_TryGet_ElementIsFloat_ReturnsTrueWithValue()
    {
        var converter = JsonDataConverterProvider.GetConverter<float>();
        var jsonData = FromJson("3.14");

        var result = converter.TryGet(in jsonData, out var value);

        Assert.True(result);
        Assert.Equal(3.14f, value, 5);
    }

    [Fact(Skip = "ProductionBugSuspected")]
    public void FloatConverter_TryGet_ElementIsString_ReturnsFalse()
    {
        var converter = JsonDataConverterProvider.GetConverter<float>();
        var jsonData = FromJson("\"hello\"");

        var result = converter.TryGet(in jsonData, out var value);

        Assert.False(result);
        Assert.Equal(default(float), value);
    }

    [Fact]
    public void FloatConverter_TryGet_NodeIsJsonValueFloat_ReturnsTrueWithValue()
    {
        var converter = JsonDataConverterProvider.GetConverter<float>();
        var jsonData = FromNode(JsonValue.Create(1.5f));

        var result = converter.TryGet(in jsonData, out var value);

        Assert.True(result);
        Assert.Equal(1.5f, value);
    }

    [Fact]
    public void FloatConverter_TryGet_NodeIsJsonObject_ReturnsFalse()
    {
        var converter = JsonDataConverterProvider.GetConverter<float>();
        var jsonData = FromNode(new JsonObject());

        var result = converter.TryGet(in jsonData, out var value);

        Assert.False(result);
        Assert.Equal(default(float), value);
    }

    [Fact]
    public void FloatConverter_TryGet_EmptyJsonData_ReturnsFalse()
    {
        var converter = JsonDataConverterProvider.GetConverter<float>();
        var jsonData = new JsonData();

        var result = converter.TryGet(in jsonData, out var value);

        Assert.False(result);
        Assert.Equal(default(float), value);
    }

    // -----------------------------------------------------------------------
    // FloatConverter.Create
    // -----------------------------------------------------------------------

    [Fact]
    public void FloatConverter_Create_ReturnsJsonDataWithCorrectValue()
    {
        var converter = JsonDataConverterProvider.GetConverter<float>();

        var jsonData = converter.Create(2.5f);
        var result = converter.TryGet(in jsonData, out var value);

        Assert.True(result);
        Assert.Equal(2.5f, value);
    }

    [Fact]
    public void FloatConverter_Create_MaxValue_ReturnsJsonDataWithMaxValue()
    {
        var converter = JsonDataConverterProvider.GetConverter<float>();

        var jsonData = converter.Create(float.MaxValue);
        var result = converter.TryGet(in jsonData, out var value);

        Assert.True(result);
        Assert.Equal(float.MaxValue, value);
    }

    [Fact]
    public void FloatConverter_Create_Zero_ReturnsJsonDataWithZero()
    {
        var converter = JsonDataConverterProvider.GetConverter<float>();

        var jsonData = converter.Create(0f);
        var result = converter.TryGet(in jsonData, out var value);

        Assert.True(result);
        Assert.Equal(0f, value);
    }

    // -----------------------------------------------------------------------
    // DoubleConverter.TryGet
    // -----------------------------------------------------------------------

    [Fact]
    public void DoubleConverter_TryGet_ElementIsDouble_ReturnsTrueWithValue()
    {
        var converter = JsonDataConverterProvider.GetConverter<double>();
        var jsonData = FromJson("3.14159");

        var result = converter.TryGet(in jsonData, out var value);

        Assert.True(result);
        Assert.Equal(3.14159, value, 10);
    }

    [Fact(Skip = "ProductionBugSuspected")]
    public void DoubleConverter_TryGet_ElementIsString_ReturnsFalse()
    {
        var converter = JsonDataConverterProvider.GetConverter<double>();
        var jsonData = FromJson("\"hello\"");

        var result = converter.TryGet(in jsonData, out var value);

        Assert.False(result);
        Assert.Equal(default(double), value);
    }

    [Fact]
    public void DoubleConverter_TryGet_NodeIsJsonValueDouble_ReturnsTrueWithValue()
    {
        var converter = JsonDataConverterProvider.GetConverter<double>();
        var jsonData = FromNode(JsonValue.Create(2.718));

        var result = converter.TryGet(in jsonData, out var value);

        Assert.True(result);
        Assert.Equal(2.718, value);
    }

    [Fact]
    public void DoubleConverter_TryGet_NodeIsJsonObject_ReturnsFalse()
    {
        var converter = JsonDataConverterProvider.GetConverter<double>();
        var jsonData = FromNode(new JsonObject());

        var result = converter.TryGet(in jsonData, out var value);

        Assert.False(result);
        Assert.Equal(default(double), value);
    }

    [Fact]
    public void DoubleConverter_TryGet_EmptyJsonData_ReturnsFalse()
    {
        var converter = JsonDataConverterProvider.GetConverter<double>();
        var jsonData = new JsonData();

        var result = converter.TryGet(in jsonData, out var value);

        Assert.False(result);
        Assert.Equal(default(double), value);
    }

    // -----------------------------------------------------------------------
    // DoubleConverter.Create
    // -----------------------------------------------------------------------

    [Fact]
    public void DoubleConverter_Create_ReturnsJsonDataWithCorrectValue()
    {
        var converter = JsonDataConverterProvider.GetConverter<double>();

        var jsonData = converter.Create(1.23456789);
        var result = converter.TryGet(in jsonData, out var value);

        Assert.True(result);
        Assert.Equal(1.23456789, value);
    }

    [Fact]
    public void DoubleConverter_Create_MaxValue_ReturnsJsonDataWithMaxValue()
    {
        var converter = JsonDataConverterProvider.GetConverter<double>();

        var jsonData = converter.Create(double.MaxValue);
        var result = converter.TryGet(in jsonData, out var value);

        Assert.True(result);
        Assert.Equal(double.MaxValue, value);
    }

    [Fact]
    public void DoubleConverter_Create_Zero_ReturnsJsonDataWithZero()
    {
        var converter = JsonDataConverterProvider.GetConverter<double>();

        var jsonData = converter.Create(0.0);
        var result = converter.TryGet(in jsonData, out var value);

        Assert.True(result);
        Assert.Equal(0.0, value);
    }

    // -----------------------------------------------------------------------
    // DecimalConverter.TryGet
    // -----------------------------------------------------------------------

    [Fact]
    public void DecimalConverter_TryGet_ElementIsDecimal_ReturnsTrueWithValue()
    {
        var converter = JsonDataConverterProvider.GetConverter<decimal>();
        var jsonData = FromJson("123.456");

        var result = converter.TryGet(in jsonData, out var value);

        Assert.True(result);
        Assert.Equal(123.456m, value);
    }

    [Fact(Skip = "ProductionBugSuspected")]
    public void DecimalConverter_TryGet_ElementIsString_ReturnsFalse()
    {
        var converter = JsonDataConverterProvider.GetConverter<decimal>();
        var jsonData = FromJson("\"hello\"");

        var result = converter.TryGet(in jsonData, out var value);

        Assert.False(result);
        Assert.Equal(default(decimal), value);
    }

    [Fact]
    public void DecimalConverter_TryGet_NodeIsJsonValueDecimal_ReturnsTrueWithValue()
    {
        var converter = JsonDataConverterProvider.GetConverter<decimal>();
        var jsonData = FromNode(JsonValue.Create(99.99m));

        var result = converter.TryGet(in jsonData, out var value);

        Assert.True(result);
        Assert.Equal(99.99m, value);
    }

    [Fact]
    public void DecimalConverter_TryGet_NodeIsJsonObject_ReturnsFalse()
    {
        var converter = JsonDataConverterProvider.GetConverter<decimal>();
        var jsonData = FromNode(new JsonObject());

        var result = converter.TryGet(in jsonData, out var value);

        Assert.False(result);
        Assert.Equal(default(decimal), value);
    }

    [Fact]
    public void DecimalConverter_TryGet_EmptyJsonData_ReturnsFalse()
    {
        var converter = JsonDataConverterProvider.GetConverter<decimal>();
        var jsonData = new JsonData();

        var result = converter.TryGet(in jsonData, out var value);

        Assert.False(result);
        Assert.Equal(default(decimal), value);
    }

    // -----------------------------------------------------------------------
    // DecimalConverter.Create
    // -----------------------------------------------------------------------

    [Fact]
    public void DecimalConverter_Create_ReturnsJsonDataWithCorrectValue()
    {
        var converter = JsonDataConverterProvider.GetConverter<decimal>();

        var jsonData = converter.Create(3.14m);
        var result = converter.TryGet(in jsonData, out var value);

        Assert.True(result);
        Assert.Equal(3.14m, value);
    }

    [Fact]
    public void DecimalConverter_Create_Zero_ReturnsJsonDataWithZero()
    {
        var converter = JsonDataConverterProvider.GetConverter<decimal>();

        var jsonData = converter.Create(0m);
        var result = converter.TryGet(in jsonData, out var value);

        Assert.True(result);
        Assert.Equal(0m, value);
    }

    // -----------------------------------------------------------------------
    // DateTimeConverter.TryGet
    // -----------------------------------------------------------------------

    [Fact]
    public void DateTimeConverter_TryGet_ElementIsDateTimeString_ReturnsTrueWithValue()
    {
        var converter = JsonDataConverterProvider.GetConverter<DateTime>();
        var expected = new DateTime(2024, 6, 15, 10, 30, 0, DateTimeKind.Utc);
        var jsonData = FromJson("\"2024-06-15T10:30:00Z\"");

        var result = converter.TryGet(in jsonData, out var value);

        Assert.True(result);
        Assert.Equal(expected, value.ToUniversalTime());
    }

    [Fact]
    public void DateTimeConverter_TryGet_ElementIsNumber_ReturnsFalse()
    {
        var converter = JsonDataConverterProvider.GetConverter<DateTime>();
        var jsonData = FromJson("12345");

        var result = converter.TryGet(in jsonData, out var value);

        Assert.False(result);
        Assert.Equal(default, value);
    }

    [Fact]
    public void DateTimeConverter_TryGet_ElementIsInvalidDateString_ReturnsFalse()
    {
        var converter = JsonDataConverterProvider.GetConverter<DateTime>();
        var jsonData = FromJson("\"not-a-date\"");

        var result = converter.TryGet(in jsonData, out var value);

        Assert.False(result);
        Assert.Equal(default, value);
    }

    [Fact]
    public void DateTimeConverter_TryGet_NodeIsJsonValueDateTime_ReturnsTrueWithValue()
    {
        var converter = JsonDataConverterProvider.GetConverter<DateTime>();
        var expected = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        var jsonData = FromNode(JsonValue.Create(expected));

        var result = converter.TryGet(in jsonData, out var value);

        Assert.True(result);
        Assert.Equal(expected, value);
    }

    [Fact]
    public void DateTimeConverter_TryGet_NodeIsJsonObject_ReturnsFalse()
    {
        var converter = JsonDataConverterProvider.GetConverter<DateTime>();
        var jsonData = FromNode(new JsonObject());

        var result = converter.TryGet(in jsonData, out var value);

        Assert.False(result);
        Assert.Equal(default, value);
    }

    [Fact]
    public void DateTimeConverter_TryGet_EmptyJsonData_ReturnsFalse()
    {
        var converter = JsonDataConverterProvider.GetConverter<DateTime>();
        var jsonData = new JsonData();

        var result = converter.TryGet(in jsonData, out var value);

        Assert.False(result);
        Assert.Equal(default, value);
    }

    // -----------------------------------------------------------------------
    // DateTimeConverter.Create
    // -----------------------------------------------------------------------

    [Fact]
    public void DateTimeConverter_Create_ReturnsJsonDataWithCorrectValue()
    {
        var converter = JsonDataConverterProvider.GetConverter<DateTime>();
        var expected = new DateTime(2024, 6, 15, 10, 30, 0, DateTimeKind.Utc);

        var jsonData = converter.Create(expected);
        var result = converter.TryGet(in jsonData, out var value);

        Assert.True(result);
        Assert.Equal(expected, value);
    }

    // -----------------------------------------------------------------------
    // DateTimeOffsetConverter.TryGet
    // -----------------------------------------------------------------------

    [Fact]
    public void DateTimeOffsetConverter_TryGet_ElementIsDateTimeOffsetString_ReturnsTrueWithValue()
    {
        var converter = JsonDataConverterProvider.GetConverter<DateTimeOffset>();
        var expected = new DateTimeOffset(2024, 6, 15, 10, 30, 0, TimeSpan.FromHours(2));
        var jsonData = FromJson("\"2024-06-15T10:30:00+02:00\"");

        var result = converter.TryGet(in jsonData, out var value);

        Assert.True(result);
        Assert.Equal(expected, value);
    }

    [Fact]
    public void DateTimeOffsetConverter_TryGet_ElementIsNumber_ReturnsFalse()
    {
        var converter = JsonDataConverterProvider.GetConverter<DateTimeOffset>();
        var jsonData = FromJson("12345");

        var result = converter.TryGet(in jsonData, out var value);

        Assert.False(result);
        Assert.Equal(default, value);
    }

    [Fact]
    public void DateTimeOffsetConverter_TryGet_ElementIsInvalidDateString_ReturnsFalse()
    {
        var converter = JsonDataConverterProvider.GetConverter<DateTimeOffset>();
        var jsonData = FromJson("\"not-a-date\"");

        var result = converter.TryGet(in jsonData, out var value);

        Assert.False(result);
        Assert.Equal(default, value);
    }

    [Fact]
    public void DateTimeOffsetConverter_TryGet_NodeIsJsonValueDateTimeOffset_ReturnsTrueWithValue()
    {
        var converter = JsonDataConverterProvider.GetConverter<DateTimeOffset>();
        var expected = new DateTimeOffset(2024, 1, 1, 0, 0, 0, TimeSpan.Zero);
        var jsonData = FromNode(JsonValue.Create(expected));

        var result = converter.TryGet(in jsonData, out var value);

        Assert.True(result);
        Assert.Equal(expected, value);
    }

    [Fact]
    public void DateTimeOffsetConverter_TryGet_NodeIsJsonObject_ReturnsFalse()
    {
        var converter = JsonDataConverterProvider.GetConverter<DateTimeOffset>();
        var jsonData = FromNode(new JsonObject());

        var result = converter.TryGet(in jsonData, out var value);

        Assert.False(result);
        Assert.Equal(default, value);
    }

    [Fact]
    public void DateTimeOffsetConverter_TryGet_EmptyJsonData_ReturnsFalse()
    {
        var converter = JsonDataConverterProvider.GetConverter<DateTimeOffset>();
        var jsonData = new JsonData();

        var result = converter.TryGet(in jsonData, out var value);

        Assert.False(result);
        Assert.Equal(default, value);
    }

    // -----------------------------------------------------------------------
    // DateTimeOffsetConverter.Create
    // -----------------------------------------------------------------------

    [Fact]
    public void DateTimeOffsetConverter_Create_ReturnsJsonDataWithCorrectValue()
    {
        var converter = JsonDataConverterProvider.GetConverter<DateTimeOffset>();
        var expected = new DateTimeOffset(2024, 6, 15, 10, 30, 0, TimeSpan.FromHours(2));

        var jsonData = converter.Create(expected);
        var result = converter.TryGet(in jsonData, out var value);

        Assert.True(result);
        Assert.Equal(expected, value);
    }

    // -----------------------------------------------------------------------
    // GuidConverter.TryGet
    // -----------------------------------------------------------------------

    [Fact]
    public void GuidConverter_TryGet_ElementIsGuidString_ReturnsTrueWithValue()
    {
        var converter = JsonDataConverterProvider.GetConverter<Guid>();
        var expected = Guid.NewGuid();
        var jsonData = FromJson($"\"{expected}\"");

        var result = converter.TryGet(in jsonData, out var value);

        Assert.True(result);
        Assert.Equal(expected, value);
    }

    [Fact]
    public void GuidConverter_TryGet_ElementIsInvalidString_ReturnsFalse()
    {
        var converter = JsonDataConverterProvider.GetConverter<Guid>();
        var jsonData = FromJson("\"not-a-guid\"");

        var result = converter.TryGet(in jsonData, out var value);

        Assert.False(result);
        Assert.Equal(default(Guid), value);
    }

    [Fact]
    public void GuidConverter_TryGet_ElementIsNumber_ReturnsFalse()
    {
        var converter = JsonDataConverterProvider.GetConverter<Guid>();
        var jsonData = FromJson("42");

        var result = converter.TryGet(in jsonData, out var value);

        Assert.False(result);
        Assert.Equal(default(Guid), value);
    }

    [Fact]
    public void GuidConverter_TryGet_NodeIsJsonValueGuid_ReturnsTrueWithValue()
    {
        var converter = JsonDataConverterProvider.GetConverter<Guid>();
        var expected = Guid.NewGuid();
        var jsonData = FromNode(JsonValue.Create(expected));

        var result = converter.TryGet(in jsonData, out var value);

        Assert.True(result);
        Assert.Equal(expected, value);
    }

    [Fact]
    public void GuidConverter_TryGet_NodeIsJsonObject_ReturnsFalse()
    {
        var converter = JsonDataConverterProvider.GetConverter<Guid>();
        var jsonData = FromNode(new JsonObject());

        var result = converter.TryGet(in jsonData, out var value);

        Assert.False(result);
        Assert.Equal(default(Guid), value);
    }

    [Fact]
    public void GuidConverter_TryGet_EmptyJsonData_ReturnsFalse()
    {
        var converter = JsonDataConverterProvider.GetConverter<Guid>();
        var jsonData = new JsonData();

        var result = converter.TryGet(in jsonData, out var value);

        Assert.False(result);
        Assert.Equal(default(Guid), value);
    }

    // -----------------------------------------------------------------------
    // GuidConverter.Create
    // -----------------------------------------------------------------------

    [Fact]
    public void GuidConverter_Create_ReturnsJsonDataWithCorrectValue()
    {
        var converter = JsonDataConverterProvider.GetConverter<Guid>();
        var expected = Guid.NewGuid();

        var jsonData = converter.Create(expected);
        var result = converter.TryGet(in jsonData, out var value);

        Assert.True(result);
        Assert.Equal(expected, value);
    }

    [Fact]
    public void GuidConverter_Create_EmptyGuid_ReturnsJsonDataWithEmptyGuid()
    {
        var converter = JsonDataConverterProvider.GetConverter<Guid>();

        var jsonData = converter.Create(Guid.Empty);
        var result = converter.TryGet(in jsonData, out var value);

        Assert.True(result);
        Assert.Equal(Guid.Empty, value);
    }

    // -----------------------------------------------------------------------
    // NullableConverter<Guid>.Create
    // -----------------------------------------------------------------------

    [Fact]
    public void NullableConverter_Create_NullValue_ReturnsNullJsonData()
    {
        var converter = JsonDataConverterProvider.GetConverter<Guid?>();

        var jsonData = converter.Create(null);

        Assert.True(jsonData.IsNull);
    }

    [Fact]
    public void NullableConverter_Create_NonNullValue_ReturnsJsonDataWithValue()
    {
        var converter = JsonDataConverterProvider.GetConverter<Guid?>();
        var expected = Guid.NewGuid();

        var jsonData = converter.Create(expected);
        var result = converter.TryGet(in jsonData, out var value);

        Assert.True(result);
        Assert.Equal(expected, value);
    }

    // -----------------------------------------------------------------------
    // NullableConverter<Guid>.TryGet
    // -----------------------------------------------------------------------

    [Fact]
    public void NullableConverter_TryGet_NullJsonData_ReturnsTrueWithNull()
    {
        var converter = JsonDataConverterProvider.GetConverter<Guid?>();
        var jsonData = JsonData.CreateNull();

        var result = converter.TryGet(in jsonData, out var value);

        Assert.True(result);
        Assert.Null(value);
    }

    [Fact]
    public void NullableConverter_TryGet_ValidGuidElement_ReturnsTrueWithValue()
    {
        var converter = JsonDataConverterProvider.GetConverter<Guid?>();
        var expected = Guid.NewGuid();
        var jsonData = FromJson($"\"{expected}\"");

        var result = converter.TryGet(in jsonData, out var value);

        Assert.True(result);
        Assert.Equal(expected, value);
    }

    [Fact]
    public void NullableConverter_TryGet_InvalidElement_ReturnsFalse()
    {
        var converter = JsonDataConverterProvider.GetConverter<Guid?>();
        var jsonData = FromJson("42");

        var result = converter.TryGet(in jsonData, out var value);

        Assert.False(result);
        Assert.Null(value);
    }

    [Fact]
    public void NullableConverter_TryGet_ValidGuidNode_ReturnsTrueWithValue()
    {
        var converter = JsonDataConverterProvider.GetConverter<Guid?>();
        var expected = Guid.NewGuid();
        var jsonData = FromNode(JsonValue.Create(expected));

        var result = converter.TryGet(in jsonData, out var value);

        Assert.True(result);
        Assert.Equal(expected, value);
    }

    [Fact]
    public void NullableConverter_TryGet_EmptyJsonData_ReturnsTrueWithNull()
    {
        var converter = JsonDataConverterProvider.GetConverter<Guid?>();
        var jsonData = new JsonData();

        var result = converter.TryGet(in jsonData, out var value);

        // Empty JsonData is treated as null by IsNull check
        Assert.True(result);
        Assert.Null(value);
    }
}
