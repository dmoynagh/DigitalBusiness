using System;
using System.Collections.Generic;
using System.Text;

namespace DigitalBusiness.Extensibility.Handlers
{
    public static class HandlerDefaults
    {
        public const int DefaultExecutionOrder = 50;
        internal const int NormalExecutionStage = (int)HandlerActionExecutionStage.Normal;
        internal const int DefaultExecutionStage = 0;
        internal const int MinExecutionStage = (int)HandlerActionExecutionStage.Init;
        internal const int MaxExecutionStage = (int)HandlerActionExecutionStage.Post;

    }
}
