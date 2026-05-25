using System;
using System.Collections.Generic;
using System.Text;

namespace DigitalBusiness.Extensibility.Handlers.Conditions
{
    public sealed class NotCondition<TContext> : IHandlerCondition<TContext>
    {
        public NotCondition(IHandlerCondition<TContext> condition) 
        { 
            Condition = condition;
        }

        public IHandlerCondition<TContext> Condition { get; }
        public bool Applies(TContext context)=>!Condition.Applies(context);

    }

    public static class NotConditionExtensions
    {
        extension<TContext>(IHandlerCondition<TContext>? condition)
        {
            public IHandlerCondition<TContext> Not(params IHandlerCondition<TContext>[] conditions)
            {
                if (conditions == null || conditions.Length == 0) throw new ArgumentNullException(nameof(conditions));
                if (condition is not null) return condition.AndNot(conditions);
                             
                return conditions.Length > 1 ? new NotCondition<TContext>(new AndCondition<TContext>(conditions)) : new NotCondition<TContext>(conditions[0]);                                
            }

            public IHandlerCondition<TContext> NotAny(params IHandlerCondition<TContext>[] conditions)
            {
                if (conditions == null || conditions.Length == 0) throw new ArgumentNullException(nameof(conditions));

                var newCondition = conditions.Length > 1 ? new NotCondition<TContext>(new OrCondition<TContext>(conditions)) : new NotCondition<TContext>(conditions[0]);

                return condition is null ? newCondition : condition.And(conditions);               
            }


            public IHandlerCondition<TContext> AndNot(params IHandlerCondition<TContext>[] conditions)
            {
                if (conditions == null || conditions.Length == 0) throw new ArgumentNullException(nameof(conditions));
                if (condition is null) return condition.Not(conditions);

                var newCondition = conditions.Length > 1 ? new NotCondition<TContext>(new AndCondition<TContext>(conditions)) : new NotCondition<TContext>(conditions[0]);
                return new AndCondition<TContext>(condition, newCondition);

            }
        }
    }
}
