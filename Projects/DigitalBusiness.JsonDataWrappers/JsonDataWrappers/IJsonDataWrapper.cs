using System;
using System.Collections.Generic;
using System.Text;

namespace DigitalBusiness.JsonDataWrappers
{
    /// <summary>
    /// Extends <see cref="IJsonData"/> to mark a struct as a typed JSON wrapper.
    /// The <c>init</c> accessor allows the <see cref="Json"/> property to be set during construction
    /// while remaining immutable afterwards, consistent with <c>readonly struct</c> semantics.
    /// </summary>
    public interface IJsonDataWrapper : IJsonData
    {
        /// <summary>The underlying untyped JSON data. Set once at construction via constructor or object initializer.</summary>
        new JsonData Json { get; init; }
    }
}
