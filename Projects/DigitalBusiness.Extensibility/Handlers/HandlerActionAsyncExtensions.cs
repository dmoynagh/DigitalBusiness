using System;
using System.Collections.Generic;
using System.Text;

namespace DigitalBusiness.Extensibility.Handlers
{
    internal static class HandlerActionAsyncExtensions
    {
        internal static HandlerActionAsync<TContext> ToHandlerActionAsync<TContext>(this Func<TContext, CancellationToken, ValueTask> handlerAction)
            => new HandlerActionAsync<TContext>(handlerAction);

        internal static HandlerActionAsync<TContext> ToHandlerActionAsync<TContext>(this Action<TContext> handlerAction)
            => new HandlerActionAsync<TContext>((context, cancellationToken) =>
            {
                handlerAction(context);
                return ValueTask.CompletedTask;
            });

        internal static HandlerActionAsync<TContext> ToHandlerActionAsync<TContext>(this Func<TContext, ValueTask> handlerAction)
            => new HandlerActionAsync<TContext>((context, cancellationToken) => handlerAction(context));

    }
}
