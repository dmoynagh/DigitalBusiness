using System;
using System.Collections.Generic;
using System.Text;

namespace DigitalBusiness.Extensibility.Handlers
{
    /// <summary>
    /// Represents a factory that creates a <see cref="HandlerActionAsync{TContext}"/> from an <see cref="IServiceProvider"/>.
    /// Used for handler actions that need to resolve scoped or transient dependencies at execution time.
    /// </summary>
    /// <typeparam name="TContext">The context type the produced action operates on.</typeparam>
    /// <param name="serviceProvider">The service provider used to resolve dependencies.</param>
    internal delegate HandlerActionAsync<TContext> HandlerActionAsyncFactory<TContext>(IServiceProvider serviceProvider);
   
}
