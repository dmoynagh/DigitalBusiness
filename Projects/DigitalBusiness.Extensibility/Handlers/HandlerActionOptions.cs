using DigitalBusiness.Extensibility.Handlers.Conditions;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Text;

namespace DigitalBusiness.Extensibility.Handlers
{
    public sealed record class HandlerActionOptions
    {           
        public List<object>? Keys { get; set; }
        public List<object>? ExecuteBeforeKeys { get; set; }
        public List<object>? ExecuteAfterKeys { get; set; }
        public int? ExecutionOrder { get; set; }
        public HandlerActionExecutionStage ExecutionStage { get; set; }  = HandlerActionExecutionStage.Default;

    }
   
    public enum HandlerActionExecutionStage
    {
        Default=0,
        Init=1,
        Pre=2,
        Normal = 3,
        Post =4,
    }
}
