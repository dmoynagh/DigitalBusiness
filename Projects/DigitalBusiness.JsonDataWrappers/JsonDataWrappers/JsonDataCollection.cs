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
    public interface IJsonDataCollection : IEnumerable<JsonData>
    {       
    }

    public abstract class JsonDataCollection : IJsonDataCollection, IDisposable
    {
        public static JsonDataCollection Create(IEnumerable<JsonNode> nodes) => new JsonDataJsonNodeCollection(nodes);

        public static JsonDataCollection Create(JsonArray array) => new JsonDataJsonArrayCollection(array);

        public static JsonDataCollection Create(JsonDocument document, bool autoDispose=false)=> new JsonDataJsonDocumentCollection(document, autoDispose);

        public static JsonDataCollection Create(IEnumerable<JsonElement> elements) => new JsonDataJsonElementCollection(elements);

        public static JsonDataCollection Create(JsonElement rootElement) => new JsonDataJsonElementChildrenCollection(rootElement);


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
                    // TODO: dispose managed state (managed objects)
                    OnDispose();
                }

                // TODO: free unmanaged resources (unmanaged objects) and override finalizer
                // TODO: set large fields to null
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


            public override IEnumerator<JsonData> GetEnumerator()=>JsonNodes.Select(n=>JsonData.Create(n)).GetEnumerator();
            
        }
        
        protected class JsonDataJsonArrayCollection : JsonDataCollection
        {
            public JsonDataJsonArrayCollection(JsonArray array)
            {
                Array = array;
            }

            protected JsonArray Array { get; }

            public override IEnumerator<JsonData> GetEnumerator() => Array.Select(n => JsonData.Create(n)).GetEnumerator();

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

    public static class JsonDataCollectionExtensions
    {
        extension(IJsonDataCollection collection)
        {
            public IEnumerable<JsonData<T>> AsJsonData<T>() => collection.Select(d => d.AsJsonData<T>());
        }        
    }
}
