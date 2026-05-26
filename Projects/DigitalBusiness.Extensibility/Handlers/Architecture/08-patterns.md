# 08 — Patterns

Applied patterns, recipes, and anti-patterns for working with the Handler component.

---

## Patterns

### Pattern 1 — Single-Operation Handler

The simplest shape: one context type, one handler, multiple actions from multiple modules.

```
┌─────────────────────────────────┐
│  Handler<ProcessPaymentContext> │
│                                 │
│  1. ValidateCard    (Payments)  │
│  2. CheckFraud      (Risk)      │
│  3. ReserveAmount   (Ledger)    │
│  4. ChargeCard      (Payments)  │
│  5. SendReceipt     (Notify)    │
└─────────────────────────────────┘
```

Each module registers its own actions independently. The plan composes them in the right order.

---

### Pattern 2 — Staged Pipeline

Use multiple context types when distinct logical phases need different data shapes.
Group them into a `Pipeline` record for clean injection.

```csharp
public sealed record CheckoutPipeline(
	Handler<ValidateBasketContext>  Validate,
	Handler<PriceBasketContext>     Price,
	Handler<CreateOrderContext>     CreateOrder,
	Handler<PaymentContext>         Payment,
	Handler<FulfilmentContext>      Fulfilment);
```

The caller controls which phases run and checks execution state between steps.

---

### Pattern 3 — Module Self-Registration

Each module owns its handler registrations. Nothing outside the module needs to know the detail.

```csharp
// Payments/PaymentsStartup.cs
public static IServiceCollection AddPaymentsModule(this IServiceCollection services)
{
	services.AddHandler<PaymentContext>("Payments", "ValidateCard",
		async (ctx, ct) => await validator.ValidateAsync(ctx.Card, ct));

	services.AddHandler<PaymentContext>("Payments", "ChargeCard",
		sp => async (ctx, ct) =>
		{
			var gateway = sp.GetRequiredService<IPaymentGateway>();
			await gateway.ChargeAsync(ctx, ct);
		},
		options: new HandlerActionOptions { ExecuteAfterKeys = [PaymentKey.Validate] });

	return services;
}
```

```csharp
// Program.cs
services.AddHandlers();
services.AddPaymentsModule();
services.AddFraudModule();     // adds its own actions to PaymentContext
services.AddAuditModule();     // adds its own Post-stage audit action
services.AddPipeline<CheckoutPipeline>();
```

---

### Pattern 4 — Conditional Feature

Use a condition to enable or disable an action based on context state or feature flags.

```csharp
services.AddHandler<OrderContext>("Loyalty", "ApplyLoyaltyDiscount",
	ctx => ApplyDiscount(ctx),
	condition: new ContextCondition<OrderContext>(ctx =>
		ctx.Customer.IsLoyaltyMember && ctx.TotalBeforeDiscount >= 50m));
```

No branching logic inside the action itself — the action either runs or it doesn't.

---

### Pattern 5 — Guard Action (Stop Pipeline on Failure)

Register an early action that cancels the pipeline if a condition isn't met.

```csharp
services.AddHandler<OrderContext>("Validation", "CheckItemAvailability",
	(ctx, ct) =>
	{
		if (ctx.Item.Stock < ctx.Quantity)
			ctx.Execution.Cancel($"Insufficient stock: {ctx.Item.Stock} available");

		return ValueTask.CompletedTask;
	},
	options: new HandlerActionOptions
	{
		ExecutionStage = HandlerActionExecutionStage.Init,
		ExecutionOrder = 1
	});
```

---

### Pattern 6 — Cross-Cutting Post-Stage Action

Register an action in `Post` stage to run after all primary logic, regardless of which module
did the primary work. Useful for audit logging, cache invalidation, metrics.

```csharp
// In an audit module — no knowledge of any other module required
services.AddHandler<OrderContext>("Audit", "RecordOrderChange",
	ctx => auditLog.Record(ctx.Order, ctx.ChangedBy),
	options: new HandlerActionOptions
	{
		ExecutionStage = HandlerActionExecutionStage.Post
	});
```

---

### Pattern 7 — Keyed Dependency Between Modules

Two modules that have a required ordering but are otherwise independent.

