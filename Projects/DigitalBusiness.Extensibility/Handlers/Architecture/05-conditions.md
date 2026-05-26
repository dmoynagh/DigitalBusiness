# 05 — Conditions

A condition is an `IHandlerCondition<TContext>` attached to an action at registration time.
Before invoking the action, the handler evaluates `condition.Applies(context)`.
If it returns `false`, the action is **silently skipped** — no error, no flow-control signal.

---

## Interface

```csharp
public interface IHandlerCondition<in TContext>
{
	bool Applies(TContext context);
}
```

The condition receives the same context object that the action would receive.
It is evaluated on **every execution** — it is not cached.

---

## Built-in Types

### `ContextCondition<TContext>`

Wraps an inline `Func<TContext, bool>`. The most common choice for simple predicates.

```csharp
var condition = new ContextCondition<OrderContext>(ctx => ctx.IsNewOrder);
```

### `TrueCondition<TContext>`

Always returns `true`. Useful as an explicit no-op default, or as a placeholder during development.

```csharp
var condition = new TrueCondition<OrderContext>(); // always runs
```

### `AndCondition<TContext>`

All inner conditions must return `true`. Short-circuits on the first `false`.

```csharp
var condition = new AndCondition<OrderContext>(
	new ContextCondition<OrderContext>(ctx => ctx.IsActive),
	new ContextCondition<OrderContext>(ctx => ctx.HasItems));
```

### `OrCondition<TContext>`

At least one inner condition must return `true`. Short-circuits on the first `true`.

```csharp
var condition = new OrCondition<OrderContext>(
	new ContextCondition<OrderContext>(ctx => ctx.IsExpress),
	new ContextCondition<OrderContext>(ctx => ctx.IsPriority));
```

### `NotCondition<TContext>`

Inverts its inner condition.

```csharp
var condition = new NotCondition<OrderContext>(
	new ContextCondition<OrderContext>(ctx => ctx.IsCancelled));
```

---

## Fluent Composition

Extension methods allow conditions to be composed without directly constructing composite types:

```csharp
var condition = new ContextCondition<OrderContext>(ctx => ctx.IsActive)
	.And(new ContextCondition<OrderContext>(ctx => ctx.HasItems))
	.Or(new ContextCondition<OrderContext>(ctx => ctx.IsOverridden))
	.Not()          // negates the entire expression built so far
	.AndNot(new ContextCondition<OrderContext>(ctx => ctx.IsArchived));
```

| Extension method | Produces |
|-----------------|----------|
| `.And(other)` | `AndCondition` of `this` and `other` |
| `.Or(other)` | `OrCondition` of `this` and `other` |
| `.Not()` | `NotCondition` wrapping `this` |
| `.AndNot(other)` | `AndCondition` of `this` and `NotCondition(other)` |
| `.NotAny(others)` | `AndCondition` of `this` and `Not(Or(others))` |

---

## Attaching a Condition at Registration

Pass a condition as the `condition:` argument to any `AddHandler` overload:

```csharp
services.AddHandler<OrderContext>("Notifications", "SendConfirmation",
	async (ctx, ct) => await notifier.SendAsync(ctx, ct),
	condition: new ContextCondition<OrderContext>(ctx => ctx.EmailConfirmationEnabled));
```

Or compose inline:

```csharp
services.AddHandler<OrderContext>("Pricing", "ApplyLoyaltyDiscount",
	ctx => ApplyDiscount(ctx),
	condition: new ContextCondition<OrderContext>(ctx => ctx.Customer.IsLoyaltyMember)
		.AndNot(new ContextCondition<OrderContext>(ctx => ctx.HasStaffDiscount)));
```

---

## Custom Conditions

Implement `IHandlerCondition<TContext>` directly when the logic is reusable or complex:

```csharp
public sealed class ActiveFeatureCondition<TContext> : IHandlerCondition<TContext>
	where TContext : IFeatureContext
{
	private readonly string _featureName;
	private readonly IFeatureFlags _flags;

	public ActiveFeatureCondition(string featureName, IFeatureFlags flags)
	{
		_featureName = featureName;
		_flags       = flags;
	}

	public bool Applies(TContext context) =>
		_flags.IsEnabled(_featureName, context.TenantId);
}
```

---

## Conditions vs Flow Control

| | Condition | Flow Control |
|---|-----------|-------------|
| Evaluated | Before the action | After the action |
| Affects | This action only | This action and potentially all subsequent actions |
| Visibility | Silent skip | Can be observed via `ExecutionSource`, `CancelReason`, etc. |
| Requires context change | No | Yes (`IHandlerExecutionController`) |

Use a **condition** when the action simply doesn't apply to the current context.
Use **flow control** when an action has determined that further work should stop.
