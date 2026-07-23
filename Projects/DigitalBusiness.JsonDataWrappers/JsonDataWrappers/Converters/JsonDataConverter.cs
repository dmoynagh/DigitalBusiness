using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Text;

namespace DigitalBusiness.JsonDataWrappers.Converters
{
    public  static class JsonDataConverter<T>
    {
        private static readonly Lazy<IJsonDataConverter<T>> _converter =
            new(() => JsonDataConverterProvider.GetConverter<T>(), isThreadSafe: true);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T Get(in JsonData jsonData) => _converter.Value.TryGet(jsonData, out var result) ? result : throw new InvalidOperationException($"Cannot convert JsonData to {typeof(T).FullName}.");

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T? TryGet(in JsonData jsonData) => _converter.Value.TryGet(jsonData, out var result) ? result : default;

        [MethodImpl(MethodImplOptions.AggressiveInlining)] 
        public static bool TryGet(in JsonData jsonData, [MaybeNullWhen(false)] out T value) => _converter.Value.TryGet(jsonData, out value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)] 
        public static JsonData Create(T value) => _converter.Value.Create(value);        


    }
}
