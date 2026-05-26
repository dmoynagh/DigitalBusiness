# 01 — Core Concepts

## The Problem

Traditional service-layer code puts all logic for an operation in one place. As applications grow, that
one place accumulates concerns from multiple modules: validation, enrichment, pricing, auditing, notifications.
The result is tight coupling — to add a new concern you must modify existing code, and modules that have
nothing to do with each other end up merged into a single class.

## The Solution

The Handler component models an operation as a **typed context** that flows through a **sequence of
independent actions**. Each module registers only its own actions. Nobody modifies anyone else's code.
The pipeline orders and executes everything automatically.

```
Operation call
	│
	▼
Handler<TContext>.ExecuteAsync(context)
	│
	├─ Action 1 (registered by Module A)
	├─ Action 2 (registered by Module B)
	├─ Action 3 (registered by Module A, conditional)
	└─ Action 4 (registered by Module C)
```

---

## Vocabulary

| Term | Definition |
|------|-----------|
| **Context** | The single object that passes through all actions. It carries input, output, and shared state for the operation. |
| **Handler action** | A single unit of work: a delegate (`async (ctx, ct) => ...`) that reads and/or mutates the context. |
| **Handler** | `Handler<TContext>` — the executor that runs all registered actions in order for a given context type. |
| **Execution plan** | The immutable, sorted list of actions resolved once at startup for a given `TContext`. |
| **Descriptor** | Internal metadata for one registered action: its delegate or factory, condition, ordering options, and identity keys. |
| **Pipeline** | A `sealed record : Pipeline` grouping multiple typed `Handler<TContext>` instances into one injectable object. |
| **Condition** | An `IHandlerCondition<TContext>` predicate that determines at runtime whether an action runs. |
| **Execution control** | Interfaces (`ISkipRemainingExecution`, `ICancelExecution`, `IEndPipelineExecution`) that a context can implement to signal early termination. |
| **Owner / Name** | Diagnostic strings assigned to every registered action — used in error messages and plan summaries. |

---

## Key Design Principles

### 1. The context is the contract
All actions share one context object. The context is both the input and the output of the operation.
Design contexts to be cohesive: one context per logical step.

### 2. Actions are stateless
Actions receive the context and a `CancellationToken`. They should not store state beyond the context itself.
Stateful dependencies (repositories, services) are injected via factory registrations.

### 3. Order is explicit, not positional
Ordering is controlled by `ExecutionStage`, `ExecutionOrder`, and `ExecuteBeforeKeys`/`ExecuteAfterKeys` —
not by registration order. Two modules registered in any order produce the same pipeline.

### 4. Everything is composable
Conditions, flow-control signals, and ordering constraints all compose independently.
A third module can constrain itself relative to existing modules without those modules knowing.

### 5. Startup is the right time to fail
Circular dependencies, invalid stage values, and self-contradicting constraints are detected when
`HandlerExecutionPlan<TContext>` is first resolved — not at runtime. This makes misconfiguration impossible to miss.

---

## Component Map

```
DigitalBusiness.Extensibility.Handlers
│
├─ Handler<TContext>                   ← executor (transient)
├─ HandlerExecutionPlan<TContext>      ← resolved order (singleton)
├─ HandlerActionDescriptor<TContext>   ← one action's metadata (registered as singletons)
├─ HandlerActionOptions                ← ordering configuration
├─ HandlerActionExecutionStage         ← stage enum
├─ Pipeline                            ← base for grouped pipeline records
│
├─ Conditions/
│   ├─ IHandlerCondition<TContext>     ← predicate interface
│   ├─ ContextCondition<TContext>      ← inline Func<> wrapper
│   ├─ AndCondition<TContext>          ← all must pass
│   ├─ OrCondition<TContext>           ← any must pass
│   ├─ NotCondition<TContext>          ← negation
│   └─ TrueCondition<TContext>         ← always passes
│
└─ Execution/
	├─ IHandlerExecution               ← execution state contract
	├─ IHandlerExecutionController     ← context exposes execution state
	├─ ISkipRemainingExecution         ← skip current handler's remaining actions
	├─ ICancelExecution                ← signal operation cancellation
	├─ IEndPipelineExecution           ← stop all subsequent pipeline handlers
	└─ HandlerExecutionSource          ← records which action stopped execution
```

See the other docs in this folder for each area in depth.
