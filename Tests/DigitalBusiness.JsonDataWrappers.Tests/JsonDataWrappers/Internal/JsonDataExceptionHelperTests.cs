using DigitalBusiness.JsonDataWrappers;
using DigitalBusiness.JsonDataWrappers.Internal;
using System;
using System.Text.Json.Nodes;

namespace DigitalBusiness.JsonDataWrappers.Tests;

public class JsonDataExceptionHelperTests
{
    // --- ReadOnlyException ---

    [Fact]
    public void ReadOnlyException_ReturnsInvalidOperationException()
    {
        var ex = JsonDataExceptionHelper.ReadOnlyException();
        Assert.IsType<InvalidOperationException>(ex);
    }

    [Fact]
    public void ReadOnlyException_HasExpectedMessage()
    {
        var ex = JsonDataExceptionHelper.ReadOnlyException();
        Assert.Equal("Cannot modify read-only JsonData.", ex.Message);
    }

    [Fact]
    public void ReadOnlyException_ReturnsNewInstanceEachCall()
    {
        var ex1 = JsonDataExceptionHelper.ReadOnlyException();
        var ex2 = JsonDataExceptionHelper.ReadOnlyException();
        Assert.NotSame(ex1, ex2);
    }

    // --- NullException ---

    [Fact]
    public void NullException_ReturnsInvalidOperationException()
    {
        var ex = JsonDataExceptionHelper.NullException();
        Assert.IsType<InvalidOperationException>(ex);
    }

    [Fact]
    public void NullException_HasExpectedMessage()
    {
        var ex = JsonDataExceptionHelper.NullException();
        Assert.Equal("JsonData is null.", ex.Message);
    }

    [Fact]
    public void NullException_ReturnsNewInstanceEachCall()
    {
        var ex1 = JsonDataExceptionHelper.NullException();
        var ex2 = JsonDataExceptionHelper.NullException();
        Assert.NotSame(ex1, ex2);
    }

    // --- GetTypedValueException ---

    [Fact]
    public void GetTypedValueException_WhenJsonDataIsNull_ReturnsNullException()
    {
        var nullData = JsonData.CreateNull();
        var ex = JsonDataExceptionHelper.GetTypedValueException<int>(nullData);
        Assert.IsType<InvalidOperationException>(ex);
        Assert.Equal("JsonData is null.", ex.Message);
    }

    [Fact]
    public void GetTypedValueException_WhenJsonDataIsNotNull_ReturnsTypeMismatchException()
    {
        var stringData = new JsonData(JsonValue.Create("hello"));
        var ex = JsonDataExceptionHelper.GetTypedValueException<int>(stringData);
        Assert.IsType<InvalidOperationException>(ex);
        Assert.Contains(typeof(int).FullName!, ex.Message);
        Assert.Contains("ValueKind", ex.Message);
    }

    [Fact]
    public void GetTypedValueException_WhenJsonDataIsNotNull_MessageContainsValueKind()
    {
        var boolData = new JsonData(JsonValue.Create(true));
        var ex = JsonDataExceptionHelper.GetTypedValueException<string>(boolData);
        Assert.Contains("True", ex.Message);
    }

    [Fact]
    public void GetTypedValueException_WhenJsonDataIsNotNull_MessageContainsTypeName()
    {
        var numData = new JsonData(JsonValue.Create(42));
        var ex = JsonDataExceptionHelper.GetTypedValueException<double>(numData);
        Assert.Contains("System.Double", ex.Message);
    }

    // --- GetTypedPropertyException ---

    [Fact]
    public void GetTypedPropertyException_WhenObjectJsonDataIsNull_ReturnsNullException()
    {
        var nullData = JsonData.CreateNull();
        var ex = JsonDataExceptionHelper.GetTypedPropertyException<string>("anyProp", nullData);
        Assert.IsType<InvalidOperationException>(ex);
        Assert.Equal("JsonData is null.", ex.Message);
    }

    [Fact]
    public void GetTypedPropertyException_WhenPropertyDoesNotExist_ReturnsTypeMismatchException()
    {
        var obj = new JsonObject { ["name"] = "test" };
        var objData = new JsonData(obj);
        // "missing" property doesn't exist ? should return type mismatch exception
        var ex = JsonDataExceptionHelper.GetTypedPropertyException<int>("missing", objData);
        Assert.IsType<InvalidOperationException>(ex);
        Assert.Contains("missing", ex.Message);
    }

    [Fact]
    public void GetTypedPropertyException_WhenPropertyExists_ReturnsPropertyNotExistException()
    {
        var obj = new JsonObject { ["name"] = "test" };
        var objData = new JsonData(obj);
        // property "name" exists ? code returns JsonDataPropertyNotExist (line 32)
        var ex = JsonDataExceptionHelper.GetTypedPropertyException<int>("name", objData);
        Assert.IsType<InvalidOperationException>(ex);
        Assert.Contains("name", ex.Message);
    }

