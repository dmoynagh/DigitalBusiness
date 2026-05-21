using System;
using System.Collections.Generic;
using System.Text;

namespace DigitalBusiness.JsonDataWrappers
{
    public static class TypedJsonDataExtensions
    {

        extension(IJsonDataWrapper wrapper)
        {
            public JsonData<T> AsJsonData<T>() => new JsonData<T> { Json = wrapper.Json };

           // public JsonData AsJsonData() => wrapper.Json;
        }

        extension(IJsonData jsonData)
        {
            public JsonData<T> AsJsonData<T>() => new JsonData<T> { Json = jsonData.Json };
        }

        extension(in JsonData jsonData)
        {
            public JsonData<T> AsJsonData<T>() => new JsonData<T> { Json = jsonData };

            public JsonData<T> GetOrCreateJsonData<T>(string property) => jsonData.GetOrCreateObject(property).AsJsonData<T>();   
                
            public JsonData<T> GetOrCreateJsonDataArray<T>(string property)=> jsonData.GetOrCreateArray(property).AsJsonData<T>();          
        }

        //extension<T>(T typedJsonDataKey) where T : IJsonDataKey
        //{
        //    public static JsonData<T> Create() => new JsonData<T> { Json = JsonData.CreateObject() };
        //}
    }
}
