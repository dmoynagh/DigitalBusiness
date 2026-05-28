using DigitalBusiness.JsonDataWrappers;
using Moq;
using System;
using System.Text.Json.Nodes;

namespace DigitalBusiness.JsonDataWrappers.Tests;

public class TypedJsonDataArrayExtensionsTests
{
    // -- Helpers -------------------------------------------------------------

    private static JsonData ArrayJsonData() => new JsonData(new JsonArray());
    private static JsonData NonArrayJsonData() => new JsonData(new JsonObject());

    // -- IJsonDataWrapper.AsJsonDataArray<T>() -------------------------------

    [Fact]
    public void AsJsonDataArray_OnWrapper_WithArrayJson_ReturnsJsonDataArrayWithSameJson()
    {
        // Arrange
        var arrayJson = ArrayJsonData();
        var mock = new Mock<IJsonDataWrapper>();
        mock.Setup(w => w.Json).Returns(arrayJson);

        // Act
        var result = mock.Object.AsJsonDataArray<string>();

        // Assert
        Assert.Equal(arrayJson, result.Json);
    }

    [Fact]
    public void AsJsonDataArray_OnWrapper_WithMultipleElementTypes_ReturnsCorrectType()
    {
        // Arrange
        var arrayJson = ArrayJsonData();
        var mock = new Mock<IJsonDataWrapper>();
        mock.Setup(w => w.Json).Returns(arrayJson);

        // Act
        var result = mock.Object.AsJsonDataArray<int>();

        // Assert
        Assert.Equal(arrayJson, result.Json);
    }

    [Fact]
    public void AsJsonDataArray_OnWrapper_WithNonArrayJson_ThrowsArgumentException()
    {
        // Arrange
        var nonArrayJson = NonArrayJsonData();
        var mock = new Mock<IJsonDataWrapper>();
        mock.Setup(w => w.Json).Returns(nonArrayJson);

        // Act & Assert
        Assert.Throws<ArgumentException>(() => mock.Object.AsJsonDataArray<string>());
    }

    // -- IJsonDataWrapper.AsJsonData() ---------------------------------------

    [Fact]
    public void AsJsonData_OnWrapper_ReturnsWrapperJson()
    {
        // Arrange
        var arrayJson = ArrayJsonData();
        var mock = new Mock<IJsonDataWrapper>();
        mock.Setup(w => w.Json).Returns(arrayJson);

        // Act
        var result = mock.Object.AsJsonData();

        // Assert
        Assert.Equal(arrayJson, result);
    }

    [Fact]
    public void AsJsonData_OnWrapper_WithNonArrayJson_ReturnsNonArrayJson()
    {
        // Arrange
        var jsonData = NonArrayJsonData();
        var mock = new Mock<IJsonDataWrapper>();
        mock.Setup(w => w.Json).Returns(jsonData);

        // Act
        var result = mock.Object.AsJsonData();

        // Assert
        Assert.Equal(jsonData, result);
    }

    // -- JsonData.AsJsonDataArray<T>() ----------------------------------------

    [Fact]
    public void AsJsonDataArray_OnJsonData_WithArrayJson_ReturnsJsonDataArrayWithSameJson()
    {
        // Arrange
        var arrayJson = ArrayJsonData();

        // Act
        var result = arrayJson.AsJsonDataArray<string>();

        // Assert
        Assert.Equal(arrayJson, result.Json);
    }

    [Fact]
    public void AsJsonDataArray_OnJsonData_WithNonArrayJson_ThrowsArgumentException()
    {
        // Arrange
        var nonArrayJson = NonArrayJsonData();

        // Act & Assert
        Assert.Throws<ArgumentException>(() => nonArrayJson.AsJsonDataArray<string>());
    }

    // -- JsonData.GetArray<T>() -----------------------------------------------

    [Fact]
    public void GetArray_OnArrayJson_ReturnsJsonDataArray()
    {
        // Arrange
        var arrayJson = ArrayJsonData();

        // Act
        var result = arrayJson.GetArray<string>();

        // Assert
        Assert.Equal(arrayJson, result.Json);
    }

