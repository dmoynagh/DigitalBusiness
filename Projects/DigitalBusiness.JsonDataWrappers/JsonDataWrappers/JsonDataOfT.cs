using DigitalBusiness.JsonDataWrappers.Converters;
using System.Diagnostics;
using System.Text.Json.Serialization;

namespace DigitalBusiness.JsonDataWrappers
{
    [DebuggerDisplay("{DebugDisplay,nq}")]
    [JsonConverter(typeof(TypedJsonDataJsonConverter<>))]
    /// <summary>
    /// A typed wrapper around <see cref="JsonData"/> that uses <typeparamref name="T"/> as a phantom type key.
    /// <para>
    /// <typeparamref name="T"/> carries no runtime data — it exists solely to allow extension methods to be
    /// scoped to a specific JSON structure. Code working with this data calls extension methods defined
    /// for the specific <typeparamref name="T"/>, keeping domain logic separate from the wrapper itself.
    /// </para>
    /// <para>
    /// Use <see cref="From"/> or the constructor to create instances. Implicit cast to <see cref="JsonData"/>
    /// allows passing to any API that accepts untyped JSON data.
    /// </para>
    /// </summary>
    /// <typeparam name="T">A key type implementing <see cref="IJsonDataKey"/>. May inherit from another key
    /// to inherit its extension methods.</typeparam>
    public readonly struct JsonData<T> : IJsonDataWrapper where T : IJsonDataKey
    {
        /// <summary>Creates a typed wrapper from an existing <see cref="JsonData"/> instance.</summary>
        public static JsonData<T> From(JsonData json) => new JsonData<T>(json);

        /// <summary>Wraps the given <see cref="JsonData"/>.</summary>
        public JsonData(JsonData json) { Json = json; }

        /// <summary>The underlying untyped JSON data.</summary>
        public JsonData Json { get; init; }

        /// <summary>Implicitly unwraps to <see cref="JsonData"/> for use with untyped APIs.</summary>
        public static implicit operator JsonData(JsonData<T> json) => json.Json;

        /// <summary>Explicitly wraps a <see cref="JsonData"/> as this typed key. Use <see cref="From"/> for clarity.</summary>
        public static explicit operator JsonData<T>(JsonData json) => new JsonData<T>(json);

        private string DebugDisplay =>
        $"[{typeof(T).Name}] " +
        $"[{(Json.IsNode ? "Node" : Json.IsElement ? "Element" : "Empty")}]" +
        $"[{(Json.ReadOnly ? "RO" : "RW")}] " +
        Json.ValueKind.ToString();

    }


}
