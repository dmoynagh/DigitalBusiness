using System;
using System.Collections.Generic;
using System.Text;

namespace DigitalBusiness.Extensibility.Handlers
{
    /// <summary>
    /// Wraps an <see cref="IServiceProvider"/> for use when resolving factory-based handler actions at execution time.
    /// </summary>
    public class HandlerActionServiceProvider
    {
        /// <summary>
        /// Initializes a new instance of <see cref="HandlerActionServiceProvider"/>.
        /// </summary>
        /// <param name="serviceProvider">The underlying service provider used to resolve handler action dependencies.</param>
        public HandlerActionServiceProvider(IServiceProvider serviceProvider)
        {
            ServiceProvider = serviceProvider;
        }

        /// <summary>Gets the underlying <see cref="IServiceProvider"/>.</summary>
        public IServiceProvider ServiceProvider { get; }
    }
}
