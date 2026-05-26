using DigitalBusiness.Extensibility.Handlers;
using DigitalBusiness.Extensibility.Handlers.Execution;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;

namespace DigitalBusiness.Extensibility.Tests.Handlers;

public class HandlerTests
{
    // ── helpers ───────────────────────────────────────────────────────────────

    private static HandlerActionDescriptor<TContext> Action<TContext>(
        string name,
        Func<TContext, CancellationToken, ValueTask> action,
        HandlerActionOptions? options = null)
        => HandlerActionDescriptor<TContext>.Create("Test", name,
            (ctx, ct) => action(ctx, ct), null, options);

    private static Handler<TContext> Build<TContext>(
        IEnumerable<HandlerActionDescriptor<TContext>> descriptors,
        IServiceProvider? sp = null)
        => HandlerTestHelpers.BuildHandler(descriptors, sp);

    // ── all actions run ───────────────────────────────────────────────────────

    [Fact]
    public async Task ExecuteAsync_RunsAllActions_InOrder()
    {
        var log = new List<string>();

        var a = Action<TestContext>("A", (_, __) => { log.Add("A"); return ValueTask.CompletedTask; });
        var b = Action<TestContext>("B", (_, __) => { log.Add("B"); return ValueTask.CompletedTask; });
        var c = Action<TestContext>("C", (_, __) => { log.Add("C"); return ValueTask.CompletedTask; });

        await Build([a, b, c]).ExecuteAsync(new TestContext(), CancellationToken.None);

        log.Should().ContainInOrder("A", "B", "C");
    }

    [Fact]
    public async Task ExecuteAsync_EmptyPlan_CompletesWithoutError()
    {
        var act = async () => await Build<TestContext>([])
            .ExecuteAsync(new TestContext(), CancellationToken.None);

        await act.Should().NotThrowAsync();
    }

    // ── condition gating ──────────────────────────────────────────────────────

    [Fact]
    public async Task ExecuteAsync_ConditionFalse_SkipsAction()
    {
        var log = new List<string>();

        var runs = HandlerActionDescriptor<TestContext>.Create("Test", "Runs",
            (_, __) => { log.Add("Runs"); return ValueTask.CompletedTask; },
            new AlwaysAppliesCondition<TestContext>());

        var skipped = HandlerActionDescriptor<TestContext>.Create("Test", "Skipped",
            (_, __) => { log.Add("Skipped"); return ValueTask.CompletedTask; },
            new NeverAppliesCondition<TestContext>());

        await Build([runs, skipped]).ExecuteAsync(new TestContext(), CancellationToken.None);

        log.Should().ContainSingle().Which.Should().Be("Runs");
    }

    [Fact]
    public async Task ExecuteAsync_NullCondition_AlwaysRunsAction()
    {
        var ran = false;
        var d = Action<TestContext>("A", (_, __) => { ran = true; return ValueTask.CompletedTask; });

        await Build([d]).ExecuteAsync(new TestContext(), CancellationToken.None);

        ran.Should().BeTrue();
    }

    // ── early exit via IHandlerExecutionController ────────────────────────────

    [Fact]
    public async Task ExecuteAsync_StopCalled_HaltsPipelineAfterCurrentAction()
    {
        var log = new List<string>();
        var ctx = new ControllableContext();

        var a = HandlerActionDescriptor<ControllableContext>.Create("Test", "A",
            (c, __) => { log.Add("A"); c.Execution.Stop(); return ValueTask.CompletedTask; }, null);

        var b = HandlerActionDescriptor<ControllableContext>.Create("Test", "B",
            (_, __) => { log.Add("B"); return ValueTask.CompletedTask; }, null);

        await Build([a, b]).ExecuteAsync(ctx, CancellationToken.None);

        log.Should().ContainSingle().Which.Should().Be("A");
    }

    [Fact]
    public async Task ExecuteAsync_WhenStopped_SetsExecutionSourceToStoppingHandler()
    {
        var ctx = new ControllableContext();

        var stopper = HandlerActionDescriptor<ControllableContext>.Create("Test", "Stopper",
            (c, __) => { c.Execution.Stop(); return ValueTask.CompletedTask; }, null);

        await Build([stopper]).ExecuteAsync(ctx, CancellationToken.None);

        ctx.Execution.ExecutionSource.Should().NotBeNull();
        ctx.Execution.ExecutionSource!.Name.Should().Be("Stopper");
        ctx.Execution.ExecutionSource!.Owner.Should().Be("Test");
    }

    [Fact]
    public async Task ExecuteAsync_ExecutionSource_NotOverwrittenIfAlreadySet()
    {
        var ctx = new ControllableContext();

        // Pre-stop before execution begins — first action should never run
        ctx.Execution.ExecutionSource = new HandlerExecutionSource("Prior", "Prior", 0);
        ctx.Execution.Stop();

        var a = HandlerActionDescriptor<ControllableContext>.Create("Test", "ShouldNotRun",
            (_, __) => ValueTask.CompletedTask, null);

        await Build([a]).ExecuteAsync(ctx, CancellationToken.None);

        ctx.Execution.ExecutionSource!.Name.Should().Be("Prior");
    }

    [Fact]
    public async Task ExecuteAsync_ContextNotController_RunsAllActionsWithoutEarlyExit()
    {
        var log = new List<string>();

        var a = Action<TestContext>("A", (_, __) => { log.Add("A"); return ValueTask.CompletedTask; });
        var b = Action<TestContext>("B", (_, __) => { log.Add("B"); return ValueTask.CompletedTask; });

        await Build([a, b]).ExecuteAsync(new TestContext(), CancellationToken.None);

        log.Should().ContainInOrder("A", "B");
    }

    // ── factory-based actions ─────────────────────────────────────────────────

    [Fact]
    public async Task ExecuteAsync_FactoryAction_ResolvedAndInvoked()
    {
        var ran = false;
        var sp  = new ServiceCollection().BuildServiceProvider();

        var d = HandlerActionDescriptor<TestContext>.Create("Test", "F",
            _ => (_, __) => { ran = true; return ValueTask.CompletedTask; }, null);

        await Build([d], sp).ExecuteAsync(new TestContext(), CancellationToken.None);

        ran.Should().BeTrue();
    }

    [Fact]
    public async Task ExecuteAsync_FactoryAction_CachedAfterFirstResolution()
    {
        int factoryCallCount = 0;
        var sp = new ServiceCollection().BuildServiceProvider();

        var d = HandlerActionDescriptor<TestContext>.Create("Test", "F", _ =>
        {
            factoryCallCount++;
            return (_, __) => ValueTask.CompletedTask;
        }, null);

        var handler = Build([d], sp);

        // Two executions — factory must only be called once
        await handler.ExecuteAsync(new TestContext(), CancellationToken.None);
        await handler.ExecuteAsync(new TestContext(), CancellationToken.None);

        factoryCallCount.Should().Be(1);
    }

    // ── cancellation ──────────────────────────────────────────────────────────

    [Fact]
    public async Task ExecuteAsync_PassesCancellationTokenToActions()
    {
        using var cts = new CancellationTokenSource();
        CancellationToken received = default;

        var d = Action<TestContext>("A", (_, ct) => { received = ct; return ValueTask.CompletedTask; });

        await Build([d]).ExecuteAsync(new TestContext(), cts.Token);

        received.Should().Be(cts.Token);
    }
}
