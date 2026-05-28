using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Nodes;
using DigitalBusiness.JsonDataWrappers;
using Xunit;

namespace DigitalBusiness.JsonDataWrappers.Tests;

public class JsonDataJsonArrayExtensionsTests
{
    // -- IsArray --------------------------------------------------------------

    [Fact]
    public void IsArray_WithJsonArrayNode_ReturnsTrue()
    {
        var data = new JsonData(new JsonArray());
        Assert.True(data.IsArray);
    }

    [Fact]
    public void IsArray_WithJsonObjectNode_ReturnsFalse()
    {
        var data = new JsonData(new JsonObject());
        Assert.False(data.IsArray);
    }

    [Fact]
    public void IsArray_WithJsonValueNode_ReturnsFalse()
    {
        var data = new JsonData(JsonValue.Create(42));
        Assert.False(data.IsArray);
    }

    [Fact]
    public void IsArray_WithNullNode_ReturnsFalse()
    {
        var data = new JsonData((JsonNode?)null);
        Assert.False(data.IsArray);
    }

    [Fact]
    public void IsArray_WithArrayJsonElement_ReturnsTrue()
    {
        var element = JsonDocument.Parse("[]").RootElement;
        var data = new JsonData(element);
        Assert.True(data.IsArray);
    }

    [Fact]
    public void IsArray_WithObjectJsonElement_ReturnsFalse()
    {
        var element = JsonDocument.Parse("{}").RootElement;
        var data = new JsonData(element);
        Assert.False(data.IsArray);
    }

    [Fact]
    public void IsArray_WithStringJsonElement_ReturnsFalse()
    {
        var element = JsonDocument.Parse("\"hello\"").RootElement;
        var data = new JsonData(element);
        Assert.False(data.IsArray);
    }

    // -- ThrowIfNotArray ------------------------------------------------------

    [Fact]
    public void ThrowIfNotArray_WhenArray_ReturnsTrue()
    {
        var data = new JsonData(new JsonArray());
        var result = data.ThrowIfNotArray();
        Assert.True(result);
    }

    [Fact]
    public void ThrowIfNotArray_WhenNotArray_ThrowsInvalidOperationException()
    {
        var data = new JsonData(new JsonObject());
        Assert.Throws<InvalidOperationException>(() => data.ThrowIfNotArray());
    }

    [Fact]
    public void ThrowIfNotArray_WhenNotArray_ExceptionMessageCorrect()
    {
        var data = new JsonData(new JsonObject());
        var ex = Assert.Throws<InvalidOperationException>(() => data.ThrowIfNotArray());
        Assert.Equal("Node is not an array.", ex.Message);
    }

    [Fact]
    public void ThrowIfNotArray_WithArrayJsonElement_ReturnsTrue()
    {
        var element = JsonDocument.Parse("[]").RootElement;
        var data = new JsonData(element);
        Assert.True(data.ThrowIfNotArray());
    }

    // -- EnsureArray ----------------------------------------------------------

    [Fact]
    public void EnsureArray_WhenArray_ReturnsSameJsonData()
    {
        var data = new JsonData(new JsonArray());
        var result = data.EnsureArray();
        Assert.Equal(data, result);
    }

    [Fact]
    public void EnsureArray_WhenNotArray_ThrowsInvalidOperationException()
    {
        var data = new JsonData(new JsonObject());
        Assert.Throws<InvalidOperationException>(() => data.EnsureArray());
    }

    [Fact]
    public void EnsureArray_WhenNotArray_ExceptionMessageCorrect()
    {
        var data = new JsonData(new JsonObject());
        var ex = Assert.Throws<InvalidOperationException>(() => data.EnsureArray());
        Assert.Equal("Node is not an array.", ex.Message);
    }

    [Fact]
    public void EnsureArray_WithArrayJsonElement_ReturnsJsonData()
    {
        var element = JsonDocument.Parse("[]").RootElement;
        var data = new JsonData(element);
        var result = data.EnsureArray();
        Assert.True(result.IsArray);
    }

