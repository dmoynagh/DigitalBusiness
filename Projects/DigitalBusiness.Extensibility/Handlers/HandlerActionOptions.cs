using DigitalBusiness.Extensibility.Handlers.Conditions;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Text;

namespace DigitalBusiness.Extensibility.Handlers
{
    /// <summary>
    /// Configures the ordering and dependency constraints for a handler action registration.
    /// </summary>
    public sealed record class HandlerActionOptions
    {
        /// <summary>
        /// Gets or sets the keys that identify this handler action for dependency ordering purposes.
        /// Other actions can reference these keys in <see cref="ExecuteBeforeKeys"/> or <see cref="ExecuteAfterKeys"/>.
        /// </summary>
        public List<object>? Keys { get; set; }

        /// <summary>
        /// Gets or sets keys identifying handler actions that this action must execute <em>before</em>.
        /// </summary>
        public List<object>? ExecuteBeforeKeys { get; set; }

        /// <summary>
        /// Gets or sets keys identifying handler actions that this action must execute <em>after</em>.
        /// </summary>
        public List<object>? ExecuteAfterKeys { get; set; }

        /// <summary>
        /// Gets or sets an explicit execution order within the same <see cref="HandlerActionExecutionStage"/>.
        /// Defaults to <see cref="HandlerDefaults.DefaultExecutionOrder"/> when <see langword="null"/>.
        /// </summary>
        public int? ExecutionOrder { get; set; }

        /// <summary>
        /// Gets or sets the execution stage that controls the broad phase in which this action runs.
        /// Defaults to <see cref="HandlerActionExecutionStage.Default"/>, which resolves to <see cref="HandlerActionExecutionStage.Normal"/>.
        /// </summary>
        public HandlerActionExecutionStage ExecutionStage { get; set; }  = HandlerActionExecutionStage.Default;

    }
   
    
}
