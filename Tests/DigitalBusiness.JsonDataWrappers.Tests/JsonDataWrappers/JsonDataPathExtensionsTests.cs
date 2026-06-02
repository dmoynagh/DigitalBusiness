using System;
using System.Collections.Generic;
using System.Text.Json.Nodes;
using DigitalBusiness.Json.JsonPaths;
using DigitalBusiness.JsonDataWrappers;
using Xunit;

namespace DigitalBusiness.JsonDataWrappers.Tests;

public class JsonDataPathExtensionsTests
{
    // ----------------------------------------------------------------
    // Helpers
    // ----------------------------------------------------------------

    private static JsonData MakeObject(Action<JsonObject> init)
    {
        var obj = new JsonObject();
        init(obj);
        return new JsonData(obj);
    }

    // ----------------------------------------------------------------
    // Contains
    // ----------------------------------------------------------------

    [Fact]
    public void Contains_ExistingPath_ReturnsTrue()
    {
        var data = MakeObject(o => o["a"] = new JsonObject { ["b"] = 1 });
        Assert.True(data.Contains(JsonPath.Parse("a.b")));
    }

    [Fact]
    public void Contains_MissingPath_ReturnsFalse()
    {
        var data = MakeObject(o => o["a"] = new JsonObject());
        Assert.False(data.Contains(JsonPath.Parse("a.missing")));
    }

    [Fact]
    public void Contains_SingleSegmentPath_EquivalentToContainsProperty()
    {
        var data = MakeObject(o => o["x"] = 42);
        Assert.True(data.Contains(JsonPath.From("x")));
    }

    // ----------------------------------------------------------------
    // HasValue
    // ----------------------------------------------------------------

    [Fact]
    public void HasValue_NullProperty_ReturnsFalse()
    {
        var data = MakeObject(o => o["a"] = (JsonNode?)null);
        Assert.False(data.HasValue(JsonPath.From("a")));
    }

    [Fact]
    public void HasValue_NonNullProperty_ReturnsTrue()
    {
        var data = MakeObject(o => o["a"] = 5);
        Assert.True(data.HasValue(JsonPath.From("a")));
    }

    // ----------------------------------------------------------------
    // TryGet / Get — single segment
    // ----------------------------------------------------------------

    [Fact]
    public void TryGet_SinglePropertySegment_ReturnsNode()
    {
        var data = MakeObject(o => o["x"] = 7);
        Assert.True(data.TryGet(JsonPath.From("x"), out var result));
        Assert.Equal(7, result.Get<int>());
    }

    [Fact]
    public void TryGet_MissingSegment_ReturnsFalse()
    {
        var data = MakeObject(_ => { });
        Assert.False(data.TryGet(JsonPath.From("missing"), out _));
    }

    [Fact]
    public void Get_ExistingPath_ReturnsNode()
    {
        var data = MakeObject(o => o["a"] = new JsonObject { ["b"] = "hello" });
        var result = data.Get(JsonPath.Parse("a.b"));
        Assert.Equal("hello", result.Get<string>());
    }

    [Fact]
    public void Get_MissingPath_ThrowsKeyNotFoundException()
    {
        var data = MakeObject(o => o["a"] = new JsonObject());
        Assert.Throws<KeyNotFoundException>(() => data.Get(JsonPath.Parse("a.missing")));
    }

    // ----------------------------------------------------------------
    // TryGet — nested array index
    // ----------------------------------------------------------------

    [Fact]
    public void TryGet_ArrayIndexPath_ReturnsElement()
    {
        var arr = new JsonArray(JsonValue.Create(10), JsonValue.Create(20));
        var data = MakeObject(o => o["arr"] = arr);
        Assert.True(data.TryGet(JsonPath.From("arr", 1), out var result));
        Assert.Equal(20, result.Get<int>());
    }

    [Fact]
    public void TryGet_OutOfRangeIndex_ReturnsFalse()
    {
        var arr = new JsonArray(JsonValue.Create(1));
        var data = MakeObject(o => o["arr"] = arr);
        Assert.False(data.TryGet(JsonPath.From("arr", 5), out _));
    }

    // ----------------------------------------------------------------
    // Set(JsonPath, JsonData?)
    // ----------------------------------------------------------------

    [Fact]
    public void Set_NestedPath_SetsValue()
    {
        var data = MakeObject(o => o["a"] = new JsonObject());
        data.Set(JsonPath.From("a", "b"), new JsonData(JsonValue.Create(99)));
        Assert.Equal(99, data.Get(JsonPath.From("a", "b")).Get<int>());
    }

    [Fact]
    public void Set_NullValue_RemovesProperty()
    {
        var data = MakeObject(o => o["a"] = new JsonObject { ["b"] = 1 });
        data.Set(JsonPath.From("a", "b"), (JsonData?)null);
        Assert.False(data.Contains(JsonPath.From("a", "b")));
    }

