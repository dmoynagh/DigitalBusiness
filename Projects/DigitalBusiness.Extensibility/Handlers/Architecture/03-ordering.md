# 03 — Ordering

The execution plan resolves the final action order once at startup using a two-phase algorithm:
a baseline priority sort followed by a topological sort for declared dependencies.

---

## Phase 1 — Baseline Sort

All registered actions are sorted by three keys, in priority order:

```
ExecutionStage  →  ExecutionOrder  →  RegistrationOrder
```

### `ExecutionStage`

Four named phases. Actions in an earlier stage always run before actions in a later stage,
regardless of any other setting.

| Stage | Int value | Intended use |
|-------|-----------|--------------|
| `Init` | 1 | Environment setup, feature-flag checks, data loading |
| `Pre` | 2 | Normalisation, transformation before main logic |
| `Normal` | 3 | Primary business logic (**default**) |
| `Post` | 4 | Auditing, notifications, cache invalidation, cleanup |

`Default` (0) resolves to `Normal` (3) at registration time.

### `ExecutionOrder`

An integer within a stage. Lower values run first. Default is `50`.

Use this to fine-tune within a stage without needing key dependencies:

```csharp
// Runs before everything else in Normal (default order = 50)
options: new HandlerActionOptions { ExecutionOrder = 10 }
```

### `RegistrationOrder`

A global monotonic counter assigned at descriptor creation. Provides a stable tie-breaker
so two actions with identical stage/order always produce a deterministic sequence.

---

## Phase 2 — Topological Sort (Key Dependencies)

When the baseline sort alone is insufficient — e.g. two modules in the same stage have a
required ordering between them — use key dependencies.

### Keys

A key is any `object` used as an identity marker. Enums are the recommended choice.

```csharp
enum InvoiceKey { Tax, Discount, Total }
```

An action declares its own keys and/or references keys of other actions:

| Property | Meaning |
|----------|---------|
| `Keys` | "I am known by these keys" |
| `ExecuteBeforeKeys` | "I must run before every action that carries any of these keys" |
| `ExecuteAfterKeys` | "I must run after every action that carries any of these keys" |

### Example

```csharp
// Tax declares itself as OrderKey.Tax and must run after OrderKey.Pricing
services.AddHandler<OrderContext>("Tax", "CalculateTax",
	ctx => ApplyTax(ctx),
	options: new HandlerActionOptions
	{
		Keys             = [InvoiceKey.Tax],
		ExecuteAfterKeys = [InvoiceKey.Pricing]
	});

// Pricing declares itself as OrderKey.Pricing (no after/before needed here)
services.AddHandler<OrderContext>("Pricing", "CalculatePrice",
	ctx => ApplyPricing(ctx),
	options: new HandlerActionOptions
	{
		Keys = [InvoiceKey.Pricing]
	});
```

Even if Tax is registered first, the plan guarantees Pricing runs before Tax.

### Algorithm

1. Build a directed edge for every `ExecuteAfterKeys`/`ExecuteBeforeKeys` constraint.
2. Apply **Kahn's algorithm** with a min-priority queue seeded by the baseline sort indices.
3. When multiple actions are ready to emit (zero in-degree), the one with the lower baseline index goes first — preserving priority within unconstrained groups.
4. If any node cannot be emitted (cycle), startup throws `InvalidOperationException` with a full
   diagnostic listing all affected actions, their stages, orders, and declared dependencies.

### Constraints Detected at Startup

| Violation | Error |
|-----------|-------|
| Same key in both `ExecuteBeforeKeys` and `ExecuteAfterKeys` on the same action | Startup exception |
| Circular dependency between two or more actions | Startup exception with cycle summary |

---

## Ordering Decision Guide

| Goal | Use |
|------|-----|
| Run in the right broad phase | `ExecutionStage` |
| Run early/late within a stage | `ExecutionOrder` |
| Run before/after a specific module's action | `ExecuteBeforeKeys` / `ExecuteAfterKeys` |
| Keep a small module fast with no ordering concerns | Omit all options (defaults apply) |

---

## Diagnostic Summary

When a cycle is detected, the error includes a formatted table like:

```
Owner         Name             Stage    Order  Before          After
──────────────────────────────────────────────────────────────────────
Pricing       CalculatePrice   Normal   50     [Tax]
Tax           CalculateTax     Normal   50                     [Pricing]
Total         CalculateTotal   Normal   50                     [Tax]
```

This makes it straightforward to identify the conflicting constraints.
