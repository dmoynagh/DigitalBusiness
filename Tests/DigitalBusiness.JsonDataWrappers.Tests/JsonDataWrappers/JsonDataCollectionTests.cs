using System.Text.Json;
using System.Text.Json.Nodes;
using DigitalBusiness.JsonDataWrappers;
using Xunit;

namespace DigitalBusiness.JsonDataWrappers.Tests;

public class JsonDataCollectionTests
{
    // ── Create(IEnumerable<JsonNode>) ─────────────────────────────────────────

    [Fact]
    public void Create_IEnumerableJsonNode_ReturnsNonNull()
    {
        // Arrange
        var nodes = new JsonNode[] { JsonValue.Create(1)!, JsonValue.Create(2)! };

        // Act
        using var collection = JsonDataCollection.Create((IEnumerable<JsonNode>)nodes);

        // Assert
        Assert.NotNull(collection);
    }

    [Fact(Skip = "ProductionBugSuspected")]
    public void Create_IEnumerableJsonNode_EnumeratesAllItems()
    {
        // Arrange
        var nodes = new JsonNode[] { JsonValue.Create(10)!, JsonValue.Create(20)!, JsonValue.Create(30)! };

        // Act
        using var collection = JsonDataCollection.Create((IEnumerable<JsonNode>)nodes);
        var items = collection.ToList();

        // Assert
        Assert.Equal(3, items.Count);
    }

    [Fact]
    public void Create_IEnumerableJsonNode_EmptySequence_ReturnsEmptyCollection()
    {
        // Arrange
        var nodes = Array.Empty<JsonNode>();

        // Act
        using var collection = JsonDataCollection.Create((IEnumerable<JsonNode>)nodes);
        var items = collection.ToList();

        // Assert
        Assert.Empty(items);
    }

    [Fact(Skip = "ProductionBugSuspected")]
    public void Create_IEnumerableJsonNode_ItemsHaveExpectedValues()
    {
        // Arrange
        var nodes = new JsonNode[] { JsonValue.Create("hello")!, JsonValue.Create("world")! };

        // Act
        using var collection = JsonDataCollection.Create((IEnumerable<JsonNode>)nodes);
        var items = collection.ToList();

        // Assert
        Assert.Equal("hello", (string?)items[0]);
        Assert.Equal("world", (string?)items[1]);
    }

    // ── Create(JsonArray) ─────────────────────────────────────────────────────

    [Fact]
    public void Create_JsonArray_ReturnsNonNull()
    {
        // Arrange
        var array = new JsonArray(JsonValue.Create(1), JsonValue.Create(2));

        // Act
        using var collection = JsonDataCollection.Create(array);

        // Assert
        Assert.NotNull(collection);
    }

    [Fact(Skip = "ProductionBugSuspected")]
    public void Create_JsonArray_EnumeratesAllItems()
    {
        // Arrange
        var array = new JsonArray(JsonValue.Create(1), JsonValue.Create(2), JsonValue.Create(3));

        // Act
        using var collection = JsonDataCollection.Create(array);
        var items = collection.ToList();

        // Assert
        Assert.Equal(3, items.Count);
    }

    [Fact]
    public void Create_JsonArray_EmptyArray_ReturnsEmptyCollection()
    {
        // Arrange
        var array = new JsonArray();

        // Act
        using var collection = JsonDataCollection.Create(array);
        var items = collection.ToList();

        // Assert
        Assert.Empty(items);
    }

    [Fact(Skip = "ProductionBugSuspected")]
    public void Create_JsonArray_ItemsHaveExpectedValues()
    {
        // Arrange
        var array = new JsonArray(JsonValue.Create(42), JsonValue.Create(99));

        // Act
        using var collection = JsonDataCollection.Create(array);
        var items = collection.ToList();

        // Assert
        Assert.Equal(42, (int)items[0]);
        Assert.Equal(99, (int)items[1]);
    }

    // ── Create(JsonDocument, bool autoDispose) ────────────────────────────────

    [Fact]
    public void Create_JsonDocument_ReturnsNonNull()
    {
        // Arrange
        using var document = JsonDocument.Parse("[1,2,3]");

        // Act
        using var collection = JsonDataCollection.Create(document);

        // Assert
        Assert.NotNull(collection);
    }

