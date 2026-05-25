using System;
using System.Collections.Generic;
using System.Text;

namespace DigitalBusiness.Extensibility.Handlers.Execution
{
    public interface ICancelExecution: IHandlerExecution
    {
        bool CancelRequested { get; }

        string? CancelReason { get; }

        void Cancel(
            string? reason = null);
    }
}
