  
  
  
  
  
  
  
  
  
  
  
  
  
  
  
  
  
  
  
  
  
  
  
  
  
  
  
  
  
  
  
  
  
  
  
  
  
  
  
  
  
  
  
  
  
  
  
  
  
  
  
  
  
  
  
  

Ok - i want to step through this model and implement in stages. The first stage I have created. files are in attached zip file. Can you check and make sure it acheives the model agreed. - A Typed Hook Execution Engine Pipelines as an inherited sealed record class If is ok then we can move to next step which is execution control

  
  
  
  

I have designed it so that the handler does not create a new IServiceProvider Often the dependant services used by the action need to be from the current request scope - particularly as this will often be used in asp.net apps. it may need to get access to already configured services within the request scope. I used HandlerActionServiceProvider so this could be changed if needed and the HandlerActionServiceProvider could create a new scope - this way it is application dependant and the developer can choose behaviour and design. Same with Factory actions currently allocate per invocation the developer defines the service scope in IServiceCollection. Is it worth considering adding a lifetime property to the descriptor. the lifetime can be set at service configuration. if it is scoped then the service result can be cached if not then it is resolved each iteration. it is not possible to tell from IServiceProvider if a service provided has transient or scoped lifetime. HandlerActionDescriptor<TContext> should be immutable. may have been old files as unsaved changes. have attached new . identify any mutable properties check ExcecuteBeforeKeys typo have added service registrations in HandlerStartupExtensions chosen not to make context dependant on a base class - is no need and nothing to gain at this stage. if hander designers want to use basecontext classes in their own design that is ok

  
  
  
  

Changed made. want to discuss caching options for resolved factory services. I think this could be good for performance. I am happy for actions from factory to be resolved once in a handler<> instance which is transient. if you want different behaviour then use a Factory<Handler<>> to get new handler instances each time. This is easier than another setting in descriptor to cache or not. the question is then where and how to store in handler<> often handlers are only invoked once for a instance unless it is operating on a list. so any storage of cached handlers much be light as it may not be used. do you create a dictionary and use the descriptors registration order which is a unique key. do you create an array same length as number of descriptors and use parallel descriptor and array index to check store for resolved delegate? how to implement?

  
  
  
  

revised

  
  
  
  
  
  
  
  
  
  
  
  
  
  
  
  
  
  
  
  
  
  

Yes — that is correct, and I think this is now a very strong and coherent execution-control architecture.

You’ve ended up with:

# capability-based execution semantics

which is exactly the right model for your CMS extensibility engine.

---

# Final Model

## Context Opts Into Execution Control

Example:

```c
public sealed class PublishContext
    : IHandlerExecutionController
{
    public PipelineExecution Execution { get; }
        = new();
}
```

This is excellent because:

-   execution control is optional
-   no forced base class
-   no framework pollution
-   fully compositional

---

# Execution Type Defines Supported Semantics

Example:

```c
public sealed class PipelineExecution
    : ISkipRemainingExecution,
      ICancelExecution
{
    public bool SkipRemaining { get; set; }

    public bool Cancel { get; set; }
}
```

Another:

```c
public sealed class PipelineShortCircuitExecution
    : ISkipRemainingExecution,
      ICancelExecution,
      IEndPipelineExecution
{
    public bool SkipRemaining { get; set; }

    public bool Cancel { get; set; }

    public bool EndPipeline { get; set; }
}
```

Another:

```c
public sealed class HandlerExecution
    : ICancelExecution
{
    public bool Cancel { get; set; }
}
```

This is a very clean model.

---

# Why This Is Good

You now have:

# compositional orchestration semantics

instead of:

-   giant enums
-   giant base contexts
-   monolithic framework assumptions

That is excellent for:

-   CMS extensibility
-   modular features
-   future evolution

---

# The Handler Engine Only Understands Capabilities

Meaning executor logic becomes:

```c
if (execution is ISkipRemainingExecution skip &&
    skip.SkipRemaining)
{
    break;
}

if (execution is IEndPipelineExecution end &&
    end.EndPipeline)
{
    break;
}

if (execution is ICancelExecution cancel &&
    cancel.Cancel)
{
    cancellationToken.ThrowIfCancellationRequested();
}
```

Very simple.  
Very extensible.

---

# Important Clarification

You said:

> determines the execution behaviour supported and is defined by the developer

Correct.

The framework only understands:

# execution capability interfaces

The developer chooses:

