using DigitalBusiness.DependencyInjectionExtensions.ServiceBuildExtensions;
using DigitalBusiness.DependencyInjectionExtensions.ServiceConfig;
using Microsoft.Extensions.DependencyInjection;

namespace DigitalBusiness.DependencyInjectionExtensions.Tests.ServiceBuildExtensions;

public class BuildPipelineFactoryTests
{
    private sealed class RecordingPreBuildAction(List<string> log, string name) : IPreBuildAction
    {
        public void Execute(IServiceCollection services) => log.Add(name);
    }

    private sealed class RecordingCleanupAction(List<string> log, string name) : ICleanupAction
    {
        public void Execute(IServiceCollection services) => log.Add(name);
    }

    private sealed class RecordingInnerFactory : IServiceProviderFactory<IServiceCollection>
    {
        public IServiceCollection? BuilderServices { get; private set; }
        public bool CreateServiceProviderCalled { get; private set; }

        public IServiceCollection CreateBuilder(IServiceCollection services)
        {
            BuilderServices = services;
            return services;
        }

        public IServiceProvider CreateServiceProvider(IServiceCollection containerBuilder)
        {
            CreateServiceProviderCalled = true;
            return containerBuilder.BuildServiceProvider();
        }
    }

    [Fact]
    public void Constructor_NullInner_Throws()
    {
        Assert.Throws<ArgumentNullException>(() => new BuildPipelineFactory<IServiceCollection>(null!));
    }

    [Fact]
    public void CreateBuilder_NullServices_Throws()
    {
        var factory = new BuildPipelineFactory<IServiceCollection>(new RecordingInnerFactory());

        Assert.Throws<ArgumentNullException>(() => factory.CreateBuilder(null!));
    }

    [Fact]
    public void CreateBuilder_DelegatesToInnerFactory()
    {
        var inner = new RecordingInnerFactory();
        var factory = new BuildPipelineFactory<IServiceCollection>(inner);
        var services = new ServiceCollection();

        var result = factory.CreateBuilder(services);

        Assert.Same(services, result);
        Assert.Same(services, inner.BuilderServices);
    }

    [Fact]
    public void CreateServiceProvider_ConfigAbsent_DelegatesDirectlyToInner()
    {
        var inner = new RecordingInnerFactory();
        var factory = new BuildPipelineFactory<IServiceCollection>(inner);
        var services = new ServiceCollection();
        factory.CreateBuilder(services);

        factory.CreateServiceProvider(services);

        Assert.True(inner.CreateServiceProviderCalled);
    }

    [Fact]
    public void CreateServiceProvider_RunsAllPreBuildActionsThenAllCleanupActionsInOrder()
    {
        var log = new List<string>();
        var services = new ServiceCollection();
        services.AddBuildPipeline();
        services.AddPreBuildAction(new RecordingPreBuildAction(log, "pre1"));
        services.AddPreBuildAction(new RecordingPreBuildAction(log, "pre2"));
        services.AddCleanupAction(new RecordingCleanupAction(log, "cleanup1"));
        var factory = new BuildPipelineFactory<IServiceCollection>(new RecordingInnerFactory());
        factory.CreateBuilder(services);

        factory.CreateServiceProvider(services);

        Assert.Equal(["pre1", "pre2", "cleanup1"], log);
    }

    [Fact]
    public void CreateServiceProvider_RunPreBuildActionsFalse_SkipsPreBuildActions()
    {
        var log = new List<string>();
        var services = new ServiceCollection();
        services.AddBuildPipeline();
        services.AddPreBuildAction(new RecordingPreBuildAction(log, "pre1"));
        services.GetConfig<BuildPipelineConfig>()!.RunPreBuildActions = false;
        var factory = new BuildPipelineFactory<IServiceCollection>(new RecordingInnerFactory());
        factory.CreateBuilder(services);

        factory.CreateServiceProvider(services);

        Assert.Empty(log);
    }

    [Fact]
    public void CreateServiceProvider_RunCleanupActionsFalse_SkipsCleanupActionsButStillRemovesBuildPipelineConfig()
    {
        var log = new List<string>();
        var services = new ServiceCollection();
        services.AddBuildPipeline();
        services.AddCleanupAction(new RecordingCleanupAction(log, "cleanup1"));
        services.GetConfig<BuildPipelineConfig>()!.RunCleanupActions = false;
        var factory = new BuildPipelineFactory<IServiceCollection>(new RecordingInnerFactory());
        factory.CreateBuilder(services);

        factory.CreateServiceProvider(services);

        Assert.Empty(log);
        Assert.False(services.HasConfig<BuildPipelineConfig>());
    }

    [Fact]
    public void CreateServiceProvider_ConfigPresent_UnconditionallyRemovesBuildPipelineConfig()
    {
        var services = new ServiceCollection();
        services.AddBuildPipeline();
        var factory = new BuildPipelineFactory<IServiceCollection>(new RecordingInnerFactory());
        factory.CreateBuilder(services);

        factory.CreateServiceProvider(services);

        Assert.False(services.HasConfig<BuildPipelineConfig>());
    }
}