    // --- GetTypedIndexException ---

    [Fact]
    public void GetTypedIndexException_WhenArrayJsonDataIsNull_ReturnsNullException()
    {
        var nullData = JsonData.CreateNull();
        var ex = JsonDataExceptionHelper.GetTypedIndexException<int>(0, nullData);
        Assert.IsType<InvalidOperationException>(ex);
        Assert.Equal("JsonData is null.", ex.Message);
    }

    [Fact]
    public void GetTypedIndexException_WhenIsArray_ReturnsArrayExpectedException()
    {
        var arr = new JsonArray(1, 2, 3);
        var arrData = new JsonData(arr);
        // line 40: if (arrayJsonData.IsArray) return JsonDataArrayExpectedException()
        var ex = JsonDataExceptionHelper.GetTypedIndexException<int>(0, arrData);
        Assert.IsType<InvalidOperationException>(ex);
    }

    [Fact]
    public void GetTypedIndexException_WhenNotArrayAndIndexNegative_ReturnsIndexOutOfRangeException()
    {
        // Use a non-array (object) JsonData so IsArray is false
        var obj = new JsonObject { ["x"] = 1 };
        var objData = new JsonData(obj);
        var ex = JsonDataExceptionHelper.GetTypedIndexException<int>(-1, objData);
        Assert.IsType<IndexOutOfRangeException>(ex);
        Assert.Contains("-1", ex.Message);
    }

    [Fact]
    public void GetTypedIndexException_WhenNotArrayAndIndexExceedsCount_ReturnsIndexOutOfRangeException()
    {
        var obj = new JsonObject { ["a"] = 1 };
        var objData = new JsonData(obj);
        var ex = JsonDataExceptionHelper.GetTypedIndexException<int>(999, objData);
        Assert.IsType<IndexOutOfRangeException>(ex);
    }

    [Fact]
    public void GetTypedIndexException_WhenNotArrayAndIndexValid_ReturnsTypeMismatchException()
    {
        // Create an object with a known key; Count for object = number of properties
        var obj = new JsonObject { ["a"] = 1, ["b"] = 2 };
        var objData = new JsonData(obj);
        // index 0 is within [0, Count=2]
        var ex = JsonDataExceptionHelper.GetTypedIndexException<string>(0, objData);
        Assert.IsType<InvalidOperationException>(ex);
        Assert.Contains("System.String", ex.Message);
    }

    // --- JsonDataArrayIndexOutOfRangeException ---

    [Fact]
    public void JsonDataArrayIndexOutOfRangeException_ReturnsIndexOutOfRangeException()
    {
        var ex = JsonDataExceptionHelper.JsonDataArrayIndexOutOfRangeException(5);
        Assert.IsType<IndexOutOfRangeException>(ex);
    }

    [Fact]
    public void JsonDataArrayIndexOutOfRangeException_MessageContainsIndex()
    {
        var ex = JsonDataExceptionHelper.JsonDataArrayIndexOutOfRangeException(42);
        Assert.Contains("42", ex.Message);
    }

    [Fact]
    public void JsonDataArrayIndexOutOfRangeException_NegativeIndex_MessageContainsIndex()
    {
        var ex = JsonDataExceptionHelper.JsonDataArrayIndexOutOfRangeException(-3);
        Assert.Contains("-3", ex.Message);
    }

    [Fact]
    public void JsonDataArrayIndexOutOfRangeException_ZeroIndex_ReturnsException()
    {
        var ex = JsonDataExceptionHelper.JsonDataArrayIndexOutOfRangeException(0);
        Assert.IsType<IndexOutOfRangeException>(ex);
        Assert.Contains("0", ex.Message);
    }

    // --- JsonDataObjectExpectedException ---

    [Fact]
    public void JsonDataObjectExpectedException_ReturnsInvalidOperationException()
    {
        var ex = JsonDataExceptionHelper.JsonDataObjectExpectedException();
        Assert.IsType<InvalidOperationException>(ex);
    }

    [Fact]
    public void JsonDataObjectExpectedException_HasExpectedMessage()
    {
        var ex = JsonDataExceptionHelper.JsonDataObjectExpectedException();
        Assert.Equal("JsonData is not an object.", ex.Message);
    }

    [Fact]
    public void JsonDataObjectExpectedException_ReturnsNewInstanceEachCall()
    {
        var ex1 = JsonDataExceptionHelper.JsonDataObjectExpectedException();
        var ex2 = JsonDataExceptionHelper.JsonDataObjectExpectedException();
        Assert.NotSame(ex1, ex2);
    }

