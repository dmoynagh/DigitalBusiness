using System;
using System.Collections.Generic;
using System.Text;

namespace DigitalBusiness.Extensibility.Handlers.Execution
{
    /// <summary>
    /// Extends <see cref="IHandlerExecution"/> with the ability to stop execution across all subsequent
    /// handlers in the pipeline, not just the current one.
    /// </summary>
    public interface IEndPipelineExecution: IHandlerExecution
    {
        /// <summary>Gets a value indicating whether a full pipeline end has been requested.</summary>
        bool EndPipelineRequested { get; }

        /// <summary>Gets an optional human-readable reason for ending the pipeline.</summary>
        string? EndReason { get; }

        /// <summary>
        /// Requests that the entire pipeline stops after the current action.
        /// </summary>
        /// <param name="reason">An optional reason stored in <see cref="EndReason"/>.</param>
        void EndPipelineExecution(
            string? reason = null);
    }
}