    // -- CreateArray ----------------------------------------------------------

    [Fact]
    public void CreateArray_ReturnsJsonDataThatIsArray()
    {
        var result = JsonData.CreateArray();
        Assert.True(result.IsArray);
    }

    [Fact]
    public void CreateArray_ReturnsNewInstanceEachCall()
    {
        var first = JsonData.CreateArray();
        var second = JsonData.CreateArray();
        Assert.NotEqual(first, second);
    }

    // -- AsArray ---------------------------------------------------------------

    [Fact]
    public void AsArray_WhenArray_ReturnsSameJsonData()
    {
        var data = new JsonData(new JsonArray());
        var result = data.AsArray();
        Assert.Equal(data, result);
    }

    [Fact]
    public void AsArray_WhenNotArray_ThrowsInvalidOperationException()
    {
        var data = new JsonData(new JsonObject());
        Assert.Throws<InvalidOperationException>(() => data.AsArray());
    }

    [Fact]
    public void AsArray_WhenNotArray_ExceptionMessageCorrect()
    {
        var data = new JsonData(new JsonObject());
        var ex = Assert.Throws<InvalidOperationException>(() => data.AsArray());
        Assert.Equal("Node is not an array.", ex.Message);
    }

    [Fact]
    public void AsArray_WithArrayJsonElement_ReturnsJsonData()
    {
        var element = JsonDocument.Parse("[]").RootElement;
        var data = new JsonData(element);
        var result = data.AsArray();
        Assert.True(result.IsArray);
    }

    [Fact]
    public void AsArray_WithNullNode_ThrowsInvalidOperationException()
    {
        var data = new JsonData((JsonNode?)null);
        Assert.Throws<InvalidOperationException>(() => data.AsArray());
    }

    // -- TryAsArray ------------------------------------------------------------

    [Fact]
    public void TryAsArray_WhenArray_ReturnsJsonData()
    {
        var data = new JsonData(new JsonArray());
        var result = data.TryAsArray();
        Assert.NotNull(result);
        Assert.True(result.Value.IsArray);
    }

    [Fact]
    public void TryAsArray_WhenNotArray_ReturnsNull()
    {
        var data = new JsonData(new JsonObject());
        var result = data.TryAsArray();
        Assert.Null(result);
    }

    [Fact]
    public void TryAsArray_WithArrayJsonElement_ReturnsJsonData()
    {
        var element = JsonDocument.Parse("[]").RootElement;
        var data = new JsonData(element);
        var result = data.TryAsArray();
        Assert.NotNull(result);
    }

    [Fact]
    public void TryAsArray_WithObjectJsonElement_ReturnsNull()
    {
        var element = JsonDocument.Parse("{}").RootElement;
        var data = new JsonData(element);
        var result = data.TryAsArray();
        Assert.Null(result);
    }

    [Fact]
    public void TryAsArray_WithNullNode_ReturnsNull()
    {
        var data = new JsonData((JsonNode?)null);
        var result = data.TryAsArray();
        Assert.Null(result);
    }

    // -- Get(int index) --------------------------------------------------------

    [Fact]
    public void Get_WithValidIndex_ReturnsElement()
    {
        var array = new JsonArray(JsonValue.Create(42));
        var data = new JsonData(array);
        var result = data.Get(0);
        Assert.Equal(42, result.Node!.GetValue<int>());
    }

    [Fact]
    public void Get_WithValidIndexOnJsonElement_ReturnsElement()
    {
        var element = JsonDocument.Parse("[1,2,3]").RootElement;
        var data = new JsonData(element);
        var result = data.Get(1);
        Assert.Equal(JsonValueKind.Number, result.ValueKind);
    }

