using System;
using System.Text.Json.Nodes;
using DigitalBusiness.Json.JsonPaths;
using DigitalBusiness.JsonDataWrappers;
using Xunit;

namespace DigitalBusiness.JsonDataWrappers.Tests;

public class JsonDataTypedPathExtensionsTests
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
    // Get<T>(JsonPath)
    // ----------------------------------------------------------------

    [Fact]
    public void GetTyped_NestedIntByPath_ReturnsValue()
    {
        var data = MakeObject(o => o["a"] = new JsonObject { ["b"] = 42 });
        Assert.Equal(42, data.Get<int>(JsonPath.Parse("a.b")));
    }

    [Fact]
    public void GetTyped_NestedStringByPath_ReturnsValue()
    {
        var data = MakeObject(o => o["name"] = "hello");
        Assert.Equal("hello", data.Get<string>(JsonPath.From("name")));
    }

    [Fact]
    public void GetTyped_MissingPath_ThrowsKeyNotFoundException()
    {
        var data = MakeObject(_ => { });
        Assert.Throws<System.Collections.Generic.KeyNotFoundException>(() =>
            data.Get<int>(JsonPath.Parse("a.b")));
    }

    // ----------------------------------------------------------------
    // TryGet<T>(JsonPath)
    // ----------------------------------------------------------------

    [Fact]
    public void TryGetTyped_ExistingPath_ReturnsValue()
    {
        var data = MakeObject(o => o["x"] = 7);
        Assert.True(data.TryGet<int>(JsonPath.From("x"), out var val));
        Assert.Equal(7, val);
    }

    [Fact]
    public void TryGetTyped_MissingPath_ReturnsFalse()
    {
        var data = MakeObject(_ => { });
        Assert.False(data.TryGet<int>(JsonPath.From("missing"), out _));
    }

    [Fact]
    public void TryGetTyped_Nullable_ExistingPath_ReturnsDefault()
    {
        var data = MakeObject(o => o["x"] = 99);
        var result = data.TryGet<int>(JsonPath.From("x"));
        Assert.Equal(99, result);
    }

    [Fact]
    public void TryGetTyped_Nullable_MissingPath_ReturnsDefault()
    {
        var data = MakeObject(_ => { });
        var result = data.TryGet<int>(JsonPath.From("missing"));
        Assert.Equal(0, result);
    }

    // ----------------------------------------------------------------
    // Set<T>(JsonPath, T?)
    // ----------------------------------------------------------------

    [Fact]
    public void SetTyped_NestedPath_StoresValue()
    {
        var data = MakeObject(o => o["a"] = new JsonObject());
        data.Set<int>(JsonPath.From("a", "count"), 5);
        Assert.Equal(5, data.Get<int>(JsonPath.Parse("a.count")));
    }

    [Fact]
    public void SetTyped_NullValue_RemovesProperty()
    {
        var data = MakeObject(o => o["a"] = new JsonObject { ["b"] = 1 });
        data.Set<string>(JsonPath.From("a", "b"), null);
        Assert.False(data.Contains(JsonPath.From("a", "b")));
    }

    // ----------------------------------------------------------------
    // SetDeep<T>(JsonPath, T?)
    // ----------------------------------------------------------------

    [Fact]
    public void SetDeepTyped_CreatesIntermediatesAndStoresValue()
    {
        var data = new JsonData(new JsonObject());
        data.SetDeep<string>(JsonPath.Parse("user.profile.name"), "Alice");
        Assert.Equal("Alice", data.Get<string>(JsonPath.Parse("user.profile.name")));
    }

    [Fact]
    public void SetDeepTyped_NullValue_RemovesLeaf()
    {
        var data = MakeObject(o => o["a"] = new JsonObject { ["b"] = 1 });
        data.SetDeep<string>(JsonPath.Parse("a.b"), null);
        Assert.False(data.Contains(JsonPath.From("a", "b")));
    }

    // ----------------------------------------------------------------
    // Ensure<T>(JsonPath, Func<T>)
    // ----------------------------------------------------------------

    [Fact]
    public void Ensure_MissingProperty_CreatesAndReturnsDefault()
    {
        var data = MakeObject(o => o["a"] = new JsonObject());
        var result = data.Ensure<int>(JsonPath.From("a", "count"), () => 10);
        Assert.Equal(10, result);
        Assert.Equal(10, data.Get<int>(JsonPath.From("a", "count")));
    }

    [Fact]
    public void Ensure_ExistingProperty_ReturnsExistingValue()
    {
        var data = MakeObject(o => o["a"] = new JsonObject { ["count"] = 7 });
        int calls = 0;
        var result = data.Ensure<int>(JsonPath.From("a", "count"), () => { calls++; return 99; });
        Assert.Equal(7, result);
        Assert.Equal(0, calls);
    }

    [Fact]
    public void Ensure_Value_MissingProperty_CreatesAndReturns()
    {
        var data = MakeObject(o => o["a"] = new JsonObject());
        var result = data.Ensure<string>(JsonPath.From("a", "tag"), "default");
        Assert.Equal("default", result);
    }

    // ----------------------------------------------------------------
    // Add<T>(JsonPath, T) and Insert<T>(JsonPath, int, T)
    // ----------------------------------------------------------------

    [Fact]
    public void AddTyped_ByPath_AppendsToArray()
    {
        var data = MakeObject(o => o["items"] = new JsonArray());
        data.Add<int>(JsonPath.From("items"), 42);
        Assert.Equal(1, data.Get(JsonPath.From("items")).Count);
        Assert.Equal(42, data.Get<int>(JsonPath.From("items", 0)));
    }

    [Fact]
    public void AddTyped_NonArrayPath_ThrowsInvalidOperation()
    {
        var data = MakeObject(o => o["items"] = new JsonObject());
        Assert.Throws<InvalidOperationException>(() => data.Add<int>(JsonPath.From("items"), 1));
    }

    [Fact]
    public void InsertTyped_ByPath_InsertsAtIndex()
    {
        var data = MakeObject(o => o["items"] = new JsonArray(JsonValue.Create(1), JsonValue.Create(3)));
        data.Insert<int>(JsonPath.From("items"), 1, 2);
        Assert.Equal(2, data.Get<int>(JsonPath.From("items", 1)));
        Assert.Equal(3, data.Get<int>(JsonPath.From("items", 2)));
    }

    [Fact]
    public void InsertTyped_NonArrayPath_ThrowsInvalidOperation()
    {
        var data = MakeObject(o => o["items"] = new JsonObject());
        Assert.Throws<InvalidOperationException>(() => data.Insert<int>(JsonPath.From("items"), 0, 1));
    }

    // ----------------------------------------------------------------
    // GetArray<T> / TryGetArray<T>
    // ----------------------------------------------------------------

    [Fact]
    public void GetArray_ByPath_ReturnsTypedArray()
    {
        var data = MakeObject(o => o["nums"] = new JsonArray(JsonValue.Create(1), JsonValue.Create(2)));
        var arr = data.GetArray<int>(JsonPath.From("nums"));
        Assert.Equal(2, arr.Count);
    }

    [Fact]
    public void GetArray_NonArrayPath_Throws()
    {
        var data = MakeObject(o => o["nums"] = 5);
        Assert.ThrowsAny<Exception>(() => data.GetArray<int>(JsonPath.From("nums")));
    }

    [Fact]
    public void TryGetArray_ExistingArray_ReturnsTrue()
    {
        var data = MakeObject(o => o["nums"] = new JsonArray(JsonValue.Create(10)));
        Assert.True(data.TryGetArray<int>(JsonPath.From("nums"), out var arr));
        Assert.Equal(1, arr.Count);
    }

    [Fact]
    public void TryGetArray_NonArray_ReturnsFalse()
    {
        var data = MakeObject(o => o["nums"] = 5);
        Assert.False(data.TryGetArray<int>(JsonPath.From("nums"), out _));
    }

    [Fact]
    public void TryGetArray_MissingPath_ReturnsFalse()
    {
        var data = MakeObject(_ => { });
        Assert.False(data.TryGetArray<int>(JsonPath.From("missing"), out _));
    }
}
