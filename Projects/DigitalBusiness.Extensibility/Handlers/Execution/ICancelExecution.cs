using System;
using System.Collections.Generic;
using System.Text;

namespace DigitalBusiness.Extensibility.Handlers.Execution
{
    /// <summary>
    /// Extends <see cref="IHandlerExecution"/> with the ability to request cancellation of the
    /// current pipeline operation (e.g. abort the save or command being processed).
    /// </summary>
    public interface ICancelExecution: IHandlerExecution
    {
        /// <summary>Gets a value indicating whether cancellation has been requested.</summary>
        bool CancelRequested { get; }

        /// <summary>Gets an optional human-readable reason for the cancellation.</summary>
        string? CancelReason { get; }

        /// <summary>
        /// Requests cancellation of the pipeline operation.
        /// </summary>
        /// <param name="reason">An optional reason that is stored in <see cref="CancelReason"/>.</param>
        void Cancel(
            string? reason = null);
    }
}
