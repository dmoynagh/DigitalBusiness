using DigitalBusiness.JsonDataWrappers;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Text;

namespace DigitalBusiness.JsonDataWrappers.Converters
{
    public class JsonDataObjectJsonDataConverterFactory : JsonDataConverterFactory
    {
        public override bool CanConvert(Type typeToConvert)
        {
            return typeof(IJsonDataObject).IsAssignableFrom(typeToConvert) && !typeToConvert.IsAbstract;
        }

        public override IJsonDataConverter? CreateConverter(Type typeToConvert)
        {
            IJsonDataConverter converter = (IJsonDataConverter)
                Activator.CreateInstance(
                    typeof(JsonDataObjectJsonDataConverter<>).MakeGenericType(typeToConvert),
                    BindingFlags.Instance | BindingFlags.Public,
                    binder:null,
                    args:null,
                    culture:null)!;

            return converter;
        }


      
    }

    public class JsonDataObjectJsonDataConverter<T> : IJsonDataConverter<T> where T : IJsonDataObject, new()
    {
        public JsonData Create(T value)
        {
            return value.Json;
        }

        public bool TryGet(in JsonData jsonData, [MaybeNullWhen(false)] out T value)
        {
            if (!jsonData.IsNull)
            {
                value = new T() { Json = jsonData };
                return true;
            }
            value = default(T);
            return false;
        }
    }
}
