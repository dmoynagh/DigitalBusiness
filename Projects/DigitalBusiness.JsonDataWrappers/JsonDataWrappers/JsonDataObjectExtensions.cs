using System;
using System.Collections.Generic;
using System.Text;

namespace DigitalBusiness.JsonDataWrappers
{
    public static class JsonDataObjectExtensions
    {
        extension(IJsonData jsonData)
        {
            public T AsJsonDataObject<T>() where T : IJsonDataWrapper, new() => new T() { Json = jsonData.Json };
        }


    }
}
