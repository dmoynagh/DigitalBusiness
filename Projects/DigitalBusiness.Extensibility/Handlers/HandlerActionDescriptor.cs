using DigitalBusiness.Extensibility.Handlers.Conditions;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Text;
using System.Xml.Linq;
using static System.Collections.Specialized.BitVector32;

namespace DigitalBusiness.Extensibility.Handlers
{

    /// <summary>
    /// Describes a single registered handler action, including its identity, ordering constraints,
    /// optional condition, and the action or factory delegate to invoke.
    /// </summary>
    /// <typeparam name="TContext">The context type the action operates on.</typeparam>
    internal sealed class HandlerActionDescriptor<TContext>: IHandlerInfo
    {

        /// <summary>Creates a descriptor from a pre-resolved async action delegate.</summary>
        /// <param name="owner">The owning component or module name.</param>
        /// <param name="name">A descriptive name for the action.</param>
        /// <param name="action">The async delegate to invoke.</param>
        /// <param name="condition">An optional condition controlling whether the action runs.</param>
        /// <param name="options">Optional ordering and dependency options.</param>
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

        /// <summary>Creates a descriptor from a factory delegate that resolves the action from an <see cref="IServiceProvider"/>.</summary>
        /// <param name="owner">The owning component or module name.</param>
        /// <param name="name">A descriptive name for the action.</param>
        /// <param name="factory">A factory that produces the async delegate at execution time.</param>
        /// <param name="condition">An optional condition controlling whether the action runs.</param>
        /// <param name="options">Optional ordering and dependency options.</param>
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

            // Materialise the enumerables to immutable snapshots so callers can't mutate them after registration.
            Keys = keys?.ToImmutableArray();

            ExecuteBeforeKeys = executeBeforeKeys?.ToImmutableArray();
            ExecuteAfterKeys = executeAfterKeys?.ToImmutableArray();

            // Default (0) is a sentinel meaning "use Normal" — resolve it here so the rest of
            // the pipeline can always treat ExecutionStage as a concrete value.
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

        // Global counter shared across all TContext specialisations — gives each descriptor a unique,
        // ever-increasing registration order that is used as a stable tie-breaker during sorting.
        private static long _registrationOrderCount = 0;
        private static long GetNewRegistrationOrder() => (long)Interlocked.Increment(ref _registrationOrderCount);

        /// <summary>Gets the monotonically increasing order in which this descriptor was registered.</summary>
        public long RegistrationOrder { get; } = GetNewRegistrationOrder();

        /// <summary>Gets the owner name (the component or module that registered the action).</summary>
        public string Owner { get; }

        /// <summary>Gets the descriptive name of the action.</summary>
        public string Name { get;  }

        /// <summary>Gets the fully-qualified name in the format <c>{Owner}.{Name}</c>.</summary>
        public string FullName => $"{Owner}.{Name}";

        /// <summary>Gets the keys that identify this descriptor for dependency-ordering purposes.</summary>
        public ImmutableArray<object>? Keys { get;  }

        /// <summary>Gets keys whose associated descriptors must execute after this one.</summary>
        public ImmutableArray<object>? ExecuteBeforeKeys { get; }

        /// <summary>Gets keys whose associated descriptors must execute before this one.</summary>
        public ImmutableArray<object>? ExecuteAfterKeys { get;  }

        /// <summary>Gets the explicit execution order within the stage, or <see langword="null"/> to use the default.</summary>
        public int? ExecutionOrder { get;  }

        /// <summary>Gets the resolved execution stage (never <c>0</c>; <see cref="HandlerDefaults.DefaultExecutionStage"/> is normalised to <see cref="HandlerDefaults.NormalExecutionStage"/>).</summary>
        public int ExecutionStage { get; }

        /// <summary>Gets the optional condition that controls whether this action runs for a given context.</summary>
        public IHandlerCondition<TContext>? Condition { get; }

        /// <summary>Gets the pre-resolved async action delegate, or <see langword="null"/> when a factory is used instead.</summary>
        public HandlerActionAsync<TContext>? Action { get;  }

        /// <summary>Gets the factory delegate used to resolve the action at execution time, or <see langword="null"/> when a direct action is used.</summary>
        public HandlerActionAsyncFactory<TContext>? ActionFactory { get;  }

        /// <summary>Gets the context type this descriptor is registered against.</summary>
        public Type ContextType => typeof(TContext);
    }
   
}

