using DigitalBusiness.JsonDataWrappers.Converters;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace DigitalBusiness.JsonDataWrappers.Converters
{
    public static partial class JsonDataConverterProvider
    {
        public static IJsonDataConverter<T> GetConverter<T>()
        {

            var converter = GetPrimitiveConverter<T>();
            if (converter is null) converter = GetNullableConverter<T>();

            if (converter is null) converter = GetJsonConverter<T>();
            if (converter is null) converter = GetDefinedConverter<T>();
            if (converter is null) converter = GetEnumConverter<T>();

            if (converter is null) converter = GetJsonDataConverter<T>();

            //if (converter is null) converter = GetTypedJsonDataArrayWrapperConverter<T>();

            if (converter is null) converter = CustomJsonDataConverters.GetConverter<T>();

            //if (converter is null) converter = GetCustomConverter<T>();
            if (converter is null) converter = GetSerializationConverter<T>();
            return converter ?? new UndefinedConverter<T>();
        }

        private static IJsonDataConverter<T>? GetPrimitiveConverter<T>()
        {
            return Type.GetTypeCode(typeof(T)) switch
            {
                TypeCode.String => new StringConverter() as IJsonDataConverter<T>,
                TypeCode.Boolean => new BoolConverter() as IJsonDataConverter<T>,
                TypeCode.Byte => new ByteConverter() as IJsonDataConverter<T>,
                TypeCode.SByte => new SByteConverter() as IJsonDataConverter<T>,
                TypeCode.Int16 => new ShortConverter() as IJsonDataConverter<T>,
                TypeCode.UInt16 => new UShortConverter() as IJsonDataConverter<T>,
                TypeCode.Int32 => new IntConverter() as IJsonDataConverter<T>,
                TypeCode.UInt32 => new UIntConverter() as IJsonDataConverter<T>,
                TypeCode.Int64 => new LongConverter() as IJsonDataConverter<T>,
                TypeCode.UInt64 => new ULongConverter() as IJsonDataConverter<T>,
                TypeCode.Single => new FloatConverter() as IJsonDataConverter<T>,
                TypeCode.Double => new DoubleConverter() as IJsonDataConverter<T>,
                TypeCode.Decimal => new DecimalConverter() as IJsonDataConverter<T>,
                TypeCode.DateTime => new DateTimeConverter() as IJsonDataConverter<T>,
                _ => null
            };
        }

        private static IJsonDataConverter<T>? GetNullableConverter<T>()
        {
            if (Nullable.GetUnderlyingType(typeof(T)) is Type underlyingType && underlyingType != null && underlyingType.IsValueType)
            {
                var converterType = typeof(NullableConverter<>).MakeGenericType(underlyingType);
                return Activator.CreateInstance(converterType) as IJsonDataConverter<T>;               
            }
            return null;
        }

        private static IJsonDataConverter<T>? GetDefinedConverter<T>()
        {
            if (typeof(T) == typeof(Guid))
                return new GuidConverter() as IJsonDataConverter<T>;
            if (typeof(T) == typeof(DateTimeOffset))
                return new DateTimeOffsetConverter() as IJsonDataConverter<T>;
            return null;
        }

      

      


      
    }
}