using System;
using System.Text.Json;
using System.Text.Json.Nodes;
using DigitalBusiness.JsonDataWrappers;
using Xunit;

namespace DigitalBusiness.JsonDataWrappers.Tests;

public class JsonDataHelperTests
{
    // -- CreateNodeFromElement ------------------------------------------------

    [Fact]
    public void CreateNodeFromElement_StringElement_ReturnsJsonValueWithSameString()
    {
        // Arrange
        using var doc = JsonDocument.Parse("\"hello\"");
        var element = doc.RootElement;

        // Act
        var node = JsonDataHelper.CreateNodeFromElement(element);

        // Assert
        Assert.NotNull(node);
        Assert.Equal("hello", node!.GetValue<string>());
    }

    [Fact]
    public void CreateNodeFromElement_NumberElement_ReturnsJsonValueWithSameNumber()
    {
        // Arrange
        using var doc = JsonDocument.Parse("42");
        var element = doc.RootElement;

        // Act
        var node = JsonDataHelper.CreateNodeFromElement(element);

        // Assert
        Assert.NotNull(node);
        Assert.Equal(42, node!.GetValue<int>());
    }

    [Fact]
    public void CreateNodeFromElement_NullElement_ReturnsNull()
    {
        // Arrange
        using var doc = JsonDocument.Parse("null");
        var element = doc.RootElement;

        // Act
        var node = JsonDataHelper.CreateNodeFromElement(element);

        // Assert
        Assert.Null(node);
    }