    [Fact]
    public void Get_WithOutOfRangeIndex_ThrowsIndexOutOfRangeException()
    {
        var array = new JsonArray(JsonValue.Create(1));
        var data = new JsonData(array);
        Assert.Throws<IndexOutOfRangeException>(() => data.Get(5));
    }

    [Fact]
    public void Get_WithNegativeIndex_ThrowsIndexOutOfRangeException()
    {
        var array = new JsonArray(JsonValue.Create(1));
        var data = new JsonData(array);
        Assert.Throws<IndexOutOfRangeException>(() => data.Get(-1));
    }

    [Fact]
    public void Get_OnNonArray_ThrowsInvalidOperationException()
    {
        var data = new JsonData(new JsonObject());
        Assert.Throws<InvalidOperationException>(() => data.Get(0));
    }

    [Fact]
    public void Get_WithOutOfRangeIndexOnJsonElement_ThrowsIndexOutOfRangeException()
    {
        var element = JsonDocument.Parse("[1]").RootElement;
        var data = new JsonData(element);
        Assert.Throws<IndexOutOfRangeException>(() => data.Get(10));
    }

    // -- TryGet(int index) ? JsonData? -----------------------------------------

    [Fact]
    public void TryGet_NullableReturn_WhenValidIndex_ReturnsElement()
    {
        var array = new JsonArray(JsonValue.Create("hello"));
        var data = new JsonData(array);
        var result = data.TryGet(0);
        Assert.NotNull(result);
    }

    [Fact]
    public void TryGet_NullableReturn_WhenOutOfRange_ReturnsNull()
    {
        var array = new JsonArray(JsonValue.Create(1));
        var data = new JsonData(array);
        var result = data.TryGet(99);
        Assert.Null(result);
    }

    [Fact]
    public void TryGet_NullableReturn_WhenNegativeIndex_ReturnsNull()
    {
        var array = new JsonArray(JsonValue.Create(1));
        var data = new JsonData(array);
        var result = data.TryGet(-1);
        Assert.Null(result);
    }

    // -- TryGet(int index, out JsonData) ---------------------------------------

    [Fact]
    public void TryGet_WithOutParam_OnJsonNodeArray_ValidIndex_ReturnsTrueAndResult()
    {
        var array = new JsonArray(JsonValue.Create(7));
        var data = new JsonData(array);
        var found = data.TryGet(0, out var result);
        Assert.True(found);
        Assert.Equal(7, result.Node!.GetValue<int>());
    }

    [Fact]
    public void TryGet_WithOutParam_OnJsonNodeArray_OutOfRange_ReturnsFalse()
    {
        var array = new JsonArray(JsonValue.Create(7));
        var data = new JsonData(array);
        var found = data.TryGet(5, out _);
        Assert.False(found);
    }

    [Fact]
    public void TryGet_WithOutParam_OnJsonNodeArray_NegativeIndex_ReturnsFalse()
    {
        var array = new JsonArray(JsonValue.Create(7));
        var data = new JsonData(array);
        var found = data.TryGet(-1, out _);
        Assert.False(found);
    }

    [Fact]
    public void TryGet_WithOutParam_OnJsonElement_ValidIndex_ReturnsTrueAndResult()
    {
        var element = JsonDocument.Parse("[10,20]").RootElement;
        var data = new JsonData(element);
        var found = data.TryGet(1, out var result);
        Assert.True(found);
        Assert.Equal(JsonValueKind.Number, result.ValueKind);
    }

    [Fact]
    public void TryGet_WithOutParam_OnJsonElement_OutOfRange_ReturnsFalse()
    {
        var element = JsonDocument.Parse("[10]").RootElement;
        var data = new JsonData(element);
        var found = data.TryGet(5, out _);
        Assert.False(found);
    }

    [Fact]
    public void TryGet_WithOutParam_OnJsonElement_NegativeIndex_ReturnsFalse()
    {
        var element = JsonDocument.Parse("[10]").RootElement;
        var data = new JsonData(element);
        var found = data.TryGet(-1, out _);
        Assert.False(found);
    }

