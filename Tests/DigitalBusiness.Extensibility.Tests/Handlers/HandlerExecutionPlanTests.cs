using DigitalBusiness.Extensibility.Handlers;
using FluentAssertions;

namespace DigitalBusiness.Extensibility.Tests.Handlers;

public class HandlerExecutionPlanTests
{
    // ── helpers ───────────────────────────────────────────────────────────────

    private static ValueTask NoOp(TestContext _, CancellationToken __) => ValueTask.CompletedTask;

    private static HandlerActionDescriptor<TestContext> D(
        string name,
        HandlerActionOptions? options = null)
        => HandlerActionDescriptor<TestContext>.Create("Test", name, NoOp, null, options);

    private static string[] Names(HandlerExecutionPlan<TestContext> plan)
        => [.. plan.HandlerDescriptors.Select(d => d.Name)];

    // ── empty / single ────────────────────────────────────────────────────────

    [Fact]
    public void Empty_ReturnsEmptyPlan()
    {
        var plan = new HandlerExecutionPlan<TestContext>([]);
        plan.HandlerDescriptors.Should().BeEmpty();
    }

    [Fact]
    public void Single_ReturnsSingleDescriptor()
    {
        var plan = new HandlerExecutionPlan<TestContext>([D("A")]);
        plan.HandlerDescriptors.Should().HaveCount(1);
    }

    // ── baseline ordering ─────────────────────────────────────────────────────

    [Fact]
    public void Baseline_OrdersByStage()
    {
        var a = D("A", new HandlerActionOptions { ExecutionStage = HandlerActionExecutionStage.Post });
        var b = D("B", new HandlerActionOptions { ExecutionStage = HandlerActionExecutionStage.Pre });
        var c = D("C", new HandlerActionOptions { ExecutionStage = HandlerActionExecutionStage.Init });

        Names(new HandlerExecutionPlan<TestContext>([a, b, c]))
            .Should().ContainInOrder("C", "B", "A");
    }

    [Fact]
    public void Baseline_SameStage_OrdersByExecutionOrder()
    {
        var a = D("A", new HandlerActionOptions { ExecutionOrder = 20 });
        var b = D("B", new HandlerActionOptions { ExecutionOrder = 10 });
        var c = D("C", new HandlerActionOptions { ExecutionOrder = 30 });

        Names(new HandlerExecutionPlan<TestContext>([a, b, c]))
            .Should().ContainInOrder("B", "A", "C");
    }

    [Fact]
    public void Baseline_SameStageAndOrder_OrdersByRegistration()
    {
        // Descriptors created in this order have ascending RegistrationOrder
        var a = D("A");
        var b = D("B");
        var c = D("C");

        Names(new HandlerExecutionPlan<TestContext>([a, b, c]))
            .Should().ContainInOrder("A", "B", "C");
    }

    // ── ExecuteBeforeKeys ─────────────────────────────────────────────────────

    [Fact]
    public void ExecuteBeforeKeys_MovesHandlerBeforeSingleTarget()
    {
        var target = D("Target", new HandlerActionOptions { Keys = ["key-a"] });
        var mover  = D("Mover",  new HandlerActionOptions { ExecuteBeforeKeys = ["key-a"] });

        // Mover registered after Target — constraint moves it before
        Names(new HandlerExecutionPlan<TestContext>([target, mover]))
            .Should().ContainInOrder("Mover", "Target");
    }

    [Fact]
    public void ExecuteBeforeKeys_MovesBeforeAllHandlersWithSameKey()
    {
        var t1    = D("T1",    new HandlerActionOptions { Keys = ["grp"] });
        var t2    = D("T2",    new HandlerActionOptions { Keys = ["grp"] });
        var mover = D("Mover", new HandlerActionOptions { ExecuteBeforeKeys = ["grp"] });

        var names = Names(new HandlerExecutionPlan<TestContext>([t1, t2, mover]));
        names.Should().ContainInOrder("Mover", "T1");
        names.Should().ContainInOrder("Mover", "T2");
    }

    [Fact]
    public void ExecuteBeforeKeys_MultipleKeys_MovesBeforeAllMatchingHandlers()
    {
        var ta    = D("TA", new HandlerActionOptions { Keys = ["key-a"] });
        var tb    = D("TB", new HandlerActionOptions { Keys = ["key-b"] });
        var mover = D("Mover", new HandlerActionOptions { ExecuteBeforeKeys = ["key-a", "key-b"] });

        var names = Names(new HandlerExecutionPlan<TestContext>([ta, tb, mover]));
        names.Should().ContainInOrder("Mover", "TA");
        names.Should().ContainInOrder("Mover", "TB");
    }

    // ── ExecuteAfterKeys ──────────────────────────────────────────────────────

    [Fact]
    public void ExecuteAfterKeys_MovesHandlerAfterSingleSource()
    {
        var mover  = D("Mover",  new HandlerActionOptions { ExecuteAfterKeys = ["key-a"] });
        var source = D("Source", new HandlerActionOptions { Keys = ["key-a"] });

        // Mover registered before Source — constraint moves it after
        Names(new HandlerExecutionPlan<TestContext>([mover, source]))
            .Should().ContainInOrder("Source", "Mover");
    }

