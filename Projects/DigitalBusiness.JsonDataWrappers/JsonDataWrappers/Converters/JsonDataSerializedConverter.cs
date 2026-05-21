using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using System.Text.Json;

namespace DigitalBusiness.JsonDataWrappers.Converters
{
  public static class JsonDataSerializedConverter<T>
    {
        public static bool TryGet(in JsonData jsonData, [MaybeNullWhen(false)] out T value, JsonSerializerOptions options)
        {
            try
            {
                if (!jsonData.IsNull)
                {
                    if (jsonData.Node != null)
                    {
                        value = jsonData.Node.Deserialize<T>(options);
                        return value is not null;
                    }
                    else if (jsonData.Element.HasValue)
                    {
                        var json = jsonData.Element.Value.GetRawText();
                        value = JsonSerializer.Deserialize<T>(json, options);
                        return value is not null;
                    }
                }
            }
            catch
            {
                // Ignore exceptions and fall through to return false
            }
            value = default;
            return false;
        }

        public static JsonData Create(T value, JsonSerializerOptions options)
        {
            var jsonNode = JsonSerializer.SerializeToNode(value, options);            
            return new JsonData(jsonNode);
        }
    }
}
