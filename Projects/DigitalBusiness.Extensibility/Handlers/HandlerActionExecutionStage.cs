using System;
using System.Collections.Generic;
using System.Text;

namespace DigitalBusiness.Extensibility.Handlers
{
    public enum HandlerActionExecutionStage
    {
        Default = 0,
        Init = 1,
        Pre = 2,
        Normal = 3,
        Post = 4,
    }
}
