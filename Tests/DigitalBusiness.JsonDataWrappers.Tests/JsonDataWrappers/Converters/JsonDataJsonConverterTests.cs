using System;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using DigitalBusiness.JsonDataWrappers;
using DigitalBusiness.JsonDataWrappers.Converters;
using Xunit;

namespace DigitalBusiness.JsonDataWrappers.Tests
{
    public class JsonDataJsonConverterTests
    {
        private readonly JsonDataJsonConverter _converter = new();
        private readonly JsonSerializerOptions _options = new();

        private JsonData ReadWithType(string json, Type type)
        {
            var bytes = Encoding.UTF8.GetBytes(json);
            var reader = new Utf8JsonReader(bytes);
            reader.Read();
            return _converter.Read(ref reader, type, _options);
        }

        // --- Read tests ---

        [Fact]
        public void Read_ValidJson_ReturnsJsonDataWithElement()
        {
            const string json = "{\"key\":42}";
            var result = ReadWithType(json, typeof(JsonData));
            Assert.True(result.IsElement);
            Assert.Equal(JsonValueKind.Object, result.ValueKind);
        }

        [Fact]
        public void Read_WrongType_ThrowsJsonException()
        {
            const string json = "{}";
            var ex = Record.Exception(() => ReadWithType(json, typeof(string)));
            Assert.IsType<JsonException>(ex);
        }

        [Fact]
        public void Read_WrongType_ExceptionMessageContainsExpectedAndActualTypes()
        {
            const string json = "{}";
            var ex = Record.Exception(() => ReadWithType(json, typeof(int)));
            Assert.IsType<JsonException>(ex);
            Assert.Contains(typeof(int).FullName!, ex.Message);
            Assert.Contains(typeof(JsonData).FullName!, ex.Message);
        }

        [Fact]
        public void Read_NumberJson_ReturnsJsonDataWithNumberKind()
        {
            var result = ReadWithType("123", typeof(JsonData));
            Assert.True(result.IsElement);
            Assert.Equal(JsonValueKind.Number, result.ValueKind);
        }

        [Fact]
        public void Read_NullJson_ReturnsJsonDataWithNullKind()
        {
            var result = ReadWithType("null", typeof(JsonData));
            Assert.True(result.IsElement);
            Assert.Equal(JsonValueKind.Null, result.ValueKind);
        }

        // --- Write tests ---

        [Fact]
        public void Write_ElementBacked_WritesElement()
        {
            var element = JsonDocument.Parse("{\"x\":1}").RootElement;
            var jsonData = new JsonData(element);

            var json = JsonSerializer.Serialize(jsonData, _options);

            Assert.Equal("{\"x\":1}", json);
        }

        [Fact]
        public void Write_NodeBacked_WritesNode()
        {
            var node = JsonNode.Parse("{\"y\":2}");
            var jsonData = new JsonData(node);

            var json = JsonSerializer.Serialize(jsonData, _options);

            Assert.Equal("{\"y\":2}", json);
        }

        [Fact]
        public void Write_Uninitialized_WritesNull()
        {
            var jsonData = new JsonData();

            var json = JsonSerializer.Serialize(jsonData, _options);

            Assert.Equal("null", json);
        }

        [Fact]
        public void Write_NullNode_WritesNull()
        {
            var jsonData = new JsonData((JsonNode?)null);

            var json = JsonSerializer.Serialize(jsonData, _options);

            Assert.Equal("null", json);
        }

        [Fact]
        public void Write_ElementBacked_ArrayJson_WritesCorrectly()
        {
            var element = JsonDocument.Parse("[1,2,3]").RootElement;
            var jsonData = new JsonData(element);

            var json = JsonSerializer.Serialize(jsonData, _options);

            Assert.Equal("[1,2,3]", json);
        }
    }
}
