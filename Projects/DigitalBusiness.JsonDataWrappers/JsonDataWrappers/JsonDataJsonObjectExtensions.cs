using DigitalBusiness.JsonDataWrappers.Internal;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace DigitalBusiness.JsonDataWrappers
{
    public static class JsonDataJsonObjectExtensions
    {
        extension(in JsonData jsonData)
        {
            /// <summary>True if this instance represents a JSON object, regardless of source type.</summary>
            public bool IsObject => (jsonData.Element.HasValue && jsonData.Element.Value.ValueKind == JsonValueKind.Object) || (jsonData.Node != null && jsonData.Node is JsonObject);

            /// <summary>Throws if this instance is not a JSON object.</summary>
            public void ThrowIfNotObject() { if (!jsonData.IsObject) throw new InvalidOperationException("JsonData is not an object."); }

            /// <summary>Creates a new writable Node-backed JSON object instance.</summary>
            public static JsonData CreateObject() => new JsonData(new JsonObject());

            /// <summary>Returns this instance asserted as an object, or throws if it is not.</summary>
            public JsonData AsObject() => jsonData.IsObject ? jsonData : throw new InvalidOperationException("JsonData is not an object.");

            /// <summary>Returns this instance as an object, or null if it is not an object.</summary>
            public JsonData? TryAsObject() => jsonData.IsObject ? jsonData : (JsonData?)null;

            /// <summary>Returns true if the object contains a property with the given key.</summary>
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

            /// <summary>Alias for <see cref="ContainsProperty"/>.</summary>
            public bool HasProperty(string key)=>jsonData.ContainsProperty(key);

            /// <summary>Returns true if the property exists and its value is not null or undefined.</summary>
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

            /// <summary>Gets the named property. Throws if the property does not exist or this is not an object.</summary>
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

            /// <summary>Gets the named property, or null if it does not exist.</summary>
            public JsonData? TryGet(string key) => jsonData.TryGet(key, out var result) ? result : (JsonData?)null;

            /// <summary>Gets the named property. Returns false if missing; child inherits parent readonly state for Node-backed sources.</summary>
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
                    value = new JsonData(nResult, jsonData.ReadOnly);
                    return true;
                }
                value = default;
                return false;
            }
            

            /// <summary>Gets the named property as an object, creating it if absent. Requires a writable instance.</summary>
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

            /// <summary>Gets the named property as an array, creating it if absent. Requires a writable instance.</summary>
            public JsonData GetOrCreateArray(string name)
            {
                ArgumentException.ThrowIfNullOrWhiteSpace(name);
                if (jsonData.TryGet(name, out var existingValue))
                {
                    existingValue.ThrowIfNotArray();
                    return existingValue;
                }
                else
                {
                    var newValue = JsonData.CreateArray();
                    jsonData.Set(name, newValue);
                    return newValue;
                }
            }



            /// <summary>Sets the named property. A null value removes the property. Requires a writable instance.</summary>
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

            /// <summary>Removes the named property. Requires a writable instance.</summary>
            public bool Remove(string key)
            {
                jsonData.ThrowIfReadOnly(); 
                jsonData.ThrowIfNotObject();
                return jsonData.Node!.AsObject().Remove(key);
            }

            /// <summary>Enumerates all property names in this JSON object.</summary>
            public IEnumerable<string> PropertyNames => JsonDataHelper.GetPropertyNames(jsonData);
      
        }

       
    }
}
