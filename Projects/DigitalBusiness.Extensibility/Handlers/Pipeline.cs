using System;
using System.Collections.Generic;
using System.Text;

namespace DigitalBusiness.Extensibility.Handlers
{
    /// <summary>
    /// Base class for pipeline definitions that group multiple typed <see cref="Handler{TContext}"/> instances.
    /// </summary>
    /// <remarks>
    /// Derive from this class using a <c>sealed record</c> whose constructor parameters are the
    /// <see cref="Handler{TContext}"/> instances that form the pipeline. Example:
    /// <code>
    /// public sealed record JsonDocumentPipeline(
    ///     Handler&lt;PrepareItemContext&gt;  PrepareItem,
    ///     Handler&lt;BeforeUpdateContext&gt; BeforeUpdate,
    ///     Handler&lt;AfterUpdateContext&gt;  AfterUpdate,
    ///     Handler&lt;BeforeSaveContext&gt;   BeforeSave,
    ///     Handler&lt;CompleteContext&gt;     Complete);
    /// </code>
    /// Register pipelines with <c>AddPipeline&lt;TPipeline&gt;()</c> during startup.
    /// </remarks>
    public abstract class Pipeline
    {
    }
}
