using DigitalBusiness.Extensibility.Handlers.Conditions;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Text;
using System.Xml.Linq;
using static System.Collections.Specialized.BitVector32;

namespace DigitalBusiness.Extensibility.Handlers
{

    internal sealed class HandlerActionDescriptor<TContext>: IHandlerInfo
    {

        public static HandlerActionDescriptor<TContext> Create(
            string owner, string name, HandlerActionAsync<TContext> action, IHandlerCondition<TContext>? condition, HandlerActionOptions? options = null)
        {            
            return new HandlerActionDescriptor<TContext>(
                owner, name, 
                action, 
                condition,
                options?.Keys, 
                options?.ExecuteBeforeKeys, options?.ExecuteAfterKeys,
                (int)(options?.ExecutionStage ?? HandlerDefaults.DefaultExecutionStage),
                options?.ExecutionOrder);
        }

        public static HandlerActionDescriptor<TContext> Create(
           string owner, string name, HandlerActionAsyncFactory<TContext> factory, IHandlerCondition<TContext>? condition, HandlerActionOptions? options = null)
        {
            return new HandlerActionDescriptor<TContext>(owner, name,
                factory,
                condition,
                options?.Keys, options?.ExecuteBeforeKeys, options?.ExecuteAfterKeys, 
                (int)(options?.ExecutionStage ?? HandlerDefaults.DefaultExecutionStage),
                options?.ExecutionOrder
                );            
        }

        public HandlerActionDescriptor(           
            string owner, string name, 
            HandlerActionAsync<TContext> action, IHandlerCondition<TContext>? condition,
            IEnumerable<object>? keys, IEnumerable<object>? executeBeforeKeys, IEnumerable<object>? executeAfterKeys,
            int executionStage, int? executionOrder
            )
            :this(owner, name, condition, keys, executeBeforeKeys, executeAfterKeys, executionStage, executionOrder)
        {
            Action = action ?? throw new ArgumentNullException(nameof(action));                    
        }

        public HandlerActionDescriptor(
            string owner, string name,
            HandlerActionAsyncFactory<TContext> actionFactory, IHandlerCondition<TContext>? condition,
            IEnumerable<object>? keys, IEnumerable<object>? executeBeforeKeys, IEnumerable<object>? executeAfterKeys,
            int executionStage,int? executionOrder
            )
            : this(owner, name, condition, keys, executeBeforeKeys, executeAfterKeys, executionStage, executionOrder)
        {
            ActionFactory = actionFactory ?? throw new ArgumentNullException(nameof(actionFactory));
        }

        private HandlerActionDescriptor(
            string owner, string name,  IHandlerCondition<TContext>? condition,IEnumerable<object>? keys,
            IEnumerable<object>? executeBeforeKeys, IEnumerable<object>? executeAfterKeys,
            int executionStage, int? executionOrder)
        {
            Owner = owner;
            Name = name;

            Condition = condition;

            Keys = keys?.ToImmutableArray();
           
            ExecuteBeforeKeys = executeBeforeKeys?.ToImmutableArray();
            ExecuteAfterKeys = executeAfterKeys?.ToImmutableArray();

            if(executionStage == HandlerDefaults.DefaultExecutionStage)
            {
                executionStage = HandlerDefaults.NormalExecutionStage;
            }            

            if(executionStage < HandlerDefaults.MinExecutionStage || executionStage > HandlerDefaults.MaxExecutionStage)
            {
                throw new ArgumentOutOfRangeException(nameof(executionStage), $"ExecutionStage must be between {HandlerDefaults.MinExecutionStage} and {HandlerDefaults.MaxExecutionStage}.");
            }

            ExecutionStage = executionStage;
            ExecutionOrder = executionOrder;
     
        }

        private static long _registrationOrderCount = 0;
        private static long GetNewRegistrationOrder() => (long)Interlocked.Increment(ref _registrationOrderCount);

        public long RegistrationOrder { get; } = GetNewRegistrationOrder();


        public string Owner { get; }       
        public string Name { get;  }   

        public string FullName => $"{Owner}.{Name}";

        public ImmutableArray<object>? Keys { get;  }     

        public ImmutableArray<object>? ExecuteBeforeKeys { get; } 
        public ImmutableArray<object>? ExecuteAfterKeys { get;  } 

        public int? ExecutionOrder { get;  } 

        public int ExecutionStage { get; }

        public IHandlerCondition<TContext>? Condition { get; }

        public HandlerActionAsync<TContext>? Action { get;  }   
        public HandlerActionAsyncFactory<TContext>? ActionFactory { get;  }     
       
      
        public Type ContextType => typeof(TContext);
    }
   
}

