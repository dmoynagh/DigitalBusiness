using System;
using System.Collections.Generic;
using System.Text;

namespace DigitalBusiness.Extensibility.Handlers.Execution
{
    /// <summary>
    /// Extends <see cref="IHandlerExecution"/> with the ability to skip all remaining handler
    /// actions in the current <see cref="Handler{TContext}"/> without affecting other handlers in the pipeline.
    /// </summary>
    public interface ISkipRemainingExecution: IHandlerExecution
    {
        /// <summary>Gets a value indicating whether a skip has been requested.</summary>
        bool SkipRemainingRequested { get; }

        /// <summary>Gets an optional human-readable reason for skipping.</summary>
        string? SkipReason { get; }

        /// <summary>
        /// Requests that all remaining actions in the current handler be skipped.
        /// </summary>
        /// <param name="reason">An optional reason stored in <see cref="SkipReason"/>.</param>
        void SkipRemaining(
            string? reason = null);
    }
}
