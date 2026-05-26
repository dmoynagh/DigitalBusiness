using DigitalBusiness.Extensibility.Handlers.Execution;
using System;
using System.Collections.Generic;
using System.Text;

namespace DigitalBusiness.Extensibility.Handlers
{
    /// <summary>
    /// Provides extension methods for <see cref="IHandlerExecution"/> and typed
    /// <see cref="IHandlerExecutionController{TExecution}"/> instances to check and request
    /// execution flow changes (skip remaining, cancel, end pipeline).
    /// </summary>
    public static class HandlerExecutionExtensions
    {
        extension(IHandlerExecution execution)
        {
            /// <summary>Gets a value indicating whether this execution supports skipping remaining handler actions.</summary>
            public bool SupportsSkipRemaining => execution is ISkipRemainingExecution;

            /// <summary>Gets a value indicating whether a skip-remaining request is active.</summary>
            public bool SkipRemainingRequested => execution is ISkipRemainingExecution skipExecution && skipExecution.SkipRemainingRequested;

            /// <summary>Gets the reason for a skip-remaining request, or <see langword="null"/> if none was provided or not supported.</summary>
            public string? SkipReason => execution is ISkipRemainingExecution skipExecution ? skipExecution.SkipReason : null;

            /// <summary>
            /// Requests that all remaining handler actions in the current <see cref="Handler{TContext}"/> be skipped.
            /// </summary>
            /// <param name="reason">An optional human-readable reason.</param>
            /// <exception cref="NotSupportedException">Thrown when the execution does not implement <see cref="ISkipRemainingExecution"/>.</exception>
            public void SkipRemaining(string? reason = null)
            {
                if (execution is ISkipRemainingExecution skipExecution)
                {
                    skipExecution.SkipRemaining(reason);
                }
                else throw new NotSupportedException("The current handler execution does not support skipping remaining handlers.");
            }

            /// <summary>Gets a value indicating whether this execution supports cancellation.</summary>
            public bool SupportsCancel => execution is ICancelExecution;

            /// <summary>Gets a value indicating whether a cancel request is active.</summary>
            public bool CancelRequested => execution is ICancelExecution cancelExecution && cancelExecution.CancelRequested;

            /// <summary>Gets the reason for a cancel request, or <see langword="null"/> if none was provided or not supported.</summary>
            public string? CancelReason => execution is ICancelExecution cancelExecution ? cancelExecution.CancelReason : null;

            /// <summary>
            /// Requests cancellation of the pipeline operation.
            /// </summary>
            /// <param name="reason">An optional human-readable reason.</param>
            /// <exception cref="NotSupportedException">Thrown when the execution does not implement <see cref="ICancelExecution"/>.</exception>
            public void Cancel(string? reason = null)
            {
                if (execution is ICancelExecution cancelExecution)
                {
                    cancelExecution.Cancel(reason);
                }
                else throw new NotSupportedException("The current handler execution does not support cancellation.");
            }

            /// <summary>Gets a value indicating whether this execution supports ending the entire pipeline.</summary>
            public bool SupportsEndPipeline => execution is IEndPipelineExecution;

            /// <summary>Gets a value indicating whether an end-pipeline request is active.</summary>
            public bool EndPipelineRequested => execution is IEndPipelineExecution endExecution && endExecution.EndPipelineRequested;

            /// <summary>Gets the reason for an end-pipeline request, or <see langword="null"/> if none was provided or not supported.</summary>
            public string? EndReason => execution is IEndPipelineExecution endExecution ? endExecution.EndReason : null;

            /// <summary>
            /// Requests that the entire pipeline stops after the current action.
            /// </summary>
            /// <param name="reason">An optional human-readable reason.</param>
            /// <exception cref="NotSupportedException">Thrown when the execution does not implement <see cref="IEndPipelineExecution"/>.</exception>
            public void EndPipelineExecution(string? reason = null)
            {
                if (execution is IEndPipelineExecution endExecution)
                {
                    endExecution.EndPipelineExecution(reason);
                }
                else throw new NotSupportedException("The current handler execution does not support ending the pipeline.");
            }

        }

        extension<TExecution>(IHandlerExecutionController<TExecution> controller) where TExecution : ISkipRemainingExecution
        {
            /// <summary>Gets a value indicating whether a skip-remaining request is active on the controller's execution.</summary>
            public bool SkipRemainingRequested => controller.Execution.SkipRemainingRequested;

            /// <summary>Gets the reason for a skip-remaining request, or <see langword="null"/> if none was provided.</summary>
            public string? SkipReason => controller.Execution.SkipReason;

            /// <summary>Requests that all remaining handler actions in the current handler be skipped.</summary>
            /// <param name="reason">An optional human-readable reason.</param>
            public void SkipRemaining(string? reason = null) => controller.Execution.SkipRemaining(reason);
        }

        extension<TExecution>(IHandlerExecutionController<TExecution> controller) where TExecution : ICancelExecution
        {
            /// <summary>Gets a value indicating whether a cancel request is active on the controller's execution.</summary>
            public bool CancelRequested => controller.Execution.CancelRequested;

            /// <summary>Gets the reason for a cancel request, or <see langword="null"/> if none was provided.</summary>
            public string? CancelReason => controller.Execution.CancelReason;

            /// <summary>Requests cancellation of the pipeline operation.</summary>
            /// <param name="reason">An optional human-readable reason.</param>
            public void Cancel(string? reason = null) => controller.Execution.Cancel(reason);
        }

        extension<TExecution>(IHandlerExecutionController<TExecution> controller) where TExecution : IEndPipelineExecution
        {
            /// <summary>Gets a value indicating whether an end-pipeline request is active on the controller's execution.</summary>
            public bool EndPipelineRequested => controller.Execution.EndPipelineRequested;

            /// <summary>Gets the reason for an end-pipeline request, or <see langword="null"/> if none was provided.</summary>
            public string? EndReason => controller.Execution.EndReason;

            /// <summary>Requests that the entire pipeline stops after the current action.</summary>
            /// <param name="reason">An optional human-readable reason.</param>
            public void EndPipelineExecution(string? reason = null) => controller.Execution.EndPipelineExecution(reason);
        }
    }

}
