# Handler Pipeline — `DigitalBusiness.Extensibility.Handlers`

## What is it?

The Handler component is a lightweight, **ordered pipeline execution system** built on top of .NET dependency injection. It lets you register any number of discrete actions against a typed context object, then execute them all in a controlled sequence with a single `await handler.ExecuteAsync(context, ct)` call.

It is the primary extensibility mechanism for business operations — instead of placing all logic in one monolithic service, each concern registers its own small action. Actions from different modules or layers are composed into a single execution plan automatically at startup.

---

## What does it add to an application?

- **Decoupled logic** — features from different modules each register their own handler actions independently; nothing needs to know about anything else.
- **Controlled ordering** — ordering is declared explicitly via stage, order weight, and before/after key dependencies rather than being implicit in code structure.
- **Conditional execution** — each action can carry an `IHandlerCondition<TContext>` that gates whether it runs for a given context.
- **Execution flow control** — a context can signal skip, cancel, or end-pipeline at any point during execution.
- **Pipeline composition** — multiple typed handlers can be grouped into a single strongly-typed `Pipeline` record for clean injection.
- **Open/closed extensibility** — later or third-party modules can add handler actions without touching any existing code.

---

## Core Types

### `Handler<TContext>`

The main execution engine. Resolved from DI per request/scope. Holds an immutable, pre-resolved `HandlerExecutionPlan<TContext>` built at startup, and runs each action in order when `ExecuteAsync` is called.

```csharp
await handler.ExecuteAsync(context, cancellationToken);
```

- Not thread-safe — designed for transient/scoped use.
- Registered automatically by `AddHandlers()`.

---

### `Pipeline`

Abstract base class for grouping multiple typed handlers into a single injectable object. Declare pipelines as `sealed record` types:

```csharp
public sealed record OrderPipeline(
	Handler<ValidateOrderContext>  Validate,
	Handler<PriceOrderContext>     Price,
	Handler<SubmitOrderContext>    Submit,
	Handler<CompleteOrderContext>  Complete);
```

Resolved from DI. Each handler in the record is independently executed by the caller in sequence:

```csharp
await pipeline.Validate.ExecuteAsync(ctx, ct);
await pipeline.Price.ExecuteAsync(ctx, ct);
// ...
```

Register with `AddPipeline<TPipeline>()`.

---

### `HandlerActionOptions`

Controls how an action is ordered within the execution plan.

| Property           | Purpose                                                             | Default  |
|--------------------|---------------------------------------------------------------------|----------|
| `ExecutionStage`   | Broad phase bucket: `Init`, `Pre`, `Normal`, `Post`                 | `Normal` |
| `ExecutionOrder`   | Fine-grained order within a stage (lower = earlier)                 | `50`     |
| `Keys`             | Identity keys this action carries (used for dependency targeting)   | —        |
| `ExecuteBeforeKeys`| This action must run before all actions carrying these keys         | —        |
| `ExecuteAfterKeys` | This action must run after all actions carrying these keys          | —        |

---

### `HandlerActionExecutionStage`

Four-phase bucket that forms the coarsest sort tier:

| Stage    | Value | Intended use                            |
|----------|-------|-----------------------------------------|
| `Init`   | 1     | Setup, pre-validation, loading          |
| `Pre`    | 2     | Transformation before the main work     |
| `Normal` | 3     | The primary business logic (default)    |
| `Post`   | 4     | Side-effects, auditing, clean-up        |

`Default` (0) resolves to `Normal` at registration time.

---

### `IHandlerCondition<TContext>`

Optional predicate attached to an action. The action only runs when `Applies(context)` returns `true`.

Built-in implementations:

| Type                        | Behaviour                                  |
|-----------------------------|--------------------------------------------|
| `ContextCondition<TContext>`| Wraps an inline `Func<TContext, bool>`      |
| `AndCondition<TContext>`    | All inner conditions must pass             |
| `OrCondition<TContext>`     | Any inner condition must pass              |
| `NotCondition<TContext>`    | Inverts its inner condition                |
| `TrueCondition<TContext>`   | Always passes — useful as a no-op default  |

Conditions can be composed fluently:

```csharp
var condition = new ContextCondition<MyCtx>(ctx => ctx.IsActive)
	.And(new ContextCondition<MyCtx>(ctx => ctx.HasPermission))
	.AndNot(new ContextCondition<MyCtx>(ctx => ctx.IsReadOnly));
```

---

## Execution Flow Control

Handler actions can optionally control whether subsequent actions run. This requires the context to implement `IHandlerExecutionController`, which exposes an `IHandlerExecution` object.

Three flow-control interfaces extend `IHandlerExecution`:

| Interface                  | Effect                                                      |
|----------------------------|-------------------------------------------------------------|
| `ISkipRemainingExecution`  | Skips remaining actions **in the current handler** only     |
| `ICancelExecution`         | Signals cancellation of the operation being processed       |
| `IEndPipelineExecution`    | Stops execution across **all subsequent handlers** in the pipeline |

Each interface has a `bool` flag (`SkipRemainingRequested`, `CancelRequested`, `EndPipelineRequested`) and an optional `string? Reason` for diagnostics.

Extension methods let actions trigger these in a discoverable way:

```csharp
// Inside a handler action:
context.Execution.SkipRemaining("No further processing needed");
context.Execution.Cancel("Validation failed: item is locked");
context.Execution.EndPipelineExecution("Pipeline halted by authorisation check");
```

When execution stops, `IHandlerExecution.ExecutionSource` is automatically set to identify which action caused it.

---

## How Execution Order is Resolved

At startup, `HandlerExecutionPlan<TContext>` resolves the final execution order from all registered descriptors:

