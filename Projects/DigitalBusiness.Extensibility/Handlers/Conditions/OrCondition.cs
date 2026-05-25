using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Text;

namespace DigitalBusiness.Extensibility.Handlers.Conditions
{
    public sealed class OrCondition<TContext> : IHandlerCondition<TContext>
    {
        public OrCondition(params IHandlerCondition<TContext>[] conditions)
        {
            Conditions = conditions.ToImmutableArray();
        }

        public ImmutableArray<IHandlerCondition<TContext>> Conditions { get; }


        public bool Applies(TContext context) => Conditions.Any(c => c.Applies(context));
        
    }

    public static class OrConditionExtensions
    {
        extension<TContext>(IHandlerCondition<TContext>? condition)
        {
            public IHandlerCondition<TContext> Or(params IHandlerCondition<TContext>[] conditions)
            {
                if (conditions == null || conditions.Length == 0) throw new ArgumentNullException(nameof(conditions));

                var newCondition = conditions.Length > 1 ? new OrCondition<TContext>(conditions) : conditions[0];
                return condition == null ? newCondition : new OrCondition<TContext>(condition, newCondition);
            }
        }

          
    }


}
