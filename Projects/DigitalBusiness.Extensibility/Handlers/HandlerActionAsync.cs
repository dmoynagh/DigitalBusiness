using System;
using System.Collections.Generic;
using System.Text;

namespace DigitalBusiness.Extensibility.Handlers
{
  
    internal delegate ValueTask HandlerActionAsync<TContext>(TContext context, CancellationToken cancellationToken);

   
}
