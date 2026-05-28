using DigitalBusiness.JsonDataWrappers;
using System;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace DigitalBusiness.JsonDataWrappers.Tests;

public class JsonDataSerializedExtensionsTests
{
    private static readonly JsonSerializerOptions DefaultOptions = new();

    // Helper to create a JsonData from a serializable value
    private static JsonData CreateJsonData<T>(T value)
    {
        var node = JsonSerializer.SerializeToNode(value);
        return new JsonData(node);
    }

    // --- Get<T>(JsonSerializerOptions) ---

    [Fact]
    public void Get_NonNullJsonData_ReturnsDeserializedValue()
    {
        var jsonData = CreateJsonData(42);
        var result = jsonData.Get<int>(DefaultOptions);
        Assert.Equal(42, result);
    }

    [Fact]
    public void Get_StringValue_ReturnsDeserializedString()
    {
        var jsonData = CreateJsonData("hello");
        var result = jsonData.Get<string>(DefaultOptions);
        Assert.Equal("hello", result);
    }

    [Fact]
    public void Get_NullJsonData_ThrowsInvalidOperationException()
    {
        var jsonData = JsonData.CreateNull();
        Assert.Throws<InvalidOperationException>(() => jsonData.Get<int>(DefaultOptions));
    }

    [Fact]
    public void Get_WrongType_ThrowsInvalidOperationException()
    {
        // A string node cannot be deserialized as SampleData (reference type), so TryGet returns false and Get throws
        var jsonData = CreateJsonData("not-an-object");
        Assert.Throws<InvalidOperationException>(() => jsonData.Get<SampleData>(DefaultOptions));
    }

    [Fact]
    public void Get_ComplexType_ReturnsDeserializedObject()
    {
        var obj = new SampleData { Name = "test", Value = 7 };
        var jsonData = CreateJsonData(obj);
        var result = jsonData.Get<SampleData>(DefaultOptions);
        Assert.Equal("test", result.Name);
        Assert.Equal(7, result.Value);
    }

    // --- TryGet<T>(JsonSerializerOptions) nullable return ---

    [Fact]
    public void TryGetNullable_NonNullJsonData_ReturnsDeserializedValue()
    {
        var jsonData = CreateJsonData(99);
        var result = jsonData.TryGet<int>(DefaultOptions);
        Assert.Equal(99, result);
    }

    [Fact]
    public void TryGetNullable_NullJsonData_ReturnsDefault()
    {
        var jsonData = JsonData.CreateNull();
        var result = jsonData.TryGet<int>(DefaultOptions);
        Assert.Equal(default, result);
    }

    [Fact]
    public void TryGetNullable_WrongType_ReturnsDefault()
    {
        var jsonData = CreateJsonData("not-a-number");
        var result = jsonData.TryGet<int>(DefaultOptions);
        Assert.Equal(default, result);
    }

    [Fact]
    public void TryGetNullable_StringValue_ReturnsString()
    {
        var jsonData = CreateJsonData("world");
        var result = jsonData.TryGet<string>(DefaultOptions);
        Assert.Equal("world", result);
    }

    // --- TryGet<T>(out T?, JsonSerializerOptions) bool return ---

    [Fact]
    public void TryGetOut_NonNullJsonData_ReturnsTrueAndValue()
    {
        var jsonData = CreateJsonData(123);
        var success = jsonData.TryGet<int>(out var value, DefaultOptions);
        Assert.True(success);
        Assert.Equal(123, value);
    }

    [Fact]
    public void TryGetOut_NullJsonData_ReturnsFalseAndDefault()
    {
        var jsonData = JsonData.CreateNull();
        var success = jsonData.TryGet<int>(out var value, DefaultOptions);
        Assert.False(success);
        Assert.Equal(default, value);
    }

    [Fact]
    public void TryGetOut_StringJsonData_ReturnsTrueAndString()
    {
        var jsonData = CreateJsonData("abc");
        var success = jsonData.TryGet<string>(out var value, DefaultOptions);
        Assert.True(success);
        Assert.Equal("abc", value);
    }

    [Fact]
    public void TryGetOut_WrongType_ReturnsTrueButValueIsDefault()
    {
        // Non-null JsonData, but type mismatch: TryGet returns true (non-null node), value will be default
        var jsonData = CreateJsonData("not-a-number");
        var success = jsonData.TryGet<int>(out var value, DefaultOptions);
        // According to the implementation, when IsNull is false we call converter and return true
        Assert.True(success);
        Assert.Equal(default, value);
    }