-   which capabilities exist
-   which combinations exist
-   which handlers support them

This is the right separation of concerns.

---

# Strong Recommendation

# Use Extension Methods

Absolutely.

Handlers should NOT do:

```c
if (context is IHandlerExecutionController controller)
{
    controller.Execution.SkipRemaining = true;
}
```

That is too noisy and leaks orchestration plumbing.

---

# Recommended Extension Methods

Example:

```c
public static class HandlerExecutionExtensions
{
    public static void SkipRemaining(
        this IHandlerExecutionController controller)
        where TExecution : ISkipRemainingExecution
    {
        controller.Execution.SkipRemaining = true;
    }
}
```

Usage:

```c
context.SkipRemaining();
```

Excellent ergonomics.

---

# More Extensions

```c
context.Cancel();

context.EndPipeline();

context.HasCancelled();

context.ShouldSkipRemaining();
```

This becomes very clean for handler authors.

---

# VERY Important Recommendation

The extension methods should:

# fail silently if unsupported

NOT throw.

Example:

```c
public static void SkipRemaining(
    this object context)
{
    if (context is IHandlerExecutionController controller)
    {
        controller.Execution.SkipRemaining = true;
    }
}
```

Why?

Because:

-   feature handlers may run across many pipelines
-   not all pipelines support same orchestration semantics
-   handlers should remain composable

This becomes especially important in your CMS.

---

# Another Important Improvement

Your execution capability interfaces should probably inherit from:

# marker interface

Example:

```c
public interface IExecutionBehavior
{
}
```

Then:

```c
public interface ISkipRemainingExecution
    : IExecutionBehavior
{
    bool SkipRemaining { get; set; }
}
```

This helps:

-   diagnostics
-   generic inspection
-   future tooling

---

# One Important Semantic Clarification

You currently have:

```
SkipRemaining
EndPipeline
Cancel
```

You should formally define scope semantics.

Example:

| Behavior | Meaning |
| --- | --- |
| SkipRemaining | Stop current handler actions |
| EndPipeline | Stop entire pipeline orchestration |
| Cancel | Abort execution/failure |

Very important for consistency later.

---

# Another Strong Recommendation

Your extension methods should preserve:

# first execution modifier wins

Example:

```c
if (!execution.SkipRemaining)
{
    execution.SkipRemaining = true;

    execution.Source ??= ...
}
```

This becomes important for diagnostics later.

---

# One Architectural Observation

Your model now resembles:

# orchestration capabilities

more than:

# workflow states

That is good.

It keeps the engine:

-   lightweight
-   deterministic
-   high performance

rather than becoming:

-   a workflow runtime
-   a rules engine
-   a state machine framework

Very important distinction.

---

# Final Assessment

Yes — your understanding and proposed architecture are correct.

You now have:

```
Context
    -> optional Execution Controller
        -> developer-defined Execution State
            -> composed Execution Capabilities
```

with:

-   capability interfaces
-   optional orchestration semantics
-   extension-method ergonomics
-   lightweight executor logic
-   compositional execution behavior

This is:

-   elegant
-   extensible
-   low-coupling
-   CMS-appropriate
-   high-performance
-   future-proof

and I think it is the right execution-control model for your system.

  
  

Are you better to split the property to be get only (private set;) and use a method to skip. this would allow the easy addition of a skip reason e.g. public bool SkipRemaining {get;} public void Skip(string reson)

  
  

Yes.  
I think that is a significantly better design.

You are now moving from:

# mutable execution flags

to:

# controlled execution transitions

which is a major architectural improvement.

This gives you:

-   encapsulation
-   validation
-   provenance tracking
-   reason tracking
-   invariant enforcement
-   future extensibility

with almost no downside.

For your architecture:

# methods are the correct approach.

---

# Instead Of

```c
public bool SkipRemaining { get; set; }
```

Use:

```c
public bool SkipRemaining { get; private set; }

public void SkipRemainingExecution(string? reason = null)
{
    SkipRemaining = true;

    Reason ??= reason;
}
```

Much better.

---

# Why This Is Better

With setters:

-   anyone can mutate state arbitrarily
-   no invariants
-   no tracking
-   no lifecycle control
-   impossible to evolve safely

With methods:

-   transitions become explicit
-   execution state becomes authoritative
-   easier diagnostics
-   easier future extension

This is especially important in:

# orchestration systems

---

# Strong Recommendation

