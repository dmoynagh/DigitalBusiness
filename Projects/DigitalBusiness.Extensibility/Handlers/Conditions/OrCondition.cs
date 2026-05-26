using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Text;

namespace DigitalBusiness.Extensibility.Handlers.Conditions
{
    /// <summary>
    /// An <see cref="IHandlerCondition{TContext}"/> that returns <see langword="true"/> when
    /// <em>any</em> of its inner conditions return <see langword="true"/>.
    /// </summary>
    /// <typeparam name="TContext">The context type.</typeparam>
    public sealed class OrCondition<TContext> : IHandlerCondition<TContext>
    {
        /// <summary>
        /// Initializes a new instance with the conditions to combine.
        /// </summary>
        /// <param name="conditions">The conditions to evaluate.</param>
        public OrCondition(params IHandlerCondition<TContext>[] conditions)
        {
            Conditions = conditions.ToImmutableArray();
        }

        /// <summary>Gets the inner conditions, any of which must pass.</summary>
        public ImmutableArray<IHandlerCondition<TContext>> Conditions { get; }

        /// <inheritdoc />
        public bool Applies(TContext context) => Conditions.Any(c => c.Applies(context));

    }

    /// <summary>
    /// Provides the <c>Or</c> extension method for composing <see cref="IHandlerCondition{TContext}"/> instances.
    /// </summary>
    public static class OrConditionExtensions
    {
        extension<TContext>(IHandlerCondition<TContext>? condition)
        {
            /// <summary>
            /// Combines this condition with one or more additional conditions using logical OR.
            /// If this condition is <see langword="null"/>, returns the new condition directly.
            /// </summary>
            /// <param name="conditions">One or more conditions to OR with the current condition.</param>
            public IHandlerCondition<TContext> Or(params IHandlerCondition<TContext>[] conditions)
            {
                if (conditions == null || conditions.Length == 0) throw new ArgumentNullException(nameof(conditions));

                var newCondition = conditions.Length > 1 ? new OrCondition<TContext>(conditions) : conditions[0];
                return condition == null ? newCondition : new OrCondition<TContext>(condition, newCondition);
            }
        }


    }


}