    // --- JsonDataArrayExpectedException ---

    [Fact]
    public void JsonDataArrayExpectedException_ReturnsInvalidOperationException()
    {
        var ex = JsonDataExceptionHelper.JsonDataArrayExpectedException();
        Assert.IsType<InvalidOperationException>(ex);
    }

    [Fact]
    public void JsonDataArrayExpectedException_HasExpectedMessage()
    {
        var ex = JsonDataExceptionHelper.JsonDataArrayExpectedException();
        Assert.Equal("JsonData is not an object.", ex.Message);
    }

    [Fact]
    public void JsonDataArrayExpectedException_ReturnsNewInstanceEachCall()
    {
        var ex1 = JsonDataExceptionHelper.JsonDataArrayExpectedException();
        var ex2 = JsonDataExceptionHelper.JsonDataArrayExpectedException();
        Assert.NotSame(ex1, ex2);
    }

    // --- PropertyExistsAndNotArrayException ---

    [Fact]
    public void PropertyExistsAndNotArrayException_ReturnsInvalidOperationException()
    {
        var ex = JsonDataExceptionHelper.PropertyExistsAndNotArrayException("myProp");
        Assert.IsType<InvalidOperationException>(ex);
    }

    [Fact]
    public void PropertyExistsAndNotArrayException_MessageContainsPropertyName()
    {
        var ex = JsonDataExceptionHelper.PropertyExistsAndNotArrayException("myProp");
        Assert.Contains("myProp", ex.Message);
    }

    [Fact]
    public void PropertyExistsAndNotArrayException_EmptyPropertyName_MessageContainsEmptyQuotes()
    {
        var ex = JsonDataExceptionHelper.PropertyExistsAndNotArrayException(string.Empty);
        Assert.Contains("''", ex.Message);
    }

    // --- PropertyExistsAndNotObjectException ---

    [Fact]
    public void PropertyExistsAndNotObjectException_ReturnsInvalidOperationException()
    {
        var ex = JsonDataExceptionHelper.PropertyExistsAndNotObjectException("someKey");
        Assert.IsType<InvalidOperationException>(ex);
    }

    [Fact]
    public void PropertyExistsAndNotObjectException_MessageContainsPropertyName()
    {
        var ex = JsonDataExceptionHelper.PropertyExistsAndNotObjectException("someKey");
        Assert.Contains("someKey", ex.Message);
    }

    [Fact]
    public void PropertyExistsAndNotObjectException_EmptyPropertyName_MessageContainsEmptyQuotes()
    {
        var ex = JsonDataExceptionHelper.PropertyExistsAndNotObjectException(string.Empty);
        Assert.Contains("''", ex.Message);
    }

    // --- IndexExistsAndNotArrayException ---

    [Fact]
    public void IndexExistsAndNotArrayException_ReturnsInvalidOperationException()
    {
        var ex = JsonDataExceptionHelper.IndexExistsAndNotArrayException(3);
        Assert.IsType<InvalidOperationException>(ex);
    }

    [Fact]
    public void IndexExistsAndNotArrayException_MessageContainsIndex()
    {
        var ex = JsonDataExceptionHelper.IndexExistsAndNotArrayException(7);
        Assert.Contains("7", ex.Message);
    }

    [Fact]
    public void IndexExistsAndNotArrayException_ZeroIndex_MessageContainsZero()
    {
        var ex = JsonDataExceptionHelper.IndexExistsAndNotArrayException(0);
        Assert.Contains("0", ex.Message);
    }

    [Fact]
    public void IndexExistsAndNotArrayException_NegativeIndex_MessageContainsNegativeIndex()
    {
        var ex = JsonDataExceptionHelper.IndexExistsAndNotArrayException(-1);
        Assert.Contains("-1", ex.Message);
    }

    [Fact]
    public void IndexExistsAndNotArrayException_MessageContainsNotArrayText()
    {
        var ex = JsonDataExceptionHelper.IndexExistsAndNotArrayException(2);
        Assert.Contains("not an array", ex.Message);
    }

    // --- IndexExistsAndNotObjectException ---

    [Fact]
    public void IndexExistsAndNotObjectException_ReturnsInvalidOperationException()
    {
        var ex = JsonDataExceptionHelper.IndexExistsAndNotObjectException(3);
        Assert.IsType<InvalidOperationException>(ex);
    }

    [Fact]
    public void IndexExistsAndNotObjectException_MessageContainsIndex()
    {
        var ex = JsonDataExceptionHelper.IndexExistsAndNotObjectException(5);
        Assert.Contains("5", ex.Message);
    }

    [Fact]
    public void IndexExistsAndNotObjectException_ZeroIndex_MessageContainsZero()
    {
        var ex = JsonDataExceptionHelper.IndexExistsAndNotObjectException(0);
        Assert.Contains("0", ex.Message);
    }

