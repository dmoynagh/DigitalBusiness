using System;
using System.Collections.Generic;
using System.Text;

namespace DigitalBusiness.Extensibility.Handlers
{
  
    /// <summary>
    /// Represents an asynchronous handler action that operates on a <typeparamref name="TContext"/>.
    /// </summary>
    /// <typeparam name="TContext">The context type passed to the action.</typeparam>
    /// <param name="context">The handler context.</param>
    /// <param name="cancellationToken">A token to observe for cancellation.</param>
    internal delegate ValueTask HandlerActionAsync<TContext>(TContext context, CancellationToken cancellationToken);

   
}
