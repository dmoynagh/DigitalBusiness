using DigitalBusiness.JsonDataWrappers.Converters;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json.Serialization;

namespace DigitalBusiness.JsonDataWrappers
{

    [JsonConverter(typeof(TypedJsonDataJsonConverter<>))]
    public readonly struct JsonData<T> : IJsonDataWrapper
    {
        public JsonData Json { get; init; }

        public static implicit operator JsonData(JsonData<T> json)=> json.Json;
        public static explicit operator JsonData<T>(JsonData json)=> new JsonData<T>() { Json = json };
    }
  
   
}