    [Fact]
    public void IndexExistsAndNotObjectException_NegativeIndex_MessageContainsNegativeIndex()
    {
        var ex = JsonDataExceptionHelper.IndexExistsAndNotObjectException(-2);
        Assert.Contains("-2", ex.Message);
    }

    [Fact]
    public void IndexExistsAndNotObjectException_MessageContainsNotObjectText()
    {
        var ex = JsonDataExceptionHelper.IndexExistsAndNotObjectException(1);
        Assert.Contains("not an object", ex.Message);
    }

    // --- JsonDataPropertyNotExist ---

    [Fact]
    public void JsonDataPropertyNotExist_ReturnsInvalidOperationException()
    {
        var ex = JsonDataExceptionHelper.JsonDataPropertyNotExist("myProperty");
        Assert.IsType<InvalidOperationException>(ex);
    }

    [Fact]
    public void JsonDataPropertyNotExist_MessageContainsPropertyName()
    {
        var ex = JsonDataExceptionHelper.JsonDataPropertyNotExist("myProperty");
        Assert.Contains("myProperty", ex.Message);
    }

    [Fact]
    public void JsonDataPropertyNotExist_MessageContainsDoesNotExistText()
    {
        var ex = JsonDataExceptionHelper.JsonDataPropertyNotExist("prop");
        Assert.Contains("does not exist", ex.Message);
    }

    [Fact]
    public void JsonDataPropertyNotExist_EmptyPropertyName_MessageContainsEmptyQuotes()
    {
        var ex = JsonDataExceptionHelper.JsonDataPropertyNotExist(string.Empty);
        Assert.Contains("''", ex.Message);
    }

    [Fact]
    public void JsonDataPropertyNotExist_ReturnsNewInstanceEachCall()
    {
        var ex1 = JsonDataExceptionHelper.JsonDataPropertyNotExist("p");
        var ex2 = JsonDataExceptionHelper.JsonDataPropertyNotExist("p");
        Assert.NotSame(ex1, ex2);
    }

    // --- GetNodeValueException ---

    [Fact]
    public void GetNodeValueException_ReturnsInvalidOperationException()
    {
        var node = new JsonData(JsonValue.Create(42));
        var ex = JsonDataExceptionHelper.GetNodeValueException(typeof(string), node);
        Assert.IsType<InvalidOperationException>(ex);
    }

    [Fact]
    public void GetNodeValueException_MessageContainsTypeName()
    {
        var node = new JsonData(JsonValue.Create(42));
        var ex = JsonDataExceptionHelper.GetNodeValueException(typeof(string), node);
        Assert.Contains("System.String", ex.Message);
    }

    [Fact]
    public void GetNodeValueException_MessageContainsNodeSourceType()
    {
        var node = new JsonData(JsonValue.Create(42));
        var ex = JsonDataExceptionHelper.GetNodeValueException(typeof(int), node);
        Assert.Contains("JsonValue", ex.Message);
    }

    [Fact]
    public void GetNodeValueException_WhenNodeSourceIsNull_MessageContainsNull()
    {
        var node = new JsonData((System.Text.Json.Nodes.JsonNode?)null);
        var ex = JsonDataExceptionHelper.GetNodeValueException(typeof(double), node);
        Assert.Contains("null", ex.Message);
    }

    // --- GetRequiredValueException ---

    [Fact]
    public void GetRequiredValueException_AllowNullNodesFalse_ReturnsInvalidOperationException()
    {
        var ex = JsonDataExceptionHelper.GetRequiredValueException(false);
        Assert.IsType<InvalidOperationException>(ex);
    }

    [Fact]
    public void GetRequiredValueException_AllowNullNodesFalse_MessageContainsNullJNode()
    {
        var ex = JsonDataExceptionHelper.GetRequiredValueException(false);
        Assert.Contains("or is a Null JNode.", ex.Message);
    }

    [Fact]
    public void GetRequiredValueException_AllowNullNodesTrue_ReturnsInvalidOperationException()
    {
        var ex = JsonDataExceptionHelper.GetRequiredValueException(true);
        Assert.IsType<InvalidOperationException>(ex);
    }

    [Fact]
    public void GetRequiredValueException_AllowNullNodesTrue_MessageDoesNotContainNullJNode()
    {
        var ex = JsonDataExceptionHelper.GetRequiredValueException(true);
        Assert.DoesNotContain("or is a Null JNode.", ex.Message);
    }

    [Fact]
    public void GetRequiredValueException_AllowNullNodesTrue_MessageContainsRequiredNodeIsNull()
    {
        var ex = JsonDataExceptionHelper.GetRequiredValueException(true);
        Assert.Contains("Required node is null", ex.Message);
    }
}