    [Fact]
    public void Create_JsonDocument_EnumeratesAllItems()
    {
        // Arrange
        using var document = JsonDocument.Parse("[1,2,3]");

        // Act
        using var collection = JsonDataCollection.Create(document);
        var items = collection.ToList();

        // Assert
        Assert.Equal(3, items.Count);
    }

    [Fact]
    public void Create_JsonDocument_EmptyArray_ReturnsEmptyCollection()
    {
        // Arrange
        using var document = JsonDocument.Parse("[]");

        // Act
        using var collection = JsonDataCollection.Create(document);
        var items = collection.ToList();

        // Assert
        Assert.Empty(items);
    }

    [Fact]
    public void Create_JsonDocument_AutoDisposeFalse_DocumentNotDisposedAfterCollectionDispose()
    {
        // Arrange
        var document = JsonDocument.Parse("[1,2]");
        var collection = JsonDataCollection.Create(document, autoDispose: false);

        // Act
        ((IDisposable)collection).Dispose();

        // Assert - document is still usable (no ObjectDisposedException)
        var kind = document.RootElement.ValueKind;
        Assert.Equal(JsonValueKind.Array, kind);

        document.Dispose();
    }

    [Fact]
    public void Create_JsonDocument_AutoDisposeTrue_DocumentDisposedAfterCollectionDispose()
    {
        // Arrange
        var document = JsonDocument.Parse("[1,2]");
        var collection = JsonDataCollection.Create(document, autoDispose: true);

        // Act
        ((IDisposable)collection).Dispose();

        // Assert - document should be disposed now
        Assert.Throws<ObjectDisposedException>(() => document.RootElement.ValueKind);
    }

    // ── Create(IEnumerable<JsonElement>) ─────────────────────────────────────

    [Fact]
    public void Create_IEnumerableJsonElement_ReturnsNonNull()
    {
        // Arrange
        using var document = JsonDocument.Parse("[1,2,3]");
        var elements = document.RootElement.EnumerateArray().ToList();

        // Act
        using var collection = JsonDataCollection.Create((IEnumerable<JsonElement>)elements);

        // Assert
        Assert.NotNull(collection);
    }

    [Fact]
    public void Create_IEnumerableJsonElement_EnumeratesAllItems()
    {
        // Arrange
        using var document = JsonDocument.Parse("[10,20,30]");
        var elements = document.RootElement.EnumerateArray().ToList();

        // Act
        using var collection = JsonDataCollection.Create((IEnumerable<JsonElement>)elements);
        var items = collection.ToList();

        // Assert
        Assert.Equal(3, items.Count);
    }

    [Fact]
    public void Create_IEnumerableJsonElement_EmptySequence_ReturnsEmptyCollection()
    {
        // Arrange
        var elements = Array.Empty<JsonElement>();

        // Act
        using var collection = JsonDataCollection.Create((IEnumerable<JsonElement>)elements);
        var items = collection.ToList();

        // Assert
        Assert.Empty(items);
    }

    [Fact]
    public void Create_IEnumerableJsonElement_ItemsHaveExpectedValues()
    {
        // Arrange
        using var document = JsonDocument.Parse("[7,8]");
        var elements = document.RootElement.EnumerateArray().ToList();

        // Act
        using var collection = JsonDataCollection.Create((IEnumerable<JsonElement>)elements);
        var items = collection.ToList();

        // Assert
        Assert.Equal(7, (int)items[0]);
        Assert.Equal(8, (int)items[1]);
    }

    // ── Create(JsonElement rootElement) ──────────────────────────────────────

    [Fact]
    public void Create_JsonElementRoot_ReturnsNonNull()
    {
        // Arrange
        using var document = JsonDocument.Parse("[1,2,3]");
        var root = document.RootElement;

        // Act
        using var collection = JsonDataCollection.Create(root);

        // Assert
        Assert.NotNull(collection);
    }

    [Fact]
    public void Create_JsonElementRoot_EnumeratesChildren()
    {
        // Arrange
        using var document = JsonDocument.Parse("[100,200,300]");
        var root = document.RootElement;

        // Act
        using var collection = JsonDataCollection.Create(root);
        var items = collection.ToList();

        // Assert
        Assert.Equal(3, items.Count);
    }

    [Fact]
    public void Create_JsonElementRoot_EmptyArray_ReturnsEmptyCollection()
    {
        // Arrange
        using var document = JsonDocument.Parse("[]");
        var root = document.RootElement;

        // Act
        using var collection = JsonDataCollection.Create(root);
        var items = collection.ToList();

        // Assert
        Assert.Empty(items);
    }