# Treat Execution State Like a State Machine

Not just:

```
bag of booleans
```

but:

```
controlled orchestration transitions
```

You are now designing orchestration semantics properly.

---

# Recommended Capability Shape

Instead of:

```c
public interface ISkipRemainingExecution
{
    bool SkipRemaining { get; set; }
}
```

I would now strongly recommend:

```c
public interface ISkipRemainingExecution
{
    bool SkipRemaining { get; }

    string? SkipReason { get; }

    void SkipRemainingExecution(
        string? reason = null);
}
```

This is MUCH better.

---

# Same For Cancel

```c
public interface ICancelExecution
{
    bool Cancelled { get; }

    string? CancelReason { get; }

    void CancelExecution(
        string? reason = null);
}
```

---

# Same For EndPipeline

```c
public interface IEndPipelineExecution
{
    bool EndPipeline { get; }

    string? EndReason { get; }

    void EndPipelineExecution(
        string? reason = null);
}
```

This is now a very strong model.

---

# Why This Architecture Is Excellent

You now have:

# explicit orchestration intent

instead of:

# mutable execution flags

Huge improvement.

---

# Another Important Benefit

You can enforce:

# first transition wins

Example:

```c
public void SkipRemainingExecution(
    string? reason = null)
{
    if (SkipRemaining)
    {
        return;
    }

    SkipRemaining = true;

    SkipReason ??= reason;
}
```

This is extremely valuable.

---

# Another Excellent Future Capability

Now you can easily add:

```c
DateTime Timestamp
```

or:

```c
ExecutionSource
```

or:

```c
HandlerActionDescriptor sourceDescriptor
```

internally during transition.

Very clean evolution path.

---

# VERY Important Improvement

Now extension methods become:

# orchestration helpers

rather than:

# raw state mutation

Example:

```c
context.SkipRemaining(
    "Schema validation failed");
```

Excellent ergonomics.

---

# One More Important Recommendation

You should strongly consider:

# immutable public state

Example:

```c
public bool SkipRemaining { get; private set; }
```

NOT:

```c
protected set;
```

The execution object itself owns state transitions.

This is important.

---

# Another Strong Recommendation

I would probably rename methods slightly.

Instead of:

```
SkipRemainingExecution
```

prefer:

```
SkipRemaining
```

Cleaner:

```c
execution.SkipRemaining("...");
```

or via extension:

```c
context.SkipRemaining("...");
```

Much more natural.

---

# Potential Final Model

Example:

```c
public interface ISkipRemainingExecution
{
    bool SkipRemainingRequested { get; }

    string? SkipReason { get; }

    void SkipRemaining(string? reason = null);
}
```

This naming avoids:

```
property and method same name
```

collision.

---

# Another Excellent Improvement

Now execution transitions become:

# semantic events

Meaning diagnostics later can naturally log:

```
SEOFeature requested SkipRemaining
Reason: Item is excluded from indexing
```

Very powerful for CMS debugging.

---

# IMPORTANT DESIGN DECISION

You should decide:

# can execution state transitions be reversed?

Example:

```
Skip -> Resume?
```

My strong recommendation:

# NO

Execution transitions should be:

# monotonic

Meaning:

-   once skipped
-   once cancelled
-   once ended

cannot be undone.

This massively simplifies reasoning.

Methods allow you to enforce this.

---

# Final Recommendation

Yes — I strongly recommend:

## Read-only public state

```c
bool SkipRemainingRequested { get; }
```

---

## Explicit transition methods

```c
void SkipRemaining(string? reason = null);
```

---

## Optional reason metadata

```c
string? SkipReason { get; }
```

---

## First transition wins

---

## Monotonic transitions

(no reset/unskip)

---

# Result

You now have:

# a real orchestration execution model

rather than:

# mutable execution flags

This is:

-   cleaner
-   safer
-   more extensible
-   easier to debug
-   easier to evolve
-   more deterministic

and absolutely the better design direction for your system.

  
  

ok summarise architecture. approach, types and roles, usage and worklow, implementation

  
  

# CMS Handler Execution Architecture Summary

# Core Architectural Goal

Provide:

# modular extensible application behavior

for a CMS where:

-   content is JSON-based
-   schemas define structure and behavior
-   features/modules contribute functionality
-   behavior composition evolves over time

The system must support:

-   strongly typed execution
-   high performance
-   deterministic ordering
-   dependency injection
-   modular extensibility
-   schema/facet/element conditional behavior
-   request-scoped execution
-   minimal allocations

