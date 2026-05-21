using DigitalBusiness.JsonDataWrappers.Internal;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Xml.Linq;

namespace DigitalBusiness.JsonDataWrappers
{
    public static class JsonDataPrimativeExtensions
    {
        
        extension(JsonData jsonData)
        {
            //String values
            public string? GetString() => jsonData.ThrowIfNotValue() && jsonData.TryGetString(out var value) ? value : default;
            public bool TryGetString([NotNullWhen(true)] out string? value)
            {
                if (jsonData.Element.HasValue)
                {
                    switch (jsonData.Element.Value.ValueKind)
                    {
                        case JsonValueKind.String:
                            value = jsonData.Element.Value.GetString();
                            return value is not null;
                        default:
                            value = default;
                            return false;
                    }
                }
                else if (jsonData.Node != null)
                {
                    if (jsonData.Node is JsonValue jv && jv.TryGetValue<string>(out value))
                    {
                        return true;
                    }
                }
                value = default;
                return false;
            }

            public static JsonData Create(string? value) => new JsonData(JsonValue.Create(value));

            public string GetString(string name) => jsonData.Get(name).GetString() ?? throw new InvalidOperationException($"Property '{name}' is not a string or is null.");
            public string? TryGetString(string name)=>(jsonData.Get(name).TryGetString(out var value)) ? value : default;
            public bool TryGetString(string name, out string? value)
            {
                if (jsonData.TryGet(name, out var propNode) && propNode.TryGetString(out value))
                {
                    return true;
                }
                value = default;
                return false;
            }
          
            public void Set(string name, string? value) => jsonData.Set(name, Create(value));

            public string GetString(int index) => jsonData.Get(index).GetString() ?? throw new InvalidOperationException($"Index '{index}' is not a string or is null.");
            public string? TryGetString(int index) => jsonData.Get(index).GetString();
            public bool TryGetString(int index, out string? value)
            {
                if (jsonData.TryGet(index, out var propNode)  && propNode.TryGetString(out value))
                {
                    return true;
                }
                value = default;
                return false;
            }
            
            public void Set(int index, string? value) => jsonData.Set(index, Create(value));


            //Boolean values
            public bool GetBoolean() => jsonData.ThrowIfNotValue() && jsonData.TryGetBoolean(out var result) ? result : throw JsonDataExceptionHelper.GetNodeValueException<bool>(jsonData);
            public bool? TryGetBoolean() => jsonData.TryGetBoolean(out var result) ? result : default;
            public bool TryGetBoolean(out bool value)
            {
                switch (jsonData.ValueKind)
                {
                    case JsonValueKind.True:
                        value = true;
                        return true;
                    case JsonValueKind.False:
                        value = false;
                        return true;
                    default:
                        value = default;
                        return false;
                }
            }

            public static JsonData Create(bool value) => new JsonData(JsonValue.Create(value));
            public static JsonData Create(bool? value) => new JsonData(JsonValue.Create(value));

            public bool GetBoolean(string name) => jsonData.Get(name).GetBoolean();
            public bool? TryGetBoolean(string name) => jsonData.Get(name).TryGetBoolean();
            public bool TryGetBoolean(string name, out bool value)
            {
                if (jsonData.TryGet(name, out var propNode) && propNode.TryGetBoolean(out value))
                {
                    return true;
                }
                value = default;
                return false;
            }

            public void Set(string name, bool value) => jsonData.Set(name, Create(value));
            public void Set(string name, bool? value) => jsonData.Set(name, Create(value));

            public bool GetBoolean(int index) => jsonData.Get(index).GetBoolean();
            public bool? TryGetBoolean(int index) => jsonData.Get(index).TryGetBoolean();
            public bool TryGetBoolean(int index, out bool value)
            {
                if (jsonData.TryGet(index, out var propNode) &&  propNode.TryGetBoolean(out value))
                {
                    return true;
                }
                value = default;
                return false;
            }

            public void Set(int index, bool value) => jsonData.Set(index, Create(value));
            public void Set(int index, bool? value) => jsonData.Set(index, Create(value));


