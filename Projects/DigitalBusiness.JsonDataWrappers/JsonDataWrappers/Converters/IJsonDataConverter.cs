using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Text;

namespace DigitalBusiness.JsonDataWrappers.Converters
{

    public interface IJsonDataConverter
    {
        Type Type { get; }
    }

    public interface IJsonDataConverter<T>: IJsonDataConverter
    {
        Type IJsonDataConverter.Type => typeof(T);

        bool TryGet(in JsonData jsonData, [MaybeNullWhen(false)] out T value);
        JsonData Create(T value);                    
      
    }
   

}
