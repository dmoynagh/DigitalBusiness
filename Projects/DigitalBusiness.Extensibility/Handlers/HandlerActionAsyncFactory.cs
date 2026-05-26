using System;
using System.Collections.Generic;
using System.Text;

namespace DigitalBusiness.Extensibility.Handlers
{
    internal delegate HandlerActionAsync<TContext> HandlerActionAsyncFactory<TContext>(IServiceProvider serviceProvider);
   
}