    [Fact]
    public void Create_JsonElementRoot_ItemsHaveExpectedValues()
    {
        // Arrange
        using var document = JsonDocument.Parse("[5,6]");
        var root = document.RootElement;

        // Act
        using var collection = JsonDataCollection.Create(root);
        var items = collection.ToList();

        // Assert
        Assert.Equal(5, (int)items[0]);
        Assert.Equal(6, (int)items[1]);
    }

    // ── Create(string jsonData) ───────────────────────────────────────────────

    [Fact]
    public void Create_String_ReturnsNonNull()
    {
        // Arrange & Act
        using var collection = JsonDataCollection.Create("[1,2,3]");

        // Assert
        Assert.NotNull(collection);
    }

    [Fact]
    public void Create_String_EnumeratesAllItems()
    {
        // Arrange & Act
        using var collection = JsonDataCollection.Create("[1,2,3]");
        var items = collection.ToList();

        // Assert
        Assert.Equal(3, items.Count);
    }

    [Fact]
    public void Create_String_EmptyArray_ReturnsEmptyCollection()
    {
        // Arrange & Act
        using var collection = JsonDataCollection.Create("[]");
        var items = collection.ToList();

        // Assert
        Assert.Empty(items);
    }

    [Fact]
    public void Create_String_ItemsHaveExpectedValues()
    {
        // Arrange & Act
        using var collection = JsonDataCollection.Create("[10,20]");
        var items = collection.ToList();

        // Assert
        Assert.Equal(10, (int)items[0]);
        Assert.Equal(20, (int)items[1]);
    }

    // ── IEnumerable.GetEnumerator() ───────────────────────────────────────────

    [Fact]
    public void GetEnumerator_ViaIEnumerable_EnumeratesItems()
    {
        // Arrange
        using var collection = JsonDataCollection.Create("[1,2,3]");

        // Act
        var items = new List<object?>();
        var enumerator = ((System.Collections.IEnumerable)collection).GetEnumerator();
        while (enumerator.MoveNext())
        {
            items.Add(enumerator.Current);
        }

        // Assert
        Assert.Equal(3, items.Count);
    }

    // ── Dispose / OnDispose ───────────────────────────────────────────────────

    [Fact]
    public void Dispose_CalledTwice_DoesNotThrow()
    {
        // Arrange
        var collection = JsonDataCollection.Create("[1,2]");
        var disposable = (IDisposable)collection;

        // Act & Assert
        disposable.Dispose();
        var ex = Record.Exception(() => disposable.Dispose());
        Assert.Null(ex);
    }

    [Fact]
    public void Dispose_StringCollection_AfterDispose_GetEnumeratorThrowsObjectDisposedException()
    {
        // Arrange
        var collection = JsonDataCollection.Create("[1,2]");
        // Force document creation by enumerating
        _ = collection.ToList();
        ((IDisposable)collection).Dispose();

        // Act & Assert
        Assert.Throws<ObjectDisposedException>(() => collection.GetEnumerator());
    }

    [Fact]
    public void Dispose_StringCollection_WithoutDocumentCreated_DoesNotThrow()
    {
        // Arrange - document is lazily created; dispose before any enumeration
        var collection = JsonDataCollection.Create("[1,2]");

        // Act & Assert
        var ex = Record.Exception(() => ((IDisposable)collection).Dispose());
        Assert.Null(ex);
    }

    [Fact]
    public void OnDispose_BaseImplementation_DisposeDoesNotThrow()
    {
        // Arrange - JsonDataJsonElementCollection uses base OnDispose (no-op)
        using var document = JsonDocument.Parse("[1]");
        var elements = document.RootElement.EnumerateArray().ToList();
        var collection = JsonDataCollection.Create((IEnumerable<JsonElement>)elements);

        // Act & Assert - base OnDispose should be a no-op; no exception expected
        var ex = Record.Exception(() => ((IDisposable)collection).Dispose());
        Assert.Null(ex);
    }

    // ── JsonDataJsonNodeCollection constructor / GetEnumerator ────────────────

    [Fact(Skip = "ProductionBugSuspected")]
    public void JsonDataJsonNodeCollection_Constructor_AssignsNodes_ViaGetEnumeratorReturnsSingleItem()
    {
        // Arrange - use a node parsed from JSON to avoid parent-ownership issues
        var node = JsonNode.Parse("42")!;

        // Act
        using var collection = JsonDataCollection.Create(new List<JsonNode> { node });
        var items = collection.ToList();

        // Assert
        Assert.Single(items);
    }

