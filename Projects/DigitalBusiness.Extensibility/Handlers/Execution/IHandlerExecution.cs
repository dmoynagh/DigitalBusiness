using System;
using System.Collections.Generic;
using System.Text;

namespace DigitalBusiness.Extensibility.Handlers.Execution
{
    public interface IHandlerExecution
    {
        HandlerExecutionSource? ExecutionSource { get; internal set; }

        bool ContinueExecution { get; }
    }
}
