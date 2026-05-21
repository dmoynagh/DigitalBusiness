using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace DigitalBusiness.JsonDataWrappers
{    
    public abstract class JsonDataObject : IJsonDataObject
    {
        protected virtual JsonData Json { get; init; }
        JsonData IJsonDataWrapper.Json { get => Json; init => Json = value; }
        JsonData IJsonData.Json => Json;
    }
  

  

}
