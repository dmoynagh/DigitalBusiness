using System;
using System.Text.Json.Nodes;
using DigitalBusiness.JsonDataWrappers;
using Xunit;

namespace DigitalBusiness.JsonDataWrappers.Tests;

public class TypedJsonDataArrayTests
{
    // -- Helpers -------------------------------------------------------------

    private static JsonData ArrayJsonData() => new JsonData(new JsonArray());
    private static JsonData NonArrayJsonData() => new JsonData(new JsonObject());

    // -- Json property --------------------------------------------------------

    [Fact]
    public void Json_InitWithArrayJsonData_StoresValue()
    {
        // Arrange
        var arrayJson = ArrayJsonData();

        // Act
        var sut = new JsonDataArray<string> { Json = arrayJson };

        // Assert
        Assert.Equal(arrayJson, sut.Json);
    }

    [Fact]
    public void Json_InitWithNonArrayJsonData_ThrowsArgumentException()
    {
        // Arrange
        var nonArray = NonArrayJsonData();

        // Act & Assert
        Assert.Throws<ArgumentException>(() => new JsonDataArray<string> { Json = nonArray });
    }

    // -- ReadOnly property ----------------------------------------------------

    [Fact]
    public void ReadOnly_WhenJsonIsReadOnly_ReturnsTrue()
    {
        // Arrange – JsonArray wrapped with readOnly=true
        var readOnlyJson = new JsonData(new JsonArray(), readOnly: true);
        var sut = new JsonDataArray<string> { Json = readOnlyJson };

        // Act & Assert
        Assert.True(sut.ReadOnly);
    }

    [Fact]
    public void ReadOnly_WhenJsonIsWritable_ReturnsFalse()
    {
        // Arrange – JsonArray wrapped with readOnly=false (default for JsonArray node)
        var writableJson = ArrayJsonData();
        var sut = new JsonDataArray<string> { Json = writableJson };

        // Act & Assert
        Assert.False(sut.ReadOnly);
    }

    // -- Count property --------------------------------------------------------

    [Fact]
    public void Count_EmptyArray_ReturnsZero()
    {
        // Arrange
        var sut = new JsonDataArray<string> { Json = ArrayJsonData() };

        // Act & Assert
        Assert.Equal(0, sut.Count);
    }

    [Fact]
    public void Count_AfterAddingItems_ReturnsCorrectCount()
    {
        // Arrange
        var array = new JsonArray();
        var sut = new JsonDataArray<string> { Json = new JsonData(array) };

        // Act
        sut.Add("a");
        sut.Add("b");

        // Assert
        Assert.Equal(2, sut.Count);
    }

    // -- Add method ------------------------------------------------------------

    [Fact]
    public void Add_SingleItem_IncreasesCountByOne()
    {
        // Arrange
        var sut = new JsonDataArray<int> { Json = new JsonData(new JsonArray()) };

        // Act
        sut.Add(42);

        // Assert
        Assert.Equal(1, sut.Count);
    }

    [Fact]
    public void Add_MultipleItems_ItemsAreRetrievable()
    {
        // Arrange
        var sut = new JsonDataArray<string> { Json = new JsonData(new JsonArray()) };

        // Act
        sut.Add("hello");
        sut.Add("world");

        // Assert
        Assert.Equal(2, sut.Count);
        Assert.Equal("hello", sut[0]);
        Assert.Equal("world", sut[1]);
    }

    // -- Insert method ---------------------------------------------------------

    [Fact]
    public void Insert_AtIndexZero_InsertsAtBeginning()
    {
        // Arrange
        var sut = new JsonDataArray<string> { Json = new JsonData(new JsonArray()) };
        sut.Add("second");

        // Act
        sut.Insert(0, "first");

        // Assert
        Assert.Equal(2, sut.Count);
        Assert.Equal("first", sut[0]);
        Assert.Equal("second", sut[1]);
    }

    [Fact]
    public void Insert_AtEnd_AppendsItem()
    {
        // Arrange
        var sut = new JsonDataArray<string> { Json = new JsonData(new JsonArray()) };
        sut.Add("first");

        // Act
        sut.Insert(1, "second");

        // Assert
        Assert.Equal(2, sut.Count);
        Assert.Equal("second", sut[1]);
    }

