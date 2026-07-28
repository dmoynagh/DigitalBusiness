using DigitalBusiness.DependencyInjectionExtensions.ServiceBuildExtensions;
using DigitalBusiness.DependencyInjectionExtensions.ServiceConfig;
using Microsoft.Extensions.DependencyInjection;

namespace DigitalBusiness.DependencyInjectionExtensions.Tests.ServiceBuildExtensions;

public class BuildPipelineExtensionsTests
{
    private sealed class TestPreBuildAction : IPreBuildAction
    {
        public void Execute(IServiceCollection services) { }
    }

    private sealed class TestCleanupAction : ICleanupAction
    {
        public void Execute(IServiceCollection services) { }
    }

    [Fact]
    public void AddBuildPipeline_FirstCall_InstallsConfigWithDefaultCleanupAction()
    {
        var services = new ServiceCollection();

        services.AddBuildPipeline();

        var config = services.GetConfig<BuildPipelineConfig>();
        Assert.NotNull(config);
        Assert.IsType<RemoveOtherServiceConfigsCleanupAction>(Assert.Single(config!.CleanupActions));
    }

    [Fact]
    public void AddBuildPipeline_RepeatCall_DoesNotDuplicateDefaultCleanupAction()
    {
        var services = new ServiceCollection();

        services.AddBuildPipeline();
        services.AddBuildPipeline();

        var config = services.GetConfig<BuildPipelineConfig>();
        Assert.Single(config!.CleanupActions);
    }

    [Fact]
    public void AddBuildPipeline_NullServices_Throws()
    {
        IServiceCollection services = null!;

        Assert.Throws<ArgumentNullException>(() => services.AddBuildPipeline());
    }

    [Fact]
    public void AddPreBuildAction_BeforeAddBuildPipeline_Throws()
    {
        var services = new ServiceCollection();

        Assert.Throws<InvalidOperationException>(() => services.AddPreBuildAction(new TestPreBuildAction()));
    }

    [Fact]
    public void AddPreBuildAction_AfterAddBuildPipeline_AppendsAction()
    {
        var services = new ServiceCollection();
        services.AddBuildPipeline();
        var action = new TestPreBuildAction();

        services.AddPreBuildAction(action);

        var config = services.GetConfig<BuildPipelineConfig>();
        Assert.Same(action, Assert.Single(config!.PreBuildActions));
    }

    [Fact]
    public void AddPreBuildAction_NullAction_Throws()
    {
        var services = new ServiceCollection();
        services.AddBuildPipeline();

        Assert.Throws<ArgumentNullException>(() => services.AddPreBuildAction(null!));
    }

    [Fact]
    public void AddPreBuildAction_NullServices_Throws()
    {
        IServiceCollection services = null!;

        Assert.Throws<ArgumentNullException>(() => services.AddPreBuildAction(new TestPreBuildAction()));
    }

    [Fact]
    public void AddCleanupAction_BeforeAddBuildPipeline_Throws()
    {
        var services = new ServiceCollection();

        Assert.Throws<InvalidOperationException>(() => services.AddCleanupAction(new TestCleanupAction()));
    }

    [Fact]
    public void AddCleanupAction_AfterAddBuildPipeline_AppendsAction()
    {
        var services = new ServiceCollection();
        services.AddBuildPipeline();
        var action = new TestCleanupAction();

        services.AddCleanupAction(action);

        var config = services.GetConfig<BuildPipelineConfig>();
        Assert.Equal(2, config!.CleanupActions.Count);
        Assert.Same(action, config.CleanupActions[1]);
    }

    [Fact]
    public void AddCleanupAction_NullAction_Throws()
    {
        var services = new ServiceCollection();
        services.AddBuildPipeline();

        Assert.Throws<ArgumentNullException>(() => services.AddCleanupAction(null!));
    }

    [Fact]
    public void AddCleanupAction_NullServices_Throws()
    {
        IServiceCollection services = null!;

        Assert.Throws<ArgumentNullException>(() => services.AddCleanupAction(new TestCleanupAction()));
    }
}
