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
   
    
}
