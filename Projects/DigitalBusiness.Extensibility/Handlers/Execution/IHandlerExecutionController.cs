using System;
using System.Collections.Generic;
using System.Text;

namespace DigitalBusiness.Extensibility.Handlers.Execution
{
    public interface IHandlerExecutionController<TExecution> where TExecution : IHandlerExecution
    {
        TExecution Execution { get; }  
    }
}