---

# High-Level Model

```
Pipeline
    -> Handlers
        -> ordered HandlerActions
            -> operate on shared Context
                -> optional Execution Control
```

---

# Core Concepts

| Type | Role |
| --- | --- |
| Pipeline | Groups related handlers |
| Handler<TContext> | Executes ordered actions |
| HandlerAction | Executable delegate |
| HandlerActionDescriptor | Registration metadata |
| Context | Shared execution state |
| ExecutionPlan | Immutable ordered action list |
| Execution Controller | Optional orchestration state |
| Execution Capability | Supported orchestration behavior |

---

# Pipelines

Pipelines group related handlers for a domain/process.

Example:

```c
public sealed record ItemPipeline(
    Handler PrepareItem,
    Handler BeforeSave,
    Handler AfterSave);
```

Benefits:

-   constructor simplification
-   discoverability
-   strong typing
-   explicit orchestration contract

Pipelines are:

-   immutable
-   explicit
-   DI-resolved
-   orchestration-focused

---

# Handlers

A handler is:

# an orchestration execution point

Example:

```c
Handler
```

Responsibilities:

-   execute ordered actions
-   manage execution flow
-   resolve factory actions
-   cache resolved delegates
-   honor execution control

Consumers only interact with:

```c
await handler.ExecuteAsync(context);
```

Consumers do NOT know:

-   how many actions exist
-   ordering
-   execution plan details
-   DI resolution behavior

---

# Contexts

Contexts contain:

-   runtime execution data
-   shared mutable orchestration state

Example:

```c
public sealed class BeforeSaveContext
{
    public required CmsItem Item { get; init; }

    public required JsonNode Data { get; init; }
}
```

Contexts are:

-   strongly typed
-   shared across actions
-   mutable
-   orchestration-aware

---

# Context Inheritance

Supported for shared pipeline execution.

Example:

```c
BaseItemContext
    -> SaveContext
        -> PublishContext
```

Allows:

-   single concrete instance
-   specialized handler views
-   shared state across pipeline

Keep inheritance:

# shallow

Prefer:

-   capability interfaces
-   composition

for orthogonal behavior.

---

# Handler Actions

Handler actions are:

# executable behavior contributions

Typically contributed by:

-   features
-   schema modules
-   facets
-   plugins

Example:

```c
services.AddHandlerAction(
    "seo",
    static async (context, ct) =>
    {
        ...
    });
```

or:

```c
services.AddHandlerAction(
    "audit",
    static (sp, context, ct) =>
    {
        var audit = sp.GetRequiredService();

        return audit.WriteAsync(...);
    });
```

---

# Action Types

Two action styles are supported:

| Type | Purpose |
| --- | --- |
| Static delegate | Fastest execution |
| Factory delegate | Scoped DI resolution |

Factory actions:

-   resolve once per handler instance
-   cache resolved delegate
-   reuse within execution lifetime

---

# HandlerActionDescriptor

Contains:

-   execution metadata
-   ordering metadata
-   applicability metadata
-   delegate/factory

Example responsibilities:

| Property | Purpose |
| --- | --- |
| Name | Action identity |
| Owner | Feature/module |
| ExecutionOrder | Priority |
| ExecutionStage | Pre/Normal/Post |
| Keys | Action identity |
| ExecuteBeforeKeys | Dependency ordering |
| ExecuteAfterKeys | Dependency ordering |
| Action | Static delegate |
| ActionFactory | Scoped resolver |

Descriptors are:

# immutable

after construction.

---

# Execution Plans

Execution plans are:

# immutable ordered descriptor collections

Built once from registrations.

Responsibilities:

-   sorting
-   dependency ordering
-   deterministic execution

Execution plans are:

-   singleton/shared
-   thread-safe
-   immutable

---

# Ordering Model

Primary ordering:

```
ExecutionStage
    -> Priority
        -> RegistrationOrder
```

Secondary ordering:

```
ExecuteBeforeKeys
ExecuteAfterKeys
```

Current implementation:

-   iterative dependency resolution

Future enhancement:

-   DAG/topological sort

---

# Execution Flow

Execution:

```
Pipeline
    -> Handler
        -> ordered actions
            -> mutate shared context
```

Execution is:

# sequential and deterministic

Parallel execution is intentionally NOT part of core engine.

Reasons:

