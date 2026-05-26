namespace DigitalBusiness.JsonDataWrappers
{
    /// <summary>
    /// Extension methods for casting any <see cref="IJsonData"/> or <see cref="JsonData"/> instance
    /// to a typed <see cref="JsonData{T}"/> wrapper.
    /// </summary>
    public static class TypedJsonDataExtensions
    {
        extension(IJsonDataWrapper wrapper)
        {
            /// <summary>Casts this wrapper's <see cref="JsonData"/> to a typed <see cref="JsonData{T}"/>.</summary>
            public JsonData<T> AsJsonData<T>() where T : IJsonDataKey => new JsonData<T> { Json = wrapper.Json };
        }

        extension(IJsonData jsonData)
        {
            /// <summary>Wraps the underlying <see cref="JsonData"/> as a typed <see cref="JsonData{T}"/>.</summary>
            public JsonData<T> AsJsonData<T>() where T : IJsonDataKey => new JsonData<T> { Json = jsonData.Json };
        }

        extension(in JsonData jsonData)
        {
            /// <summary>Wraps this instance as a typed <see cref="JsonData{T}"/>.</summary>
            public JsonData<T> AsJsonData<T>() where T : IJsonDataKey => new JsonData<T> { Json = jsonData };

            /// <summary>Gets the named property as an object, creating it if absent, then wraps as <see cref="JsonData{T}"/>. Requires a writable instance.</summary>
            public JsonData<T> GetOrCreateJsonData<T>(string property) where T : IJsonDataKey => jsonData.GetOrCreateObject(property).AsJsonData<T>();

            /// <summary>Gets the named property as an array, creating it if absent, then wraps as <see cref="JsonData{T}"/>. Requires a writable instance.</summary>
            public JsonData<T> GetOrCreateJsonDataArray<T>(string property) where T : IJsonDataKey => jsonData.GetOrCreateArray(property).AsJsonData<T>();
        }
    }
}