    [Fact]
    public void TryGetOut_ComplexType_ReturnsTrueAndValue()
    {
        var obj = new SampleData { Name = "foo", Value = 5 };
        var jsonData = CreateJsonData(obj);
        var success = jsonData.TryGet<SampleData>(out var value, DefaultOptions);
        Assert.True(success);
        Assert.NotNull(value);
        Assert.Equal("foo", value!.Name);
        Assert.Equal(5, value.Value);
    }

    // --- Create<T>(T, JsonSerializerOptions) static ---

    [Fact]
    public void Create_IntValue_ReturnsJsonDataWithCorrectValue()
    {
        var result = JsonData.Create<int>(42, DefaultOptions);
        var retrieved = result.Get<int>(DefaultOptions);
        Assert.Equal(42, retrieved);
    }

    [Fact]
    public void Create_StringValue_ReturnsJsonDataWithCorrectValue()
    {
        var result = JsonData.Create<string>("hello", DefaultOptions);
        var retrieved = result.Get<string>(DefaultOptions);
        Assert.Equal("hello", retrieved);
    }

    [Fact]
    public void Create_ComplexType_ReturnsJsonDataWithCorrectValue()
    {
        var obj = new SampleData { Name = "bar", Value = 3 };
        var result = JsonData.Create<SampleData>(obj, DefaultOptions);
        var retrieved = result.Get<SampleData>(DefaultOptions);
        Assert.Equal("bar", retrieved.Name);
        Assert.Equal(3, retrieved.Value);
    }

    [Fact]
    public void Create_NullValue_ReturnsNullJsonData()
    {
        var result = JsonData.Create<string?>(null, DefaultOptions);
        Assert.True(result.IsNull);
    }

    // --- Get<T>(string name, JsonSerializerOptions) ---

    [Fact]
    public void GetByName_ExistingProperty_ReturnsDeserializedValue()
    {
        var obj = new SampleData { Name = "test", Value = 10 };
        var jsonData = CreateJsonData(obj);
        var result = jsonData.Get<int>("Value", DefaultOptions);
        Assert.Equal(10, result);
    }

    [Fact]
    public void GetByName_ExistingStringProperty_ReturnsString()
    {
        var obj = new SampleData { Name = "myname", Value = 1 };
        var jsonData = CreateJsonData(obj);
        var result = jsonData.Get<string>("Name", DefaultOptions);
        Assert.Equal("myname", result);
    }

    [Fact]
    public void GetByName_NonExistingProperty_ThrowsException()
    {
        var jsonData = CreateJsonData(new SampleData { Name = "x", Value = 1 });
        Assert.Throws<InvalidOperationException>(() => jsonData.Get<int>("NonExistent", DefaultOptions));
    }

    [Fact]
    public void GetByName_NullJsonData_ThrowsInvalidOperationException()
    {
        var jsonData = JsonData.CreateNull();
        Assert.Throws<InvalidOperationException>(() => jsonData.Get<int>("Value", DefaultOptions));
    }

    // --- TryGet<T>(string name, JsonSerializerOptions) nullable return ---

    [Fact]
    public void TryGetByName_ExistingProperty_ReturnsDeserializedValue()
    {
        var obj = new SampleData { Name = "Alice", Value = 42 };
        var jsonData = CreateJsonData(obj);
        var result = jsonData.TryGet<int>("Value", DefaultOptions);
        Assert.Equal(42, result);
    }

    [Fact]
    public void TryGetByName_NonExistingProperty_ReturnsDefault()
    {
        var obj = new SampleData { Name = "Alice", Value = 42 };
        var jsonData = CreateJsonData(obj);
        var result = jsonData.TryGet<int>("Missing", DefaultOptions);
        Assert.Equal(default, result);
    }

    [Fact]
    public void TryGetByName_NullJsonData_ReturnsDefault()
    {
        var jsonData = JsonData.CreateNull();
        var result = jsonData.TryGet<int>("Value", DefaultOptions);
        Assert.Equal(default, result);
    }

    [Fact]
    public void TryGetByName_StringProperty_ReturnsString()
    {
        var obj = new SampleData { Name = "Bob", Value = 1 };
        var jsonData = CreateJsonData(obj);
        var result = jsonData.TryGet<string>("Name", DefaultOptions);
        Assert.Equal("Bob", result);
    }

    // --- TryGet<T>(string name, out T? value, JsonSerializerOptions) bool return ---