    [Fact]
    public void TryGet_WithOutParam_OnNonArrayNode_ReturnsFalse()
    {
        var data = new JsonData(new JsonObject());
        var found = data.TryGet(0, out _);
        Assert.False(found);
    }

    [Fact]
    public void TryGet_WithOutParam_OnNullNode_ReturnsFalse()
    {
        var data = new JsonData((JsonNode?)null);
        var found = data.TryGet(0, out _);
        Assert.False(found);
    }

    // -- GetOrCreateObject -----------------------------------------------------

    [Fact]
    public void GetOrCreateObject_WhenIndexHasObject_ReturnsExistingObject()
    {
        var inner = new JsonObject();
        inner["key"] = JsonValue.Create("value");
        var array = new JsonArray(inner);
        var data = new JsonData(array);
        var result = data.GetOrCreateObject(0);
        Assert.True(result.IsObject);
    }

    [Fact]
    public void GetOrCreateObject_WhenIndexDoesNotExist_CreatesAndReturnsNewObject()
    {
        var array = new JsonArray();
        var data = new JsonData(array);
        var result = data.GetOrCreateObject(0);
        Assert.True(result.IsObject);
    }

    [Fact]
    public void GetOrCreateObject_WhenIndexHasNonObject_ThrowsInvalidOperationException()
    {
        var array = new JsonArray(JsonValue.Create(42));
        var data = new JsonData(array);
        Assert.Throws<InvalidOperationException>(() => data.GetOrCreateObject(0));
    }

    // -- GetOrCreateArray ------------------------------------------------------

    [Fact]
    public void GetOrCreateArray_WhenIndexHasArray_ReturnsExistingArray()
    {
        var inner = new JsonArray(JsonValue.Create(1));
        var outer = new JsonArray(inner);
        var data = new JsonData(outer);
        var result = data.GetOrCreateArray(0);
        Assert.True(result.IsArray);
    }

    [Fact]
    public void GetOrCreateArray_WhenIndexDoesNotExist_CreatesAndReturnsNewArray()
    {
        var array = new JsonArray();
        var data = new JsonData(array);
        var result = data.GetOrCreateArray(0);
        Assert.True(result.IsArray);
    }

    [Fact]
    public void GetOrCreateArray_WhenIndexDoesNotExist_AddsNewArrayToParent()
    {
        var array = new JsonArray();
        var data = new JsonData(array);
        _ = data.GetOrCreateArray(0);
        Assert.Single(array);
    }

    [Fact]
    public void GetOrCreateArray_WhenIndexHasNonArray_ThrowsInvalidOperationException()
    {
        var array = new JsonArray(JsonValue.Create(42));
        var data = new JsonData(array);
        Assert.Throws<InvalidOperationException>(() => data.GetOrCreateArray(0));
    }

    // -- Contains --------------------------------------------------------------

    [Fact]
    public void Contains_WhenPredicateMatchesItemInJsonNodeArray_ReturnsTrue()
    {
        var array = new JsonArray(JsonValue.Create(5), JsonValue.Create(10));
        var data = new JsonData(array);
        var result = data.Contains(static (in JsonData item) => item.Node!.GetValue<int>() == 10);
        Assert.True(result);
    }

    [Fact]
    public void Contains_WhenPredicateDoesNotMatchAnyItemInJsonNodeArray_ReturnsFalse()
    {
        var array = new JsonArray(JsonValue.Create(5), JsonValue.Create(10));
        var data = new JsonData(array);
        var result = data.Contains(static (in JsonData item) => item.Node!.GetValue<int>() == 99);
        Assert.False(result);
    }

    [Fact]
    public void Contains_WhenPredicateMatchesItemInJsonElement_ReturnsTrue()
    {
        var element = JsonDocument.Parse("[1,2,3]").RootElement;
        var data = new JsonData(element);
        var result = data.Contains(static (in JsonData item) => item.Element!.Value.GetInt32() == 2);
        Assert.True(result);
    }

