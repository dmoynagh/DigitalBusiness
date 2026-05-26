# Handler Pipeline — Architecture Documentation

This folder contains architecture-level documentation for the `DigitalBusiness.Extensibility.Handlers` component.
The docs are ordered from foundational concepts through to applied patterns. Read them in sequence when learning
the system; jump to individual files when you need a specific reference.

---

## Documents

| # | File | What it covers |
|---|------|----------------|
| 1 | [01-concepts.md](01-concepts.md) | Mental model, core vocabulary, and what problem this solves |
| 2 | [02-execution-model.md](02-execution-model.md) | How the pipeline executes at runtime — lifecycle, caching, stopping |
| 3 | [03-ordering.md](03-ordering.md) | How actions are sorted: stages, weights, and topological key dependencies |
| 4 | [04-flow-control.md](04-flow-control.md) | How a context stops or modifies pipeline execution |
| 5 | [05-conditions.md](05-conditions.md) | Conditional execution — built-in types and composition |
| 6 | [06-context-design.md](06-context-design.md) | How to design context objects and when to add execution control |
| 7 | [07-registration.md](07-registration.md) | All registration overloads, startup wiring, and pipeline setup |
| 8 | [08-patterns.md](08-patterns.md) | Applied patterns, recipes, and anti-patterns |

---

## Quick-start

```csharp
// 1. Register infrastructure (once, in Program.cs / Startup)
services.AddHandlers();

// 2. Register an action
services.AddHandler<MyContext>("MyModule", "DoWork",
	async (ctx, ct) => await DoSomethingAsync(ctx, ct));

// 3. Resolve and run
var handler = serviceProvider.GetRequiredService<Handler<MyContext>>();
await handler.ExecuteAsync(new MyContext(), cancellationToken);
```

See [07-registration.md](07-registration.md) for all overloads and [08-patterns.md](08-patterns.md) for real-world usage.