    [Fact]
    public void Insert_InMiddle_ShiftsExistingItems()
    {
        // Arrange
        var sut = new JsonDataArray<string> { Json = new JsonData(new JsonArray()) };
        sut.Add("a");
        sut.Add("c");

        // Act
        sut.Insert(1, "b");

        // Assert
        Assert.Equal(3, sut.Count);
        Assert.Equal("a", sut[0]);
        Assert.Equal("b", sut[1]);
        Assert.Equal("c", sut[2]);
    }

    // -- Clear method ----------------------------------------------------------

    [Fact]
    public void Clear_EmptyArray_CountRemainsZero()
    {
        // Arrange
        var sut = new JsonDataArray<string> { Json = new JsonData(new JsonArray()) };

        // Act
        sut.Clear();

        // Assert
        Assert.Equal(0, sut.Count);
    }

    [Fact]
    public void Clear_ArrayWithItems_CountBecomesZero()
    {
        // Arrange
        var sut = new JsonDataArray<string> { Json = new JsonData(new JsonArray()) };
        sut.Add("a");
        sut.Add("b");
        sut.Add("c");

        // Act
        sut.Clear();

        // Assert
        Assert.Equal(0, sut.Count);
    }

    // -- RemoveAt method -------------------------------------------------------

    [Fact]
    public void RemoveAt_ValidIndex_RemovesItem()
    {
        // Arrange
        var sut = new JsonDataArray<string> { Json = new JsonData(new JsonArray()) };
        sut.Add("a");
        sut.Add("b");
        sut.Add("c");

        // Act
        sut.RemoveAt(1);

        // Assert
        Assert.Equal(2, sut.Count);
        Assert.Equal("a", sut[0]);
        Assert.Equal("c", sut[1]);
    }

    [Fact]
    public void RemoveAt_FirstIndex_RemovesFirstItem()
    {
        // Arrange
        var sut = new JsonDataArray<string> { Json = new JsonData(new JsonArray()) };
        sut.Add("first");
        sut.Add("second");

        // Act
        sut.RemoveAt(0);

        // Assert
        Assert.Equal(1, sut.Count);
        Assert.Equal("second", sut[0]);
    }

    [Fact]
    public void RemoveAt_LastIndex_RemovesLastItem()
    {
        // Arrange
        var sut = new JsonDataArray<string> { Json = new JsonData(new JsonArray()) };
        sut.Add("first");
        sut.Add("last");

        // Act
        sut.RemoveAt(1);

        // Assert
        Assert.Equal(1, sut.Count);
        Assert.Equal("first", sut[0]);
    }

    // -- GetEnumerator methods -------------------------------------------------

    [Fact]
    public void GetEnumeratorGeneric_EmptyArray_YieldsNoItems()
    {
        // Arrange
        var sut = new JsonDataArray<string> { Json = new JsonData(new JsonArray()) };

        // Act
        var items = new List<string>();
        foreach (var item in sut)
            items.Add(item);

        // Assert
        Assert.Empty(items);
    }

    [Fact]
    public void GetEnumeratorGeneric_ArrayWithItems_YieldsAllItems()
    {
        // Arrange
        var sut = new JsonDataArray<int> { Json = new JsonData(new JsonArray()) };
        sut.Add(1);
        sut.Add(2);
        sut.Add(3);

        // Act
        var items = new List<int>();
        foreach (var item in sut)
            items.Add(item);

        // Assert
        Assert.Equal(new[] { 1, 2, 3 }, items);
    }

    [Fact]
    public void GetEnumeratorNonGeneric_ArrayWithItems_YieldsAllItems()
    {
        // Arrange
        var sut = new JsonDataArray<string> { Json = new JsonData(new JsonArray()) };
        sut.Add("x");
        sut.Add("y");

        // Act
        var items = new List<object?>();
        var enumerator = ((System.Collections.IEnumerable)sut).GetEnumerator();
        while (enumerator.MoveNext())
            items.Add(enumerator.Current);

        // Assert
        Assert.Equal(new object[] { "x", "y" }, items);
    }
}
