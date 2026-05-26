# 06 — Context Design

The context object is the central design decision when using the Handler component.
It is the interface between the caller, the handler actions, and any post-execution inspection.

---

## Minimal Context

If you don't need flow control, a plain class or record is sufficient:

```csharp
public class SendEmailContext
{
	public required string To      { get; init; }
	public required string Subject { get; init; }
	public required string Body    { get; init; }

	// Populated by handler actions:
	public bool Sent           { get; set; }
	public string? FailReason  { get; set; }
}
```

Actions read and write properties freely. The caller inspects the context after execution.

---

## Context with Execution Control

To support flow-control signals (skip, cancel, end-pipeline), the context must:

1. Have an `IHandlerExecution` implementation as a property.
2. Implement `IHandlerExecutionController` (or the generic variant) to expose it.

### Step 1 — Define the execution state

```csharp
public class OrderExecution : IHandlerExecution, ICancelExecution, IEndPipelineExecution
{
	// Required by IHandlerExecution
	public HandlerExecutionSource? ExecutionSource { get; set; }

	// ContinueExecution drives the handler loop
	public bool ContinueExecution => !CancelRequested && !EndPipelineRequested;

	// ICancelExecution
	public bool    CancelRequested { get; private set; }
	public string? CancelReason    { get; private set; }
	public void Cancel(string? reason = null) { CancelRequested = true; CancelReason = reason; }

	// IEndPipelineExecution
	public bool    EndPipelineRequested { get; private set; }
	public string? EndReason            { get; private set; }
	public void EndPipelineExecution(string? reason = null) { EndPipelineRequested = true; EndReason = reason; }
}
```

### Step 2 — Implement the controller on the context

```csharp
public class OrderContext : IHandlerExecutionController<OrderExecution>
{
	public OrderExecution Execution { get; } = new();

	// Explicit implementation bridges the non-generic interface
	IHandlerExecution IHandlerExecutionController.Execution => Execution;

	// Context properties
	public required Order Order { get; init; }
	public decimal FinalPrice   { get; set; }
}
```

### Step 3 — Check state after execution

```csharp
await handler.ExecuteAsync(ctx, ct);

if (ctx.Execution.CancelRequested)
{
	logger.LogWarning("Order cancelled: {Reason}", ctx.Execution.CancelReason);
	return ValidationResult.Fail(ctx.Execution.CancelReason);
}
```

---

## Shared Execution State Across a Pipeline

When a pipeline contains multiple handlers and you want `EndPipelineExecution` from one handler
to stop all subsequent handlers, use the **same execution object** across all contexts,
or have each context's `ContinueExecution` defer to a shared state.

Pattern: shared execution object injected into all contexts:

```csharp
public class PipelineExecution : IHandlerExecution, IEndPipelineExecution
{
	public HandlerExecutionSource? ExecutionSource { get; set; }
	public bool ContinueExecution     => !EndPipelineRequested;
	public bool    EndPipelineRequested { get; private set; }
	public string? EndReason            { get; private set; }
	public void EndPipelineExecution(string? reason = null) { EndPipelineRequested = true; EndReason = reason; }
}

// Each context holds a reference to the shared execution
public class ValidateOrderContext : IHandlerExecutionController
{
	public ValidateOrderContext(PipelineExecution execution) => Execution = execution;
	public IHandlerExecution Execution { get; }
	// ...
}

public class PriceOrderContext : IHandlerExecutionController
{
	public PriceOrderContext(PipelineExecution execution) => Execution = execution;
	public IHandlerExecution Execution { get; }
	// ...
}

// Caller creates the shared state and passes it to each context
var execution = new PipelineExecution();
var validateCtx = new ValidateOrderContext(execution) { ... };
var priceCtx    = new PriceOrderContext(execution) { ... };

await pipeline.Validate.ExecuteAsync(validateCtx, ct);
await pipeline.Price.ExecuteAsync(priceCtx, ct); // skipped if execution stopped above
```

---

## Optional Base Class

`HandlerContext` is an abstract base class available if you want a shared root type for contexts:

```csharp
public class MyContext : HandlerContext
{
	// ...
}
```

It has no members — it exists as a marker/convention base only.

---

## `IHandlerContextSchema`

`IHandlerContextSchema` is a marker interface for schema/metadata types associated with a context.
Use it to attach strongly-typed schema information without mixing it into the context class itself.

---

## Design Checklist

| Question | Guidance |
|----------|----------|
| Does the caller need to know if execution was cancelled or stopped? | Implement `IHandlerExecutionController` |
| Do multiple handlers share state? | Use a shared `IHandlerExecution` instance |
| Are there properties only some actions write? | Keep them on the context; actions that don't use them simply ignore them |
| Should a context be immutable input only? | Yes — but then you cannot signal flow control or write results back |
| Is the context complex? | Consider splitting into multiple focused contexts with a pipeline |