    [Fact]
    public void Contains_WhenPredicateDoesNotMatchItemInJsonElement_ReturnsFalse()
    {
        var element = JsonDocument.Parse("[1,2,3]").RootElement;
        var data = new JsonData(element);
        var result = data.Contains(static (in JsonData item) => item.Element!.Value.GetInt32() == 99);
        Assert.False(result);
    }

    [Fact]
    public void Contains_OnEmptyJsonNodeArray_ReturnsFalse()
    {
        var data = new JsonData(new JsonArray());
        var result = data.Contains(static (in JsonData _) => true);
        Assert.False(result);
    }

    // -- IndexOf ---------------------------------------------------------------

    [Fact]
    public void IndexOf_WhenPredicateMatchesFirstItemInJsonNodeArray_ReturnsZero()
    {
        var array = new JsonArray(JsonValue.Create(42), JsonValue.Create(99));
        var data = new JsonData(array);
        var index = data.IndexOf(static (in JsonData item) => item.Node!.GetValue<int>() == 42);
        Assert.Equal(0, index);
    }

    [Fact]
    public void IndexOf_WhenPredicateMatchesSecondItemInJsonNodeArray_ReturnsOne()
    {
        var array = new JsonArray(JsonValue.Create(1), JsonValue.Create(2));
        var data = new JsonData(array);
        var index = data.IndexOf(static (in JsonData item) => item.Node!.GetValue<int>() == 2);
        Assert.Equal(1, index);
    }

    [Fact]
    public void IndexOf_WhenPredicateMatchesNothingInJsonNodeArray_ReturnsMinusOne()
    {
        var array = new JsonArray(JsonValue.Create(1), JsonValue.Create(2));
        var data = new JsonData(array);
        var index = data.IndexOf(static (in JsonData item) => item.Node!.GetValue<int>() == 99);
        Assert.Equal(-1, index);
    }

    [Fact]
    public void IndexOf_WhenPredicateMatchesFirstItemInJsonElement_ReturnsZero()
    {
        var element = JsonDocument.Parse("[10,20,30]").RootElement;
        var data = new JsonData(element);
        var index = data.IndexOf(static (in JsonData item) => item.Element!.Value.GetInt32() == 10);
        Assert.Equal(0, index);
    }

    [Fact]
    public void IndexOf_WhenPredicateMatchesLastItemInJsonElement_ReturnsCorrectIndex()
    {
        var element = JsonDocument.Parse("[10,20,30]").RootElement;
        var data = new JsonData(element);
        var index = data.IndexOf(static (in JsonData item) => item.Element!.Value.GetInt32() == 30);
        Assert.Equal(2, index);
    }

    [Fact]
    public void IndexOf_WhenPredicateMatchesNothingInJsonElement_ReturnsMinusOne()
    {
        var element = JsonDocument.Parse("[10,20,30]").RootElement;
        var data = new JsonData(element);
        var index = data.IndexOf(static (in JsonData item) => item.Element!.Value.GetInt32() == 99);
        Assert.Equal(-1, index);
    }

    [Fact]
    public void IndexOf_OnNonArrayNode_ReturnsMinusOne()
    {
        var data = new JsonData(new JsonObject());
        var index = data.IndexOf(static (in JsonData _) => true);
        Assert.Equal(-1, index);
    }

    [Fact]
    public void IndexOf_OnNullNode_ReturnsMinusOne()
    {
        var data = new JsonData((JsonNode?)null);
        var index = data.IndexOf(static (in JsonData _) => true);
        Assert.Equal(-1, index);
    }

    // -- IndexHasValue ---------------------------------------------------------

    [Fact]
    public void IndexHasValue_NegativeIndex_ReturnsFalse()
    {
        var data = new JsonData(new JsonArray(JsonValue.Create(1)));
        Assert.False(data.IndexHasValue(-1));
    }