1. **Baseline sort** — all actions sorted by `ExecutionStage` → `ExecutionOrder` → `RegistrationOrder`.
2. **Key map built** — each key is mapped to all actions carrying it.
3. **Constraint validation** — self-contradicting constraints (same key in both `ExecuteBeforeKeys` and `ExecuteAfterKeys`) throw at startup.
4. **Directed graph** — `ExecuteBeforeKeys`/`ExecuteAfterKeys` become directed edges (`A → B` means A runs before B).
5. **Kahn's topological sort** with a min-priority queue — unconstrained actions fall back to the baseline sort order as a tie-breaker.
6. **Cycle detection** — a circular dependency throws `InvalidOperationException` at startup with a full diagnostic summary of the offending actions.

The resolved plan is a frozen singleton `ImmutableArray` — execution at runtime is a plain loop with no recomputation.

---

## Startup Registration

### 1. Register the infrastructure

```csharp
services.AddHandlers();
```

Call once. Registers the open-generic `Handler<>` (transient) and `HandlerExecutionPlan<>` (singleton).

### 2. Register actions

All `AddHandler` overloads accept `owner` (module name for diagnostics) and `name` (action name for diagnostics).

**Synchronous:**
```csharp
services.AddHandler<MyContext>("MyModule", "SetDefaults",
	ctx => ctx.Value ??= "default");
```

**Async (context + CancellationToken):**
```csharp
services.AddHandler<MyContext>("MyModule", "SaveRecord",
	async (ctx, ct) => await repository.SaveAsync(ctx.Item, ct));
```

**With a condition:**
```csharp
services.AddHandler<MyContext>("MyModule", "SendNotification",
	async (ctx, ct) => await notifier.NotifyAsync(ctx, ct),
	condition: new ContextCondition<MyContext>(ctx => ctx.NotificationsEnabled));
```

**With ordering options:**
```csharp
services.AddHandler<MyContext>("MyModule", "Audit",
	ctx => auditLog.Record(ctx),
	options: new HandlerActionOptions
	{
		ExecutionStage = HandlerActionExecutionStage.Post,
		ExecutionOrder = 10
	});
```

**With explicit before/after dependency keys:**
```csharp
// Enums work well as keys
enum OrderKey { Pricing, Tax }

services.AddHandler<MyContext>("Pricing", "CalculatePrice",
	ctx => CalculatePrice(ctx),
	options: new HandlerActionOptions { Keys = [OrderKey.Pricing] });

// Tax is guaranteed to run after Pricing, regardless of registration order
services.AddHandler<MyContext>("Tax", "CalculateTax",
	ctx => CalculateTax(ctx),
	options: new HandlerActionOptions
	{
		Keys             = [OrderKey.Tax],
		ExecuteAfterKeys = [OrderKey.Pricing]
	});
```

**Factory-based (resolves scoped services at execution time):**
```csharp
services.AddHandler<MyContext>("MyModule", "Enrich",
	sp => async (ctx, ct) =>
	{
		var svc = sp.GetRequiredService<IEnrichmentService>();
		await svc.EnrichAsync(ctx, ct);
	});
```

### 3. Register a pipeline

```csharp
services.AddPipeline<OrderPipeline>();
```

### 4. Execute

```csharp
public class OrderService(OrderPipeline pipeline)
{
	public async Task ProcessAsync(OrderContext ctx, CancellationToken ct)
	{
		await pipeline.Validate.ExecuteAsync(ctx, ct);

		if (ctx.Execution.CancelRequested)
			return; // an action stopped the pipeline

		await pipeline.Price.ExecuteAsync(ctx, ct);
		await pipeline.Submit.ExecuteAsync(ctx, ct);
		await pipeline.Complete.ExecuteAsync(ctx, ct);
	}
}
```

---

## Type Reference Summary

| Type                          | Namespace              | Role                                              |
|-------------------------------|------------------------|---------------------------------------------------|
| `Handler<TContext>`            | `Handlers`             | Executes the ordered action list                  |
| `Pipeline`                    | `Handlers`             | Base for grouped handler pipeline records         |
| `HandlerActionOptions`        | `Handlers`             | Ordering and dependency options per action        |
| `HandlerActionExecutionStage` | `Handlers`             | Stage enum (`Init` / `Pre` / `Normal` / `Post`)   |
| `HandlerDefaults`             | `Handlers`             | Default constant values                           |
| `IHandlerCondition<TContext>` | `Handlers.Conditions`  | Predicate interface                               |
| `ContextCondition<TContext>`  | `Handlers.Conditions`  | Inline `Func<TContext, bool>` condition            |
| `AndCondition<TContext>`      | `Handlers.Conditions`  | All-must-pass composite                           |
| `OrCondition<TContext>`       | `Handlers.Conditions`  | Any-must-pass composite                           |
| `NotCondition<TContext>`      | `Handlers.Conditions`  | Negation                                          |
| `TrueCondition<TContext>`     | `Handlers.Conditions`  | Always-true no-op                                 |
| `IHandlerExecution`           | `Handlers.Execution`   | Execution state object                            |
| `IHandlerExecutionController` | `Handlers.Execution`   | Context interface to expose execution state       |
| `ISkipRemainingExecution`     | `Handlers.Execution`   | Skip remaining actions in current handler         |
| `ICancelExecution`            | `Handlers.Execution`   | Signal operation cancellation                     |
| `IEndPipelineExecution`       | `Handlers.Execution`   | Stop all subsequent pipeline handlers             |
| `HandlerExecutionSource`      | `Handlers.Execution`   | Identifies the action that stopped execution      |
| `HandlerContext`              | `Handlers`             | Optional base class for context types             |