    [Fact]
    public void TryGetOutByName_ExistingProperty_ReturnsTrueAndValue()
    {
        var obj = new SampleData { Name = "Carol", Value = 7 };
        var jsonData = CreateJsonData(obj);
        var success = jsonData.TryGet<int>("Value", out var value, DefaultOptions);
        Assert.True(success);
        Assert.Equal(7, value);
    }

    [Fact]
    public void TryGetOutByName_NonExistingProperty_ReturnsFalseAndDefault()
    {
        var obj = new SampleData { Name = "Carol", Value = 7 };
        var jsonData = CreateJsonData(obj);
        var success = jsonData.TryGet<int>("Missing", out var value, DefaultOptions);
        Assert.False(success);
        Assert.Equal(default, value);
    }

    [Fact]
    public void TryGetOutByName_NullJsonData_ReturnsFalseAndDefault()
    {
        var jsonData = JsonData.CreateNull();
        var success = jsonData.TryGet<int>("Value", out var value, DefaultOptions);
        Assert.False(success);
        Assert.Equal(default, value);
    }

    [Fact]
    public void TryGetOutByName_StringProperty_ReturnsTrueAndString()
    {
        var obj = new SampleData { Name = "Dave", Value = 3 };
        var jsonData = CreateJsonData(obj);
        var success = jsonData.TryGet<string>("Name", out var value, DefaultOptions);
        Assert.True(success);
        Assert.Equal("Dave", value);
    }

    // --- Set<T>(string name, T? value, JsonSerializerOptions) ---

    [Fact]
    public void SetByName_WithNonNullValue_SetsProperty()
    {
        var obj = new SampleData { Name = "original", Value = 1 };
        var jsonData = CreateJsonData(obj);
        jsonData.Set<int>("Value", 99, DefaultOptions);
        var result = jsonData.Get<int>("Value", DefaultOptions);
        Assert.Equal(99, result);
    }

    [Fact]
    public void SetByName_WithNullValue_RemovesProperty()
    {
        var obj = new SampleData { Name = "original", Value = 1 };
        var jsonData = CreateJsonData(obj);
        jsonData.Set<string?>("Name", null, DefaultOptions);
        var success = jsonData.TryGet<string>("Name", out _, DefaultOptions);
        Assert.False(success);
    }

    [Fact]
    public void SetByName_WithStringValue_SetsProperty()
    {
        var obj = new SampleData { Name = "old", Value = 0 };
        var jsonData = CreateJsonData(obj);
        jsonData.Set<string>("Name", "new", DefaultOptions);
        var result = jsonData.Get<string>("Name", DefaultOptions);
        Assert.Equal("new", result);
    }

    [Fact]
    public void SetByName_NewProperty_AddsProperty()
    {
        var obj = new SampleData { Name = "test", Value = 0 };
        var jsonData = CreateJsonData(obj);
        jsonData.Set<int>("Extra", 55, DefaultOptions);
        var result = jsonData.Get<int>("Extra", DefaultOptions);
        Assert.Equal(55, result);
    }

    // --- Get<T>(int index, JsonSerializerOptions) ---

    [Fact]
    public void GetByIndex_ValidIndex_ReturnsDeserializedValue()
    {
        var arr = new[] { 10, 20, 30 };
        var jsonData = CreateJsonData(arr);
        var result = jsonData.Get<int>(0, DefaultOptions);
        Assert.Equal(10, result);
    }

    [Fact]
    public void GetByIndex_SecondElement_ReturnsCorrectValue()
    {
        var arr = new[] { 10, 20, 30 };
        var jsonData = CreateJsonData(arr);
        var result = jsonData.Get<int>(1, DefaultOptions);
        Assert.Equal(20, result);
    }

    [Fact]
    public void GetByIndex_OutOfRange_ThrowsException()
    {
        var arr = new[] { 10, 20 };
        var jsonData = CreateJsonData(arr);
        Assert.ThrowsAny<Exception>(() => jsonData.Get<int>(5, DefaultOptions));
    }

    [Fact]
    public void GetByIndex_StringArray_ReturnsString()
    {
        var arr = new[] { "a", "b", "c" };
        var jsonData = CreateJsonData(arr);
        var result = jsonData.Get<string>(2, DefaultOptions);
        Assert.Equal("c", result);
    }

    // --- TryGet<T>(int index, JsonSerializerOptions) nullable return ---

    [Fact]
    public void TryGetByIndex_ValidIndex_ReturnsDeserializedValue()
    {
        var arr = new[] { 10, 20, 30 };
        var jsonData = CreateJsonData(arr);
        var result = jsonData.TryGet<int>(1, DefaultOptions);
        Assert.Equal(20, result);
    }