    [Fact]
    public void IndexHasValue_OnJsonNodeArray_WithValueAtIndex_ReturnsTrue()
    {
        var array = new JsonArray(JsonValue.Create(42));
        var data = new JsonData(array);
        Assert.True(data.IndexHasValue(0));
    }
    [Fact]
    public void IndexHasValue_OnJsonNodeArray_WithNullAtIndex_ReturnsFalse()
    {
        var array = new JsonArray((JsonNode?)null);
        var data = new JsonData(array);
        Assert.False(data.IndexHasValue(0));
    }

    [Fact]
    public void IndexHasValue_OnJsonNodeArray_IndexOutOfRange_ReturnsFalse()
    {
        var array = new JsonArray(JsonValue.Create(1));
        var data = new JsonData(array);
        Assert.False(data.IndexHasValue(5));
    }

    [Fact]
    public void IndexHasValue_OnJsonElement_WithValueAtIndex_ReturnsTrue()
    {
        var element = JsonDocument.Parse("[1,2,3]").RootElement;
        var data = new JsonData(element);
        Assert.True(data.IndexHasValue(0));
    }

    [Fact]
    public void IndexHasValue_OnJsonElement_WithNullAtIndex_ReturnsFalse()
    {
        var element = JsonDocument.Parse("[null]").RootElement;
        var data = new JsonData(element);
        Assert.False(data.IndexHasValue(0));
    }

    [Fact]
    public void IndexHasValue_OnJsonElement_IndexOutOfRange_ReturnsFalse()
    {
        var element = JsonDocument.Parse("[1]").RootElement;
        var data = new JsonData(element);
        Assert.False(data.IndexHasValue(5));
    }

    [Fact]
    public void IndexHasValue_OnJsonElement_NonArray_ReturnsFalse()
    {
        var element = JsonDocument.Parse("{}").RootElement;
        var data = new JsonData(element);
        Assert.False(data.IndexHasValue(0));
    }

    [Fact]
    public void IndexHasValue_OnNullNode_ReturnsFalse()
    {
        var data = new JsonData((JsonNode?)null);
        Assert.False(data.IndexHasValue(0));
    }

    // -- Set -------------------------------------------------------------------

    [Fact]
    public void Set_WithValidIndexAndValue_SetsElementAtIndex()
    {
        var array = new JsonArray(JsonValue.Create(1), JsonValue.Create(2));
        var data = new JsonData(array);
        var newValue = new JsonData(JsonValue.Create(99));
        data.Set(0, newValue);
        Assert.Equal(99, array[0]!.GetValue<int>());
    }

    [Fact]
    public void Set_WithNullValue_SetsNullAtIndex()
    {
        var array = new JsonArray(JsonValue.Create(1));
        var data = new JsonData(array);
        data.Set(0, null);
        Assert.Null(array[0]);
    }

    [Fact]
    public void Set_OnNonArray_ThrowsInvalidOperationException()
    {
        var data = new JsonData(new JsonObject());
        Assert.Throws<InvalidOperationException>(() => data.Set(0, new JsonData(JsonValue.Create(1))));
    }

    [Fact]
    public void Set_OnReadOnly_ThrowsInvalidOperationException()
    {
        var array = new JsonArray(JsonValue.Create(1));
        var data = JsonData.CreateReadOnly(array);
        Assert.Throws<InvalidOperationException>(() => data.Set(0, new JsonData(JsonValue.Create(99))));
    }

    // -- Add -------------------------------------------------------------------

    [Fact]
    public void Add_WithValue_AppendsElementToArray()
    {
        var array = new JsonArray();
        var data = new JsonData(array);
        data.Add(new JsonData(JsonValue.Create(42)));
        Assert.Single(array);
        Assert.Equal(42, array[0]!.GetValue<int>());
    }

    [Fact]
    public void Add_WithNullValue_AppendsNullToArray()
    {
        var array = new JsonArray();
        var data = new JsonData(array);
        data.Add(null);
        Assert.Single(array);
        Assert.Null(array[0]);
    }

