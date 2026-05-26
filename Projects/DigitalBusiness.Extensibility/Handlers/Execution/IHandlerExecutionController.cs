using System;
using System.Collections.Generic;
using System.Text;

namespace DigitalBusiness.Extensibility.Handlers.Execution
{
    /// <summary>
    /// Allows a handler context to expose an <see cref="IHandlerExecution"/> object that the
    /// pipeline uses to evaluate whether execution should continue.
    /// </summary>
    public interface IHandlerExecutionController
    {
        /// <summary>Gets the current execution state.</summary>
        IHandlerExecution Execution { get; }
    }

    /// <summary>
    /// Strongly-typed variant of <see cref="IHandlerExecutionController"/> that exposes a
    /// concrete <typeparamref name="TExecution"/> rather than the base <see cref="IHandlerExecution"/>.
    /// </summary>
    /// <typeparam name="TExecution">The concrete execution state type.</typeparam>
    public interface IHandlerExecutionController<TExecution> : IHandlerExecutionController
        where TExecution : IHandlerExecution
    {
        /// <summary>Gets the strongly-typed execution state.</summary>
        new TExecution Execution { get; }
        IHandlerExecution IHandlerExecutionController.Execution => Execution;
    }
}