            //Byte values
            public byte GetByte() => jsonData.ThrowIfNotValue() && jsonData.TryGetByte(out var result) ? result : throw JsonDataExceptionHelper.GetNodeValueException<byte>(jsonData);
            public byte? TryGetByte() => jsonData.TryGetByte(out var result) ? result : default;
            public bool TryGetByte(out byte value)
            {
                if (jsonData.Element.HasValue)
                {
                    return jsonData.Element.Value.TryGetByte(out value);
                }
                else if (jsonData.Node != null && jsonData.Node is JsonValue jv)
                {
                    return jv.TryGetValue<byte>(out value);
                }

                value = default;
                return false;
            }

            public static JsonData Create(byte value) => new JsonData(JsonValue.Create(value));
            public static JsonData Create(byte? value) => new JsonData(JsonValue.Create(value));

            public byte GetByte(string name) => jsonData.Get(name).GetByte();
            public byte? TryGetByte(string name) => jsonData.Get(name).TryGetByte();
            public bool TryGetByte(string name, out byte value)
            {
                if (jsonData.TryGet(name, out var propNode)  && propNode.TryGetByte(out value))
                {
                    return true;
                }
                value = default;
                return false;
            }

            public void Set(string name, byte value) => jsonData.Set(name, Create(value));
            public void Set(string name, byte? value) => jsonData.Set(name, Create(value));

            public byte GetByte(int index) => jsonData.Get(index).GetByte();
            public byte? TryGetByte(int index) => jsonData.Get(index).TryGetByte();
            public bool TryGetByte(int index, out byte value)
            {
                if (jsonData.TryGet(index, out var propNode) &&  propNode.TryGetByte(out value))
                {
                    return true;
                }
                value = default;
                return false;
            }

            public void Set(int index, byte value) => jsonData.Set(index, Create(value));
            public void Set(int index, byte? value) => jsonData.Set(index, Create(value));


            //SByte values
            public sbyte GetSByte() => jsonData.ThrowIfNotValue() && jsonData.TryGetSByte(out var result) ? result : throw JsonDataExceptionHelper.GetNodeValueException<sbyte>(jsonData);
            public sbyte? TryGetSByte() => jsonData.TryGetSByte(out var result) ? result : default;
            public bool TryGetSByte(out sbyte value)
            {
                if (jsonData.Element.HasValue)
                {
                    return jsonData.Element.Value.TryGetSByte(out value);
                }
                else if (jsonData.Node != null && jsonData.Node is JsonValue jv)
                {
                    return jv.TryGetValue<sbyte>(out value);
                }
                value = default;
                return false;
            }

            public static JsonData Create(sbyte value) => new JsonData(JsonValue.Create(value));
            public static JsonData Create(sbyte? value) => new JsonData(JsonValue.Create(value));

            public sbyte GetSByte(string name) => jsonData.Get(name).GetSByte();
            public sbyte? TryGetSByte(string name) => jsonData.Get(name).TryGetSByte();
            public bool TryGetSByte(string name, out sbyte value)
            {
                if (jsonData.TryGet(name, out var propNode) && propNode.TryGetSByte(out value))
                {
                    return true;
                }
                value = default;
                return false;
            }

            public void Set(string name, sbyte value) => jsonData.Set(name, Create(value));
            public void Set(string name, sbyte? value) => jsonData.Set(name, Create(value));

            public sbyte GetSByte(int index) => jsonData.Get(index).GetSByte();
            public sbyte? TryGetSByte(int index) => jsonData.Get(index).TryGetSByte();
            public bool TryGetSByte(int index, out sbyte value)
            {
                if (jsonData.TryGet(index, out var propNode) &&  propNode.TryGetSByte(out value))
                {
                    return true;
                }
                value = default;
                return false;
            }

            public void Set(int index, sbyte value) => jsonData.Set(index, Create(value));
            public void Set(int index, sbyte? value) => jsonData.Set(index, Create(value));