    [Fact]
    public void Set_MissingParent_Throws()
    {
        var data = MakeObject(_ => { });
        Assert.ThrowsAny<Exception>(() => data.Set(JsonPath.From("missing", "b"), new JsonData(JsonValue.Create(1))));
    }

    // ----------------------------------------------------------------
    // Remove(JsonPath)
    // ----------------------------------------------------------------

    [Fact]
    public void Remove_ExistingNestedProperty_ReturnsTrue()
    {
        var data = MakeObject(o => o["a"] = new JsonObject { ["b"] = 5 });
        bool removed = data.Remove(JsonPath.From("a", "b"));
        Assert.True(removed);
        Assert.False(data.Contains(JsonPath.From("a", "b")));
    }

    [Fact]
    public void Remove_NonExistentProperty_ReturnsFalse()
    {
        var data = MakeObject(o => o["a"] = new JsonObject());
        bool removed = data.Remove(JsonPath.From("a", "missing"));
        Assert.False(removed);
    }

    [Fact]
    public void Remove_ArrayIndex_RemovesItem()
    {
        var arr = new JsonArray(JsonValue.Create(1), JsonValue.Create(2), JsonValue.Create(3));
        var data = MakeObject(o => o["arr"] = arr);
        data.Remove(JsonPath.From("arr", 1));
        Assert.Equal(3, data.Get<int>(JsonPath.From("arr", 1))); // was index 2, now shifted to index 1
    }

    // ----------------------------------------------------------------
    // SetDeep — creates intermediates
    // ----------------------------------------------------------------

    [Fact]
    public void SetDeep_CreatesIntermediateObjects()
    {
        var data = new JsonData(new JsonObject());
        data.SetDeep(JsonPath.Parse("a.b.c"), new JsonData(JsonValue.Create(42)));
        Assert.Equal(42, data.Get<int>(JsonPath.Parse("a.b.c")));
    }

    [Fact]
    public void SetDeep_CreatesIntermediateArray()
    {
        var data = new JsonData(new JsonObject());
        // path: arr[0].name  -> arr should be created as array, arr[0] as object
        data.SetDeep(JsonPath.Parse("arr[0].name"), new JsonData(JsonValue.Create("test")));
        Assert.Equal("test", data.Get<string>(JsonPath.Parse("arr[0].name")));
    }

    [Fact]
    public void SetDeep_WrongIntermediateKind_Throws()
    {
        // arr is a string value, not an object — trying to navigate through it should throw
        var data = MakeObject(o => o["a"] = "not-an-object");
        Assert.ThrowsAny<InvalidOperationException>(() =>
            data.SetDeep(JsonPath.Parse("a.b"), new JsonData(JsonValue.Create(1))));
    }

    [Fact]
    public void SetDeep_ExistingPath_OverwritesValue()
    {
        var data = MakeObject(o => o["a"] = new JsonObject { ["b"] = 1 });
        data.SetDeep(JsonPath.Parse("a.b"), new JsonData(JsonValue.Create(99)));
        Assert.Equal(99, data.Get<int>(JsonPath.Parse("a.b")));
    }

    // ----------------------------------------------------------------
    // GetOrCreateObjectDeep
    // ----------------------------------------------------------------

    [Fact]
    public void GetOrCreateObjectDeep_CreatesNestedObject()
    {
        var data = new JsonData(new JsonObject());
        var obj = data.GetOrCreateObjectDeep(JsonPath.Parse("a.b.c"));
        Assert.True(obj.IsObject);
        Assert.True(data.Contains(JsonPath.Parse("a.b.c")));
    }

    [Fact]
    public void GetOrCreateObjectDeep_ExistingObject_ReturnsIt()
    {
        var data = MakeObject(o => o["a"] = new JsonObject { ["b"] = new JsonObject() });
        var obj = data.GetOrCreateObjectDeep(JsonPath.Parse("a.b"));
        Assert.True(obj.IsObject);
    }

    // ----------------------------------------------------------------
    // GetOrCreateArrayDeep
    // ----------------------------------------------------------------

    [Fact]
    public void GetOrCreateArrayDeep_CreatesNestedArray()
    {
        var data = new JsonData(new JsonObject());
        var arr = data.GetOrCreateArrayDeep(JsonPath.Parse("a.items"));
        Assert.True(arr.IsArray);
        Assert.True(data.Contains(JsonPath.Parse("a.items")));
    }

    [Fact]
    public void GetOrCreateArrayDeep_ExistingArray_ReturnsIt()
    {
        var data = MakeObject(o => o["a"] = new JsonObject { ["items"] = new JsonArray() });
        var arr = data.GetOrCreateArrayDeep(JsonPath.Parse("a.items"));
        Assert.True(arr.IsArray);
    }
}
