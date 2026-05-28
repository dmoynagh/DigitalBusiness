using System;
using System.Text.Json.Nodes;
using DigitalBusiness.JsonDataWrappers;
using Xunit;

namespace DigitalBusiness.JsonDataWrappers.Tests;

public class JsonDataTypedExtensionsTests
{
    // -- Create<T> -------------------------------------------------------------

    [Fact]
    public void Create_Int_ReturnsNonNullJsonData()
    {
        // Act
        var data = JsonData.Create<int>(42);

        // Assert
        Assert.False(data.IsNull);
    }

    [Fact]
    public void Create_String_CanBeReadBackWithGet()
    {
        // Arrange
        var data = JsonData.Create<string>("hello");

        // Act
        var result = data.Get<string>();

        // Assert
        Assert.Equal("hello", result);
    }

    [Fact]
    public void Create_Int_CanBeReadBackWithGet()
    {
        // Arrange
        var data = JsonData.Create<int>(99);

        // Act
        var result = data.Get<int>();

        // Assert
        Assert.Equal(99, result);
    }

    [Fact]
    public void Create_Bool_CanBeReadBack()
    {
        // Arrange
        var data = JsonData.Create<bool>(true);

        // Act
        var result = data.Get<bool>();

        // Assert
        Assert.True(result);
    }

    // -- TryGet<T>(out T) ------------------------------------------------------

    [Fact]
    public void TryGet_Out_IntJsonData_ReturnsTrueAndValue()
    {
        // Arrange
        var data = JsonData.Create(42);

        // Act
        var success = data.TryGet<int>(out var value);

        // Assert
        Assert.True(success);
        Assert.Equal(42, value);
    }

    [Fact]
    public void TryGet_Out_StringJsonData_ReturnsTrueAndValue()
    {
        // Arrange
        var data = JsonData.Create("world");

        // Act
        var success = data.TryGet<string>(out var value);

        // Assert
        Assert.True(success);
        Assert.Equal("world", value);
    }

    [Fact]
    public void TryGet_Out_NullJsonData_ReturnsFalseAndDefault()
    {
        // Arrange
        var data = JsonData.CreateNull();

        // Act
        var success = data.TryGet<int>(out var value);

        // Assert
        Assert.False(success);
        Assert.Equal(default, value);
    }

    [Fact]
    public void TryGet_Out_WrongType_ReturnsFalse()
    {
        // Arrange
        var data = JsonData.Create("not-an-int");

        // Act
        var success = data.TryGet<int>(out var value);

        // Assert
        Assert.False(success);
        Assert.Equal(default, value);
    }

    [Fact]
    public void TryGet_Out_DefaultJsonData_ReturnsFalse()
    {
        // Arrange
        var data = new JsonData();

        // Act
        var success = data.TryGet<string>(out var value);

        // Assert
        Assert.False(success);
        Assert.Null(value);
    }

    // -- TryGet<T>() (no out) -------------------------------------------------

    [Fact]
    public void TryGetNoOut_IntJsonData_ReturnsValue()
    {
        // Arrange
        var data = JsonData.Create(7);

        // Act
        var result = data.TryGet<int>();

        // Assert
        Assert.Equal(7, result);
    }

    [Fact]
    public void TryGetNoOut_NullJsonData_ReturnsDefault()
    {
        // Arrange
        var data = JsonData.CreateNull();

        // Act
        var result = data.TryGet<int>();

        // Assert
        Assert.Equal(default, result);
    }

    [Fact]
    public void TryGetNoOut_WrongType_ReturnsDefault()
    {
        // Arrange
        var data = JsonData.Create("not-a-double");

        // Act
        var result = data.TryGet<double>();

        // Assert
        Assert.Equal(default, result);
    }

    [Fact]
    public void TryGetNoOut_String_NullJsonData_ReturnsNull()
    {
        // Arrange
        var data = JsonData.CreateNull();

        // Act
        var result = data.TryGet<string>();

        // Assert
        Assert.Null(result);
    }

    // -- Get<T>() --------------------------------------------------------------

    [Fact]
    public void Get_IntJsonData_ReturnsValue()
    {
        // Arrange
        var data = JsonData.Create(123);

        // Act
        var result = data.Get<int>();

        // Assert
        Assert.Equal(123, result);
    }

    [Fact]
    public void Get_StringJsonData_ReturnsValue()
    {
        // Arrange
        var data = JsonData.Create("test");

        // Act
        var result = data.Get<string>();

        // Assert
        Assert.Equal("test", result);
    }

