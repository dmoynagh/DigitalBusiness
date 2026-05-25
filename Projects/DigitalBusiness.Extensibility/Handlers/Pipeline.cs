using System;
using System.Collections.Generic;
using System.Text;

namespace DigitalBusiness.Extensibility.Handlers
{
    //Build derived pipelines from this class
    //as a sealed record class is easiest.
    //example below
    //  public sealed record JsonDocumentPipeline(
    //      Handler<PrepareItemContext> PrepareItem,
    //      Handler<BeforeUpdateContext> BeforeUpdate,
    //      Handler<AfterUpdateContext> AfterUpdate,
    //      Handler<BeforeSaveContext> BeforeSave,
    //      Handler<CompleteContext> Complete);


    public abstract class Pipeline
    {
    }
}
