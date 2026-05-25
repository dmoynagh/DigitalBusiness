
using DigitalBusiness.Extensibility.Handlers.Execution;
using System;
using System.Collections.Generic;
using System.IO.Pipes;
using System.Text;

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
            
            var execution = context is IHandlerExecutionController<IHandlerExecution> executionController ? executionController .Execution : null;
      
            if (_hasFactories)
            {
                var count = descriptors.Length;
                for (int i = 0; i < count; i++)
                {
                    if((execution is not null && !execution.ContinueExecution))
                    {
                        break;
                    }

                    var descriptor = descriptors[i];

                    if (descriptor.Action is not null) 
                    {
                        await descriptor.Action(context, cancellationToken);
                    }
                    else
                    {
                        var action = _cachedActions[i] ??= descriptor.ActionFactory!(_serviceProvider.ServiceProvider);
                        await action(context, cancellationToken);
                    }
                    if (execution is not null && !execution.ContinueExecution)
                    {
                        execution.ExecutionSource ??= HandlerExecutionSource.Create(descriptors[i]);
                        break;
                    }

                }   
            }
            else
            {
                for (int i = 0; i < descriptors.Length; i++)
                {
                    if ((execution is not null && !execution.ContinueExecution))
                    {
                        break;
                    }

                    await descriptors[i].Action!(context, cancellationToken);

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