    [Fact]
    public void Get_NullJsonData_ThrowsInvalidOperationException()
    {
        // Arrange
        var data = JsonData.CreateNull();

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => data.Get<int>());
    }

    [Fact]
    public void Get_WrongType_ThrowsInvalidOperationException()
    {
        // Arrange
        var data = JsonData.Create("not-an-int");

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => data.Get<int>());
    }

    [Fact]
    public void Get_DefaultJsonData_ThrowsInvalidOperationException()
    {
        // Arrange
        var data = new JsonData();

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => data.Get<string>());
    }

    // -- Get<T>(string name) ---------------------------------------------------

    [Fact]
    public void GetByName_ExistingIntProperty_ReturnsValue()
    {
        // Arrange
        var obj = new JsonObject { ["age"] = 30 };
        var data = new JsonData(obj);

        // Act
        var result = data.Get<int>("age");

        // Assert
        Assert.Equal(30, result);
    }

    [Fact]
    public void GetByName_ExistingStringProperty_ReturnsValue()
    {
        // Arrange
        var obj = new JsonObject { ["name"] = "Alice" };
        var data = new JsonData(obj);

        // Act
        var result = data.Get<string>("name");

        // Assert
        Assert.Equal("Alice", result);
    }

    [Fact]
    public void GetByName_MissingProperty_ThrowsInvalidOperationException()
    {
        // Arrange
        var obj = new JsonObject { ["x"] = 1 };
        var data = new JsonData(obj);

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => data.Get<int>("missing"));
    }

    [Fact]
    public void GetByName_WrongType_ThrowsInvalidOperationException()
    {
        // Arrange
        var obj = new JsonObject { ["val"] = "notANumber" };
        var data = new JsonData(obj);

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => data.Get<int>("val"));
    }

    // -- TryGet<T>(string name) (no out) ---------------------------------------

    [Fact]
    public void TryGetByName_NoOut_ExistingIntProperty_ReturnsValue()
    {
        // Arrange
        var obj = new JsonObject { ["count"] = 5 };
        var data = new JsonData(obj);

        // Act
        var result = data.TryGet<int>("count");

        // Assert
        Assert.Equal(5, result);
    }

    [Fact]
    public void TryGetByName_NoOut_MissingProperty_ReturnsDefault()
    {
        // Arrange
        var obj = new JsonObject { ["x"] = 1 };
        var data = new JsonData(obj);

        // Act
        var result = data.TryGet<int>("missing");

        // Assert
        Assert.Equal(default, result);
    }

    [Fact]
    public void TryGetByName_NoOut_NullJsonData_ReturnsDefault()
    {
        // Arrange
        var data = JsonData.CreateNull();

        // Act
        var result = data.TryGet<int>("any");

        // Assert
        Assert.Equal(default, result);
    }

    [Fact]
    public void TryGetByName_NoOut_StringProperty_ReturnsValue()
    {
        // Arrange
        var obj = new JsonObject { ["name"] = "Bob" };
        var data = new JsonData(obj);

        // Act
        var result = data.TryGet<string>("name");

        // Assert
        Assert.Equal("Bob", result);
    }

    // -- TryGet<T>(string name, out T) -----------------------------------------

    [Fact]
    public void TryGetByName_Out_ExistingIntProperty_ReturnsTrueAndValue()
    {
        // Arrange
        var obj = new JsonObject { ["score"] = 100 };
        var data = new JsonData(obj);

        // Act
        var success = data.TryGet<int>("score", out var value);

        // Assert
        Assert.True(success);
        Assert.Equal(100, value);
    }

    [Fact]
    public void TryGetByName_Out_MissingProperty_ReturnsFalseAndDefault()
    {
        // Arrange
        var obj = new JsonObject { ["x"] = 1 };
        var data = new JsonData(obj);

        // Act
        var success = data.TryGet<int>("missing", out var value);

        // Assert
        Assert.False(success);
        Assert.Equal(default, value);
    }

    [Fact]
    public void TryGetByName_Out_NullJsonData_ReturnsFalseAndDefault()
    {
        // Arrange
        var data = JsonData.CreateNull();

        // Act
        var success = data.TryGet<int>("any", out var value);

        // Assert
        Assert.False(success);
        Assert.Equal(default, value);
    }

    [Fact]
    public void TryGetByName_Out_WrongType_ReturnsFalse()
    {
        // Arrange
        var obj = new JsonObject { ["val"] = "text" };
        var data = new JsonData(obj);

        // Act
        var success = data.TryGet<int>("val", out var value);

        // Assert
        Assert.False(success);
        Assert.Equal(default, value);
    }

    // -- Set<T>(string name, T? value) -----------------------------------------

    [Fact]
    public void SetByName_NonNullValue_SetsProperty()
    {
        // Arrange
        var data = JsonData.CreateObject();

        // Act
        data.Set<int>("age", 42);

        // Assert
        Assert.Equal(42, data.Get<int>("age"));
    }

    [Fact]
    public void SetByName_NullValue_RemovesProperty()
    {
        // Arrange
        var obj = new JsonObject { ["name"] = "Alice" };
        var data = new JsonData(obj);

        // Act
        data.Set<string>("name", null);

        // Assert
        Assert.False(data.TryGet<string>("name", out _));
    }

    [Fact]
    public void SetByName_OverwritesExistingProperty()
    {
        // Arrange
        var obj = new JsonObject { ["val"] = 1 };
        var data = new JsonData(obj);

        // Act
        data.Set<int>("val", 99);

        // Assert
        Assert.Equal(99, data.Get<int>("val"));
    }

    [Fact]
    public void SetByName_StringValue_CanBeReadBack()
    {
        // Arrange
        var data = JsonData.CreateObject();

        // Act
        data.Set<string>("key", "hello");

        // Assert
        Assert.Equal("hello", data.Get<string>("key"));
    }

    // -- Ensure<T>(string name, Func<T> defaultValue) -------------------------

    [Fact]
    public void Ensure_Func_PropertyExists_ReturnsExistingValue()
    {
        // Arrange
        var obj = new JsonObject { ["x"] = 10 };
        var data = new JsonData(obj);
        var factoryCalled = false;

        // Act
        var result = data.Ensure<int>("x", () => { factoryCalled = true; return 99; });

        // Assert
        Assert.Equal(10, result);
        Assert.False(factoryCalled);
    }

    [Fact]
    public void Ensure_Func_PropertyMissing_CallsFactoryAndSets()
    {
        // Arrange
        var data = JsonData.CreateObject();

        // Act
        var result = data.Ensure<int>("x", () => 42);

        // Assert
        Assert.Equal(42, result);
        Assert.Equal(42, data.Get<int>("x"));
    }

    [Fact]
    public void Ensure_Func_PropertyMissing_SubsequentCallReturnsSaved()
    {
        // Arrange
        var data = JsonData.CreateObject();
        var callCount = 0;

        // Act
        data.Ensure<string>("key", () => { callCount++; return "first"; });
        var second = data.Ensure<string>("key", () => { callCount++; return "second"; });

        // Assert
        Assert.Equal("first", second);
        Assert.Equal(1, callCount);
    }

    // -- Ensure<T>(string name, T defaultValue) --------------------------------

    [Fact]
    public void Ensure_Value_PropertyExists_ReturnsExistingValue()
    {
        // Arrange
        var obj = new JsonObject { ["num"] = 7 };
        var data = new JsonData(obj);

        // Act
        var result = data.Ensure<int>("num", 100);

        // Assert
        Assert.Equal(7, result);
    }

    [Fact]
    public void Ensure_Value_PropertyMissing_SetsAndReturnsDefault()
    {
        // Arrange
        var data = JsonData.CreateObject();

        // Act
        var result = data.Ensure<int>("num", 55);

        // Assert
        Assert.Equal(55, result);
        Assert.Equal(55, data.Get<int>("num"));
    }

    [Fact]
    public void Ensure_Value_StringPropertyMissing_SetsAndReturnsDefault()
    {
        // Arrange
        var data = JsonData.CreateObject();

        // Act
        var result = data.Ensure<string>("label", "default");

        // Assert
        Assert.Equal("default", result);
        Assert.Equal("default", data.Get<string>("label"));
    }

    // -- Get<T>(int index) -----------------------------------------------------

    [Fact]
    public void GetByIndex_ExistingIntElement_ReturnsValue()
    {
        // Arrange
        var arr = new System.Text.Json.Nodes.JsonArray(42);
        var data = new JsonData(arr);

        // Act
        var result = data.Get<int>(0);

        // Assert
        Assert.Equal(42, result);
    }

    [Fact]
    public void GetByIndex_ExistingStringElement_ReturnsValue()
    {
        // Arrange
        var arr = new System.Text.Json.Nodes.JsonArray("hello");
        var data = new JsonData(arr);

        // Act
        var result = data.Get<string>(0);

        // Assert
        Assert.Equal("hello", result);
    }

    [Fact]
    public void GetByIndex_OutOfRange_Throws()
    {
        // Arrange
        var data = JsonData.CreateArray();

        // Act & Assert
        Assert.ThrowsAny<Exception>(() => data.Get<int>(0));
    }

    // -- TryGet<T>(int index) (no out) -----------------------------------------

    [Fact]
    public void TryGetByIndex_NoOut_ExistingIntElement_ReturnsValue()
    {
        // Arrange
        var arr = new System.Text.Json.Nodes.JsonArray(10);
        var data = new JsonData(arr);

        // Act
        var result = data.TryGet<int>(0);

        // Assert
        Assert.Equal(10, result);
    }

    [Fact]
    public void TryGetByIndex_NoOut_NullJsonData_ReturnsDefault()
    {
        // Arrange
        var data = JsonData.CreateNull();

        // Act
        var result = data.TryGet<int>(0);

        // Assert
        Assert.Equal(default, result);
    }

    [Fact]
    public void TryGetByIndex_NoOut_OutOfRange_ReturnsDefault()
    {
        // Arrange
        var data = JsonData.CreateArray();

        // Act
        var result = data.TryGet<int>(5);

        // Assert
        Assert.Equal(default, result);
    }

    [Fact]
    public void TryGetByIndex_NoOut_WrongType_ReturnsDefault()
    {
        // Arrange
        var arr = new System.Text.Json.Nodes.JsonArray("not-an-int");
        var data = new JsonData(arr);

        // Act
        var result = data.TryGet<int>(0);

        // Assert
        Assert.Equal(default, result);
    }

    // -- TryGet<T>(int index, out T) -------------------------------------------

    [Fact]
    public void TryGetByIndex_Out_ExistingIntElement_ReturnsTrueAndValue()
    {
        // Arrange
        var arr = new System.Text.Json.Nodes.JsonArray(99);
        var data = new JsonData(arr);

        // Act
        var success = data.TryGet<int>(0, out var value);

        // Assert
        Assert.True(success);
        Assert.Equal(99, value);
    }

    [Fact]
    public void TryGetByIndex_Out_ExistingStringElement_ReturnsTrueAndValue()
    {
        // Arrange
        var arr = new System.Text.Json.Nodes.JsonArray("world");
        var data = new JsonData(arr);

        // Act
        var success = data.TryGet<string>(0, out var value);

        // Assert
        Assert.True(success);
        Assert.Equal("world", value);
    }

    [Fact]
    public void TryGetByIndex_Out_NullJsonData_ReturnsFalseAndDefault()
    {
        // Arrange
        var data = JsonData.CreateNull();

        // Act
        var success = data.TryGet<int>(0, out var value);

        // Assert
        Assert.False(success);
        Assert.Equal(default, value);
    }

    [Fact]
    public void TryGetByIndex_Out_OutOfRange_ReturnsFalseAndDefault()
    {
        // Arrange
        var data = JsonData.CreateArray();

        // Act
        var success = data.TryGet<int>(5, out var value);

        // Assert
        Assert.False(success);
        Assert.Equal(default, value);
    }

    [Fact]
    public void TryGetByIndex_Out_WrongType_ReturnsFalse()
    {
        // Arrange
        var arr = new System.Text.Json.Nodes.JsonArray("text");
        var data = new JsonData(arr);

        // Act
        var success = data.TryGet<int>(0, out var value);

        // Assert
        Assert.False(success);
        Assert.Equal(default, value);
    }

    // -- Set<T>(int index, T? value) -------------------------------------------

    [Fact]
    public void SetByIndex_NonNullValue_SetsElement()
    {
        // Arrange
        var arr = new System.Text.Json.Nodes.JsonArray(1);
        var data = new JsonData(arr);

        // Act
        data.Set<int>(0, 77);

        // Assert
        Assert.Equal(77, data.Get<int>(0));
    }

    [Fact]
    public void SetByIndex_NullValue_RemovesElement()
    {
        // Arrange
        var arr = new System.Text.Json.Nodes.JsonArray("hello", "world");
        var data = new JsonData(arr);

        // Act
        data.Set<string>(0, null);

        // Assert
        // After removal, index 0 now points to former index 1
        Assert.Equal("world", data.Get<string>(0));
    }

    [Fact]
    public void SetByIndex_OverwritesExistingElement()
    {
        // Arrange
        var arr = new System.Text.Json.Nodes.JsonArray(10, 20, 30);
        var data = new JsonData(arr);

        // Act
        data.Set<int>(1, 200);

        // Assert
        Assert.Equal(200, data.Get<int>(1));
    }

    [Fact]
    public void SetByIndex_StringValue_CanBeReadBack()
    {
        // Arrange
        var arr = new System.Text.Json.Nodes.JsonArray("old");
        var data = new JsonData(arr);

        // Act
        data.Set<string>(0, "new");

        // Assert
        Assert.Equal("new", data.Get<string>(0));
    }

    // -- Add<T>(T value) -------------------------------------------------------

    [Fact]
    public void Add_Int_AppendsElement()
    {
        // Arrange
        var data = JsonData.CreateArray();

        // Act
        data.Add<int>(5);

        // Assert
        Assert.Equal(5, data.Get<int>(0));
    }

    [Fact]
    public void Add_String_AppendsElement()
    {
        // Arrange
        var data = JsonData.CreateArray();

        // Act
        data.Add<string>("foo");

        // Assert
        Assert.Equal("foo", data.Get<string>(0));
    }

    [Fact]
    public void Add_MultipleItems_AppendsInOrder()
    {
        // Arrange
        var data = JsonData.CreateArray();

        // Act
        data.Add<int>(1);
        data.Add<int>(2);
        data.Add<int>(3);

        // Assert
        Assert.Equal(1, data.Get<int>(0));
        Assert.Equal(2, data.Get<int>(1));
        Assert.Equal(3, data.Get<int>(2));
    }

    [Fact]
    public void Add_Bool_AppendsElement()
    {
        // Arrange
        var data = JsonData.CreateArray();

        // Act
        data.Add<bool>(true);

        // Assert
        Assert.True(data.Get<bool>(0));
    }

    // -- Insert<T>(int index, T value) -----------------------------------------

    [Fact]
    public void Insert_AtIndexZero_InsertsAtBeginning()
    {
        // Arrange
        var arr = new System.Text.Json.Nodes.JsonArray(10, 20);
        var data = new JsonData(arr);

        // Act
        data.Insert<int>(0, 5);

        // Assert
        Assert.Equal(5, data.Get<int>(0));
        Assert.Equal(10, data.Get<int>(1));
        Assert.Equal(20, data.Get<int>(2));
    }

    [Fact]
    public void Insert_AtMiddleIndex_ShiftsElementsRight()
    {
        // Arrange
        var arr = new System.Text.Json.Nodes.JsonArray(1, 3);
        var data = new JsonData(arr);

        // Act
        data.Insert<int>(1, 2);

        // Assert
        Assert.Equal(1, data.Get<int>(0));
        Assert.Equal(2, data.Get<int>(1));
        Assert.Equal(3, data.Get<int>(2));
    }

    [Fact]
    public void Insert_AtEndIndex_AppendsElement()
    {
        // Arrange
        var arr = new System.Text.Json.Nodes.JsonArray(100, 200);
        var data = new JsonData(arr);

        // Act
        data.Insert<int>(2, 300);

        // Assert
        Assert.Equal(100, data.Get<int>(0));
        Assert.Equal(200, data.Get<int>(1));
        Assert.Equal(300, data.Get<int>(2));
    }

    [Fact]
    public void Insert_IntoEmptyArray_InsertsElement()
    {
        // Arrange
        var data = JsonData.CreateArray();

        // Act
        data.Insert<int>(0, 42);

        // Assert
        Assert.Equal(42, data.Get<int>(0));
    }

    [Fact]
    public void Insert_StringValue_CanBeReadBack()
    {
        // Arrange
        var arr = new System.Text.Json.Nodes.JsonArray("a", "c");
        var data = new JsonData(arr);

        // Act
        data.Insert<string>(1, "b");

        // Assert
        Assert.Equal("a", data.Get<string>(0));
        Assert.Equal("b", data.Get<string>(1));
        Assert.Equal("c", data.Get<string>(2));
    }

    [Fact]
    public void Insert_BoolValue_CanBeReadBack()
    {
        // Arrange
        var data = JsonData.CreateArray();
        data.Add<bool>(false);

        // Act
        data.Insert<bool>(0, true);

        // Assert
        Assert.True(data.Get<bool>(0));
        Assert.False(data.Get<bool>(1));
    }
}
