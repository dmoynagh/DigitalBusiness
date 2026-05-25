using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Text;

namespace DigitalBusiness.Extensibility.Handlers.Conditions
{
    public sealed class AndCondition<TContext> : IHandlerCondition<TContext>
    {
        public AndCondition(params IHandlerCondition<TContext>[] conditions) 
        {
            Conditions = conditions.Where(c=>c is not null).ToImmutableArray();
        }

        public ImmutableArray<IHandlerCondition<TContext>> Conditions { get; }


        public bool Applies(TContext context)=>Conditions.All(c=>c.Applies(context));
        
    }

    public static class AndConditionExtensions
    {
        extension<TContext>(IHandlerCondition<TContext>? condition)
        {
            public IHandlerCondition<TContext> And(params IHandlerCondition<TContext>[] conditions)
            {
                if (conditions == null || conditions.Length == 0) throw new ArgumentNullException(nameof(conditions));

                var newCondition = conditions.Length > 1 ? new AndCondition<TContext>(conditions) : conditions[0];
                return condition == null ? newCondition : new AndCondition<TContext>(condition, newCondition);
            }
        }      
    }
}
