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
    /// <summary>
    /// Extension methods for reading and writing enum values via <see cref="JsonData"/>.
    /// Enums are stored as strings in JSON. Conversion is handled by <see cref="Converters.JsonDataEnumValueConverter{TEnum}"/>.
    /// </summary>
    public static class JsonDataEnumExtensions
    {
        extension(in JsonData jsonData)
        {
            // Direct value access

            /// <summary>Gets the enum value from this node. Throws if null, not a value, or cannot be converted.</summary>
            public TEnum GetEnum<TEnum>() where TEnum : struct, Enum => jsonData.TryGetEnum<TEnum>(out var result) ? result : throw JsonDataExceptionHelper.GetTypedValueException<TEnum>(jsonData);
            /// <summary>Gets the enum value, or default if null or conversion fails.</summary>
            public TEnum? TryGetEnum<TEnum>() where TEnum : struct, Enum => jsonData.TryGetEnum<TEnum>(out var result) ? result : default;
            /// <summary>Tries to get the enum value. Returns false if not a value or conversion fails.</summary>
            public bool TryGetEnum<TEnum>([NotNullWhen(true)] out TEnum value) where TEnum : struct, Enum
            {
                jsonData.ThrowIfNotValue();
                if (jsonData.IsNode && jsonData.Node is JsonValue jv)
                    return JsonDataEnumValueConverter<TEnum>.FromJson(jv, out value);
                if (jsonData.IsElement && jsonData.Element.HasValue)
                    return JsonDataEnumValueConverter<TEnum>.FromJson(jsonData.Element.Value, out value);
                value = default;
                return false;
            }

            /// <summary>Creates a <see cref="JsonData"/> wrapping an enum value serialized as its JSON string representation.</summary>
            public static JsonData CreateFromEnum<TEnum>(TEnum value) where TEnum : struct, Enum
            {
                if (JsonDataEnumValueConverter<TEnum>.TryToJsonValue(value, out var jsonValue))
                {
                    return new JsonData(jsonValue);
                }
                throw new InvalidOperationException($"Unable to convert enum value '{value}' of type '{typeof(TEnum).FullName}' to a JSON node.");
            }
            /// <summary>Creates a <see cref="JsonData"/> from a nullable enum. Returns <see cref="JsonData.CreateNull"/> if null.</summary>
            public static JsonData CreateFromEnum<TEnum>(TEnum? value) where TEnum : struct, Enum => value.HasValue ? CreateFromEnum(value.Value) : JsonData.CreateNull();
            

            public TEnum GetEnum<TEnum>(string name) where TEnum : struct, Enum => jsonData.Get(name).GetEnum<TEnum>();
            public TEnum? TryGetEnum<TEnum>(string name) where TEnum : struct, Enum
            {
                if (jsonData.TryGet(name, out var child) && child.IsValue)
                {
                    if (child.IsNode && child.Node is JsonValue jv)
                        return JsonDataEnumValueConverter<TEnum>.FromJson(jv, out var r) ? r : default(TEnum?);
                    return JsonDataEnumValueConverter<TEnum>.FromJsonData(child, out var r2) ? r2 : default(TEnum?);
                }
                return default;
            }
            public bool TryGetEnum<TEnum>(string name, [NotNullWhen(true)] out TEnum value) where TEnum : struct, Enum
            {
                if (jsonData.TryGet(name, out var childNode))
                {
                    if (childNode.IsValue)
                    {
                        if (childNode.IsNode && childNode.Node is JsonValue jv)
                            return JsonDataEnumValueConverter<TEnum>.FromJson(jv, out value);
                        return JsonDataEnumValueConverter<TEnum>.FromJsonData(childNode, out value);
                    }
                }
                value = default;
                return false;
            }

            public TEnum GetEnum<TEnum>(int index) where TEnum : struct, Enum => jsonData.Get(index).GetEnum<TEnum>();
            public TEnum? TryGetEnum<TEnum>(int index) where TEnum : struct, Enum
            {
                if (jsonData.TryGet(index, out var child) && child.IsValue)
                {
                    if (child.IsNode && child.Node is JsonValue jv)
                        return JsonDataEnumValueConverter<TEnum>.FromJson(jv, out var r) ? r : default(TEnum?);
                    return JsonDataEnumValueConverter<TEnum>.FromJsonData(child, out var r2) ? r2 : default(TEnum?);
                }
                return default;
            }
            public bool TryGetEnum<TEnum>(int index, [NotNullWhen(true)] out TEnum value) where TEnum : struct, Enum
            {
                if (jsonData.TryGet(index, out var childNode))
                {
                    if (childNode.IsValue)
                    {
                        if (childNode.IsNode && childNode.Node is JsonValue jv)
                            return JsonDataEnumValueConverter<TEnum>.FromJson(jv, out value);
                        return JsonDataEnumValueConverter<TEnum>.FromJsonData(childNode, out value);
                    }
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