    [Fact]
    public void Add_MultipleValues_AppendsInOrder()
    {
        var array = new JsonArray();
        var data = new JsonData(array);
        data.Add(new JsonData(JsonValue.Create(1)));
        data.Add(new JsonData(JsonValue.Create(2)));
        Assert.Equal(2, array.Count);
        Assert.Equal(1, array[0]!.GetValue<int>());
        Assert.Equal(2, array[1]!.GetValue<int>());
    }

    [Fact]
    public void Add_OnNonArray_ThrowsInvalidOperationException()
    {
        var data = new JsonData(new JsonObject());
        Assert.Throws<InvalidOperationException>(() => data.Add(new JsonData(JsonValue.Create(1))));
    }

    [Fact]
    public void Add_OnReadOnly_ThrowsInvalidOperationException()
    {
        var array = new JsonArray();
        var data = JsonData.CreateReadOnly(array);
        Assert.Throws<InvalidOperationException>(() => data.Add(new JsonData(JsonValue.Create(1))));
    }

    // -- Insert ----------------------------------------------------------------

    [Fact]
    public void Insert_AtZero_InsertsAtBeginning()
    {
        var array = new JsonArray(JsonValue.Create(2), JsonValue.Create(3));
        var data = new JsonData(array);
        data.Insert(0, new JsonData(JsonValue.Create(1)));
        Assert.Equal(3, array.Count);
        Assert.Equal(1, array[0]!.GetValue<int>());
        Assert.Equal(2, array[1]!.GetValue<int>());
    }

    [Fact]
    public void Insert_AtEnd_AppendsElement()
    {
        var array = new JsonArray(JsonValue.Create(1));
        var data = new JsonData(array);
        data.Insert(1, new JsonData(JsonValue.Create(2)));
        Assert.Equal(2, array.Count);
        Assert.Equal(2, array[1]!.GetValue<int>());
    }

    [Fact]
    public void Insert_WithNullValue_InsertsNullAtIndex()
    {
        var array = new JsonArray(JsonValue.Create(1));
        var data = new JsonData(array);
        data.Insert(0, null);
        Assert.Equal(2, array.Count);
        Assert.Null(array[0]);
    }

    [Fact]
    public void Insert_OnNonArray_ThrowsInvalidOperationException()
    {
        var data = new JsonData(new JsonObject());
        Assert.Throws<InvalidOperationException>(() => data.Insert(0, new JsonData(JsonValue.Create(1))));
    }

    [Fact]
    public void Insert_OnReadOnly_ThrowsInvalidOperationException()
    {
        var array = new JsonArray(JsonValue.Create(1));
        var data = JsonData.CreateReadOnly(array);
        Assert.Throws<InvalidOperationException>(() => data.Insert(0, new JsonData(JsonValue.Create(99))));
    }

    // -- RemoveAt --------------------------------------------------------------

    [Fact]
    public void RemoveAt_ValidIndex_RemovesElement()
    {
        var array = new JsonArray(JsonValue.Create(1), JsonValue.Create(2), JsonValue.Create(3));
        var data = new JsonData(array);
        data.RemoveAt(1);
        Assert.Equal(2, array.Count);
        Assert.Equal(1, array[0]!.GetValue<int>());
        Assert.Equal(3, array[1]!.GetValue<int>());
    }

    [Fact]
    public void RemoveAt_FirstIndex_RemovesFirstElement()
    {
        var array = new JsonArray(JsonValue.Create(10), JsonValue.Create(20));
        var data = new JsonData(array);
        data.RemoveAt(0);
        Assert.Single(array);
        Assert.Equal(20, array[0]!.GetValue<int>());
    }

    [Fact]
    public void RemoveAt_OnNonArray_ThrowsInvalidOperationException()
    {
        var data = new JsonData(new JsonObject());
        Assert.Throws<InvalidOperationException>(() => data.RemoveAt(0));
    }