            //Short values
            public short GetShort() => jsonData.ThrowIfNotValue() && jsonData.TryGetShort(out var result) ? result : throw JsonDataExceptionHelper.GetNodeValueException<short>(jsonData);
            public short? TryGetShort() => jsonData.TryGetShort(out var result) ? result : default;
            public bool TryGetShort(out short value)
            {
                if (jsonData.Element.HasValue && jsonData.Element.Value.ValueKind == JsonValueKind.Number)
                {
                    return jsonData.Element.Value.TryGetInt16(out value);
                }
                else if (jsonData.Node != null && jsonData.Node is JsonValue jv)
                {
                    return jv.TryGetValue<short>(out value);
                }
                value = default;
                return false;
            }

            public static JsonData Create(short value) => new JsonData(JsonValue.Create(value));
            public static JsonData Create(short? value) => new JsonData(JsonValue.Create(value));

            public short GetShort(string name) => jsonData.Get(name).GetShort();
            public short? TryGetShort(string name) => jsonData.Get(name).TryGetShort();
            public bool TryGetShort(string name, out short value)
            {
                if (jsonData.TryGet(name, out var propNode) && propNode.TryGetShort(out value))
                {
                    return true;
                }
                value = default;
                return false;
            }

            public void Set(string name, short value) => jsonData.Set(name, Create(value));
            public void Set(string name, short? value) => jsonData.Set(name, Create(value));

            public short GetShort(int index) => jsonData.Get(index).GetShort();
            public short? TryGetShort(int index) => jsonData.Get(index).TryGetShort();
            public bool TryGetShort(int index, out short value)
            {
                if (jsonData.TryGet(index, out var propNode) &&  propNode.TryGetShort(out value))
                {
                    return true;
                }
                value = default;
                return false;
            }

            public void Set(int index, short value) => jsonData.Set(index, Create(value));
            public void Set(int index, short? value) => jsonData.Set(index, Create(value));


            //UShort values
            public ushort GetUShort() => jsonData.ThrowIfNotValue() && jsonData.TryGetUShort(out var result) ? result : throw JsonDataExceptionHelper.GetNodeValueException<ushort>(jsonData);
            public ushort? TryGetUShort() => jsonData.TryGetUShort(out var result) ? result : default;
            public bool TryGetUShort(out ushort value)
            {
                if (jsonData.Element.HasValue)
                {
                    return jsonData.Element.Value.TryGetUInt16(out value);
                }
                else if (jsonData.Node != null && jsonData.Node is JsonValue jv)
                {
                    return jv.TryGetValue<ushort>(out value);
                }
                value = default;
                return false;
            }

            public static JsonData Create(ushort value) => new JsonData(JsonValue.Create(value));
            public static JsonData Create(ushort? value) => new JsonData(JsonValue.Create(value));

            public ushort GetUShort(string name) => jsonData.Get(name).GetUShort();
            public ushort? TryGetUShort(string name) => jsonData.Get(name).TryGetUShort();
            public bool TryGetUShort(string name, out ushort value)
            {
                if (jsonData.TryGet(name, out var propNode) && propNode.TryGetUShort(out value))
                {
                    return true;
                }
                value = default;
                return false;
            }

            public void Set(string name, ushort value) => jsonData.Set(name, Create(value));
            public void Set(string name, ushort? value) => jsonData.Set(name, Create(value));

            public ushort GetUShort(int index) => jsonData.Get(index).GetUShort();
            public ushort? TryGetUShort(int index) => jsonData.Get(index).TryGetUShort();
            public bool TryGetUShort(int index, out ushort value)
            {
                if (jsonData.TryGet(index, out var propNode) &&  propNode.TryGetUShort(out value))
                {
                    return true;
                }
                value = default;
                return false;
            }

            public void Set(int index, ushort value) => jsonData.Set(index, Create(value));
            public void Set(int index, ushort? value) => jsonData.Set(index, Create(value));


