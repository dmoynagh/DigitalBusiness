using System;
using System.Collections.Generic;
using System.Text;

namespace DigitalBusiness.Extensibility.Handlers.Conditions
{
    /// <summary>
    /// An <see cref="IHandlerCondition{TContext}"/> that always returns <see langword="true"/>.
    /// Useful as a default or no-op condition.
    /// </summary>
    /// <typeparam name="TContext">The context type.</typeparam>
    public sealed class TrueCondition<TContext> : IHandlerCondition<TContext>
    {
        /// <inheritdoc />
        public bool Applies(TContext context) => true;
    }
}
