
using DigitalBusiness.Extensibility.Handlers.Execution;
using System;

namespace DigitalBusiness.Extensibility.Handlers
{
    /// <summary>
    /// Executes an ordered sequence of handler actions against a typed context.
    /// </summary>
    /// <typeparam name="TContext">The context type that is passed to each handler action during execution.</typeparam>
    /// <remarks>
    /// <see cref="Handler{TContext}"/> instances are not thread-safe and are intended for scoped or transient
    /// single-flow execution. Register via <c>AddHandlers()</c> and resolve through DI.
    /// </remarks>
    public sealed class Handler<TContext>
    {
        internal Handler(HandlerExecutionPlan<TContext> executionPlan, HandlerActionServiceProvider serviceProvider)
        {
            _executionPlan = executionPlan;
            _serviceProvider = serviceProvider;

            if(_executionPlan.HasActionFactories)
            {
                _cachedActions = new HandlerActionAsync<TContext>[_executionPlan.HandlerDescriptors.Length];
                _hasFactories = true;
            }
            else
            {
                _cachedActions = Array.Empty<HandlerActionAsync<TContext>>();
                _hasFactories = false;
            }
        }

        private readonly HandlerExecutionPlan<TContext> _executionPlan;
        private readonly HandlerActionServiceProvider _serviceProvider;
        private readonly bool _hasFactories;
        private readonly HandlerActionAsync<TContext>[] _cachedActions;

        /// <summary>
        /// Executes all registered handler actions against the supplied <paramref name="context"/> in their
        /// resolved order, stopping early if execution is halted via <see cref="IHandlerExecutionController"/>.
        /// </summary>
        /// <param name="context">The context passed to each handler action.</param>
        /// <param name="cancellationToken">A token that can be used to cancel asynchronous work within individual actions.</param>
        public async ValueTask ExecuteAsync(TContext context, CancellationToken cancellationToken)
        {
            var descriptors = _executionPlan.HandlerDescriptors;
            
            var execution = context is IHandlerExecutionController executionController ? executionController .Execution : null;
            if ((execution is not null && !execution.ContinueExecution))
            {
                return;               
            }
           
            var count = descriptors.Length;
            for (int i = 0; i < count; i++)
            {                   
                var descriptor = descriptors[i];
                if(descriptor.Condition is null || descriptor.Condition.Applies(context))
                {
                    var action = _hasFactories
                        ? descriptor.Action ?? (_cachedActions[i] ??= descriptor.ActionFactory!(_serviceProvider.ServiceProvider))
                        : descriptor.Action!;

                    await action(context, cancellationToken);

                    if (execution is not null && !execution.ContinueExecution)
                    {
                        execution.ExecutionSource ??= HandlerExecutionSource.Create(descriptors[i]);
                        break;
                    }
                }
            }   
            
        }

    }
}
