using DigitalBusiness.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace DigitalBusiness.JsonDataWrappers.Converters
{
    public static class JsonDataEnumValueConverter<TEnum> where TEnum : struct, Enum
    {
        public static readonly bool PersistAsNumber = EnumJsonPersistanceAttribute.PersistAsNumber<TEnum>();

        public static bool TryToJsonValue(TEnum value,[NotNullWhen(true)] out JsonValue jsonValue)
        {            
            if(PersistAsNumber)
            {
                jsonValue = JsonValue.Create(Convert.ToInt64(value));
                return true;
            }
            else 
            {
                jsonValue = JsonValue.Create(value.ToString());
                return true;
            }
        }


        public static bool FromJsonData(JsonData jsonData, [NotNullWhen(true)] out TEnum value)
        {
            if (!jsonData.IsValue)
            {
                if(jsonData.IsElement) 
                    return FromJson(jsonData.Element!.Value, out value);
                else if(jsonData.IsNode && jsonData.Node is JsonValue jValue) 
                    return FromJson(jValue, out value);               
            }
            value = default;
            return false;
        }

        public static bool FromJson(JsonValue jsonValue, [NotNullWhen(true)] out TEnum value) => jsonValue.TryGetValue<TEnum>(out value);

        public static bool FromJson(JsonElement jsonElement, [NotNullWhen(true)] out TEnum value)
        {
            switch (jsonElement.ValueKind)
            {
                case JsonValueKind.String:
                    return Enum.TryParse<TEnum>(jsonElement.GetString(), out value);                    
                case JsonValueKind.Number:
                    var jValue = jsonElement.GetInt64();
                    value = (TEnum)Enum.ToObject(typeof(TEnum), jValue);    
                    return true;                    
                case JsonValueKind.Null:
                    value = default;
                    return true;
                default:
                    value= default;
                    return false;   
            }
        }       
            



    }
}
