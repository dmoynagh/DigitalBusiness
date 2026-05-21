using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace DigitalBusiness.JsonDataWrappers
{
    public readonly partial struct JsonData
    {
     
        public static implicit operator JsonData(string? value) => new JsonData(JsonValue.Create(value));
        public static explicit operator string?(in JsonData jsonData) => jsonData.Get<string>();

        public static implicit operator JsonData(bool value) => new JsonData(JsonValue.Create(value));
        public static implicit operator JsonData(bool? value) => new JsonData(JsonValue.Create(value));
        public static explicit operator bool(in JsonData jsonData) => jsonData.Get<bool>();
        public static explicit operator bool?(in JsonData jsonData) => jsonData.TryGet<bool>();


        public static implicit operator JsonData(byte value) => new JsonData(JsonValue.Create(value));
        public static implicit operator JsonData(byte? value) => new JsonData(JsonValue.Create(value));
        public static explicit operator byte(in JsonData jsonData) => jsonData.Get<byte>();        
        public static explicit operator byte?(in JsonData jsonData) => jsonData.TryGet<byte>();

        public static implicit operator JsonData(sbyte value) => new JsonData(JsonValue.Create(value));
        public static implicit operator JsonData(sbyte? value) => new JsonData(JsonValue.Create(value));
        public static explicit operator sbyte(in JsonData jsonData) => jsonData.Get<sbyte>();
        public static explicit operator sbyte?(in JsonData jsonData) => jsonData.TryGet<sbyte>();

        public static implicit operator JsonData(short value) => new JsonData(JsonValue.Create(value));
        public static implicit operator JsonData(short? value) => new JsonData(JsonValue.Create(value));        
        public static explicit operator short(in JsonData jsonData) => jsonData.Get<short>();
        public static explicit operator short?(in JsonData jsonData) => jsonData.TryGet<short>();

        public static implicit operator JsonData(int value) => new JsonData(JsonValue.Create(value));
        public static implicit operator JsonData(int? value) => new JsonData(JsonValue.Create(value) );
        public static explicit operator int(in JsonData jsonData) => jsonData.Get<int>();
        public static explicit operator int?(in JsonData jsonData) => jsonData.TryGet<int>();

        public static implicit operator JsonData(long value) => new JsonData(JsonValue.Create(value));
        public static implicit operator JsonData(long? value) => new JsonData(JsonValue.Create(value));
        public static explicit operator long(in JsonData jsonData) => jsonData.Get<long>();
        public static explicit operator long?(in JsonData jsonData) => jsonData.TryGet<long>();

        public static implicit operator JsonData(ushort value) => new JsonData(JsonValue.Create(value));
        public static implicit operator JsonData(ushort? value) => new JsonData(JsonValue.Create(value));
        public static explicit operator ushort(in JsonData jsonData) => jsonData.Get<ushort>();
        public static explicit operator ushort?(in JsonData jsonData) => jsonData.TryGet<ushort>();

        public static implicit operator JsonData(uint value) => new JsonData(JsonValue.Create(value));
        public static implicit operator JsonData(uint? value) => new JsonData(JsonValue.Create(value));
        public static explicit operator uint(in JsonData jsonData) => jsonData.Get<uint>();
        public static explicit operator uint?(in JsonData jsonData) => jsonData.TryGet<uint>();

        public static implicit operator JsonData(ulong value) => new JsonData(JsonValue.Create(value));
        public static implicit operator JsonData(ulong? value) => new JsonData(JsonValue.Create(value));
        public static explicit operator ulong(in JsonData jsonData) => jsonData.Get<ulong>();
        public static explicit operator ulong?(in JsonData jsonData) => jsonData.TryGet<ulong>();

        public static implicit operator JsonData(double value) => new JsonData(JsonValue.Create(value));
        public static implicit operator JsonData(double? value) => new JsonData(JsonValue.Create(value));
        public static explicit operator double(in JsonData jsonData) => jsonData.Get<double>();
        public static explicit operator double?(in JsonData jsonData) => jsonData.TryGet<double>();

        public static implicit operator JsonData(float value) => new JsonData(JsonValue.Create(value));
        public static implicit operator JsonData(float? value) => new JsonData(JsonValue.Create(value));
        public static explicit operator float(in JsonData jsonData) => jsonData.Get<float>();
        public static explicit operator float?(in JsonData jsonData) => jsonData.TryGet<float>();

        public static implicit operator JsonData(decimal value) => new JsonData(JsonValue.Create(value));
        public static implicit operator JsonData(decimal? value) => new JsonData(JsonValue.Create(value));
        public static explicit operator decimal(in JsonData jsonData) => jsonData.Get<decimal>();
        public static explicit operator decimal?(in JsonData jsonData) => jsonData.TryGet<decimal>();

        public static implicit operator JsonData(DateTime value) => new JsonData(JsonValue.Create(value));
        public static implicit operator JsonData(DateTime? value) => new JsonData(JsonValue.Create(value));
        public static explicit operator DateTime(in JsonData jsonData) => jsonData.Get<DateTime>();
        public static explicit operator DateTime?(in JsonData jsonData) => jsonData.TryGet<DateTime>();

        public static implicit operator JsonData(DateTimeOffset value) => new JsonData(JsonValue.Create(value));
        public static implicit operator JsonData(DateTimeOffset? value) => new JsonData(JsonValue.Create(value));
        public static explicit operator DateTimeOffset(in JsonData jsonData) => jsonData.Get<DateTimeOffset>();
        public static explicit operator DateTimeOffset?(in JsonData jsonData) => jsonData.TryGet<DateTimeOffset>();

        public static implicit operator JsonData(Guid value) => new JsonData(JsonValue.Create(value));
        public static implicit operator JsonData(Guid? value) => new JsonData(JsonValue.Create(value));
        public static explicit operator Guid(in JsonData jsonData) => jsonData.Get<Guid>();
        public static explicit operator Guid?(in JsonData jsonData) => jsonData.TryGet<Guid>();            

    }
}
