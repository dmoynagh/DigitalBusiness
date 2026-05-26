using System;
using System.Collections.Generic;
using System.Text;

namespace DigitalBusiness.Extensibility.Handlers.Execution
{
    public interface IHandlerExecutionController
    {
        IHandlerExecution Execution { get; }
    }

    public interface IHandlerExecutionController<TExecution> : IHandlerExecutionController
        where TExecution : IHandlerExecution
    {
        new TExecution Execution { get; }
        IHandlerExecution IHandlerExecutionController.Execution => Execution;
    }
}
