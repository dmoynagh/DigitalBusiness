using DigitalBusiness.JsonDataWrappers.Converters;
using DigitalBusiness.JsonDataWrappers.Internal;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace DigitalBusiness.JsonDataWrappers
{
    public static class JsonDataSerializedExtensions
    {

        extension(in JsonData jsonData)
        {
            //Typed JNode 

            public T Get<T>(JsonSerializerOptions options) => jsonData.TryGet<T>(out var result,options) && result != null ? result : throw JsonDataExceptionHelper.GetTypedValueException<T>(jsonData);
            public T? TryGet<T>(JsonSerializerOptions options) => jsonData.TryGet<T>(out var result,  options) ? result : default;
            public bool TryGet<T>(out T? value, JsonSerializerOptions options)
            {
                if (!jsonData.IsNull)
                {
                    JsonDataSerializedConverter<T>.TryGet(jsonData, out value,options);
                    return true;
                }
                value = default;
                return false;
            }

            public static JsonData Create<T>(T value, JsonSerializerOptions options) => JsonDataSerializedConverter<T>.Create(value, options);



            //Typed JNode object 

            public T Get<T>(string name, JsonSerializerOptions options) => jsonData.Get(name).Get<T>(options);
            public T? TryGet<T>(string name, JsonSerializerOptions options) => jsonData.TryGet<T>(name, out var result, options) ? result : default;
            public bool TryGet<T>(string name, [NotNullWhen(true)] out T? value, JsonSerializerOptions options)
            { 
                if (!jsonData.IsNull && jsonData.TryGet(name, out var propNode))
                {
                    return propNode.TryGet<T>(out value, options);
                }
                value = default;
                return false;
            }

            public void Set<T>(string name, T? value, JsonSerializerOptions options)
            {
                if (value == null) { jsonData.Remove(name); return; }

                var newNode = JsonData.Create<T>(value, options);
                jsonData.Set(name, newNode);
            }

            //public T GetOrCreateValue<T>(string propertyName, T newValue, JsonSerializerOptions options) => jsonData.GetOrCreateValue<T>(propertyName, () => newValue, options);                                 
            //public T GetOrCreateValue<T>(string propertyName, Func<T> valueFactory, JsonSerializerOptions options)
            //{
            //    if (jsonData.TryGet<T>(propertyName, out var result, options))
            //    {
            //        return result;
            //    }
            //    else
            //    {
            //        var newValue = valueFactory();

            //        jsonData.Set<T>(propertyName, newValue,options);
            //        return newValue;
            //    }
            //}



            //Typed JNode array

            public T Get<T>(int index, JsonSerializerOptions options) => jsonData.Get(index).Get<T>(options);
            public T? TryGet<T>(int index, JsonSerializerOptions options) => jsonData.TryGet<T>(index, out var result, options) ? result : default;
            public bool TryGet<T>(int index, [NotNullWhen(true)] out T? value, JsonSerializerOptions options)
            {
                if (!jsonData.IsNull && jsonData.TryGet(index, out var arrayNode))
                {
                    return arrayNode.TryGet<T>(out value,options);
                }
                value = default;
                return false;
            }

            public void Set<T>(int index, T? value, JsonSerializerOptions options)
            {
                if (value == null) { jsonData.RemoveAt(index); return; }

                var newNode = JsonData.Create<T>(value, options);
                jsonData.Set(index, newNode);
            }

            //public T GetOrCreateValue<T>(int index, T value, JsonSerializerOptions options) => jsonData.GetOrCreateValue<T>(index, () => value, options);
            //public T GetOrCreateValue<T>(int index, Func<T> valueFactory, JsonSerializerOptions options)
            //{
            //    if (jsonData.TryGet<T>(index, out var result,options))
            //    {
            //        return result;
            //    }
            //    else
            //    {
            //        var newValue = valueFactory();

            //        jsonData.Set<T>(index, newValue,options);
            //        return newValue;
            //    }
            //}


        }
    }
}