    [Fact]
    public void TryGetByIndex_InvalidIndex_ReturnsDefault()
    {
        var arr = new[] { 10, 20 };
        var jsonData = CreateJsonData(arr);
        var result = jsonData.TryGet<int>(99, DefaultOptions);
        Assert.Equal(default, result);
    }

    [Fact]
    public void TryGetByIndex_NullJsonData_ReturnsDefault()
    {
        var jsonData = JsonData.CreateNull();
        var result = jsonData.TryGet<int>(0, DefaultOptions);
        Assert.Equal(default, result);
    }

    [Fact]
    public void TryGetByIndex_StringArray_ReturnsString()
    {
        var arr = new[] { "x", "y" };
        var jsonData = CreateJsonData(arr);
        var result = jsonData.TryGet<string>(0, DefaultOptions);
        Assert.Equal("x", result);
    }

    // --- TryGet<T>(int index, out T? value, JsonSerializerOptions) bool return ---

    [Fact]
    public void TryGetOutByIndex_ValidIndex_ReturnsTrueAndValue()
    {
        var arr = new[] { 10, 20, 30 };
        var jsonData = CreateJsonData(arr);
        var success = jsonData.TryGet<int>(0, out var value, DefaultOptions);
        Assert.True(success);
        Assert.Equal(10, value);
    }

    [Fact]
    public void TryGetOutByIndex_SecondElement_ReturnsTrueAndCorrectValue()
    {
        var arr = new[] { 10, 20, 30 };
        var jsonData = CreateJsonData(arr);
        var success = jsonData.TryGet<int>(2, out var value, DefaultOptions);
        Assert.True(success);
        Assert.Equal(30, value);
    }

    [Fact]
    public void TryGetOutByIndex_NullJsonData_ReturnsFalseAndDefault()
    {
        var jsonData = JsonData.CreateNull();
        var success = jsonData.TryGet<int>(0, out var value, DefaultOptions);
        Assert.False(success);
        Assert.Equal(default, value);
    }

    [Fact]
    public void TryGetOutByIndex_InvalidIndex_ReturnsFalseAndDefault()
    {
        var arr = new[] { 10, 20 };
        var jsonData = CreateJsonData(arr);
        var success = jsonData.TryGet<int>(99, out var value, DefaultOptions);
        Assert.False(success);
        Assert.Equal(default, value);
    }

    [Fact]
    public void TryGetOutByIndex_StringArray_ReturnsTrueAndString()
    {
        var arr = new[] { "hello", "world" };
        var jsonData = CreateJsonData(arr);
        var success = jsonData.TryGet<string>(1, out var value, DefaultOptions);
        Assert.True(success);
        Assert.Equal("world", value);
    }

    // --- Set<T>(int index, T? value, JsonSerializerOptions) ---

    [Fact]
    public void SetByIndex_WithNonNullValue_SetsElement()
    {
        var arr = new[] { 1, 2, 3 };
        var jsonData = CreateJsonData(arr);
        jsonData.Set<int>(1, 99, DefaultOptions);
        var result = jsonData.Get<int>(1, DefaultOptions);
        Assert.Equal(99, result);
    }

    [Fact]
    public void SetByIndex_FirstElement_SetsCorrectElement()
    {
        var arr = new[] { 10, 20 };
        var jsonData = CreateJsonData(arr);
        jsonData.Set<int>(0, 77, DefaultOptions);
        var result = jsonData.Get<int>(0, DefaultOptions);
        Assert.Equal(77, result);
    }

    [Fact]
    public void SetByIndex_WithNullValue_RemovesElement()
    {
        var arr = new[] { "a", "b", "c" };
        var jsonData = CreateJsonData(arr);
        jsonData.Set<string?>(0, null, DefaultOptions);
        // After removal array shrinks; original index 1 element "b" is now at index 0
        var success = jsonData.TryGet<string>(2, out _, DefaultOptions);
        Assert.False(success);
    }

    [Fact]
    public void SetByIndex_WithStringValue_SetsElement()
    {
        var arr = new[] { "old", "keep" };
        var jsonData = CreateJsonData(arr);
        jsonData.Set<string>(0, "new", DefaultOptions);
        var result = jsonData.Get<string>(0, DefaultOptions);
        Assert.Equal("new", result);
    }

    private class SampleData
    {
        public string? Name { get; set; }
        public int Value { get; set; }
    }
}
