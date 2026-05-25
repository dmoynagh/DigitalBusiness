using DigitalBusiness.Extensibility.Handlers.Conditions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System;
using System.Collections.Generic;
using System.Text;

namespace DigitalBusiness.Extensibility.Handlers
{
    public static class HandlerStartupExtensions
    {
        extension(IServiceCollection services)
        {

            public IServiceCollection AddHandlers()
            {
                services.TryAddTransient(typeof(Handler<>), typeof(Handler<>));
                services.TryAddSingleton(typeof(HandlerExecutionPlan<>), typeof(HandlerExecutionPlan<>));

                return services;
            }


            public IServiceCollection AddPipeline<TPipeline>() where TPipeline : Pipeline
            {
                services.AddTransient<TPipeline>();
                return services;
            }

            internal IServiceCollection AddHandler<TContext>(string owner, string name,HandlerActionAsync<TContext> handlerAction, IHandlerCondition<TContext>? condition = null, HandlerActionOptions? options = null)
            {                
                var handlerDescriptor = HandlerActionDescriptor<TContext>.Create(owner, name, handlerAction,condition, options);
                services.AddSingleton<HandlerActionDescriptor<TContext>>(handlerDescriptor);

                return services;
            }

            public IServiceCollection AddHandler<TContext>(string owner, string name, Func<TContext, CancellationToken, ValueTask> handlerAction, IHandlerCondition<TContext>? condition = null, HandlerActionOptions? options = null)
                => services.AddHandler(owner, name, handlerAction.ToHandlerActionAsync(), condition, options);

            public IServiceCollection AddHandler<TContext>(string owner, string name, Func<TContext, ValueTask> handlerAction, IHandlerCondition<TContext>? condition = null, HandlerActionOptions? options = null)
               => services.AddHandler(owner, name, handlerAction.ToHandlerActionAsync(), condition, options);

            public IServiceCollection AddHandler<TContext>(string owner, string name, Action<TContext> handlerAction, IHandlerCondition<TContext>? condition = null, HandlerActionOptions? options = null)
                => services.AddHandler(owner, name, handlerAction.ToHandlerActionAsync(), condition, options);



            internal IServiceCollection AddHandler<TContext>(string owner, string name, HandlerActionAsyncFactory<TContext> handlerActionFactory, IHandlerCondition<TContext>? condition = null, HandlerActionOptions? options = null)
            {                     
                var handlerDescriptor = HandlerActionDescriptor<TContext>.Create(owner, name, handlerActionFactory, condition, options);
                services.AddSingleton<HandlerActionDescriptor<TContext>>(handlerDescriptor);
                return services;
            }

            public IServiceCollection AddHandler<TContext>(string owner, string name, Func<IServiceProvider, Func<TContext, CancellationToken, ValueTask>> handlerActionFactory, IHandlerCondition<TContext>? condition = null, HandlerActionOptions? options = null)
                => services.AddHandler(owner, name, handlerActionFactory.ToHandlerActionFactory(), condition, options);

            public IServiceCollection AddHandler<TContext>(string owner, string name, Func<IServiceProvider, Func<TContext, ValueTask>> handlerActionFactory, IHandlerCondition<TContext>? condition = null, HandlerActionOptions? options = null)
                => services.AddHandler(owner, name, handlerActionFactory.ToHandlerActionFactory(), condition, options);

            public IServiceCollection AddHandler<TContext>(string owner, string name, Func<IServiceProvider, Action<TContext>> handlerActionFactory, IHandlerCondition<TContext>? condition = null, HandlerActionOptions? options = null)
                => services.AddHandler(owner, name, handlerActionFactory.ToHandlerActionFactory(), condition, options);


        }

    }
}