            //Integer values
            public int GetInt() => jsonData.ThrowIfNotValue() && jsonData.TryGetInt(out var result) ? result : throw JsonDataExceptionHelper.GetNodeValueException<int>(jsonData);
            public int? TryGetInt() => jsonData.TryGetInt(out var result) ? result : default;
            public bool TryGetInt(out int value)
            {
                if (jsonData.Element.HasValue)
                {
                    return jsonData.Element.Value.TryGetInt32(out value);
                }
                else if (jsonData.Node != null && jsonData.Node is JsonValue jv)
                {
                    return jv.TryGetValue<int>(out value);
                }
                value = default;
                return false;
            }

            public static JsonData Create(int value) => new JsonData(JsonValue.Create(value));
            public static JsonData Create(int? value) => new JsonData(JsonValue.Create(value));

            public int GetInt(string name) => jsonData.Get(name).GetInt();
            public int? TryGetInt(string name) => jsonData.Get(name).TryGetInt();
            public bool TryGetInt(string name, out int value)
            {
                if (jsonData.TryGet(name, out var propNode) && propNode.TryGetInt(out value))
                {
                    return true;
                }
                value = default;
                return false;
            }

            public void Set(string name, int value) => jsonData.Set(name, Create(value));
            public void Set(string name, int? value) => jsonData.Set(name, Create(value));

            public int GetInt(int index) => jsonData.Get(index).GetInt();
            public int? TryGetInt(int index) => jsonData.Get(index).TryGetInt();
            public bool TryGetInt(int index, out int value)
            {
                if (jsonData.TryGet(index, out var propNode) &&  propNode.TryGetInt(out value))
                {
                    return true;
                }
                value = default;
                return false;
            }

            public void Set(int index, int value) => jsonData.Set(index, Create(value));
            public void Set(int index, int? value) => jsonData.Set(index, Create(value));


            //UInt values
            public uint GetUInt() => jsonData.ThrowIfNotValue() && jsonData.TryGetUInt(out var result) ? result : throw JsonDataExceptionHelper.GetNodeValueException<uint>(jsonData);
            public uint? TryGetUInt() => jsonData.TryGetUInt(out var result) ? result : default;
            public bool TryGetUInt(out uint value)
            {
                if (jsonData.Element.HasValue)
                {
                    return jsonData.Element.Value.TryGetUInt32(out value);
                }
                else if (jsonData.Node != null && jsonData.Node is JsonValue jv)
                {
                    return jv.TryGetValue<uint>(out value);
                }
                value = default;
                return false;
            }

            public static JsonData Create(uint value) => new JsonData(JsonValue.Create(value));
            public static JsonData Create(uint? value) => new JsonData(JsonValue.Create(value));

            public uint GetUInt(string name) => jsonData.Get(name).GetUInt();
            public uint? TryGetUInt(string name) => jsonData.Get(name).TryGetUInt();
            public bool TryGetUInt(string name, out uint value)
            {
                if (jsonData.TryGet(name, out var propNode) && propNode.TryGetUInt(out value))
                {
                    return true;
                }
                value = default;
                return false;
            }

            public void Set(string name, uint value) => jsonData.Set(name, Create(value));
            public void Set(string name, uint? value) => jsonData.Set(name, Create(value));

            public uint GetUInt(int index) => jsonData.Get(index).GetUInt();
            public uint? TryGetUInt(int index) => jsonData.Get(index).TryGetUInt();
            public bool TryGetUInt(int index, out uint value)
            {
                if (jsonData.TryGet(index, out var propNode) &&  propNode.TryGetUInt(out value))
                {
                    return true;
                }
                value = default;
                return false;
            }

            public void Set(int index, uint value) => jsonData.Set(index, Create(value));
            public void Set(int index, uint? value) => jsonData.Set(index, Create(value));