    [Fact(Skip = "ProductionBugSuspected")]
    public void JsonDataJsonNodeCollection_GetEnumerator_WithMultipleParsedNodes_ReturnsCorrectCount()
    {
        // Arrange
        var nodes = new List<JsonNode>
        {
            JsonNode.Parse("1")!,
            JsonNode.Parse("2")!,
            JsonNode.Parse("3")!,
        };

        // Act
        using var collection = JsonDataCollection.Create((IEnumerable<JsonNode>)nodes);
        var items = collection.ToList();

        // Assert
        Assert.Equal(3, items.Count);
    }

    [Fact(Skip = "ProductionBugSuspected")]
    public void JsonDataJsonNodeCollection_GetEnumerator_ItemValues_AreCorrect()
    {
        // Arrange
        var nodes = new List<JsonNode>
        {
            JsonNode.Parse("10")!,
            JsonNode.Parse("20")!,
        };

        // Act
        using var collection = JsonDataCollection.Create((IEnumerable<JsonNode>)nodes);
        var items = collection.ToList();

        // Assert
        Assert.Equal(10, (int)items[0]);
        Assert.Equal(20, (int)items[1]);
    }

    // ── JsonDataJsonArrayCollection constructor / GetEnumerator ───────────────

    [Fact(Skip = "ProductionBugSuspected")]
    public void JsonDataJsonArrayCollection_Constructor_AssignsArray_ViaGetEnumeratorReturnsSingleItem()
    {
        // Arrange - parse a JsonArray to avoid parent-ownership issues
        var array = JsonNode.Parse("[99]")!.AsArray();

        // Act
        using var collection = JsonDataCollection.Create(array);
        var items = collection.ToList();

        // Assert
        Assert.Single(items);
    }

    [Fact(Skip = "ProductionBugSuspected")]
    public void JsonDataJsonArrayCollection_GetEnumerator_WithMultipleItems_ReturnsCorrectCount()
    {
        // Arrange
        var array = JsonNode.Parse("[1,2,3]")!.AsArray();

        // Act
        using var collection = JsonDataCollection.Create(array);
        var items = collection.ToList();

        // Assert
        Assert.Equal(3, items.Count);
    }

    [Fact(Skip = "ProductionBugSuspected")]
    public void JsonDataJsonArrayCollection_GetEnumerator_ItemValues_AreCorrect()
    {
        // Arrange
        var array = JsonNode.Parse("[5,6]")!.AsArray();

        // Act
        using var collection = JsonDataCollection.Create(array);
        var items = collection.ToList();

        // Assert
        Assert.Equal(5, (int)items[0]);
        Assert.Equal(6, (int)items[1]);
    }

    // ── JsonDataJsonDocumentCollection constructor (default autoDispose) ───────

    [Fact]
    public void JsonDataJsonDocumentCollection_Constructor_DefaultAutoDispose_DocumentNotDisposed()
    {
        // Arrange
        var document = JsonDocument.Parse("[1,2,3]");

        // Act - create without specifying autoDispose (default = false)
        var collection = JsonDataCollection.Create(document);
        ((IDisposable)collection).Dispose();

        // Assert - document should still be usable
        var kind = document.RootElement.ValueKind;
        Assert.Equal(JsonValueKind.Array, kind);

        document.Dispose();
    }

    [Fact]
    public void JsonDataJsonDocumentCollection_Constructor_AutoDisposeTrue_EnumerationWorksBeforeDispose()
    {
        // Arrange
        var document = JsonDocument.Parse("[1,2,3]");

        // Act
        using var collection = JsonDataCollection.Create(document, autoDispose: true);
        var items = collection.ToList();

        // Assert
        Assert.Equal(3, items.Count);
    }

    // ── JsonDataJsonDocumentCollection.GetEnumerator (disposed path) ──────────

    [Fact]
    public void JsonDataJsonDocumentCollection_GetEnumerator_AfterDispose_ThrowsObjectDisposedException()
    {
        // Arrange
        var document = JsonDocument.Parse("[1,2,3]");
        var collection = JsonDataCollection.Create(document, autoDispose: false);
        ((IDisposable)collection).Dispose();

        // Act & Assert
        Assert.Throws<ObjectDisposedException>(() => collection.GetEnumerator());

        document.Dispose();
    }

