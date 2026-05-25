using System;
using System.Collections.Generic;
using System.Text;

namespace DigitalBusiness.Extensibility.Handlers.Conditions
{
    public class ContextCondition<TContext> : IHandlerCondition<TContext>
    {
        public ContextCondition(Func<TContext,bool> condition)
        {
            Condition = condition;
        }

        protected Func<TContext, bool> Condition { get; }

        public bool Applies(TContext context)=>Condition(context);  

    }
}
