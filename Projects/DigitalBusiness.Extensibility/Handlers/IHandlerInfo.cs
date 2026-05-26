using System;
using System.Collections.Generic;
using System.Text;

namespace DigitalBusiness.Extensibility.Handlers
{
    /// <summary>
    /// Provides identifying metadata about a registered handler action.
    /// </summary>
    public interface IHandlerInfo
    {
        /// <summary>Gets the monotonically increasing order in which this handler was registered.</summary>
        long RegistrationOrder { get; }

        /// <summary>Gets the owner name (typically the component or module that registered the handler).</summary>
        public string Owner { get; }

        /// <summary>Gets the handler action name.</summary>
        public string Name { get; }

        /// <summary>Gets the fully-qualified name in the format <c>{Owner}.{Name}</c>.</summary>
        public string FullName {  get; }
    }
}