            //Long values
            public long GetLong() => jsonData.ThrowIfNotValue() && jsonData.TryGetLong(out var result) ? result : throw JsonDataExceptionHelper.GetNodeValueException<long>(jsonData);
            public long? TryGetLong() => jsonData.TryGetLong(out var result) ? result : default;
            public bool TryGetLong(out long value)
            {
                if (jsonData.Element.HasValue)
                {
                    return jsonData.Element.Value.TryGetInt64(out value);
                }
                else if (jsonData.Node != null && jsonData.Node is JsonValue jv)
                {
                    return jv.TryGetValue<long>(out value);
                }
                value = default;
                return false;
            }

            public static JsonData Create(long value) => new JsonData(JsonValue.Create(value));
            public static JsonData Create(long? value) => new JsonData(JsonValue.Create(value));

            public long GetLong(string name) => jsonData.Get(name).GetLong();
            public long? TryGetLong(string name) => jsonData.Get(name).TryGetLong();
            public bool TryGetLong(string name, out long value)
            {
                if (jsonData.TryGet(name, out var propNode) && propNode.TryGetLong(out value))
                {
                    return true;
                }
                value = default;
                return false;
            }

            public void Set(string name, long value) => jsonData.Set(name, Create(value));
            public void Set(string name, long? value) => jsonData.Set(name, Create(value));

            public long GetLong(int index) => jsonData.Get(index).GetLong();
            public long? TryGetLong(int index) => jsonData.Get(index).TryGetLong();
            public bool TryGetLong(int index, out long value)
            {
                if (jsonData.TryGet(index, out var propNode) &&  propNode.TryGetLong(out value))
                {
                    return true;
                }
                value = default;
                return false;
            }

            public void Set(int index, long value) => jsonData.Set(index, Create(value));
            public void Set(int index, long? value) => jsonData.Set(index, Create(value));


            //ULong values
            public ulong GetULong() => jsonData.ThrowIfNotValue() && jsonData.TryGetULong(out var result) ? result : throw JsonDataExceptionHelper.GetNodeValueException<ulong>(jsonData);
            public ulong? TryGetULong() => jsonData.TryGetULong(out var result) ? result : default;
            public bool TryGetULong(out ulong value)
            {
                if (jsonData.Element.HasValue)
                {
                    return jsonData.Element.Value.TryGetUInt64(out value);
                }
                else if (jsonData.Node != null && jsonData.Node is JsonValue jv)
                {
                    return jv.TryGetValue<ulong>(out value);
                }
                value = default;
                return false;
            }

            public static JsonData Create(ulong value) => new JsonData(JsonValue.Create(value));
            public static JsonData Create(ulong? value) => new JsonData(JsonValue.Create(value));

            public ulong GetULong(string name) => jsonData.Get(name).GetULong();
            public ulong? TryGetULong(string name) => jsonData.Get(name).TryGetULong();
            public bool TryGetULong(string name, out ulong value)
            {
                if (jsonData.TryGet(name, out var propNode) && propNode.TryGetULong(out value))
                {
                    return true;
                }
                value = default;
                return false;
            }

            public void Set(string name, ulong value) => jsonData.Set(name, Create(value));
            public void Set(string name, ulong? value) => jsonData.Set(name, Create(value));

            public ulong GetULong(int index) => jsonData.Get(index).GetULong();
            public ulong? TryGetULong(int index) => jsonData.Get(index).TryGetULong();
            public bool TryGetULong(int index, out ulong value)
            {
                if (jsonData.TryGet(index, out var propNode) &&  propNode.TryGetULong(out value))
                {
                    return true;
                }
                value = default;
                return false;
            }

            public void Set(int index, ulong value) => jsonData.Set(index, Create(value));
            public void Set(int index, ulong? value) => jsonData.Set(index, Create(value));


            //Float values
            public float GetFloat() => jsonData.ThrowIfNotValue() && jsonData.TryGetFloat(out var result) ? result : throw JsonDataExceptionHelper.GetNodeValueException<float>(jsonData);
            public float? TryGetFloat() => jsonData.TryGetFloat(out var result) ? result : default;
            public bool TryGetFloat(out float value)
            {
                if (jsonData.Element.HasValue)
                {
                    return jsonData.Element.Value.TryGetSingle(out value);
                }
                else if (jsonData.Node != null && jsonData.Node is JsonValue jv)
                {
                    return jv.TryGetValue<float>(out value);
                }
                value = default;
                return false;
            }

