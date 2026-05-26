using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Text;

namespace DigitalBusiness.Extensibility.Handlers.Conditions
{
    /// <summary>
    /// An <see cref="IHandlerCondition{TContext}"/> that returns <see langword="true"/> only when
    /// <em>all</em> of its inner conditions return <see langword="true"/>.
    /// </summary>
    /// <typeparam name="TContext">The context type.</typeparam>
    public sealed class AndCondition<TContext> : IHandlerCondition<TContext>
    {
        /// <summary>
        /// Initializes a new instance with the conditions to combine.
        /// <see langword="null"/> entries are silently ignored.
        /// </summary>
        /// <param name="conditions">The conditions to evaluate.</param>
        public AndCondition(params IHandlerCondition<TContext>[] conditions) 
        {
            Conditions = conditions.Where(c=>c is not null).ToImmutableArray();
        }

        /// <summary>Gets the inner conditions that are all required to pass.</summary>
        public ImmutableArray<IHandlerCondition<TContext>> Conditions { get; }

        /// <inheritdoc />
        public bool Applies(TContext context)=>Conditions.All(c=>c.Applies(context));

    }

    /// <summary>
    /// Provides the <c>And</c> extension method for composing <see cref="IHandlerCondition{TContext}"/> instances.
    /// </summary>
    public static class AndConditionExtensions
    {
        extension<TContext>(IHandlerCondition<TContext>? condition)
        {
            /// <summary>
            /// Combines this condition with one or more additional conditions using logical AND.
            /// If this condition is <see langword="null"/>, returns the new condition directly.
            /// </summary>
            /// <param name="conditions">One or more conditions to AND with the current condition.</param>
            public IHandlerCondition<TContext> And(params IHandlerCondition<TContext>[] conditions)
            {
                if (conditions == null || conditions.Length == 0) throw new ArgumentNullException(nameof(conditions));

                var newCondition = conditions.Length > 1 ? new AndCondition<TContext>(conditions) : conditions[0];
                return condition == null ? newCondition : new AndCondition<TContext>(condition, newCondition);
            }
        }      
    }
}
