using System;
using System.Collections.Generic;
using System.Text;

namespace DigitalBusiness.Extensibility.Handlers.Execution
{
    /// <summary>
    /// Identifies the handler action that caused execution to stop (skip, cancel, or end pipeline).
    /// </summary>
    /// <param name="Owner">The owner of the handler action that halted execution.</param>
    /// <param name="Name">The name of the handler action that halted execution.</param>
    /// <param name="RegistrationOrder">The registration order of the handler action.</param>
    public record class HandlerExecutionSource(string? Owner, string? Name, long RegistrationOrder)
    {
        /// <summary>Gets the fully-qualified name in the format <c>{Owner}.{Name}</c>.</summary>
        public string FullName => Owner is not null && Name is not null ? $"{Owner}.{Name}" : string.Concat(Owner, Name);
    }

    /// <summary>
    /// Extension methods for creating <see cref="HandlerExecutionSource"/> instances.
    /// </summary>
    public static class HandlerExecutionSourceExtensions
    {
        extension(HandlerExecutionSource)
        {
            /// <summary>Creates a <see cref="HandlerExecutionSource"/> from an <see cref="IHandlerInfo"/> descriptor.</summary>
            /// <param name="handlerInfo">The handler descriptor to capture.</param>
            public static HandlerExecutionSource Create(IHandlerInfo handlerInfo) => new HandlerExecutionSource(handlerInfo.Owner, handlerInfo.Name, handlerInfo.RegistrationOrder);
        }
    }
}
