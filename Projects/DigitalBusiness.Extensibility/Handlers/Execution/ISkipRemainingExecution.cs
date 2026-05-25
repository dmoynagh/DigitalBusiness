using System;
using System.Collections.Generic;
using System.Text;

namespace DigitalBusiness.Extensibility.Handlers.Execution
{
    public interface ISkipRemainingExecution: IHandlerExecution
    {
        bool SkipRemainingRequested { get; }

        string? SkipReason { get; }

        void SkipRemaining(
            string? reason = null);
    }
}
