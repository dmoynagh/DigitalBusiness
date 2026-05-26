using System;
using System.Collections.Generic;
using System.Text;

namespace DigitalBusiness.Extensibility.Handlers
{
    /// <summary>
    /// Defines default values used throughout the handler pipeline.
    /// </summary>
    public static class HandlerDefaults
    {
        /// <summary>
        /// The default execution order applied when <see cref="HandlerActionOptions.ExecutionOrder"/> is not specified (value: 50).
        /// </summary>
        public const int DefaultExecutionOrder = 50;

        internal const int NormalExecutionStage = (int)HandlerActionExecutionStage.Normal;
        internal const int DefaultExecutionStage = 0;
        internal const int MinExecutionStage = (int)HandlerActionExecutionStage.Init;
        internal const int MaxExecutionStage = (int)HandlerActionExecutionStage.Post;

    }
}
