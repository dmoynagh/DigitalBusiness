using System;
using System.Collections.Generic;
using System.Text;

namespace DigitalBusiness.JsonDataWrappers
{
    /// <summary>
    /// Marker interface for typed JSON wrappers that represent a JSON object (as opposed to an array or primitive value).
    /// Derive from this when the wrapped JSON is always expected to be an object structure.
    /// </summary>
    public interface IJsonDataObject : IJsonDataWrapper
    {
    }
}