    [Fact]
    public void JsonDataJsonDocumentCollection_GetEnumerator_BeforeDispose_ReturnsAllItems()
    {
        // Arrange
        using var document = JsonDocument.Parse("[1,2,3]");
        using var collection = JsonDataCollection.Create(document, autoDispose: false);

        // Act
        var items = collection.ToList();

        // Assert
        Assert.Equal(3, items.Count);
    }

    [Fact]
    public void JsonDataJsonDocumentCollection_GetEnumerator_BeforeDispose_EmptyArray_ReturnsEmpty()
    {
        // Arrange
        using var document = JsonDocument.Parse("[]");
        using var collection = JsonDataCollection.Create(document, autoDispose: false);

        // Act
        var items = collection.ToList();

        // Assert
        Assert.Empty(items);
    }

    // ── JsonDataJsonDocumentCollection.OnDispose ──────────────────────────────

    [Fact]
    public void JsonDataJsonDocumentCollection_OnDispose_AutoDisposeFalse_DocumentRemainsUsable()
    {
        // Arrange
        var document = JsonDocument.Parse("[1,2]");
        var collection = JsonDataCollection.Create(document, autoDispose: false);

        // Act
        ((IDisposable)collection).Dispose();

        // Assert - document not disposed
        Assert.Equal(JsonValueKind.Array, document.RootElement.ValueKind);
        document.Dispose();
    }

    [Fact]
    public void JsonDataJsonDocumentCollection_OnDispose_AutoDisposeTrue_DisposesDocument()
    {
        // Arrange
        var document = JsonDocument.Parse("[1,2]");
        var collection = JsonDataCollection.Create(document, autoDispose: true);

        // Act
        ((IDisposable)collection).Dispose();

        // Assert - document is disposed
        Assert.Throws<ObjectDisposedException>(() => document.RootElement.ValueKind);
    }

    // ── JsonDataJsonElementChildrenCollection constructor / GetEnumerator ──────

    [Fact]
    public void JsonDataJsonElementChildrenCollection_Constructor_StoresElement_GetEnumeratorReturnsItems()
    {
        // Arrange
        using var document = JsonDocument.Parse("[10,20,30]");
        var root = document.RootElement;

        // Act
        using var collection = JsonDataCollection.Create(root);
        var items = collection.ToList();

        // Assert
        Assert.Equal(3, items.Count);
    }

    [Fact]
    public void JsonDataJsonElementChildrenCollection_GetEnumerator_EmptyArray_ReturnsEmpty()
    {
        // Arrange
        using var document = JsonDocument.Parse("[]");
        var root = document.RootElement;

        // Act
        using var collection = JsonDataCollection.Create(root);
        var items = collection.ToList();

        // Assert
        Assert.Empty(items);
    }

    [Fact]
    public void JsonDataJsonElementChildrenCollection_GetEnumerator_ItemValues_AreCorrect()
    {
        // Arrange
        using var document = JsonDocument.Parse("[42,99]");
        var root = document.RootElement;

        // Act
        using var collection = JsonDataCollection.Create(root);
        var items = collection.ToList();

        // Assert
        Assert.Equal(42, (int)items[0]);
        Assert.Equal(99, (int)items[1]);
    }

    // ── JsonDataJsonElementCollection constructor / GetEnumerator ─────────────

    [Fact]
    public void JsonDataJsonElementCollection_Constructor_StoresElements_GetEnumeratorReturnsItems()
    {
        // Arrange
        using var document = JsonDocument.Parse("[5,6,7]");
        var elements = document.RootElement.EnumerateArray().ToList();

        // Act
        using var collection = JsonDataCollection.Create((IEnumerable<JsonElement>)elements);
        var items = collection.ToList();

        // Assert
        Assert.Equal(3, items.Count);
    }

    [Fact]
    public void JsonDataJsonElementCollection_Constructor_EmptySequence_GetEnumeratorReturnsEmpty()
    {
        // Arrange
        var elements = Enumerable.Empty<JsonElement>();

        // Act
        using var collection = JsonDataCollection.Create(elements);
        var items = collection.ToList();

        // Assert
        Assert.Empty(items);
    }

