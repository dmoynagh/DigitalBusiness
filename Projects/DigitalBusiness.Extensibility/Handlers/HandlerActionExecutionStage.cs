using System;
using System.Collections.Generic;
using System.Text;

namespace DigitalBusiness.Extensibility.Handlers
{
    /// <summary>
    /// Defines the broad execution phase in which a handler action is placed within the pipeline.
    /// Actions are sorted first by stage, then by <see cref="HandlerActionOptions.ExecutionOrder"/>, then by registration order.
    /// </summary>
    public enum HandlerActionExecutionStage
    {
        /// <summary>Resolves to <see cref="Normal"/> at registration time.</summary>
        Default = 0,

        /// <summary>Earliest phase — use for setup or pre-validation work.</summary>
        Init = 1,

        /// <summary>Runs after <see cref="Init"/> and before <see cref="Normal"/>.</summary>
        Pre = 2,

        /// <summary>Standard execution phase for the majority of handler actions.</summary>
        Normal = 3,

        /// <summary>Runs after <see cref="Normal"/> — use for clean-up, auditing, or side-effects.</summary>
        Post = 4,
    }
}
