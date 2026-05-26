using System;
using System.Collections.Generic;
using System.Text;

namespace DigitalBusiness.Extensibility.Handlers.Conditions
{
    /// <summary>
    /// An <see cref="IHandlerCondition{TContext}"/> that evaluates a caller-supplied predicate against the context.
    /// </summary>
    /// <typeparam name="TContext">The context type the predicate evaluates.</typeparam>
    public class ContextCondition<TContext> : IHandlerCondition<TContext>
    {
        /// <summary>
        /// Initializes a new instance of <see cref="ContextCondition{TContext}"/>.
        /// </summary>
        /// <param name="condition">The predicate to evaluate against the context.</param>
        public ContextCondition(Func<TContext,bool> condition)
        {
            Condition = condition;
        }

        /// <summary>Gets the predicate used to evaluate the condition.</summary>
        protected Func<TContext, bool> Condition { get; }

        /// <inheritdoc />
        public bool Applies(TContext context)=>Condition(context);  

    }
}