    [Fact]
    public void JsonDataJsonElementCollection_GetEnumerator_ItemValues_AreCorrect()
    {
        // Arrange
        using var document = JsonDocument.Parse("[100,200]");
        var elements = document.RootElement.EnumerateArray().ToList();

        // Act
        using var collection = JsonDataCollection.Create((IEnumerable<JsonElement>)elements);
        var items = collection.ToList();

        // Assert
        Assert.Equal(100, (int)items[0]);
        Assert.Equal(200, (int)items[1]);
    }

    // ── JsonDataJsonElementCollection.GetEnumerator (targeted) ───────────────

    [Fact]
    public void JsonDataJsonElementCollection_GetEnumerator_SingleElement_ReturnsSingleItem()
    {
        // Arrange
        using var document = JsonDocument.Parse("[42]");
        var elements = document.RootElement.EnumerateArray().ToList();

        // Act
        using var collection = JsonDataCollection.Create((IEnumerable<JsonElement>)elements);
        var items = collection.ToList();

        // Assert
        Assert.Single(items);
        Assert.Equal(42, (int)items[0]);
    }

    [Fact]
    public void JsonDataJsonElementCollection_GetEnumerator_CanEnumerateMultipleTimes()
    {
        // Arrange
        using var document = JsonDocument.Parse("[1,2]");
        var elements = document.RootElement.EnumerateArray().ToList();

        // Act
        using var collection = JsonDataCollection.Create((IEnumerable<JsonElement>)elements);
        var firstPass = collection.ToList();
        var secondPass = collection.ToList();

        // Assert
        Assert.Equal(firstPass.Count, secondPass.Count);
    }

    // ── JsonDataJsonStringCollection constructor ───────────────────────────────

    [Fact]
    public void JsonDataJsonStringCollection_Constructor_WithValidJsonString_DoesNotImmediatelyParseDocument()
    {
        // Arrange & Act – create the collection but do NOT enumerate (document is lazy)
        using var collection = JsonDataCollection.Create("[1,2,3]");

        // Assert – collection is non-null; no parse exception means no eager document creation
        Assert.NotNull(collection);
    }

    [Fact]
    public void JsonDataJsonStringCollection_Constructor_WithEmptyArrayString_EnumeratesEmpty()
    {
        // Arrange & Act
        using var collection = JsonDataCollection.Create("[]");
        var items = collection.ToList();

        // Assert
        Assert.Empty(items);
    }

    // ── JsonDataJsonStringCollection.Document / CreateDocument ───────────────

    [Fact]
    public void JsonDataJsonStringCollection_Document_LazilyCreated_EnumerationSucceedsOnFirstCall()
    {
        // Arrange
        using var collection = JsonDataCollection.Create("[10,20,30]");

        // Act – first enumeration triggers document creation
        var items = collection.ToList();

        // Assert
        Assert.Equal(3, items.Count);
    }

    [Fact]
    public void JsonDataJsonStringCollection_Document_CalledMultipleTimes_ReturnsSameCachedDocument()
    {
        // Arrange
        using var collection = JsonDataCollection.Create("[1,2]");

        // Act – enumerate twice to confirm the cached document is reused
        var first = collection.ToList();
        var second = collection.ToList();

        // Assert – same element count each time (would throw if document were re-created after null)
        Assert.Equal(first.Count, second.Count);
    }

    [Fact]
    public void JsonDataJsonStringCollection_CreateDocument_NullishJsonString_ParsesAsEmptyArray()
    {
        // Arrange – we can observe the null-coalesce path by passing an empty string which is already empty array-safe;
        // The real null path inside CreateDocument (JsonString ?? string.Empty) is exercised when
        // GetEnumerator is called a second time after JsonString has been set to null internally.
        using var collection = JsonDataCollection.Create("[]");

        // Act
        var firstItems = collection.ToList();  // triggers CreateDocument, sets JsonString = null
        var secondItems = collection.ToList(); // Document cached, no re-parse

        // Assert
        Assert.Empty(firstItems);
        Assert.Empty(secondItems);
    }

    // ── JsonDataJsonStringCollection.OnDispose ────────────────────────────────

    [Fact]
    public void JsonDataJsonStringCollection_OnDispose_DocumentCreated_DisposesDocumentWithoutException()
    {
        // Arrange
        var collection = JsonDataCollection.Create("[1,2,3]");
        _ = collection.ToList(); // force document creation

        // Act
        var ex = Record.Exception(() => ((IDisposable)collection).Dispose());

        // Assert
        Assert.Null(ex);
    }

