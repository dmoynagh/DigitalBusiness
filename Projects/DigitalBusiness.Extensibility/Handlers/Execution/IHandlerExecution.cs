using System;
using System.Collections.Generic;
using System.Text;

namespace DigitalBusiness.Extensibility.Handlers.Execution
{
    /// <summary>
    /// Represents the execution state for a handler pipeline run.
    /// Contexts that implement this interface (via <see cref="IHandlerExecutionController"/>) can
    /// control whether the pipeline continues after each action.
    /// </summary>
    public interface IHandlerExecution
    {
        /// <summary>
        /// Gets or sets the <see cref="HandlerExecutionSource"/> that caused execution to stop.
        /// Set by <see cref="Handler{TContext}"/> when <see cref="ContinueExecution"/> returns <see langword="false"/>.
        /// </summary>
        HandlerExecutionSource? ExecutionSource { get; internal set; }

        /// <summary>
        /// Gets a value indicating whether the pipeline should continue executing subsequent handler actions.
        /// When <see langword="false"/>, <see cref="Handler{TContext}"/> stops after the current action.
        /// </summary>
        bool ContinueExecution { get; }
    }
}
