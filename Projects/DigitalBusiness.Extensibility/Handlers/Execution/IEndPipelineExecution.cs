using System;
using System.Collections.Generic;
using System.Text;

namespace DigitalBusiness.Extensibility.Handlers.Execution
{
    public interface IEndPipelineExecution: IHandlerExecution
    {
        bool EndPipelineRequested { get; }

        string? EndReason { get; }

        void EndPipelineExecution(
            string? reason = null);
    }
}
