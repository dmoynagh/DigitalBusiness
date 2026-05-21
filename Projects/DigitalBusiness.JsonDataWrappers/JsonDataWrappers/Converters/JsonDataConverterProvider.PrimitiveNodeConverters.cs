using DigitalBusiness.JsonDataWrappers.Converters;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace DigitalBusiness.JsonDataWrappers.Converters
{
	public static partial class JsonDataConverterProvider
	{

		private class StringConverter : IJsonDataConverter<string>
		{
			public bool TryGet(in JsonData jsonData, [MaybeNullWhen(false)] out string value)
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
            public JsonData Create(string value) => new JsonData(JsonValue.Create(value));
        }

		private class BoolConverter : IJsonDataConverter<bool>
		{
			public bool TryGet(in JsonData jsonData, [MaybeNullWhen(false)] out bool value)
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
            public JsonData Create(bool value) => new JsonData(JsonValue.Create(value));
		}

		private class ByteConverter : IJsonDataConverter<byte>
		{
			public bool TryGet(in JsonData jsonData, [MaybeNullWhen(false)] out byte value)
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
            public JsonData Create(byte value) => new JsonData(JsonValue.Create(value));
		}

		private class SByteConverter : IJsonDataConverter<sbyte>
		{
			public bool TryGet(in JsonData jsonData, [MaybeNullWhen(false)] out sbyte value)
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
            public JsonData Create(sbyte value) => new JsonData(JsonValue.Create(value));
		}

		private class ShortConverter : IJsonDataConverter<short>
		{
			public bool TryGet(in JsonData jsonData, [MaybeNullWhen(false)] out short value)
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
            public JsonData Create(short value) => new JsonData(JsonValue.Create(value));
		}

		private class UShortConverter : IJsonDataConverter<ushort>
		{
			public bool TryGet(in JsonData jsonData, [MaybeNullWhen(false)] out ushort value)
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
            public JsonData Create(ushort value) => new JsonData(JsonValue.Create(value));
		}

		private class IntConverter : IJsonDataConverter<int>
		{
			public bool TryGet(in JsonData jsonData, [MaybeNullWhen(false)] out int value)
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
            public JsonData Create(int value) => new JsonData(JsonValue.Create(value));
		}

		private class UIntConverter : IJsonDataConverter<uint>
		{
			public bool TryGet(in JsonData jsonData, [MaybeNullWhen(false)] out uint value)
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
            public JsonData Create(uint value) => new JsonData(JsonValue.Create(value));
		}

		private class LongConverter : IJsonDataConverter<long>
		{
			public bool TryGet(in JsonData jsonData, [MaybeNullWhen(false)] out long value)
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
            public JsonData Create(long value) => new JsonData(JsonValue.Create(value));
		}

		private class ULongConverter : IJsonDataConverter<ulong>
		{
			public bool TryGet(in JsonData jsonData, [MaybeNullWhen(false)] out ulong value)
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
            public JsonData Create(ulong value) => new JsonData(JsonValue.Create(value));
		}

		private class FloatConverter : IJsonDataConverter<float>
		{
			public bool TryGet(in JsonData jsonData, [MaybeNullWhen(false)] out float value)
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
            public JsonData Create(float value) => new JsonData(JsonValue.Create(value));
		}

		private class DoubleConverter : IJsonDataConverter<double>
		{
			public bool TryGet(in JsonData jsonData, [MaybeNullWhen(false)] out double value)
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
            public JsonData Create(double value) => new JsonData(JsonValue.Create(value));
		}

		private class DecimalConverter : IJsonDataConverter<decimal>
		{
			public bool TryGet(in JsonData jsonData, [MaybeNullWhen(false)] out decimal value)
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
            public JsonData Create(decimal value) => new JsonData(JsonValue.Create(value));

		}

		private class DateTimeConverter : IJsonDataConverter<DateTime>
		{
			public bool TryGet(in JsonData jsonData, [MaybeNullWhen(false)] out DateTime value)
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

            public JsonData Create(DateTime value) => new JsonData(JsonValue.Create(value));
		}

		private class DateTimeOffsetConverter : IJsonDataConverter<DateTimeOffset>
		{
			public bool TryGet(in JsonData jsonData, [MaybeNullWhen(false)] out DateTimeOffset value)
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
            public JsonData Create(DateTimeOffset value) => new JsonData(JsonValue.Create(value));
		}

		private class GuidConverter : IJsonDataConverter<Guid>
		{
			public bool TryGet(in JsonData jsonData, [MaybeNullWhen(false)] out Guid value)
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

            public JsonData Create(Guid value) => new JsonData(JsonValue.Create(value));
		}

        private class NullableConverter<T> : IJsonDataConverter<Nullable<T>> where T : struct
        {
            public JsonData Create(T? value)
            {
                if (value == null) return JsonData.CreateNull();
                return JsonDataConverter<T>.Create(value.Value);
            }

            public bool TryGet(in JsonData node, [MaybeNullWhen(false)] out T? value)
            {
                if (node.IsNull)
                {
                    value = null;
                    return true;
                }
                else if (JsonDataConverter<T>.TryGet(node, out var underlyingValue))
                {
                    value = underlyingValue;
                    return true;
                }
                value = null;
                return false;
            }
        }
    }
}