    [Fact]
    public void RemoveAt_OnReadOnly_ThrowsInvalidOperationException()
    {
        var array = new JsonArray(JsonValue.Create(1));
        var data = JsonData.CreateReadOnly(array);
        Assert.Throws<InvalidOperationException>(() => data.RemoveAt(0));
    }

    [Fact]
    public void RemoveAt_InvalidIndex_ThrowsArgumentOutOfRangeException()
    {
        var array = new JsonArray(JsonValue.Create(1));
        var data = new JsonData(array);
        Assert.Throws<ArgumentOutOfRangeException>(() => data.RemoveAt(5));
    }

    // -- Items -----------------------------------------------------------------

    [Fact]
    public void Items_OnJsonNodeArray_ReturnsAllItems()
    {
        var array = new JsonArray(JsonValue.Create(1), JsonValue.Create(2), JsonValue.Create(3));
        var data = new JsonData(array);
        var items = data.Items.ToList();
        Assert.Equal(3, items.Count);
    }

    [Fact]
    public void Items_OnJsonElementArray_ReturnsAllItems()
    {
        var element = JsonDocument.Parse("[10,20]").RootElement;
        var data = new JsonData(element);
        var items = data.Items.ToList();
        Assert.Equal(2, items.Count);
    }

    [Fact]
    public void Items_OnEmptyJsonNodeArray_ReturnsEmptyEnumerable()
    {
        var data = new JsonData(new JsonArray());
        var items = data.Items.ToList();
        Assert.Empty(items);
    }

    // -- Readonly propagation -- TryGet(int) ------------------------------------

    [Fact]
    public void TryGet_ReadOnlyParent_ChildIsReadOnly()
    {
        var array = new JsonArray(new JsonObject());
        var parent = JsonData.CreateReadOnly(array);

        var found = parent.TryGet(0, out var child);

        Assert.True(found);
        Assert.True(child.ReadOnly);
    }

    [Fact]
    public void TryGet_WritableParent_ChildIsWritable()
    {
        var array = new JsonArray(new JsonObject());
        var parent = new JsonData(array);

        var found = parent.TryGet(0, out var child);

        Assert.True(found);
        Assert.False(child.ReadOnly);
    }

    // -- Readonly propagation -- IndexOf ----------------------------------------

    [Fact]
    public void IndexOf_ReadOnlyParent_PredicateReceivesReadOnlyChild()
    {
        var array = new JsonArray(JsonValue.Create(1));
        var parent = JsonData.CreateReadOnly(array);
        bool childWasReadOnly = false;

        parent.IndexOf((in JsonData item) => { childWasReadOnly = item.ReadOnly; return true; });

        Assert.True(childWasReadOnly);
    }

    [Fact]
    public void IndexOf_WritableParent_PredicateReceivesWritableChild()
    {
        var array = new JsonArray(new JsonObject());
        var parent = new JsonData(array);
        bool childWasReadOnly = true;

        parent.IndexOf((in JsonData item) => { childWasReadOnly = item.ReadOnly; return true; });

        // JsonObject children from writable parent should be writable
        Assert.False(childWasReadOnly);
    }

    // -- Readonly propagation -- Items ------------------------------------------

    [Fact]
    public void Items_ReadOnlyParent_AllChildrenAreReadOnly()
    {
        var array = new JsonArray(new JsonObject(), new JsonObject());
        var parent = JsonData.CreateReadOnly(array);

        var items = parent.Items.ToList();

        Assert.All(items, item => Assert.True(item.ReadOnly));
    }

    [Fact]
    public void Items_WritableParent_ObjectChildrenAreWritable()
    {
        var array = new JsonArray(new JsonObject(), new JsonObject());
        var parent = new JsonData(array);

        var items = parent.Items.ToList();

        Assert.All(items, item => Assert.False(item.ReadOnly));
    }
}
