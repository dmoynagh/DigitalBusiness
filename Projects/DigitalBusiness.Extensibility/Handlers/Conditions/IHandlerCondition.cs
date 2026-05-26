using System;
using System.Collections.Generic;
using System.Text;

namespace DigitalBusiness.Extensibility.Handlers.Conditions
{
    /// <summary>
    /// Defines a condition that determines whether a handler action should be executed for a given context.
    /// </summary>
    /// <typeparam name="TContext">The context type the condition evaluates.</typeparam>
    public interface IHandlerCondition<in TContext>
    {
        /// <summary>
        /// Evaluates the condition against the supplied <paramref name="context"/>.
        /// </summary>
        /// <param name="context">The handler context to evaluate.</param>
        /// <returns><see langword="true"/> if the associated handler action should run; otherwise <see langword="false"/>.</returns>
        bool Applies(TContext context);
    }
}
