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
    public static class JsonDataEnumExtensions
    {
        extension(in JsonData jsonData)
        {
            //Enum As String Values

            //Enum
            public TEnum GetEnum<TEnum>() where TEnum : struct, Enum => jsonData.TryGetEnum<TEnum>(out var result) ? result : throw JsonDataExceptionHelper.GetTypedValueException<TEnum>(jsonData);
            public TEnum? TryGetEnum<TEnum>() where TEnum : struct, Enum => jsonData.TryGetEnum<TEnum>(out var result) ? result : default;
            public bool TryGetEnum<TEnum>([NotNullWhen(true)] out TEnum value) where TEnum : struct, Enum
            {
                jsonData.ThrowIfNotValue();
                return JsonDataEnumValueConverter<TEnum>.FromJsonData(jsonData, out value);
            }

            public static JsonData CreateFromEnum<TEnum>(TEnum value) where TEnum : struct, Enum
            {
                if (JsonDataEnumValueConverter<TEnum>.TryToJsonValue(value, out var jsonValue))
                {
                    return new JsonData(jsonValue);
                }
                throw new InvalidOperationException($"Unable to convert enum value '{value}' of type '{typeof(TEnum).FullName}' to a JSON node.");
            }
            public static JsonData CreateFromEnum<TEnum>(TEnum? value) where TEnum : struct, Enum => value.HasValue ? CreateFromEnum(value.Value) : JsonData.CreateNull();
            

            public TEnum GetEnum<TEnum>(string name) where TEnum : struct, Enum => jsonData.Get(name).GetEnum<TEnum>();
            public TEnum? TryGetEnum<TEnum>(string name) where TEnum : struct, Enum => jsonData.TryGetEnum<TEnum>(out var result) ? result : default;
            public  bool TryGetEnum<TEnum>(string name, [NotNullWhen(true)] out TEnum value) where TEnum : struct, Enum
            {
                if (jsonData.TryGet(name, out var childNode) && childNode.TryGetEnum<TEnum>(out value))
                {
                    return true;
                }
                value = default;
                return false;
            }

            public TEnum GetEnum<TEnum>(int index) where TEnum : struct, Enum => jsonData.Get(index).GetEnum<TEnum>();
            public TEnum? TryGetEnum<TEnum>(int index) where TEnum : struct, Enum => jsonData.TryGetEnum<TEnum>(out var result) ? result : default;
            public bool TryGetEnum<TEnum>(int index, [NotNullWhen(true)] out TEnum value) where TEnum : struct, Enum
            {
                if (jsonData.TryGet(index, out var childNode) && childNode.TryGetEnum<TEnum>(out value))
                {
                    return true;
                }
                value = default;
                return false;
            }


            public void SetEnum<TEnum>(string name, TEnum value) where TEnum : struct, Enum => jsonData.Set(name, CreateFromEnum(value));
            public void SetEnum<TEnum>(string name, TEnum? value) where TEnum : struct, Enum => jsonData.Set(name, CreateFromEnum(value));
            public void SetEnum<TEnum>(int index, TEnum value) where TEnum : struct, Enum => jsonData.Set(index, CreateFromEnum(value));
            public void SetEnum<TEnum>(int index, TEnum? value) where TEnum : struct, Enum => jsonData.Set(index, CreateFromEnum(value));

        }


    }
}
