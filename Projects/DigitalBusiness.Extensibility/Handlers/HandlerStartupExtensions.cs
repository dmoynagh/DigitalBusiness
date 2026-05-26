using DigitalBusiness.Extensibility.Handlers.Conditions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System;
using System.Collections.Generic;
using System.Text;

namespace DigitalBusiness.Extensibility.Handlers
{
    /// <summary>
    /// Provides <see cref="IServiceCollection"/> extension methods for registering handlers and pipelines.
    /// </summary>
    public static class HandlerStartupExtensions
    {
        extension(IServiceCollection services)
        {

            /// <summary>
            /// Registers the core handler infrastructure (<see cref="Handler{TContext}"/> and
            /// <see cref="HandlerExecutionPlan{TContext}"/>) with the DI container.
            /// Call this once during application startup.
            /// </summary>
            /// <returns>The same <see cref="IServiceCollection"/> for chaining.</returns>
            public IServiceCollection AddHandlers()
            {
                services.TryAddTransient(typeof(Handler<>), typeof(Handler<>));
                services.TryAddSingleton(typeof(HandlerExecutionPlan<>), typeof(HandlerExecutionPlan<>));

                return services;
            }


            /// <summary>
            /// Registers a <see cref="Pipeline"/>-derived type as a transient service.
            /// </summary>
            /// <typeparam name="TPipeline">The pipeline type to register.</typeparam>
            /// <returns>The same <see cref="IServiceCollection"/> for chaining.</returns>
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

            /// <summary>
            /// Registers an async handler action that receives the context and a <see cref="CancellationToken"/>.
            /// </summary>
            /// <typeparam name="TContext">The context type the handler operates on.</typeparam>
            /// <param name="owner">The owning component or module name (used for diagnostics).</param>
            /// <param name="name">A descriptive name for the action (used for diagnostics).</param>
            /// <param name="handlerAction">The async delegate to invoke.</param>
            /// <param name="condition">An optional condition that must be satisfied for the action to run.</param>
            /// <param name="options">Optional ordering and dependency options.</param>
            /// <returns>The same <see cref="IServiceCollection"/> for chaining.</returns>
            public IServiceCollection AddHandler<TContext>(string owner, string name, Func<TContext, CancellationToken, ValueTask> handlerAction, IHandlerCondition<TContext>? condition = null, HandlerActionOptions? options = null)
                => services.AddHandler(owner, name, handlerAction.ToHandlerActionAsync(), condition, options);

            /// <summary>
            /// Registers an async handler action that receives only the context.
            /// </summary>
            /// <typeparam name="TContext">The context type the handler operates on.</typeparam>
            /// <param name="owner">The owning component or module name (used for diagnostics).</param>
            /// <param name="name">A descriptive name for the action (used for diagnostics).</param>
            /// <param name="handlerAction">The async delegate to invoke.</param>
            /// <param name="condition">An optional condition that must be satisfied for the action to run.</param>
            /// <param name="options">Optional ordering and dependency options.</param>
            /// <returns>The same <see cref="IServiceCollection"/> for chaining.</returns>
            public IServiceCollection AddHandler<TContext>(string owner, string name, Func<TContext, ValueTask> handlerAction, IHandlerCondition<TContext>? condition = null, HandlerActionOptions? options = null)
               => services.AddHandler(owner, name, handlerAction.ToHandlerActionAsync(), condition, options);

            /// <summary>
            /// Registers a synchronous handler action.
            /// </summary>
            /// <typeparam name="TContext">The context type the handler operates on.</typeparam>
            /// <param name="owner">The owning component or module name (used for diagnostics).</param>
            /// <param name="name">A descriptive name for the action (used for diagnostics).</param>
            /// <param name="handlerAction">The synchronous delegate to invoke.</param>
            /// <param name="condition">An optional condition that must be satisfied for the action to run.</param>
            /// <param name="options">Optional ordering and dependency options.</param>
            /// <returns>The same <see cref="IServiceCollection"/> for chaining.</returns>
            public IServiceCollection AddHandler<TContext>(string owner, string name, Action<TContext> handlerAction, IHandlerCondition<TContext>? condition = null, HandlerActionOptions? options = null)
                => services.AddHandler(owner, name, handlerAction.ToHandlerActionAsync(), condition, options);