            public static JsonData Create(float value) => new JsonData(JsonValue.Create(value));
            public static JsonData Create(float? value) => new JsonData(JsonValue.Create(value));

            public float GetFloat(string name) => jsonData.Get(name).GetFloat();
            public float? TryGetFloat(string name) => jsonData.Get(name).TryGetFloat();
            public bool TryGetFloat(string name, out float value)
            {
                if (jsonData.TryGet(name, out var propNode) && propNode.TryGetFloat(out value))
                {
                    return true;
                }
                value = default;
                return false;
            }

            public void Set(string name, float value) => jsonData.Set(name, Create(value));
            public void Set(string name, float? value) => jsonData.Set(name, Create(value));

            public float GetFloat(int index) => jsonData.Get(index).GetFloat();
            public float? TryGetFloat(int index) => jsonData.Get(index).TryGetFloat();
            public bool TryGetFloat(int index, out float value)
            {
                if (jsonData.TryGet(index, out var propNode) &&  propNode.TryGetFloat(out value))
                {
                    return true;
                }
                value = default;
                return false;
            }

            public void Set(int index, float value) => jsonData.Set(index, Create(value));
            public void Set(int index, float? value) => jsonData.Set(index, Create(value));


            //Double values
            public double GetDouble() => jsonData.ThrowIfNotValue() && jsonData.TryGetDouble(out var result) ? result : throw JsonDataExceptionHelper.GetNodeValueException<double>(jsonData);
            public double? TryGetDouble() => jsonData.TryGetDouble(out var result) ? result : default;
            public bool TryGetDouble(out double value)
            {
                if (jsonData.Element.HasValue)
                {
                    return jsonData.Element.Value.TryGetDouble(out value);
                }
                else if (jsonData.Node != null && jsonData.Node is JsonValue jv)
                {
                    return jv.TryGetValue<double>(out value);
                }
                value = default;
                return false;
            }

            public static JsonData Create(double value) => new JsonData(JsonValue.Create(value));
            public static JsonData Create(double? value) => new JsonData(JsonValue.Create(value));

            public double GetDouble(string name) => jsonData.Get(name).GetDouble();
            public double? TryGetDouble(string name) => jsonData.Get(name).TryGetDouble();
            public bool TryGetDouble(string name, out double value)
            {
                if (jsonData.TryGet(name, out var propNode) && propNode.TryGetDouble(out value))
                {
                    return true;
                }
                value = default;
                return false;
            }

            public void Set(string name, double value) => jsonData.Set(name, Create(value));
            public void Set(string name, double? value) => jsonData.Set(name, Create(value));

            public double GetDouble(int index) => jsonData.Get(index).GetDouble();
            public double? TryGetDouble(int index) => jsonData.Get(index).TryGetDouble();
            public bool TryGetDouble(int index, out double value)
            {
                if (jsonData.TryGet(index, out var propNode) &&  propNode.TryGetDouble(out value))
                {
                    return true;
                }
                value = default;
                return false;
            }

            public void Set(int index, double value) => jsonData.Set(index, Create(value));
            public void Set(int index, double? value) => jsonData.Set(index, Create(value));


            //Decimal values
            public decimal GetDecimal() => jsonData.ThrowIfNotValue() && jsonData.TryGetDecimal(out var result) ? result : throw JsonDataExceptionHelper.GetNodeValueException<decimal>(jsonData);
            public decimal? TryGetDecimal() => jsonData.TryGetDecimal(out var result) ? result : default;
            public bool TryGetDecimal(out decimal value)
            {
                if (jsonData.Element.HasValue)
                {
                    return jsonData.Element.Value.TryGetDecimal(out value);
                }
                else if (jsonData.Node != null && jsonData.Node is JsonValue jv)
                {
                    return jv.TryGetValue<decimal>(out value);
                }
                value = default;
                return false;
            }

