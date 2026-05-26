
using DigitalBusiness.Extensibility.Handlers.Execution;
using System;

namespace DigitalBusiness.Extensibility.Handlers
{
    //Handler<TContext> instances are not thread - safe and are intended for scoped/transient single-flow execution.
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
