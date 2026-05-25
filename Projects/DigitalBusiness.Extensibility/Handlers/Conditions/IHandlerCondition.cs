using System;
using System.Collections.Generic;
using System.Text;

namespace DigitalBusiness.Extensibility.Handlers.Conditions
{
    public interface IHandlerCondition<in TContext>
    {
        bool Applies(TContext context);
    }
}