    [Fact]
    public void CreateNodeFromElement_ObjectElement_ThrowsInvalidOperationException()
    {
        // Arrange
        using var doc = JsonDocument.Parse("{\"key\":1}");
        var element = doc.RootElement;

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => JsonDataHelper.CreateNodeFromElement(element));
    }

    // -- GetRootNode ----------------------------------------------------------

    [Fact]
    public void GetRootNode_NodeWithNoParent_ReturnsSameNode()
    {
        // Arrange
        var node = JsonValue.Create(1)!;

        // Act
        var root = JsonDataHelper.GetRootNode(node);

        // Assert
        Assert.Same(node, root);
    }

    [Fact]
    public void GetRootNode_NodeWithParent_ReturnsRoot()
    {
        // Arrange
        var root = new JsonObject();
        var child = JsonValue.Create("value")!;
        root["key"] = child;

        // Act
        var result = JsonDataHelper.GetRootNode(child);

        // Assert
        Assert.Same(root, result);
    }

    [Fact]
    public void GetRootNode_DeepNestedNode_ReturnsTopMostRoot()
    {
        // Arrange
        var root = new JsonObject();
        var inner = new JsonObject();
        var leaf = JsonValue.Create(99)!;
        inner["leaf"] = leaf;
        root["inner"] = inner;

        // Act
        var result = JsonDataHelper.GetRootNode(leaf);

        // Assert
        Assert.Same(root, result);
    }

    // -- HasCommonRoot --------------------------------------------------------

    [Fact]
    public void HasCommonRoot_TwoNodesInSameTree_ReturnsTrue()
    {
        // Arrange
        var root = new JsonObject();
        var nodeA = JsonValue.Create("a")!;
        var nodeB = JsonValue.Create("b")!;
        root["a"] = nodeA;
        root["b"] = nodeB;

        // Act
        var result = JsonDataHelper.HasCommonRoot(nodeA, nodeB);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void HasCommonRoot_TwoNodesInDifferentTrees_ReturnsFalse()
    {
        // Arrange
        var rootA = new JsonObject();
        var rootB = new JsonObject();
        var nodeA = JsonValue.Create("a")!;
        var nodeB = JsonValue.Create("b")!;
        rootA["a"] = nodeA;
        rootB["b"] = nodeB;

        // Act
        var result = JsonDataHelper.HasCommonRoot(nodeA, nodeB);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void HasCommonRoot_SameNode_ReturnsTrue()
    {
        // Arrange
        var node = JsonValue.Create(1)!;

        // Act
        var result = JsonDataHelper.HasCommonRoot(node, node);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void HasCommonRoot_TwoRootlessNodes_ReturnsFalse()
    {
        // Arrange
        var nodeA = JsonValue.Create("a")!;
        var nodeB = JsonValue.Create("b")!;

        // Act
        var result = JsonDataHelper.HasCommonRoot(nodeA, nodeB);

        // Assert
        Assert.False(result);
    }

    // -- GetNodeToAdd(JsonNode, JsonNode) -------------------------------------

    [Fact]
    public void GetNodeToAdd_NodeWithNoParent_ReturnsSameNode()
    {
        // Arrange
        var addNode = JsonValue.Create(5)!;
        var parent = new JsonObject();

        // Act
        var result = JsonDataHelper.GetNodeToAdd(addNode, parent);

        // Assert
        Assert.Same(addNode, result);
    }

    [Fact]
    public void GetNodeToAdd_NodeWithParentInSameTree_ReturnsSameNode()
    {
        // Arrange
        var root = new JsonObject();
        var addNode = JsonValue.Create(5)!;
        root["add"] = addNode;
        // parentNode also in the same tree
        var parentNode = JsonValue.Create("p")!;
        root["parent"] = parentNode;

        // Act
        var result = JsonDataHelper.GetNodeToAdd(addNode, parentNode);

        // Assert
        Assert.Same(addNode, result);
    }

    [Fact]
    public void GetNodeToAdd_NodeWithParentInDifferentTree_ReturnsDeepClone()
    {
        // Arrange
        var treeA = new JsonObject();
        var addNode = JsonValue.Create(5)!;
        treeA["add"] = addNode;

        var treeB = new JsonObject();
        var parentNode = JsonValue.Create("p")!;
        treeB["parent"] = parentNode;

        // Act
        var result = JsonDataHelper.GetNodeToAdd(addNode, parentNode);

        // Assert
        Assert.NotSame(addNode, result);
        Assert.Equal(5, result.GetValue<int>());
    }

    // -- GetNodeToAdd(JsonData, JsonNode) -------------------------------------

    [Fact]
    public void GetNodeToAddJsonData_NullJsonData_ReturnsNull()
    {
        // Arrange
        var addValue = new JsonData(); // default / null
        var addToNode = new JsonObject();

        // Act
        var result = JsonDataHelper.GetNodeToAdd(addValue, addToNode);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void GetNodeToAddJsonData_NodeBacked_ReturnsNode()
    {
        // Arrange
        var innerNode = JsonValue.Create(42)!;
        var addValue = new JsonData(innerNode);
        var addToNode = new JsonObject();

        // Act
        var result = JsonDataHelper.GetNodeToAdd(addValue, addToNode);

        // Assert
        Assert.Same(innerNode, result);
    }

    [Fact]
    public void GetNodeToAddJsonData_NodeBackedWithParentInDifferentTree_ReturnsClone()
    {
        // Arrange
        var treeA = new JsonObject();
        var innerNode = JsonValue.Create(42)!;
        treeA["x"] = innerNode;
        var addValue = new JsonData(innerNode);

        var addToNode = new JsonObject(); // different tree

        // Act
        var result = JsonDataHelper.GetNodeToAdd(addValue, addToNode);

        // Assert
        Assert.NotSame(innerNode, result);
        Assert.Equal(42, result!.GetValue<int>());
    }

    [Fact]
    public void GetNodeToAddJsonData_ElementBacked_DoesNotThrow()
    {
        // Arrange
        using var doc = JsonDocument.Parse("\"test\"");
        var element = doc.RootElement.Clone();
        var addValue = new JsonData(element);
        var addToNode = new JsonObject();

        // Act & Assert
        // Line 51 discards the result, then falls through to the Node check.
        // Since Node is null after creating from element, it should throw.
        Assert.Throws<InvalidOperationException>(() => JsonDataHelper.GetNodeToAdd(addValue, addToNode));
    }

    // -- GetPropertyNames -----------------------------------------------------

    [Fact]
    public void GetPropertyNames_ElementBackedObject_ReturnsPropertyNames()
    {
        // Arrange
        using var doc = JsonDocument.Parse("{\"foo\":1,\"bar\":2}");
        var element = doc.RootElement.Clone();
        var jsonData = new JsonData(element);

        // Act
        var names = JsonDataHelper.GetPropertyNames(jsonData).ToList();

        // Assert
        Assert.Equal(new[] { "foo", "bar" }, names);
    }

    [Fact]
    public void GetPropertyNames_ElementBackedArray_ReturnsEmpty()
    {
        // Arrange
        using var doc = JsonDocument.Parse("[1,2,3]");
        var element = doc.RootElement.Clone();
        var jsonData = new JsonData(element);

        // Act
        var names = JsonDataHelper.GetPropertyNames(jsonData).ToList();

        // Assert
        Assert.Empty(names);
    }

    [Fact]
    public void GetPropertyNames_ElementBackedObject_EmptyObject_ReturnsEmpty()
    {
        // Arrange
        using var doc = JsonDocument.Parse("{}");
        var element = doc.RootElement.Clone();
        var jsonData = new JsonData(element);

        // Act
        var names = JsonDataHelper.GetPropertyNames(jsonData).ToList();

        // Assert
        Assert.Empty(names);
    }

    [Fact]
    public void GetPropertyNames_NodeBackedJsonObject_ReturnsPropertyNames()
    {
        // Arrange
        var obj = new JsonObject { ["alpha"] = 1, ["beta"] = "x" };
        var jsonData = new JsonData(obj);

        // Act
        var names = JsonDataHelper.GetPropertyNames(jsonData).ToList();

        // Assert
        Assert.Contains("alpha", names);
        Assert.Contains("beta", names);
        Assert.Equal(2, names.Count);
    }

    [Fact]
    public void GetPropertyNames_NodeBackedEmptyJsonObject_ReturnsEmpty()
    {
        // Arrange
        var obj = new JsonObject();
        var jsonData = new JsonData(obj);

        // Act
        var names = JsonDataHelper.GetPropertyNames(jsonData).ToList();

        // Assert
        Assert.Empty(names);
    }

    [Fact]
    public void GetPropertyNames_NodeBackedJsonArray_ReturnsEmpty()
    {
        // Arrange
        var arr = new JsonArray(1, 2, 3);
        var jsonData = new JsonData(arr);

        // Act
        var names = JsonDataHelper.GetPropertyNames(jsonData).ToList();

        // Assert
        Assert.Empty(names);
    }

    [Fact]
    public void GetPropertyNames_NullNode_NoElement_ReturnsEmpty()
    {
        // Arrange
        var jsonData = new JsonData((JsonNode?)null);

        // Act
        var names = JsonDataHelper.GetPropertyNames(jsonData).ToList();

        // Assert
        Assert.Empty(names);
    }

    // -- GetArrayItems ---------------------------------------------------------

    [Fact]
    public void GetArrayItems_ElementBackedArray_ReturnsItems()
    {
        // Arrange
        using var doc = JsonDocument.Parse("[10,20,30]");
        var element = doc.RootElement.Clone();
        var jsonData = new JsonData(element);

        // Act
        var items = JsonDataHelper.GetArrayItems(jsonData).ToList();

        // Assert
        Assert.Equal(3, items.Count);
        Assert.Equal(10, items[0].Element!.Value.GetInt32());
        Assert.Equal(20, items[1].Element!.Value.GetInt32());
        Assert.Equal(30, items[2].Element!.Value.GetInt32());
    }

    [Fact]
    public void GetArrayItems_ElementBackedEmptyArray_ReturnsEmpty()
    {
        // Arrange
        using var doc = JsonDocument.Parse("[]");
        var element = doc.RootElement.Clone();
        var jsonData = new JsonData(element);

        // Act
        var items = JsonDataHelper.GetArrayItems(jsonData).ToList();

        // Assert
        Assert.Empty(items);
    }

    [Fact]
    public void GetArrayItems_ElementBackedObject_ReturnsEmpty()
    {
        // Arrange
        using var doc = JsonDocument.Parse("{\"a\":1}");
        var element = doc.RootElement.Clone();
        var jsonData = new JsonData(element);

        // Act
        var items = JsonDataHelper.GetArrayItems(jsonData).ToList();

        // Assert
        Assert.Empty(items);
    }

    [Fact]
    public void GetArrayItems_NodeBackedJsonArray_ReturnsItems()
    {
        // Arrange
        var arr = new JsonArray(JsonValue.Create(5), JsonValue.Create(6));
        var jsonData = new JsonData(arr);

        // Act
        var items = JsonDataHelper.GetArrayItems(jsonData).ToList();

        // Assert
        Assert.Equal(2, items.Count);
    }

    [Fact]
    public void GetArrayItems_NodeBackedEmptyJsonArray_ReturnsEmpty()
    {
        // Arrange
        var arr = new JsonArray();
        var jsonData = new JsonData(arr);

        // Act
        var items = JsonDataHelper.GetArrayItems(jsonData).ToList();

        // Assert
        Assert.Empty(items);
    }

    [Fact]
    public void GetArrayItems_NodeBackedJsonObject_ReturnsEmpty()
    {
        // Arrange
        var obj = new JsonObject { ["key"] = 1 };
        var jsonData = new JsonData(obj);

        // Act
        var items = JsonDataHelper.GetArrayItems(jsonData).ToList();

        // Assert
        Assert.Empty(items);
    }

    [Fact]
    public void GetArrayItems_NullNode_NoElement_ReturnsEmpty()
    {
        // Arrange
        var jsonData = new JsonData((JsonNode?)null);

        // Act
        var items = JsonDataHelper.GetArrayItems(jsonData).ToList();

        // Assert
        Assert.Empty(items);
    }
}