            public static JsonData Create(decimal value) => new JsonData(JsonValue.Create(value));
            public static JsonData Create(decimal? value) => new JsonData(JsonValue.Create(value));

            public decimal GetDecimal(string name) => jsonData.Get(name).GetDecimal();
            public decimal? TryGetDecimal(string name) => jsonData.Get(name).TryGetDecimal();
            public bool TryGetDecimal(string name, out decimal value)
            {
                if (jsonData.TryGet(name, out var propNode) && propNode.TryGetDecimal(out value))
                {
                    return true;
                }
                value = default;
                return false;
            }

            public void Set(string name, decimal value) => jsonData.Set(name, Create(value));
            public void Set(string name, decimal? value) => jsonData.Set(name, Create(value));

            public decimal GetDecimal(int index) => jsonData.Get(index).GetDecimal();
            public decimal? TryGetDecimal(int index) => jsonData.Get(index).TryGetDecimal();
            public bool TryGetDecimal(int index, out decimal value)
            {
                if (jsonData.TryGet(index, out var propNode) &&  propNode.TryGetDecimal(out value))
                {
                    return true;
                }
                value = default;
                return false;
            }

            public void Set(int index, decimal value) => jsonData.Set(index, Create(value));
            public void Set(int index, decimal? value) => jsonData.Set(index, Create(value));


            //DateTime values
            public DateTime GetDateTime() => jsonData.ThrowIfNotValue() && jsonData.TryGetDateTime(out var result) ? result : throw JsonDataExceptionHelper.GetNodeValueException<DateTime>(jsonData);
            public DateTime? TryGetDateTime() => jsonData.TryGetDateTime(out var result) ? result : default;
            public bool TryGetDateTime(out DateTime value)
            {
                if (jsonData.Element.HasValue && jsonData.Element.Value.ValueKind == JsonValueKind.String)
                {
                    return jsonData.Element.Value.TryGetDateTime(out value);
                }
                else if (jsonData.Node != null && jsonData.Node is JsonValue jv)
                {
                    return jv.TryGetValue<DateTime>(out value);
                }
                value = default;
                return false;
            }

            public static JsonData Create(DateTime value) => new JsonData(JsonValue.Create(value));
            public static JsonData Create(DateTime? value) => new JsonData(JsonValue.Create(value));

            public DateTime GetDateTime(string name) => jsonData.Get(name).GetDateTime();
            public DateTime? TryGetDateTime(string name) => jsonData.Get(name).TryGetDateTime();
            public bool TryGetDateTime(string name, out DateTime value)
            {
                if (jsonData.TryGet(name, out var propNode) && propNode.TryGetDateTime(out value))
                {
                    return true;
                }
                value = default;
                return false;
            }

            public void Set(string name, DateTime value) => jsonData.Set(name, Create(value));
            public void Set(string name, DateTime? value) => jsonData.Set(name, Create(value));

            public DateTime GetDateTime(int index) => jsonData.Get(index).GetDateTime();
            public DateTime? TryGetDateTime(int index) => jsonData.Get(index).TryGetDateTime();
            public bool TryGetDateTime(int index, out DateTime value)
            {
                if (jsonData.TryGet(index, out var propNode) &&  propNode.TryGetDateTime(out value))
                {
                    return true;
                }
                value = default;
                return false;
            }

            public void Set(int index, DateTime value) => jsonData.Set(index, Create(value));
            public void Set(int index, DateTime? value) => jsonData.Set(index, Create(value));


            //DateTimeOffset values
            public DateTimeOffset GetDateTimeOffset() => jsonData.ThrowIfNotValue() && jsonData.TryGetDateTimeOffset(out var result) ? result : throw JsonDataExceptionHelper.GetNodeValueException<DateTimeOffset>(jsonData);
            public DateTimeOffset? TryGetDateTimeOffset() => jsonData.TryGetDateTimeOffset(out var result) ? result : default;
            public bool TryGetDateTimeOffset(out DateTimeOffset value)
            {
                if (jsonData.Element.HasValue && jsonData.Element.Value.ValueKind == JsonValueKind.String)
                {
                    return jsonData.Element.Value.TryGetDateTimeOffset(out value);
                }
                else if (jsonData.Node != null && jsonData.Node is JsonValue jv)
                {
                    return jv.TryGetValue<DateTimeOffset>(out value);
                }
                value = default;
                return false;
            }

