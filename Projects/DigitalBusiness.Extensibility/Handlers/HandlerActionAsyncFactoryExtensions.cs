using System;
using System.Collections.Generic;
using System.Text;

namespace DigitalBusiness.Extensibility.Handlers
{
    internal static class HandlerActionAsyncFactoryExtensions
    {
        internal static HandlerActionAsyncFactory<TContext> ToHandlerActionFactory<TContext>(this Func<IServiceProvider, Func<TContext, CancellationToken, ValueTask>> handlerActionFactory)
            => serviceProvider => new HandlerActionAsync<TContext>(handlerActionFactory(serviceProvider));

        internal static HandlerActionAsyncFactory<TContext> ToHandlerActionFactory<TContext>(this Func<IServiceProvider, Action<TContext>> handlerActionFactory)
            => serviceProvider => new HandlerActionAsync<TContext>((context, cancellationToken) =>
            {
                handlerActionFactory(serviceProvider)(context);
                return ValueTask.CompletedTask;
            });

        internal static HandlerActionAsyncFactory<TContext> ToHandlerActionFactory<TContext>(this Func<IServiceProvider, Func<TContext, ValueTask>> handlerActionFactory)
            => serviceProvider => new HandlerActionAsync<TContext>((context, cancellationToken) => handlerActionFactory(serviceProvider)(context));

    }
}
