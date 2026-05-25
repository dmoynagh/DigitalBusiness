using System;
using System.Collections.Generic;
using System.Text;

namespace DigitalBusiness.Extensibility.Handlers.Execution
{
    public record class HandlerExecutionSource(string? Owner, string? Name, long RegistrationOrder)
    {
        public string FullName => Owner is not null && Name is not null ? $"{Owner}.{Name}" : string.Concat(Owner, Name);
    }

    public static class HandlerExecutionSourceExtensions
    {
        extension(HandlerExecutionSource)
        {
            public static HandlerExecutionSource Create(IHandlerInfo handlerInfo) => new HandlerExecutionSource(handlerInfo.Owner, handlerInfo.Name, handlerInfo.RegistrationOrder);
        }
    }
}