    [Fact]
    public void ExecuteAfterKeys_MovesAfterAllHandlersWithSameKey()
    {
        var s1    = D("S1",    new HandlerActionOptions { Keys = ["grp"] });
        var s2    = D("S2",    new HandlerActionOptions { Keys = ["grp"] });
        var mover = D("Mover", new HandlerActionOptions { ExecuteAfterKeys = ["grp"] });

        var names = Names(new HandlerExecutionPlan<TestContext>([mover, s1, s2]));
        names.Should().ContainInOrder("S1", "Mover");
        names.Should().ContainInOrder("S2", "Mover");
    }

    [Fact]
    public void ExecuteAfterKeys_MultipleKeys_MovesAfterAllMatchingHandlers()
    {
        var sa    = D("SA", new HandlerActionOptions { Keys = ["key-a"] });
        var sb    = D("SB", new HandlerActionOptions { Keys = ["key-b"] });
        var mover = D("Mover", new HandlerActionOptions { ExecuteAfterKeys = ["key-a", "key-b"] });

        var names = Names(new HandlerExecutionPlan<TestContext>([mover, sa, sb]));
        names.Should().ContainInOrder("SA", "Mover");
        names.Should().ContainInOrder("SB", "Mover");
    }

    // ── combined Before + After ───────────────────────────────────────────────

    [Fact]
    public void Combined_BeforeAndAfter_PlacesHandlerInCorrectWindow()
    {
        var early  = D("Early",  new HandlerActionOptions { Keys = ["early"] });
        var late   = D("Late",   new HandlerActionOptions { Keys = ["late"] });
        var middle = D("Middle", new HandlerActionOptions
        {
            ExecuteAfterKeys  = ["early"],
            ExecuteBeforeKeys = ["late"]
        });

        var names = Names(new HandlerExecutionPlan<TestContext>([early, late, middle]));
        names.Should().ContainInOrder("Early", "Middle");
        names.Should().ContainInOrder("Middle", "Late");
    }

    // ── HasActionFactories ────────────────────────────────────────────────────

    [Fact]
    public void HasActionFactories_False_WhenAllAreDirectActions()
    {
        var plan = new HandlerExecutionPlan<TestContext>([D("A"), D("B")]);
        plan.HasActionFactories.Should().BeFalse();
    }

    [Fact]
    public void HasActionFactories_True_WhenAnyDescriptorUsesFactory()
    {
        var factory = HandlerActionDescriptor<TestContext>.Create(
            "Test", "F", _ => (_, __) => ValueTask.CompletedTask, null);

        var plan = new HandlerExecutionPlan<TestContext>([D("A"), factory]);
        plan.HasActionFactories.Should().BeTrue();
    }

    // ── error cases ───────────────────────────────────────────────────────────

    [Fact]
    public void SameKeyInBeforeAndAfter_Throws()
    {
        var conflicted = D("Bad", new HandlerActionOptions
        {
            ExecuteBeforeKeys = ["key-x"],
            ExecuteAfterKeys  = ["key-x"]
        });

        var act = () => new HandlerExecutionPlan<TestContext>([conflicted]);
        act.Should().Throw<InvalidOperationException>()
           .WithMessage("*both ExecuteBeforeKeys and ExecuteAfterKeys*");
    }

    [Fact]
    public void CircularDependency_Throws()
    {
        // A must run before B, B must run before A → cycle
        var a = D("A", new HandlerActionOptions { Keys = ["key-a"], ExecuteBeforeKeys = ["key-b"] });
        var b = D("B", new HandlerActionOptions { Keys = ["key-b"], ExecuteBeforeKeys = ["key-a"] });

        var act = () => new HandlerExecutionPlan<TestContext>([a, b]);
        act.Should().Throw<InvalidOperationException>()
           .WithMessage("*circular dependency*");
    }

    [Fact]
    public void CircularDependency_ErrorMessage_IncludesCycleParticipantCount()
    {
        var a = D("Alpha", new HandlerActionOptions { Keys = ["key-a"], ExecuteBeforeKeys = ["key-b"] });
        var b = D("Beta",  new HandlerActionOptions { Keys = ["key-b"], ExecuteBeforeKeys = ["key-a"] });

        var act = () => new HandlerExecutionPlan<TestContext>([a, b]);
        act.Should().Throw<InvalidOperationException>()
           .WithMessage("*2 handler(s)*");
    }

    [Fact]
    public void UnknownKey_InExecuteAfterKeys_IsIgnoredGracefully()
    {
        // References a key no handler carries — should not throw
        var mover = D("Mover", new HandlerActionOptions { ExecuteAfterKeys = ["nonexistent-key"] });
        var act = () => new HandlerExecutionPlan<TestContext>([mover]);
        act.Should().NotThrow();
    }

    [Fact]
    public void UnknownKey_InExecuteBeforeKeys_IsIgnoredGracefully()
    {
        var mover = D("Mover", new HandlerActionOptions { ExecuteBeforeKeys = ["nonexistent-key"] });
        var act = () => new HandlerExecutionPlan<TestContext>([mover]);
        act.Should().NotThrow();
    }
}