```csharp
// Defined in a shared constants file:
enum OrderKey { Tax, Discount, Total }

// Discount module:
services.AddHandler<InvoiceContext>("Discounts", "ApplyDiscount",
	ctx => ApplyDiscount(ctx),
	options: new HandlerActionOptions { Keys = [OrderKey.Discount] });

// Tax module — must run after discounts are applied:
services.AddHandler<InvoiceContext>("Tax", "CalculateTax",
	ctx => CalculateTax(ctx),
	options: new HandlerActionOptions
	{
		Keys             = [OrderKey.Tax],
		ExecuteAfterKeys = [OrderKey.Discount]
	});

// Totals module — must run after both:
services.AddHandler<InvoiceContext>("Invoice", "CalculateTotal",
	ctx => ctx.Total = ctx.Subtotal + ctx.Tax - ctx.Discount,
	options: new HandlerActionOptions
	{
		ExecuteAfterKeys = [OrderKey.Tax, OrderKey.Discount]
	});
```

---

## Anti-Patterns

### ❌ Sharing a `Handler<TContext>` instance across requests

`Handler<TContext>` is transient and holds per-execution factory caches. Storing it as a field
on a singleton service means factory slots are not re-evaluated per request.

```csharp
// WRONG — handler is cached for the lifetime of OrderService (if singleton)
public class OrderService
{
	private readonly Handler<OrderContext> _handler;
	public OrderService(Handler<OrderContext> handler) => _handler = handler;
}
```

```csharp
// CORRECT — resolve per operation, or inject into a scoped/transient service
public class OrderService(IServiceProvider sp)
{
	public async Task RunAsync(OrderContext ctx, CancellationToken ct)
	{
		var handler = sp.GetRequiredService<Handler<OrderContext>>();
		await handler.ExecuteAsync(ctx, ct);
	}
}
```

---

### ❌ Putting business logic inside a condition

Conditions are for gates, not work. Side effects in conditions are invisible and won't be
reflected in `ExecutionSource`.

```csharp
// WRONG
condition: new ContextCondition<OrderContext>(ctx =>
{
	ctx.Price = CalculatePrice(ctx); // side effect in condition!
	return ctx.Price > 0;
})
```

```csharp
// CORRECT — compute in the action, use condition only to gate
services.AddHandler<OrderContext>("Pricing", "CalculatePrice", ctx => ctx.Price = CalculatePrice(ctx));
services.AddHandler<OrderContext>("Pricing", "ValidatePrice",
	ctx => { /* validate */ },
	condition: new ContextCondition<OrderContext>(ctx => ctx.Price > 0));
```

---

### ❌ Encoding order via registration sequence

The topological sort uses `RegistrationOrder` only as a tie-breaker within identical stage/weight.
Never rely on file or method registration order as the primary ordering mechanism.

```csharp
// FRAGILE — relies on registration order in startup
services.AddHandler<Ctx>("A", "First",  ctx => ...);
services.AddHandler<Ctx>("B", "Second", ctx => ...); // what if B is in a different module?
```

```csharp
// ROBUST — order is explicit and survives module reordering
services.AddHandler<Ctx>("A", "First",  ctx => ..., options: new() { Keys = [Key.First] });
services.AddHandler<Ctx>("B", "Second", ctx => ..., options: new() { ExecuteAfterKeys = [Key.First] });
```

---

### ❌ One large context for an entire workflow

A context with 30+ properties for all workflow stages becomes hard to understand and test.

```csharp
// WRONG — one context for everything
public class OrderWorkflowContext
{
	// Validate phase
	public bool IsValid { get; set; }
	// Price phase
	public decimal Price { get; set; }
	// Payment phase
	public string? PaymentRef { get; set; }
	// Fulfilment phase
	public string? TrackingNumber { get; set; }
	// ... 25 more properties
}
```

Prefer a `Pipeline` with separate, focused context types per phase (see Pattern 2 above).

---

## Testing Handler Actions

Because actions are delegates, they can be tested by calling them directly with a constructed context:

```csharp
[Fact]
public async Task ApplyDiscount_SetsDiscountOnContext()
{
	// Arrange
	var ctx = new InvoiceContext { Subtotal = 100m, Customer = LoyalCustomer() };

	// Act — call the action logic directly, no Handler<> needed
	await ApplyLoyaltyDiscount(ctx, CancellationToken.None);

	// Assert
	Assert.Equal(10m, ctx.Discount);
}
```

For integration-level tests that exercise the full pipeline including ordering and conditions,
resolve `Handler<TContext>` from a test `IServiceCollection` and execute against a real context.
