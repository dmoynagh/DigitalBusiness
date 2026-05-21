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
    public static class JsonDataTypedExtensions
    {

        extension(in JsonData jsonData)
        {
            //Typed JNode 

            public T Get<T>() => jsonData.TryGet<T>(out var result) && result != null ? result : throw JsonDataExceptionHelper.GetTypedValueException<T>(jsonData);
            public T? TryGet<T>() => jsonData.TryGet<T>(out var result) ? result : default;
            public bool TryGet<T>([MaybeNullWhen(false)] out T value)
            {
                if (!jsonData.IsNull && JsonDataConverter<T>.TryGet(jsonData, out value)) return true;

                value = default;
                return false;
            }

            public static JsonData Create<T>(T value) => JsonDataConverter<T>.Create(value);



            //Typed JsonData json object property
            public T Get<T>(string name) => jsonData.Get(name).Get<T>();
            public T? TryGet<T>(string name) => jsonData.TryGet<T>(name, out var result) ? result : default;
            public bool TryGet<T>(string name, [MaybeNullWhen(false)] out T? value)
            {
                if (!jsonData.IsNull && jsonData.TryGet(name, out var propertyResult))
                {
                    return propertyResult.TryGet<T>(out value);
                }
                value = default;
                return false;
            }

            public void Set<T>(string name, T? value)
            {
                if (value == null) { jsonData.Remove(name); return; }

                var newNode = JsonData.Create<T>(value);
                jsonData.Set(name, newNode);
            }

          

            public T Ensure<T>(string name, Func<T> defaultValue)
            {
                if (jsonData.TryGet<T>(name, out var result) && result is not null)
                {
                    return result;
                }
                result = defaultValue();
                jsonData.Set(name, result);
                return result;
            }

            public T Ensure<T>(string name, T defaultValue) => jsonData.Ensure<T>(name, () => defaultValue);



            //public T GetOrCreateValue<T>(string propertyName, T newValue)=> jsonData.GetOrCreateValue<T>(propertyName, () => newValue);
            //public T GetOrCreateValue<T>(string propertyName, Func<T> valueFactory)
            //{
            //    if (jsonData.TryGet<T>(propertyName, out var result))
            //    {
            //        return result;
            //    }
            //    else
            //    {
            //        var newValue = valueFactory();

            //        jsonData.Set<T>(propertyName, newValue);
            //        return newValue;
            //    }
            //}



            //Typed JsonData json array 

            public T Get<T>(int index) => jsonData.Get(index).Get<T>();
            public T? TryGet<T>(int index) => jsonData.TryGet<T>(index, out var result) ? result : default;
            public bool TryGet<T>(int index, [MaybeNullWhen(false)] out T? value)
            {
                if (!jsonData.IsNull && jsonData.TryGet(index, out var arrayItemResult))
                {
                    return arrayItemResult.TryGet<T>(out value);
                }
                value = default;
                return false;
            }

            public void Set<T>(int index, T? value)
            {
                if (value == null) { jsonData.RemoveAt(index); return; }

                var newNode = JsonData.Create<T>(value);
                jsonData.Set(index, newNode);
            }

            public void Add<T>(T value)
            {
                var newNode = JsonData.Create<T>(value);
                jsonData.Add(newNode);
            }

            public void Insert<T>(int index, T value)
            {
                var newNode = JsonData.Create<T>(value);
                jsonData.Insert(index, newNode);
            }


        }


    }
}
