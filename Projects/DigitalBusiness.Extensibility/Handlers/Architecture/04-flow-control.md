# 04 — Flow Control

By default, `Handler<TContext>` runs every registered action unconditionally. Flow control is
**opt-in**: the context must implement `IHandlerExecutionController` to expose an `IHandlerExecution`
state object that actions can read and write.

---

## The Contract

```
IHandlerExecutionController
  └─ IHandlerExecution Execution
	   ├─ bool ContinueExecution     ← read by the handler loop
	   ├─ HandlerExecutionSource? ExecutionSource  ← set by Handler when execution stops
	   └─ (extended by flow-control interfaces below)
```

The handler loop checks `ContinueExecution` after every action. If `false`, it records
`ExecutionSource` and breaks. On entry, if `ContinueExecution` is already `false` (set by a
previous handler in a pipeline), the entire handler returns without running any actions.

---

## Flow Control Interfaces

Implement one or more of these on your `IHandlerExecution` class to enable the corresponding signal.
All three are independent and can be combined.

### `ISkipRemainingExecution`

Skips all remaining actions **in the current `Handler<TContext>` only**.
The next handler in the pipeline (if any) still runs from the beginning.

```csharp
public interface ISkipRemainingExecution : IHandlerExecution
{
	bool SkipRemainingRequested { get; }
	string? SkipReason { get; }
	void SkipRemaining(string? reason = null);
}
```

Use when one action determines that the rest of this handler's work is unnecessary,
but the overall operation should continue.

### `ICancelExecution`

Signals that the **operation has been cancelled** (e.g. validation failure, business rule violation).
Stops the current handler and can be inspected by the caller after execution.

```csharp
public interface ICancelExecution : IHandlerExecution
{
	bool CancelRequested { get; }
	string? CancelReason { get; }
	void Cancel(string? reason = null);
}
```

### `IEndPipelineExecution`

Stops all execution — the current handler **and all subsequent handlers** in the pipeline.
Use for authoritative decisions: "nothing further should run at all."

```csharp
public interface IEndPipelineExecution : IHandlerExecution
{
	bool EndPipelineRequested { get; }
	string? EndReason { get; }
	void EndPipelineExecution(string? reason = null);
}
```

---

## Extension Methods

`HandlerExecutionExtensions` provides discoverable helpers so actions don't need to cast:

```csharp
// Check support before calling (returns false if interface not implemented)
if (context.Execution.SupportsSkipRemaining())
	context.Execution.SkipRemaining("Early exit — nothing to do");

if (context.Execution.SupportsCancel())
	context.Execution.Cancel("Item is locked");

if (context.Execution.SupportsEndPipeline())
	context.Execution.EndPipelineExecution("Authorisation denied");

// Read state
bool skipping  = context.Execution.SkipRemainingRequested();
bool cancelled = context.Execution.CancelRequested();
string? reason = context.Execution.SkipReason();
```

---

## `HandlerExecutionSource`

When execution stops, the handler sets `IHandlerExecution.ExecutionSource` to a record containing
the `FullName` of the action that caused it (`"{Owner}.{Name}"`).

Callers can inspect this after execution:

```csharp
await handler.ExecuteAsync(ctx, ct);

if (ctx.Execution.ExecutionSource is { } source)
	logger.LogInformation("Execution stopped by: {Source}", source.FullName);
```

---

## Typical Implementation Pattern

```csharp
// 1. Define execution state
public class OrderExecution : IHandlerExecution, ICancelExecution
{
	public HandlerExecutionSource? ExecutionSource { get; set; }
	public bool ContinueExecution => !CancelRequested;

	public bool CancelRequested { get; private set; }
	public string? CancelReason { get; private set; }

	public void Cancel(string? reason = null)
	{
		CancelRequested = true;
		CancelReason    = reason;
	}
}

// 2. Context implements the controller interface
public class OrderContext : IHandlerExecutionController<OrderExecution>
{
	public OrderExecution Execution { get; } = new();
	IHandlerExecution IHandlerExecutionController.Execution => Execution;

	// ... other context properties
}

// 3. An action signals cancellation
services.AddHandler<OrderContext>("Validation", "CheckItemAvailability",
	(ctx, ct) =>
	{
		if (!ctx.Item.IsAvailable)
			ctx.Execution.Cancel("Item is out of stock");

		return ValueTask.CompletedTask;
	});

// 4. Caller inspects state after execution
await handler.ExecuteAsync(ctx, ct);

if (ctx.Execution.CancelRequested)
{
	// Handle: ctx.Execution.CancelReason contains the reason
}
```

---

## Scope of Each Signal

| Signal | Stops current handler? | Stops subsequent handlers? |
|--------|------------------------|---------------------------|
| `SkipRemaining` | Yes (remaining actions only) | No |
| `Cancel` | Yes | Depends — `ContinueExecution` must return `false` |
| `EndPipelineExecution` | Yes | Yes — checked by every subsequent `Handler<>` on entry |

> `ContinueExecution` is the only property the handler loop reads. If your `IHandlerExecution`
> implementation maps `ContinueExecution` to `!CancelRequested || !EndPipelineRequested`, both
> signals stop the current handler. Only `EndPipelineExecution` affects subsequent handlers
> because the handler checks `ContinueExecution` on **entry** as well as after each action.
