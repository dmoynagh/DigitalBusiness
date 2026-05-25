using DigitalBusiness.Extensibility.Handlers.Execution;
using System;
using System.Collections.Generic;
using System.Text;

namespace DigitalBusiness.Extensibility.Handlers
{
    public static class HandlerExecutionExtensions
    {
        extension(IHandlerExecution execution)
        {

            public bool SupportsSkipRemaining => execution is ISkipRemainingExecution;
            public bool SkipRemainingRequested => execution is ISkipRemainingExecution skipExecution && skipExecution.SkipRemainingRequested;
            public string? SkipReason => execution is ISkipRemainingExecution skipExecution ? skipExecution.SkipReason : null;
            public void SkipRemaining(string? reason = null)
            {
                if (execution is ISkipRemainingExecution skipExecution)
                {
                    skipExecution.SkipRemaining(reason);
                }
                else throw new NotSupportedException("The current handler execution does not support skipping remaining handlers.");
            }


            public bool SupportsCancel => execution is ICancelExecution;
            public bool CancelRequested => execution is ICancelExecution cancelExecution && cancelExecution.CancelRequested;
            public string? CancelReason => execution is ICancelExecution cancelExecution ? cancelExecution.CancelReason : null;
            public void Cancel(string? reason = null)
            {
                if (execution is ICancelExecution cancelExecution)
                {
                    cancelExecution.Cancel(reason);
                }
                else throw new NotSupportedException("The current handler execution does not support cancellation.");
            }


            public bool SupportsEndPipeline => execution is IEndPipelineExecution;
            public bool EndPipelineRequested => execution is IEndPipelineExecution endExecution && endExecution.EndPipelineRequested;
            public string? EndReason => execution is IEndPipelineExecution endExecution ? endExecution.EndReason : null;
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
            public bool SkipRemainingRequested => controller.Execution.SkipRemainingRequested;
            public string? SkipReason => controller.Execution.SkipReason;
            public void SkipRemaining(string? reason = null) => controller.Execution.SkipRemaining(reason);
        }

        extension<TExecution>(IHandlerExecutionController<TExecution> controller) where TExecution : ICancelExecution
        {
            public bool CancelRequested => controller.Execution.CancelRequested;
            public string? CancelReason => controller.Execution.CancelReason;
            public void Cancel(string? reason = null) => controller.Execution.Cancel(reason);
        }

        extension<TExecution>(IHandlerExecutionController<TExecution> controller) where TExecution : IEndPipelineExecution
        {
            public bool EndPipelineRequested => controller.Execution.EndPipelineRequested;
            public string? EndReason => controller.Execution.EndReason;
            public void EndPipelineExecution(string? reason = null) => controller.Execution.EndPipelineExecution(reason);
        }
    }

}
