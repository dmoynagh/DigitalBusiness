using System;
using System.Collections.Generic;
using System.Text;

namespace DigitalBusiness.JsonDataWrappers
{
    /// <summary>
    /// Base interface for any type that exposes an underlying <see cref="JsonData"/> value.
    /// Allows code to work uniformly with both <see cref="JsonData"/> and <see cref="JsonData{T}"/> via a common contract.
    /// </summary>
    public interface IJsonData
    {
        /// <summary>The underlying untyped JSON data.</summary>
        JsonData Json { get; }
    }
}
