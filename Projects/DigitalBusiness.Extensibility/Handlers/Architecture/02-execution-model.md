# 02 — Execution Model

## Lifecycle Overview

```
Startup
  │
  ├─ HandlerActionDescriptor<TContext> singletons registered (one per AddHandler call)
  └─ HandlerExecutionPlan<TContext> singleton resolved
	   └─ Sorts all descriptors into an ImmutableArray (see 03-ordering.md)

Per request / scope
  │
  └─ Handler<TContext> transient resolved from DI
	   └─ ExecuteAsync(context, ct) called
			├─ Check if already stopped (ContinueExecution == false → return immediately)
			├─ For each descriptor in the plan:
			│    ├─ Evaluate condition (skip if false)
			│    ├─ Resolve action (direct or factory-cached)
			│    ├─ await action(context, ct)
			│    └─ Check ContinueExecution → break + record ExecutionSource if stopped
			└─ Return
```

---

## `Handler<TContext>` — Transient Executor

`Handler<TContext>` is registered as **transient**. Each DI resolution produces a new instance,
but the `HandlerExecutionPlan<TContext>` it holds is a **singleton** shared across all instances.

This means:
- The sorted action list is computed once and reused forever.
- Each handler instance is independent and can be used for one execution without thread-safety concerns.
- Do not cache a `Handler<TContext>` instance; always resolve fresh from DI.

### Constructor Optimisation

At construction time the handler checks whether any registered action uses a factory delegate.
If none do, `_cachedActions` is set to `Array.Empty<>()` — no allocation.
If factories exist, one slot per descriptor is pre-allocated. Slots start `null` and are filled
on first invocation; subsequent executions reuse the cached delegate.

---

## The Execution Loop

```csharp
// Simplified view of what ExecuteAsync does:
var execution = context is IHandlerExecutionController c ? c.Execution : null;

if (execution?.ContinueExecution == false) return; // already stopped by a prior handler

foreach (var descriptor in plan)
{
	if (descriptor.Condition == null || descriptor.Condition.Applies(context))
	{
		var action = ResolveAction(descriptor); // direct or factory-cached
		await action(context, ct);

		if (execution?.ContinueExecution == false)
		{
			execution.ExecutionSource = HandlerExecutionSource.Create(descriptor);
			break;
		}
	}
}
```

Key behaviours:

| Situation | What happens |
|-----------|-------------|
| Context does not implement `IHandlerExecutionController` | All actions run unconditionally; no flow-control checks |
| `ContinueExecution` is already `false` on entry | Method returns immediately without running any action |
| An action sets `ContinueExecution` to `false` | Loop breaks after the current action; `ExecutionSource` is recorded |
| A condition returns `false` | That action is skipped; loop continues with the next |

---

## Factory Actions and Caching

When an action is registered via a factory (`AddHandler` with `Func<IServiceProvider, ...>`),
the factory is called **once per `Handler<TContext>` instance** on first execution, not at startup.

This allows factory actions to resolve **scoped** services from the request `IServiceProvider`:

```csharp
services.AddHandler<MyContext>("Payments", "ChargeCard",
	sp => async (ctx, ct) =>
	{
		// sp is the scoped IServiceProvider for this request
		var gateway = sp.GetRequiredService<IPaymentGateway>();
		await gateway.ChargeAsync(ctx.Order, ct);
	});
```

Because `Handler<TContext>` is transient, each resolution produces a fresh instance with empty
factory slots, so there is no cross-request sharing of resolved services.

---

## DI Lifetime Summary

| Type | Lifetime | Why |
|------|----------|-----|
| `HandlerExecutionPlan<TContext>` | Singleton | Computed once; immutable; safe to share |
| `HandlerActionDescriptor<TContext>` | Singleton | Metadata only; no mutable state |
| `Handler<TContext>` | Transient | One per execution; holds per-execution factory cache |
| `Pipeline` subtypes | Transient | Contains `Handler<>` constructor params; must be transient |