-   shared mutable context
-   orchestration semantics
-   debugging complexity
-   locking/race conditions
-   limited practical CMS benefit

Parallel work should occur:

-   inside handlers
-   or via background/event systems

---

# DI Scope Model

Handlers:

# participate in caller scope

They do NOT create scopes automatically.

This preserves:

-   ASP.NET request scope
-   tenant scope
-   DbContext consistency
-   ambient services

`HandlerActionServiceProvider` abstracts:

-   service resolution behavior
-   future scope strategies

---

# Delegate Caching

Factory actions are:

-   resolved lazily
-   cached per handler instance

Implementation:

-   indexed parallel array cache
-   descriptor index = cache index

Benefits:

-   O(1) lookup
-   no dictionary overhead
-   excellent locality
-   minimal allocations

Handlers are:

# transient execution sessions

not singleton execution engines.

---

# Execution Control

Execution control is:

# optional capability-based orchestration

Contexts opt in via:

```c
IHandlerExecutionController
```

Example:

```c
public sealed class PublishContext
    : IHandlerExecutionController
{
    public PipelineExecution Execution { get; }
        = new();
}
```

---

# Execution Capability Interfaces

Composable orchestration capabilities:

```c
ISkipRemainingExecution
ICancelExecution
IEndPipelineExecution
```

Execution types compose capabilities:

```c
public sealed class PipelineExecution
    : ISkipRemainingExecution,
      ICancelExecution
{
}
```

---

# Execution State Design

Execution state uses:

# controlled transitions

NOT mutable public setters.

Example:

```c
public interface ISkipRemainingExecution
{
    bool SkipRemainingRequested { get; }

    string? SkipReason { get; }

    void SkipRemaining(
        string? reason = null);
}
```

Benefits:

-   encapsulation
-   monotonic transitions
-   diagnostics
-   provenance tracking
-   future extensibility

---

# Execution Provenance

Execution state tracks:

-   who changed execution state
-   why

Example:

```c
public sealed class HandlerExecutionSource
{
    public required string Name { get; init; }

    public string? Owner { get; init; }
}
```

Executor detects state changes after actions.

First modifier wins.

---

# Applicability Filtering

(Future Stage)

Handlers will commonly apply conditionally based on:

-   schema type
-   facets
-   elements
-   owner
-   JSON values
-   feature configuration

Planned model:

```c
IHandlerCondition
```

Examples:

-   schema conditions
-   facet conditions
-   owner conditions

Long-term:

-   precomputed applicability maps
-   schema capability indexing

---

# CMS Schema Model

Schemas behave similarly to:

-   OO classes
-   interfaces
-   metadata contracts

Capabilities:

-   inheritance
-   facets
-   elements
-   behavior contribution

Features contribute:

-   handlers
-   schema extensions
-   validation
-   workflows
-   indexing
-   transformations

---

# Intended Usage Pattern

## Feature Registration

```c
services.AddHandlerAction(
    "seo",
    ...);
```

---

## Pipeline Consumption

```c
public class ItemService
{
    private readonly ItemPipeline _pipeline;
}
```

---

## Execution

```c
await _pipeline.BeforeSave.ExecuteAsync(context);
```

---

# Architectural Characteristics

| Characteristic | Result |
| --- | --- |
| Strong typing | Yes |
| High performance | Yes |
| Deterministic execution | Yes |
| Scoped DI support | Yes |
| Extensible | Yes |
| CMS-oriented | Yes |
| Shared mutable orchestration | Yes |
| Parallel orchestration | No |
| Runtime dynamic workflows | No |
| Reflection invocation | No |

---

# Architectural Style

The system most closely resembles:

| Similar To | Why |
| --- | --- |
| Orchard Core pipelines | CMS extensibility |
| Roslyn analyzers | Typed passes/actions |
| Compiler passes | Ordered transformations |
| Sitecore processors | Modular orchestration |
| ASP.NET endpoint pipelines | Delegate execution |

NOT:

-   event buses
-   workflow engines
-   MediatR-style dispatch
-   middleware chains

---

# Overall Design Philosophy

Optimize for:

-   correctness
-   determinism
-   modularity
-   extensibility
-   maintainability
-   diagnostics
-   low allocations
-   CMS feature evolution

NOT:

-   dynamic runtime scripting
-   distributed orchestration
-   generalized workflow execution
-   high-parallel execution graphs

This architecture is specifically optimized for:

# a modular schema-driven CMS platform.