    [Fact]
    public void GetArray_OnNonArrayJson_Throws()
    {
        // Arrange
        var nonArrayJson = NonArrayJsonData();

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => nonArrayJson.GetArray<string>());
    }

    // -- JsonData.TryGetArray<T>() --------------------------------------------

    [Fact]
    public void TryGetArray_OnArrayJson_ReturnsJsonDataArray()
    {
        // Arrange
        var arrayJson = ArrayJsonData();

        // Act
        var result = arrayJson.TryGetArray<string>();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(arrayJson, result!.Value.Json);
    }

    [Fact]
    public void TryGetArray_OnNonArrayJson_ReturnsNull()
    {
        // Arrange
        var nonArrayJson = NonArrayJsonData();

        // Act
        var result = nonArrayJson.TryGetArray<string>();

        // Assert
        Assert.Null(result);
    }

    // -- JsonData.TryGetArray<T>(out array) ----------------------------------

    [Fact]
    public void TryGetArrayOut_OnArrayJson_ReturnsTrueAndArray()
    {
        // Arrange
        var arrayJson = ArrayJsonData();

        // Act
        var success = arrayJson.TryGetArray<string>(out var array);

        // Assert
        Assert.True(success);
        Assert.Equal(arrayJson, array.Json);
    }

    [Fact]
    public void TryGetArrayOut_OnNonArrayJson_ReturnsFalseAndDefault()
    {
        // Arrange
        var nonArrayJson = NonArrayJsonData();

        // Act
        var success = nonArrayJson.TryGetArray<string>(out var array);

        // Assert
        Assert.False(success);
        Assert.Equal(default, array);
    }

    // -- JsonData.GetArray<T>(string name) -----------------------------------

    [Fact]
    public void GetArrayByName_WhenPropertyExistsAndIsArray_ReturnsJsonDataArray()
    {
        // Arrange
        var parent = NonArrayJsonData();
        var child = ArrayJsonData();
        parent.Set("items", child);

        // Act
        var result = parent.GetArray<string>("items");

        // Assert
        Assert.Equal(child, result.Json);
    }

    [Fact]
    public void GetArrayByName_WhenPropertyMissing_Throws()
    {
        // Arrange
        var parent = NonArrayJsonData();

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => parent.GetArray<string>("missing"));
    }

    [Fact]
    public void GetArrayByName_WhenPropertyExistsButNotArray_Throws()
    {
        // Arrange
        var parent = NonArrayJsonData();
        parent.Set("items", NonArrayJsonData());

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => parent.GetArray<string>("items"));
    }

    // -- JsonData.TryGetArray<T>(string name) --------------------------------

    [Fact]
    public void TryGetArrayByName_WhenPropertyExistsAndIsArray_ReturnsArray()
    {
        // Arrange
        var parent = NonArrayJsonData();
        var child = ArrayJsonData();
        parent.Set("items", child);

        // Act
        var result = parent.TryGetArray<string>("items");

        // Assert
        Assert.NotNull(result);
        Assert.Equal(child, result!.Value.Json);
    }

    [Fact]
    public void TryGetArrayByName_WhenPropertyMissing_ReturnsNull()
    {
        // Arrange
        var parent = NonArrayJsonData();

        // Act
        var result = parent.TryGetArray<string>("missing");

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void TryGetArrayByName_WhenPropertyExistsButNotArray_ReturnsNull()
    {
        // Arrange
        var parent = NonArrayJsonData();
        parent.Set("items", NonArrayJsonData());

        // Act
        var result = parent.TryGetArray<string>("items");

        // Assert
        Assert.Null(result);
    }

    // -- JsonData.TryGetArray<T>(string name, out array) ---------------------

    [Fact]
    public void TryGetArrayByNameOut_WhenPropertyExistsAndIsArray_ReturnsTrueAndArray()
    {
        // Arrange
        var parent = NonArrayJsonData();
        var child = ArrayJsonData();
        parent.Set("items", child);

        // Act
        var success = parent.TryGetArray<string>("items", out var array);

        // Assert
        Assert.True(success);
        Assert.Equal(child, array.Json);
    }

    [Fact]
    public void TryGetArrayByNameOut_WhenPropertyMissing_ReturnsFalse()
    {
        // Arrange
        var parent = NonArrayJsonData();

        // Act
        var success = parent.TryGetArray<string>("missing", out var array);

        // Assert
        Assert.False(success);
        Assert.Equal(default, array);
    }

    [Fact]
    public void TryGetArrayByNameOut_WhenPropertyExistsButNotArray_ReturnsFalse()
    {
        // Arrange
        var parent = NonArrayJsonData();
        parent.Set("items", NonArrayJsonData());

        // Act
        var success = parent.TryGetArray<string>("items", out var array);

        // Assert
        Assert.False(success);
        Assert.Equal(default, array);
    }

    // -- JsonData.Set<T>(string name, JsonDataArray<T>?) ---------------------

    [Fact]
    public void Set_WithNonNullArray_SetsPropertyOnJsonData()
    {
        // Arrange
        var parent = NonArrayJsonData();
        var child = ArrayJsonData();
        var array = child.AsJsonDataArray<string>();

        // Act
        parent.Set<string>("items", array);

        // Assert
        Assert.True(parent.TryGetArray<string>("items", out _));
    }

    [Fact]
    public void Set_WithNullArray_RemovesPropertyFromJsonData()
    {
        // Arrange
        var parent = NonArrayJsonData();
        var child = ArrayJsonData();
        parent.Set("items", child);
        Assert.True(parent.TryGetArray<string>("items", out _)); // pre-condition

        // Act
        parent.Set<string>("items", null);

        // Assert
        Assert.False(parent.TryGetArray<string>("items", out _));
    }

    [Fact]
    public void Set_WithNullArray_WhenPropertyNotPresent_DoesNotThrow()
    {
        // Arrange
        var parent = NonArrayJsonData();

        // Act & Assert
        var exception = Record.Exception(() => parent.Set<string>("items", null));
        Assert.Null(exception);
    }

    // -- JsonData.GetOrCreateArray<T>(string name) ---------------------------

    [Fact]
    public void GetOrCreateArray_WhenPropertyExistsAndIsArray_ReturnsExistingArray()
    {
        // Arrange
        var parent = NonArrayJsonData();
        var child = ArrayJsonData();
        parent.Set("items", child);

        // Act
        var result = parent.GetOrCreateArray<string>("items");

        // Assert
        Assert.True(result.Json.IsArray);
        Assert.True(parent.TryGetArray<string>("items", out _));
    }

    [Fact]
    public void GetOrCreateArray_WhenPropertyMissing_CreatesAndReturnsNewArray()
    {
        // Arrange
        var parent = NonArrayJsonData();

        // Act
        var result = parent.GetOrCreateArray<string>("items");

        // Assert
        Assert.True(result.Json.IsArray);
        Assert.True(parent.TryGetArray<string>("items", out _));
    }

    [Fact]
    public void GetOrCreateArray_WhenReadOnlyAndPropertyMissing_ThrowsInvalidOperationException()
    {
        // Arrange
        var parent = JsonData.CreateReadOnly(new System.Text.Json.Nodes.JsonObject());

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => parent.GetOrCreateArray<string>("items"));
    }

    [Fact]
    public void GetOrCreateArray_WhenPropertyExistsButNotArray_ThrowsInvalidOperationException()
    {
        // Arrange
        var parent = NonArrayJsonData();
        parent.Set("items", NonArrayJsonData());

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => parent.GetOrCreateArray<string>("items"));
    }

    // -- JsonData.GetArray<T>(int index) -------------------------------------

    [Fact]
    public void GetArrayByIndex_WhenIndexExistsAndIsArray_ReturnsJsonDataArray()
    {
        // Arrange
        var parent = ArrayJsonData();
        var child = ArrayJsonData();
        parent.Add(child);

        // Act
        var result = parent.GetArray<string>(0);

        // Assert
        Assert.True(result.Json.IsArray);
    }

    [Fact]
    public void GetArrayByIndex_WhenIndexOutOfRange_Throws()
    {
        // Arrange
        var parent = ArrayJsonData();

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => parent.GetArray<string>(0));
    }

    [Fact]
    public void GetArrayByIndex_WhenIndexExistsButNotArray_Throws()
    {
        // Arrange
        var parent = ArrayJsonData();
        parent.Add(NonArrayJsonData());

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => parent.GetArray<string>(0));
    }

    // -- JsonData.TryGetArray<T>(int index) ----------------------------------

    [Fact]
    public void TryGetArrayByIndex_WhenIndexExistsAndIsArray_ReturnsArray()
    {
        // Arrange
        var parent = ArrayJsonData();
        var child = ArrayJsonData();
        parent.Add(child);

        // Act
        var result = parent.TryGetArray<string>(0);

        // Assert
        Assert.NotNull(result);
        Assert.True(result!.Value.Json.IsArray);
    }

    [Fact]
    public void TryGetArrayByIndex_WhenIndexOutOfRange_ReturnsNull()
    {
        // Arrange
        var parent = ArrayJsonData();

        // Act
        var result = parent.TryGetArray<string>(0);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void TryGetArrayByIndex_WhenIndexExistsButNotArray_ReturnsNull()
    {
        // Arrange
        var parent = ArrayJsonData();
        parent.Add(NonArrayJsonData());

        // Act
        var result = parent.TryGetArray<string>(0);

        // Assert
        Assert.Null(result);
    }

    // -- JsonData.TryGetArray<T>(int index, out array) -----------------------

    [Fact]
    public void TryGetArrayByIndexOut_WhenIndexExistsAndIsArray_ReturnsTrueAndArray()
    {
        // Arrange
        var parent = ArrayJsonData();
        var child = ArrayJsonData();
        parent.Add(child);

        // Act
        var success = parent.TryGetArray<string>(0, out var array);

        // Assert
        Assert.True(success);
        Assert.True(array.Json.IsArray);
    }

    [Fact]
    public void TryGetArrayByIndexOut_WhenIndexOutOfRange_ReturnsFalse()
    {
        // Arrange
        var parent = ArrayJsonData();

        // Act
        var success = parent.TryGetArray<string>(0, out var array);

        // Assert
        Assert.False(success);
        Assert.Equal(default, array);
    }

    [Fact]
    public void TryGetArrayByIndexOut_WhenIndexExistsButNotArray_ReturnsFalse()
    {
        // Arrange
        var parent = ArrayJsonData();
        parent.Add(NonArrayJsonData());

        // Act
        var success = parent.TryGetArray<string>(0, out var array);

        // Assert
        Assert.False(success);
        Assert.Equal(default, array);
    }

    // -- JsonData.Set<T>(int index, JsonDataArray<T>?) -----------------------

    [Fact]
    public void SetByIndex_WithNonNullArray_SetsElementAtIndex()
    {
        // Arrange
        var parent = ArrayJsonData();
        parent.Add(NonArrayJsonData()); // placeholder at index 0
        var child = ArrayJsonData();
        var array = child.AsJsonDataArray<string>();

        // Act
        parent.Set<string>(0, array);

        // Assert
        Assert.True(parent.TryGetArray<string>(0, out _));
    }

    [Fact]
    public void SetByIndex_WithNullArray_RemovesElementAtIndex()
    {
        // Arrange
        var parent = ArrayJsonData();
        parent.Add(ArrayJsonData()); // element at index 0
        Assert.True(parent.TryGetArray<string>(0, out _)); // pre-condition

        // Act
        parent.Set<string>(0, null);

        // Assert
        Assert.False(parent.TryGetArray<string>(0, out _));
    }

    // -- JsonData.EnsureArray<T>(int index) ----------------------------------

    [Fact]
    public void EnsureArrayByIndex_WhenIndexExistsAndIsArray_ReturnsExistingArray()
    {
        // Arrange
        var parent = ArrayJsonData();
        var child = ArrayJsonData();
        parent.Add(child);

        // Act
        var result = parent.EnsureArray<string>(0);

        // Assert
        Assert.True(result.Json.IsArray);
        Assert.Equal(child, result.Json);
    }

    [Fact]
    public void EnsureArrayByIndex_WhenIndexMissing_CreatesAndReturnsNewArray()
    {
        // Arrange
        var parent = ArrayJsonData();

        // Act
        var result = parent.EnsureArray<string>(0);

        // Assert
        Assert.True(result.Json.IsArray);
        Assert.True(parent.TryGetArray<string>(0, out _));
    }

    [Fact]
    public void EnsureArrayByIndex_WhenReadOnlyAndIndexMissing_ThrowsInvalidOperationException()
    {
        // Arrange
        var parent = JsonData.CreateReadOnly(new System.Text.Json.Nodes.JsonArray());

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => parent.EnsureArray<string>(0));
    }

    [Fact]
    public void EnsureArrayByIndex_WhenIndexExistsButNotArray_ThrowsInvalidOperationException()
    {
        // Arrange
        var parent = ArrayJsonData();
        parent.Add(NonArrayJsonData());

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => parent.EnsureArray<string>(0));
    }
}
