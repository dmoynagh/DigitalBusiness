using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace DigitalBusiness.JsonDataWrappers.Converters
{
    /// <summary>
    /// Explicit registration and lookup surface for custom <see cref="IJsonDataConverter{T}"/> and
    /// <see cref="IJsonDataConverterFactory"/> implementations.
    /// <para>
    /// No ambient assembly scanning happens automatically — registration is always an explicit,
    /// deliberate bootstrap act via <see cref="Register{T}(IJsonDataConverter{T})"/>,
    /// <see cref="Register(IJsonDataConverterFactory)"/>, or <see cref="RegisterFromAssembly(Assembly)"/>.
    /// Call <see cref="Freeze"/> once application bootstrap has finished registering converters to
    /// lock the registry against further changes.
    /// </para>
    /// </summary>
    public static class JsonDataConverters
    {
        private static readonly ConcurrentDictionary<Type, IJsonDataConverter> _converters = new();
        private static readonly List<IJsonDataConverterFactory> _factories = new();
        private static readonly object _lock = new();
        private static readonly HashSet<Assembly> _scannedAssemblies = new();
        private static volatile bool _frozen;

        /// <summary>Explicitly registers a converter for <typeparamref name="T"/>. Throws if a converter for
        /// <typeparamref name="T"/> is already registered, or if the registry is frozen.</summary>
        public static void Register<T>(IJsonDataConverter<T> converter)
        {
            ThrowIfFrozen();
            if (!_converters.TryAdd(typeof(T), converter))
                throw new InvalidOperationException($"A converter for {typeof(T).FullName} is already registered.");
        }

        /// <summary>Explicitly registers a converter factory. Throws if the registry is frozen.</summary>
        public static void Register(IJsonDataConverterFactory factory)
        {
            ThrowIfFrozen();
            lock (_lock) { _factories.Add(factory); }
        }

        /// <summary>Scans <paramref name="assembly"/> for <see cref="IJsonDataConverter{T}"/> and
        /// <see cref="IJsonDataConverterFactory"/> implementations and registers them. A type found by a
        /// scan that duplicates an already-registered type is skipped (first-wins, no throw) — only
        /// explicit <see cref="Register{T}(IJsonDataConverter{T})"/> calls throw on duplicate. Throws if
        /// the registry is frozen.</summary>
        public static void RegisterFromAssembly(Assembly assembly)
        {
            ThrowIfFrozen();
            ScanAssemblies([assembly]);
        }

        /// <summary>Freezes the registry — further calls to <see cref="Register{T}(IJsonDataConverter{T})"/>,
        /// <see cref="Register(IJsonDataConverterFactory)"/>, or <see cref="RegisterFromAssembly(Assembly)"/>
        /// will throw. Irreversible for the lifetime of the process.</summary>
        public static void Freeze() => _frozen = true;

        private static void ThrowIfFrozen()
        {
            if (_frozen) throw new InvalidOperationException(
                "JsonDataConverters is frozen — Register/RegisterFromAssembly can no longer be called.");
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

        /// <summary>Looks up an explicitly registered converter or factory-produced converter for
        /// <typeparamref name="T"/>. Returns null if none is registered.</summary>
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