            internal IServiceCollection AddHandler<TContext>(string owner, string name, HandlerActionAsyncFactory<TContext> handlerActionFactory, IHandlerCondition<TContext>? condition = null, HandlerActionOptions? options = null)
            {                     
                var handlerDescriptor = HandlerActionDescriptor<TContext>.Create(owner, name, handlerActionFactory, condition, options);
                services.AddSingleton<HandlerActionDescriptor<TContext>>(handlerDescriptor);
                return services;
            }

            /// <summary>
            /// Registers a factory-based async handler action that receives the context and a <see cref="CancellationToken"/>.
            /// The factory receives an <see cref="IServiceProvider"/> so it can resolve scoped dependencies at execution time.
            /// </summary>
            /// <typeparam name="TContext">The context type the handler operates on.</typeparam>
            /// <param name="owner">The owning component or module name (used for diagnostics).</param>
            /// <param name="name">A descriptive name for the action (used for diagnostics).</param>
            /// <param name="handlerActionFactory">A factory that produces the async delegate from an <see cref="IServiceProvider"/>.</param>
            /// <param name="condition">An optional condition that must be satisfied for the action to run.</param>
            /// <param name="options">Optional ordering and dependency options.</param>
            /// <returns>The same <see cref="IServiceCollection"/> for chaining.</returns>
            public IServiceCollection AddHandler<TContext>(string owner, string name, Func<IServiceProvider, Func<TContext, CancellationToken, ValueTask>> handlerActionFactory, IHandlerCondition<TContext>? condition = null, HandlerActionOptions? options = null)
                => services.AddHandler(owner, name, handlerActionFactory.ToHandlerActionFactory(), condition, options);

            /// <summary>
            /// Registers a factory-based async handler action that receives only the context.
            /// The factory receives an <see cref="IServiceProvider"/> so it can resolve scoped dependencies at execution time.
            /// </summary>
            /// <typeparam name="TContext">The context type the handler operates on.</typeparam>
            /// <param name="owner">The owning component or module name (used for diagnostics).</param>
            /// <param name="name">A descriptive name for the action (used for diagnostics).</param>
            /// <param name="handlerActionFactory">A factory that produces the async delegate from an <see cref="IServiceProvider"/>.</param>
            /// <param name="condition">An optional condition that must be satisfied for the action to run.</param>
            /// <param name="options">Optional ordering and dependency options.</param>
            /// <returns>The same <see cref="IServiceCollection"/> for chaining.</returns>
            public IServiceCollection AddHandler<TContext>(string owner, string name, Func<IServiceProvider, Func<TContext, ValueTask>> handlerActionFactory, IHandlerCondition<TContext>? condition = null, HandlerActionOptions? options = null)
                => services.AddHandler(owner, name, handlerActionFactory.ToHandlerActionFactory(), condition, options);

            /// <summary>
            /// Registers a factory-based synchronous handler action.
            /// The factory receives an <see cref="IServiceProvider"/> so it can resolve scoped dependencies at execution time.
            /// </summary>
            /// <typeparam name="TContext">The context type the handler operates on.</typeparam>
            /// <param name="owner">The owning component or module name (used for diagnostics).</param>
            /// <param name="name">A descriptive name for the action (used for diagnostics).</param>
            /// <param name="handlerActionFactory">A factory that produces the synchronous delegate from an <see cref="IServiceProvider"/>.</param>
            /// <param name="condition">An optional condition that must be satisfied for the action to run.</param>
            /// <param name="options">Optional ordering and dependency options.</param>
            /// <returns>The same <see cref="IServiceCollection"/> for chaining.</returns>
            public IServiceCollection AddHandler<TContext>(string owner, string name, Func<IServiceProvider, Action<TContext>> handlerActionFactory, IHandlerCondition<TContext>? condition = null, HandlerActionOptions? options = null)
                => services.AddHandler(owner, name, handlerActionFactory.ToHandlerActionFactory(), condition, options);


        }

    }
}
