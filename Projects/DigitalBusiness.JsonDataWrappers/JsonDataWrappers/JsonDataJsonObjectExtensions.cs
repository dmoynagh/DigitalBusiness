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
    public static class JsonDataJsonObjectExtensions
    {
        extension(in JsonData jsonData)
        {

            public bool IsObject => (jsonData.Element.HasValue && jsonData.Element.Value.ValueKind == JsonValueKind.Object) || (jsonData.Node != null && jsonData.Node is JsonObject);

            public void ThrowIfNotObject() { if (!jsonData.IsObject) throw new InvalidOperationException("JsonData is not an object."); }

            //public JsonData RequireObject() => jsonData.IsObject ? jsonData : throw new InvalidOperationException("JsonData is not an object.");
            

            public static JsonData CreateObject() => new JsonData(new JsonObject());

            public JsonData AsObject() => jsonData.IsObject ? jsonData : throw new InvalidOperationException("JsonData is not an object.");
            public JsonData? TryAsObject()=> jsonData.IsObject ? jsonData : default;

            public bool ContainsProperty(string key)
            {
                ArgumentException.ThrowIfNullOrWhiteSpace(key);
                if (jsonData.Element.HasValue)
                {
                    return jsonData.Element!.Value.ValueKind == JsonValueKind.Object && jsonData.Element.Value.TryGetProperty(key, out _);
                }
                else if (jsonData.Node != null && jsonData.Node is JsonObject jsonObject)
                {
                    return jsonObject.ContainsKey(key);
                }
                return false;
            }

            public bool HasProperty(string key)=>jsonData.ContainsProperty(key);

            public bool PropertyHasValue(string key)
            {
                ArgumentException.ThrowIfNullOrWhiteSpace(key);
                if (jsonData.Element.HasValue)
                {
                    return jsonData.Element!.Value.ValueKind == JsonValueKind.Object && jsonData.Element.Value.TryGetProperty(key, out var property) 
                        && (property.ValueKind != JsonValueKind.Null && property.ValueKind != JsonValueKind.Undefined)  ;
                }
                else if (jsonData.Node != null && jsonData.Node is JsonObject jsonObject)
                {
                    return jsonObject.TryGetPropertyValue(key, out var value) && value is not null &&  (value.GetValueKind() != JsonValueKind.Null && value.GetValueKind() != JsonValueKind.Undefined);
                }
                return false;
            }

            public JsonData Get(string name)
            {
                ArgumentException.ThrowIfNullOrWhiteSpace(name);
                if (jsonData.TryGet(name, out var result))
                {
                    return result;
                }
                else
                {
                    if (!jsonData.IsObject) throw JsonDataExceptionHelper.JsonDataObjectExpectedException();
                    
                    throw JsonDataExceptionHelper.JsonDataPropertyNotExist(name);
                }
            }

            public JsonData? TryGet(string key) => jsonData.TryGet(key, out var result) ? result : default;

            public bool TryGet(string key,[MaybeNullWhen(false)] out JsonData value)
            {
                ArgumentException.ThrowIfNullOrWhiteSpace(key);
                if (jsonData.Element.HasValue)
                {
                    if (jsonData.Element!.Value.ValueKind == JsonValueKind.Object 
                        && jsonData.Element.Value.TryGetProperty(key, out JsonElement eResult) && eResult.ValueKind != JsonValueKind.Undefined)
                    {
                        value = new JsonData(eResult);
                        return true;
                    }
                    else
                    {
                        value = default;
                        return false;
                    }
                }
                else if (jsonData.Node != null && jsonData.Node is JsonObject jsonObject 
                    && jsonObject.TryGetPropertyValue(key, out var nResult))
                {
                    value = new JsonData(nResult);
                    return true;
                }
                value = default;
                return false;
            }
            

            public JsonData GetOrCreateObject(string name)
            {
                ArgumentException.ThrowIfNullOrWhiteSpace(name);
                if (jsonData.TryGet(name, out var existingValue))
                {
                    existingValue.ThrowIfNotObject();
                    return existingValue;
                }
                else
                {
                    var newValue = JsonData.CreateObject();
                    jsonData.Set(name, newValue);
                    return newValue;
                }
            }

            public JsonData GetOrCreateArray(string name)
            {
                ArgumentException.ThrowIfNullOrWhiteSpace(name);
                if (jsonData.TryGet(name, out var existingValue))
                {
                    existingValue.ThrowIfNotObject();
                    return existingValue;
                }
                else
                {
                    var newValue = JsonData.CreateArray();
                    jsonData.Set(name, newValue);
                    return newValue;
                }
            }



            public void Set(string key, JsonData? value)
            {               
                jsonData.ThrowIfReadOnly(); 
                jsonData.ThrowIfNotObject();

                var addNode = value.HasValue ? JsonDataHelper.GetNodeToAdd(value.Value, jsonData.Node!) : null;
                if(addNode is not null)
                {
                    jsonData.Node![key] = addNode;
                }
                else
                {
                    jsonData.Remove(key);
                }                
            }
        

            public bool Remove(string key)
            {
                jsonData.ThrowIfReadOnly(); 
                jsonData.ThrowIfNotObject();
                return jsonData.Node!.AsObject().Remove(key);
            }



            public IEnumerable<string> PropertyNames => JsonDataHelper.GetPropertyNames(jsonData);
      
        }

       
    }
}
