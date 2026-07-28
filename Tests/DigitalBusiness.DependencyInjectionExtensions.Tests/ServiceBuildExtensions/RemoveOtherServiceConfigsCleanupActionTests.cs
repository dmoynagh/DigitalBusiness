using DigitalBusiness.DependencyInjectionExtensions.ServiceBuildExtensions;
using DigitalBusiness.DependencyInjectionExtensions.ServiceConfig;
using Microsoft.Extensions.DependencyInjection;

namespace DigitalBusiness.DependencyInjectionExtensions.Tests.ServiceBuildExtensions;

public class RemoveOtherServiceConfigsCleanupActionTests
{
    private sealed class FirstConfig;

    private sealed class SecondConfig;

    [Fact]
    public void Execute_RemovesAllServiceConfigDescriptorsRegardlessOfType()
    {
        var services = new ServiceCollection();
        services.GetOrAddConfig(() => new FirstConfig());
        services.GetOrAddConfig(() => new SecondConfig());

        new RemoveOtherServiceConfigsCleanupAction().Execute(services);

        Assert.False(services.HasConfig<FirstConfig>());
        Assert.False(services.HasConfig<SecondConfig>());
    }

    [Fact]
    public void Execute_LeavesUnrelatedDescriptorsUntouched()
    {
        var services = new ServiceCollection();
        services.GetOrAddConfig(() => new FirstConfig());
        services.AddSingleton("unrelated");

        new RemoveOtherServiceConfigsCleanupAction().Execute(services);

        Assert.Contains(services, d => d.ServiceType == typeof(string));
    }

    [Fact]
    public void Execute_AlsoRemovesServiceConfigOfBuildPipelineConfigItself()
    {
        // Documents the actual behavior of the scan as specified: it matches every
        // ServiceConfig<> closed-generic descriptor regardless of T, which includes
        // ServiceConfig<BuildPipelineConfig> itself. BuildPipelineFactory's own unconditional
        // removal step still runs afterward regardless, so BuildPipelineConfig ends up removed
        // either way — this action incidentally removing it first is harmless.
        var services = new ServiceCollection();
        services.AddBuildPipeline();

        new RemoveOtherServiceConfigsCleanupAction().Execute(services);

        Assert.False(services.HasConfig<BuildPipelineConfig>());
    }

    [Fact]
    public void Execute_NoServiceConfigDescriptors_NoOp()
    {
        var services = new ServiceCollection();
        services.AddSingleton("value");

        new RemoveOtherServiceConfigsCleanupAction().Execute(services);

        Assert.Single(services);
    }
}