            public static JsonData Create(DateTimeOffset value) => new JsonData(JsonValue.Create(value));
            public static JsonData Create(DateTimeOffset? value) => new JsonData(JsonValue.Create(value));

            public DateTimeOffset GetDateTimeOffset(string name) => jsonData.Get(name).GetDateTimeOffset();
            public DateTimeOffset? TryGetDateTimeOffset(string name) => jsonData.Get(name).TryGetDateTimeOffset();
            public bool TryGetDateTimeOffset(string name, out DateTimeOffset value)
            {
                if (jsonData.TryGet(name, out var propNode) && propNode.TryGetDateTimeOffset(out value))
                {
                    return true;
                }
                value = default;
                return false;
            }

            public void Set(string name, DateTimeOffset value) => jsonData.Set(name, Create(value));
            public void Set(string name, DateTimeOffset? value) => jsonData.Set(name, Create(value));

            public DateTimeOffset GetDateTimeOffset(int index) => jsonData.Get(index).GetDateTimeOffset();
            public DateTimeOffset? TryGetDateTimeOffset(int index) => jsonData.Get(index).TryGetDateTimeOffset();
            public bool TryGetDateTimeOffset(int index, out DateTimeOffset value)
            {
                if (jsonData.TryGet(index, out var propNode) &&  propNode.TryGetDateTimeOffset(out value))
                {
                    return true;
                }
                value = default;
                return false;
            }

            public void Set(int index, DateTimeOffset value) => jsonData.Set(index, Create(value));
            public void Set(int index, DateTimeOffset? value) => jsonData.Set(index, Create(value));


            //Guid values
            public Guid GetGuid() => jsonData.ThrowIfNotValue() && jsonData.TryGetGuid(out var result) ? result : throw JsonDataExceptionHelper.GetNodeValueException<Guid>(jsonData);
            public Guid? TryGetGuid() => jsonData.TryGetGuid(out var result) ? result : default;
            public bool TryGetGuid(out Guid value)
            {
                if (jsonData.Element.HasValue && jsonData.Element.Value.ValueKind == JsonValueKind.String)
                {
                    return jsonData.Element.Value.TryGetGuid(out value);
                }
                else if (jsonData.Node != null && jsonData.Node is JsonValue jv)
                {
                    return jv.TryGetValue<Guid>(out value);
                }
                value = default;
                return false;
            }

            public static JsonData Create(Guid value) => new JsonData(JsonValue.Create(value));
            public static JsonData Create(Guid? value) => new JsonData(JsonValue.Create(value));

            public Guid GetGuid(string name) => jsonData.Get(name).GetGuid();
            public Guid? TryGetGuid(string name) => jsonData.Get(name).TryGetGuid();
            public bool TryGetGuid(string name, out Guid value)
            {
                if (jsonData.TryGet(name, out var propNode) && propNode.TryGetGuid(out value))
                {
                    return true;
                }
                value = default;
                return false;
            }

            public void Set(string name, Guid value) => jsonData.Set(name, Create(value));
            public void Set(string name, Guid? value) => jsonData.Set(name, Create(value));

            public Guid GetGuid(int index) => jsonData.Get(index).GetGuid();
            public Guid? TryGetGuid(int index) => jsonData.Get(index).TryGetGuid();
            public bool TryGetGuid(int index, out Guid value)
            {
                if (jsonData.TryGet(index, out var propNode) &&  propNode.TryGetGuid(out value))
                {
                    return true;
                }
                value = default;
                return false;
            }

            public void Set(int index, Guid value) => jsonData.Set(index, Create(value));
            public void Set(int index, Guid? value) => jsonData.Set(index, Create(value));  
        }
    }
}
