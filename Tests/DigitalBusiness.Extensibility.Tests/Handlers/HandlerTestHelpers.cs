using DigitalBusiness.Extensibility.Handlers;
using DigitalBusiness.Extensibility.Handlers.Conditions;
using DigitalBusiness.Extensibility.Handlers.Execution;
using Microsoft.Extensions.DependencyInjection;

namespace DigitalBusiness.Extensibility.Tests.Handlers;

// ── Simple context ────────────────────────────────────────────────────────────

internal class TestContext { }

// ── Context that supports early-exit ─────────────────────────────────────────

internal class ControllableContext : IHandlerExecutionController<TestExecution>
{
    private readonly TestExecution _execution = new();
    public TestExecution Execution => _execution;
}

internal class TestExecution : IHandlerExecution
{
    public HandlerExecutionSource? ExecutionSource { get; set; }
    public bool ContinueExecution { get; private set; } = true;
    public void Stop() => ContinueExecution = false;
}

// ── Condition helpers ─────────────────────────────────────────────────────────

internal class AlwaysAppliesCondition<TContext> : IHandlerCondition<TContext>
{
    public bool Applies(TContext context) => true;
}

internal class NeverAppliesCondition<TContext> : IHandlerCondition<TContext>
{
    public bool Applies(TContext context) => false;
}

// ── Factory ───────────────────────────────────────────────────────────────────

internal static class HandlerTestHelpers
{
    internal static Handler<TContext> BuildHandler<TContext>(
        IEnumerable<HandlerActionDescriptor<TContext>> descriptors,
        IServiceProvider? serviceProvider = null)
    {
        var plan = new HandlerExecutionPlan<TContext>(descriptors);
        var sp   = serviceProvider ?? new ServiceCollection().BuildServiceProvider();
        return new Handler<TContext>(plan, new HandlerActionServiceProvider(sp));
    }
}
