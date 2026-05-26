using System;
using System.Collections.Generic;
using System.Text;

namespace DigitalBusiness.Extensibility.Handlers.Conditions
{
    /// <summary>
    /// An <see cref="IHandlerCondition{TContext}"/> that inverts (negates) its inner condition.
    /// Returns <see langword="true"/> when the inner condition returns <see langword="false"/>, and vice versa.
    /// </summary>
    /// <typeparam name="TContext">The context type.</typeparam>
    public sealed class NotCondition<TContext> : IHandlerCondition<TContext>
    {
        /// <summary>
        /// Initializes a new instance of <see cref="NotCondition{TContext}"/>.
        /// </summary>
        /// <param name="condition">The condition whose result will be inverted.</param>
        public NotCondition(IHandlerCondition<TContext> condition) 
        { 
            Condition = condition;
        }

        /// <summary>Gets the inner condition whose result is inverted.</summary>
        public IHandlerCondition<TContext> Condition { get; }

        /// <inheritdoc />
        public bool Applies(TContext context)=>!Condition.Applies(context);

    }

    /// <summary>
    /// Provides NOT-composition extension methods for <see cref="IHandlerCondition{TContext}"/> instances.
    /// </summary>
    public static class NotConditionExtensions
    {
        extension<TContext>(IHandlerCondition<TContext>? condition)
        {
            /// <summary>
            /// Negates all of the supplied <paramref name="conditions"/> (AND-combined) and appends the result
            /// to the current condition with logical AND.
            /// If the current condition is <see langword="null"/>, returns the negated condition directly.
            /// </summary>
            /// <param name="conditions">One or more conditions to negate.</param>
            public IHandlerCondition<TContext> Not(params IHandlerCondition<TContext>[] conditions)
            {
                if (conditions == null || conditions.Length == 0) throw new ArgumentNullException(nameof(conditions));
                if (condition is not null) return condition.AndNot(conditions);

                return conditions.Length > 1 ? new NotCondition<TContext>(new AndCondition<TContext>(conditions)) : new NotCondition<TContext>(conditions[0]);                                
            }

            /// <summary>
            /// Negates the OR-combination of <paramref name="conditions"/> (i.e. NOR) and appends the result
            /// to the current condition with logical AND.
            /// If the current condition is <see langword="null"/>, returns the NOR condition directly.
            /// </summary>
            /// <param name="conditions">One or more conditions to NOR-combine.</param>
            public IHandlerCondition<TContext> NotAny(params IHandlerCondition<TContext>[] conditions)
            {
                if (conditions == null || conditions.Length == 0) throw new ArgumentNullException(nameof(conditions));

                var newCondition = conditions.Length > 1 ? new NotCondition<TContext>(new OrCondition<TContext>(conditions)) : new NotCondition<TContext>(conditions[0]);

                return condition is null ? newCondition : condition.And(conditions);               
            }

            /// <summary>
            /// Combines this condition with a negated AND-combination of <paramref name="conditions"/>
            /// (i.e. <c>this AND NOT(conditions)</c>).
            /// </summary>
            /// <param name="conditions">One or more conditions to negate and AND with the current condition.</param>
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
