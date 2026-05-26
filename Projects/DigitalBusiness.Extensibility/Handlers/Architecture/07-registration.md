# 07 — Registration

All handler registration is done via `IServiceCollection` extension methods in `HandlerStartupExtensions`.

---

## Infrastructure Setup

Call once during application startup, before any `AddHandler` calls:

```csharp
services.AddHandlers();
```

This registers:
- `Handler<>` as **transient** (open generic)
- `HandlerExecutionPlan<>` as **singleton** (open generic)

---

## `AddHandler` Overloads

All overloads share the same signature shape:

```
services.AddHandler<TContext>(owner, name, action/factory, condition?, options?)
```

| Parameter | Type | Purpose |
|-----------|------|---------|
| `owner` | `string` | Module or component name — used in diagnostic output |
| `name` | `string` | Action name — used in diagnostic output |
| `condition` | `IHandlerCondition<TContext>?` | Optional runtime gate (see [05-conditions.md](05-conditions.md)) |
| `options` | `HandlerActionOptions?` | Ordering and dependency constraints (see [03-ordering.md](03-ordering.md)) |

### Synchronous action

```csharp
services.AddHandler<MyContext>("Module", "ActionName",
	ctx => ctx.Value = ComputeValue(ctx));
```

### Async — context + CancellationToken

```csharp
services.AddHandler<MyContext>("Module", "ActionName",
	async (ctx, ct) => await DoWorkAsync(ctx, ct));
```

### Async — context only

```csharp
services.AddHandler<MyContext>("Module", "ActionName",
	async ctx => await DoWorkAsync(ctx));
```

### Factory — async (context + CancellationToken)

The factory receives an `IServiceProvider` scoped to the current request.
Use this to resolve scoped services (repositories, HTTP clients, etc.).

```csharp
services.AddHandler<MyContext>("Module", "ActionName",
	sp => async (ctx, ct) =>
	{
		var repo = sp.GetRequiredService<IMyRepository>();
		await repo.SaveAsync(ctx.Item, ct);
	});
```

### Factory — async (context only)

```csharp
services.AddHandler<MyContext>("Module", "ActionName",
	sp => async ctx =>
	{
		var svc = sp.GetRequiredService<IMyService>();
		await svc.ProcessAsync(ctx.Data);
	});
```

### Factory — synchronous

```csharp
services.AddHandler<MyContext>("Module", "ActionName",
	sp => ctx =>
	{
		var cache = sp.GetRequiredService<IMyCache>();
		cache.Set(ctx.Key, ctx.Value);
	});
```

---

## With a Condition

Pass any `IHandlerCondition<TContext>` as the `condition:` argument:

```csharp
services.AddHandler<OrderContext>("Notifications", "SendEmail",
	async (ctx, ct) => await emailService.SendAsync(ctx, ct),
	condition: new ContextCondition<OrderContext>(ctx => ctx.EmailEnabled));
```

---

## With Ordering Options

Pass a `HandlerActionOptions` instance as the `options:` argument:

```csharp
services.AddHandler<OrderContext>("Audit", "RecordChange",
	ctx => auditLog.Record(ctx),
	options: new HandlerActionOptions
	{
		ExecutionStage   = HandlerActionExecutionStage.Post,
		ExecutionOrder   = 10,
		Keys             = [AuditKey.Record],
		ExecuteAfterKeys = [OrderKey.Submit]
	});
```

---

## Pipeline Registration

Declare a pipeline as a `sealed record` inheriting `Pipeline`:

```csharp
public sealed record OrderPipeline(
	Handler<ValidateOrderContext>  Validate,
	Handler<PriceOrderContext>     Price,
	Handler<SubmitOrderContext>    Submit,
	Handler<CompleteOrderContext>  Complete);
```

Register it with:

```csharp
services.AddPipeline<OrderPipeline>();
```

This registers `OrderPipeline` as **transient**. DI auto-constructs it because its constructor
parameters are `Handler<>` instances already registered by `AddHandlers()`.

---

## Execution

### Single handler

```csharp
public class MyService(Handler<MyContext> handler)
{
	public async Task RunAsync(CancellationToken ct)
	{
		var ctx = new MyContext { ... };
		await handler.ExecuteAsync(ctx, ct);
	}
}
```

### Pipeline

```csharp
public class OrderService(OrderPipeline pipeline)
{
	public async Task ProcessAsync(Order order, CancellationToken ct)
	{
		var execution = new PipelineExecution();

		var validateCtx = new ValidateOrderContext(execution) { Order = order };
		await pipeline.Validate.ExecuteAsync(validateCtx, ct);

		if (!execution.ContinueExecution) return;

		var priceCtx = new PriceOrderContext(execution) { Order = order };
		await pipeline.Price.ExecuteAsync(priceCtx, ct);

		// ... continue for each step
	}
}
```

---

## Registration Placement

Handlers can be registered from any `IServiceCollection`-accessible location:

```csharp
// In a module's own startup extension
public static IServiceCollection AddOrderModule(this IServiceCollection services)
{
	services.AddHandler<OrderContext>("Orders", "ValidateStock",   /* ... */);
	services.AddHandler<OrderContext>("Orders", "ReserveInventory",/* ... */);
	return services;
}

// In Program.cs / Startup.cs
services.AddHandlers();
services.AddOrderModule();
services.AddPricingModule();
services.AddNotificationsModule();
services.AddPipeline<OrderPipeline>();
```

Each module is self-contained. No module needs to know about any other.
