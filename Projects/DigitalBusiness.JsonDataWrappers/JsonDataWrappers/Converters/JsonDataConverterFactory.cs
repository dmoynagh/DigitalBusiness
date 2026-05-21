using System;
using System.Collections.Generic;
using System.Text;

namespace DigitalBusiness.JsonDataWrappers.Converters
{
    public abstract class JsonDataConverterFactory : IJsonDataConverterFactory
    {
        public abstract bool CanConvert(Type type);
        public abstract IJsonDataConverter? CreateConverter(Type typeToConvert);
    }
}
