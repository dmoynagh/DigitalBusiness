using DigitalBusiness.JsonDataWrappers;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection.Metadata;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace DigitalBusiness.JsonDataWrappers
{
    /// <summary>Marks a collection as enumerable as <see cref="JsonData"/> items.</summary>
    public interface IJsonDataCollection : IEnumerable<JsonData>
    {       
    }

    /// <summary>
    /// Abstract base for collections that enumerate JSON data from various sources as <see cref="JsonData"/> items.
    /// Concrete implementations wrap <see cref="JsonNode"/>, <see cref="JsonArray"/>, <see cref="JsonDocument"/>,
    /// <see cref="JsonElement"/> sequences, or raw JSON strings.
    /// <para>
    /// Implements <see cref="IDisposable"/> because some sources (<see cref="JsonDocument"/>, parsed strings)
    /// own unmanaged memory and must be disposed. Use <c>autoDispose</c> where the collection owns the document.
    /// </para>
    /// </summary>
    public abstract class JsonDataCollection : IJsonDataCollection, IDisposable
    {
        /// <summary>Creates a collection from a sequence of <see cref="JsonNode"/> instances.</summary>
        public static JsonDataCollection Create(IEnumerable<JsonNode> nodes) => new JsonDataJsonNodeCollection(nodes);
        /// <summary>Creates a collection from a <see cref="JsonArray"/>.</summary>
        public static JsonDataCollection Create(JsonArray array) => new JsonDataJsonArrayCollection(array);
        /// <summary>
        /// Creates a collection from a <see cref="JsonDocument"/>.
        /// Set <paramref name="autoDispose"/> to true if this collection should dispose the document when it is disposed.
        /// </summary>
        public static JsonDataCollection Create(JsonDocument document, bool autoDispose=false)=> new JsonDataJsonDocumentCollection(document, autoDispose);
        /// <summary>Creates a collection from a sequence of <see cref="JsonElement"/> instances.</summary>
        public static JsonDataCollection Create(IEnumerable<JsonElement> elements) => new JsonDataJsonElementCollection(elements);
        /// <summary>Creates a collection from the children of a <see cref="JsonElement"/> array.</summary>
        public static JsonDataCollection Create(JsonElement rootElement) => new JsonDataJsonElementChildrenCollection(rootElement);
        /// <summary>Creates a collection by parsing a raw JSON array string. The document is lazily created and disposed with this collection.</summary>
        public static JsonDataCollection Create(string jsonData) => new JsonDataJsonStringCollection(jsonData);

     

        public abstract IEnumerator<JsonData> GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator()=>GetEnumerator();


        private bool disposedValue;

        protected virtual void OnDispose() { }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    OnDispose();
                }

                disposedValue = true;
            }
        }      

        void IDisposable.Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

       

        protected class JsonDataJsonNodeCollection : JsonDataCollection
        {
            public JsonDataJsonNodeCollection(IEnumerable<JsonNode> jsonNodes)
            {
                JsonNodes = jsonNodes;
            } 

            protected IEnumerable<JsonNode> JsonNodes { get; }


            public override IEnumerator<JsonData> GetEnumerator()=>JsonNodes.Select(n=>JsonData.Create(n, false)).GetEnumerator();
            
        }
        
        protected class JsonDataJsonArrayCollection : JsonDataCollection
        {
            public JsonDataJsonArrayCollection(JsonArray array)
            {
                Array = array;
            }

            protected JsonArray Array { get; }

            public override IEnumerator<JsonData> GetEnumerator() => Array.Select(n => JsonData.Create(n, false)).GetEnumerator();

        }


        protected class JsonDataJsonDocumentCollection : JsonDataCollection
        {
            public JsonDataJsonDocumentCollection(JsonDocument document, bool autoDispose=false)
            {
                Document = document;
                AutoDispose = autoDispose;
            }

            protected JsonDocument Document { get; }

            protected bool AutoDispose { get; }

            public override IEnumerator<JsonData> GetEnumerator() => !disposedValue ? Document.RootElement.EnumerateArray().Select(n => JsonData.Create(n)).GetEnumerator() : throw new ObjectDisposedException(nameof(Document));

            protected override void OnDispose()
            {
                if (AutoDispose)
                {
                    Document.Dispose();
                }
                    
            }
        }

        protected class JsonDataJsonElementChildrenCollection : JsonDataCollection
        {
            public JsonDataJsonElementChildrenCollection(JsonElement element)
            {
                Element = element;
            }

            protected JsonElement Element { get; }

            public override IEnumerator<JsonData> GetEnumerator() => Element.EnumerateArray().Select(n => JsonData.Create(n)).GetEnumerator();

        }

        protected class JsonDataJsonElementCollection : JsonDataCollection
        {
            public JsonDataJsonElementCollection(IEnumerable<JsonElement> elements)
            {
                Elements = elements;
            }

            protected IEnumerable<JsonElement> Elements { get; }

            public override IEnumerator<JsonData> GetEnumerator() => Elements.Select(n => JsonData.Create(n)).GetEnumerator();

        }

        protected class JsonDataJsonStringCollection: JsonDataCollection
        {
            public JsonDataJsonStringCollection(string jsonString)
            {
                JsonString = jsonString;
            }

            protected string? JsonString { get; private set; }

            private JsonDocument? _document;
            protected JsonDocument Document { get => _document ??= CreateDocument(); }

            protected JsonDocument CreateDocument()
            {
                var result = JsonDocument.Parse(JsonString ?? string.Empty);
                JsonString = null;
                return result;
            }

            protected override void OnDispose()
            {
                if(_document != null) _document.Dispose();
                base.OnDispose();
            }

            public override IEnumerator<JsonData> GetEnumerator()=> !disposedValue? Document.RootElement.EnumerateArray().Select(n => JsonData.Create(n)).GetEnumerator() : throw new ObjectDisposedException(nameof(Document));
            
        }



    }

    /// <summary>Extension methods for <see cref="IJsonDataCollection"/>.</summary>
    public static class JsonDataCollectionExtensions
    {
        extension(IJsonDataCollection collection)
        {
            /// <summary>Projects the collection as a sequence of typed <see cref="JsonData{T}"/> wrappers.</summary>
            public IEnumerable<JsonData<T>> AsJsonData<T>() where T : IJsonDataKey => collection.Select(d => d.AsJsonData<T>());
        }        
    }
}