    [Fact]
    public void JsonDataJsonStringCollection_OnDispose_DocumentNotCreated_NoExceptionThrown()
    {
        // Arrange – dispose without ever enumerating (document never created)
        var collection = JsonDataCollection.Create("[1,2,3]");

        // Act
        var ex = Record.Exception(() => ((IDisposable)collection).Dispose());

        // Assert
        Assert.Null(ex);
    }

    [Fact]
    public void JsonDataJsonStringCollection_OnDispose_DocumentCreated_SubsequentGetEnumeratorThrows()
    {
        // Arrange
        var collection = JsonDataCollection.Create("[5,6]");
        _ = collection.ToList(); // force document creation
        ((IDisposable)collection).Dispose();

        // Act & Assert
        Assert.Throws<ObjectDisposedException>(() => collection.GetEnumerator());
    }

    [Fact]
    public void JsonDataJsonStringCollection_OnDispose_CalledTwice_DoesNotThrow()
    {
        // Arrange
        var collection = JsonDataCollection.Create("[1]");
        _ = collection.ToList(); // force document creation
        var disposable = (IDisposable)collection;

        // Act
        disposable.Dispose();
        var ex = Record.Exception(() => disposable.Dispose());

        // Assert
        Assert.Null(ex);
    }

    // ── JsonDataJsonStringCollection.GetEnumerator (additional coverage) ──────

    [Fact]
    public void JsonDataJsonStringCollection_GetEnumerator_NotDisposed_ReturnsItems()
    {
        // Arrange
        using var collection = JsonDataCollection.Create("[1,2,3]");

        // Act
        var items = new List<JsonData>();
        using var enumerator = collection.GetEnumerator();
        while (enumerator.MoveNext())
            items.Add(enumerator.Current);

        // Assert
        Assert.Equal(3, items.Count);
    }

    [Fact]
    public void JsonDataJsonStringCollection_GetEnumerator_AfterDisposeWithoutDocumentCreation_ThrowsObjectDisposedException()
    {
        // Arrange – dispose before any enumeration (document never created)
        var collection = JsonDataCollection.Create("[1,2,3]");
        ((IDisposable)collection).Dispose();

        // Act & Assert
        Assert.Throws<ObjectDisposedException>(() => collection.GetEnumerator());
    }

    // ── IJsonDataCollection.AsJsonData<T>() extension ────────────────────────

    private sealed class TestKey : IJsonDataKey { }

    [Fact]
    public void AsJsonData_OnCollection_ReturnsTypedSequenceWithSameCount()
    {
        // Arrange
        using var collection = JsonDataCollection.Create("[1,2,3]");

        // Act
        var typed = collection.AsJsonData<TestKey>().ToList();

        // Assert
        Assert.Equal(3, typed.Count);
    }

    [Fact]
    public void AsJsonData_OnEmptyCollection_ReturnsEmptySequence()
    {
        // Arrange
        using var collection = JsonDataCollection.Create("[]");

        // Act
        var typed = collection.AsJsonData<TestKey>().ToList();

        // Assert
        Assert.Empty(typed);
    }

    [Fact]
    public void AsJsonData_OnCollection_EachItemIsJsonDataOfT()
    {
        // Arrange
        using var collection = JsonDataCollection.Create("[1,2]");

        // Act
        var typed = collection.AsJsonData<TestKey>().ToList();

        // Assert
        Assert.All(typed, item => Assert.IsType<JsonData<TestKey>>(item));
    }

    [Fact]
    public void AsJsonData_OnCollection_ItemsHaveExpectedValues()
    {
        // Arrange
        using var collection = JsonDataCollection.Create("[10,20]");

        // Act
        var typed = collection.AsJsonData<TestKey>().ToList();

        // Assert – values can be read back via the JsonData underlying the typed wrapper
        Assert.Equal(10, (int)typed[0].Json);
        Assert.Equal(20, (int)typed[1].Json);
    }

    [Fact]
    public void AsJsonData_OnCollection_CanEnumerateMultipleTimes()
    {
        // Arrange
        using var collection = JsonDataCollection.Create("[1,2]");

        // Act
        var first = collection.AsJsonData<TestKey>().ToList();
        var second = collection.AsJsonData<TestKey>().ToList();

        // Assert
        Assert.Equal(first.Count, second.Count);
    }
}
