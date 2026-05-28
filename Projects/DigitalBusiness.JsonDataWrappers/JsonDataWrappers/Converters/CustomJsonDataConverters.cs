using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Reflection;

namespace DigitalBusiness.JsonDataWrappers.Converters
{
    public static class CustomJsonDataConverters
    {
        private static readonly ConcurrentDictionary<Type, IJsonDataConverter> _converters = new();
        private static readonly List<IJsonDataConverterFactory> _factories = new();
        private static readonly object _lock = new();
        private static readonly HashSet<Assembly> _scannedAssemblies = new();

        static CustomJsonDataConverters()
        {
            ScanAssemblies(AppDomain.CurrentDomain.GetAssemblies());
            AppDomain.CurrentDomain.AssemblyLoad += (_, args) => ScanAssemblies([args.LoadedAssembly]);
        }

        private static void ScanAssemblies(IEnumerable<Assembly> assemblies)
        {
            var converterFactoryType = typeof(IJsonDataConverterFactory);

            lock (_lock)
            {
                foreach (var assembly in assemblies)
                {
                    if (!_scannedAssemblies.Add(assembly))
                        continue;

                    foreach (var type in assembly.GetExportedTypes())
                    {
                        if (type.IsValueType || type.IsInterface || type.IsAbstract || type.ContainsGenericParameters)
                            continue;

                        if (typeof(IJsonDataConverter).IsAssignableFrom(type))
                        {
                            var converterInterface = type.GetInterfaces()
                                .FirstOrDefault(i => i.IsGenericType &&
                                    i.GetGenericTypeDefinition() == typeof(IJsonDataConverter<>));

                            if (converterInterface?.GetGenericArguments() is [var converterTargetType])
                            {
                                if (Activator.CreateInstance(type) is IJsonDataConverter instance)
                                    _converters.TryAdd(converterTargetType, instance);
                            }
                        }
                        else if (converterFactoryType.IsAssignableFrom(type))
                        {
                            if (Activator.CreateInstance(type) is IJsonDataConverterFactory factory)
                                _factories.Add(factory);
                        }
                    }
                }
            }
        }

        public static IJsonDataConverter<T>? GetConverter<T>()
        {
            if (_converters.TryGetValue(typeof(T), out var converter))
                return converter as IJsonDataConverter<T>;

            var typeToConvert = typeof(T);

            lock (_lock)
            {
                foreach (var factory in _factories)
                {
                    if (factory.CanConvert(typeToConvert) &&
                        factory.CreateConverter(typeToConvert) is IJsonDataConverter<T> typedConverter)
                        return typedConverter;
                }
            }

            return null;
        }
    }
}
