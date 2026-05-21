using System;
using System.Collections.Generic;
using System.Text;

namespace DigitalBusiness.JsonDataWrappers.Converters
{
    public interface IJsonDataConverterFactory
    {
        bool CanConvert(Type type);
        IJsonDataConverter? CreateConverter(Type typeToConvert);
    }
}
