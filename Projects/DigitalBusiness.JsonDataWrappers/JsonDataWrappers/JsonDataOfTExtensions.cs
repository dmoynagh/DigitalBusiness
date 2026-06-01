using System;
using System.Collections.Generic;
using System.Text;

namespace DigitalBusiness.JsonDataWrappers
{
    //public delegate JsonData<T> JsonDataAction<T>(JsonData<T> jsonData) where T : IJsonDataKey;

    public static class JsonDataOfTExtensions
    {

        extension<T>(in JsonData<T> jsonData) where T : IJsonDataKey
        {
            /// <summary>
            /// Performs <paramref name="action"/> on this instance and returns it,
            /// enabling fluent chained set calls.
            /// </summary>
            public JsonData<T> With(Action<JsonData<T>> action)
            {
                action(jsonData);
                return jsonData;
            }

        }
    }
}
