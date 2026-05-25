
  

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