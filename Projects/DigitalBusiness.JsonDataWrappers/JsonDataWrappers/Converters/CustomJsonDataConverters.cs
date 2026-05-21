using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Text;

namespace DigitalBusiness.JsonDataWrappers.Converters
{
    public static class CustomJsonDataConverters
    {
        static CustomJsonDataConverters()
        {
            var converters = LoadConverters();
            JsonDataConverters = converters.JsonDataConverters;
            JsonDataConverterFactories = converters.JsonDataConverterFactories;
            
        }

        private static readonly ImmutableDictionary<Type, IJsonDataConverter> JsonDataConverters;
        private static readonly ImmutableList<IJsonDataConverterFactory> JsonDataConverterFactories;

        private static (ImmutableDictionary<Type, IJsonDataConverter> JsonDataConverters, ImmutableList<IJsonDataConverterFactory> JsonDataConverterFactories) LoadConverters()
        {
            Dictionary<Type, IJsonDataConverter> converters = new Dictionary<Type, IJsonDataConverter>();
            var factories = new List<IJsonDataConverterFactory>();

            var converterFactoryType = typeof(IJsonDataConverterFactory);

            var assemblies = AppDomain.CurrentDomain.GetAssemblies();

            foreach (var assembly in assemblies)
            {
                foreach (var type in assembly.GetExportedTypes())
                {
                    if (!type.IsValueType && !type.IsInterface && !type.IsAbstract && typeof(IJsonDataConverter).IsAssignableFrom(type))
                    {

                        var converterInterface = type.GetInterfaces()
                            .FirstOrDefault(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IJsonDataConverter<>));

                        if (converterInterface != null)
                        {
                            var genericArgs = converterInterface.GetGenericArguments();
                            if (genericArgs.Length == 1)
                            {
                                var converterType = genericArgs[0];
                                var converterInstance = Activator.CreateInstance(type) as IJsonDataConverter;
                                if (converterInstance != null)
                                    converters.Add(converterType, converterInstance);
                            }
                        }
                    }
                    else if (!type.IsValueType && !type.IsInterface && !type.IsAbstract && converterFactoryType.IsAssignableFrom(type))
                    {
                        var factoryInstance = Activator.CreateInstance(type) as IJsonDataConverterFactory;
                        if (factoryInstance != null)
                            factories.Add(factoryInstance);
                    }
                }
            }

            return (converters.ToImmutableDictionary(), factories.ToImmutableList());
        }
        
        public static IJsonDataConverter<T>? GetConverter<T>()
        {
            if (JsonDataConverters.TryGetValue(typeof(T), out var converter))
            {
                return converter as IJsonDataConverter<T>;
            }
            else
            {
                var typeToConvert = typeof(T);

                foreach (var factory in JsonDataConverterFactories)
                {
                    if (factory.CanConvert(typeToConvert))
                    {
                        var factoryConverter = factory.CreateConverter(typeToConvert);
                        if (factoryConverter is IJsonDataConverter<T> typedConverter)
                        {
                            return typedConverter;
                        }
                    }                    
                }
            }
            return null;
        }
      

    }
}
