using System;
using System.Collections.Generic;
using System.Text;

namespace DigitalBusiness.Extensibility.Handlers.Conditions
{
    public sealed class TrueCondition<TContext> : IHandlerCondition<TContext>
    {
        public bool Applies(TContext context) => true;
    }
}
