using DigitalBusiness.DependencyInjectionExtensions.ServiceBuildExtensions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace DigitalBusiness.DependencyInjectionExtensions.Tests.ServiceBuildExtensions;

public class ServiceBuildExtensionsHostExtensionsTests
{
    private sealed class DelegatePreBuildAction(Action<IServiceCollection> execute) : IPreBuildAction
    {
        public void Execute(IServiceCollection services) => execute(services);
    }

    private sealed class RecordingInnerFactory : IServiceProviderFactory<IServiceCollection>
    {
        public bool CreateServiceProviderCalled { get; private set; }

        public IServiceCollection CreateBuilder(IServiceCollection services) => services;

        public IServiceProvider CreateServiceProvider(IServiceCollection containerBuilder)
        {
            CreateServiceProviderCalled = true;
            return containerBuilder.BuildServiceProvider();
        }
    }

    [Fact]
    public void UseServiceBuildExtensions_HostBuilderNoArgs_NullHost_Throws()
    {
        IHostBuilder host = null!;

        Assert.Throws<ArgumentNullException>(() => host.UseServiceBuildExtensions());
    }

    [Fact]
    public void UseServiceBuildExtensions_HostBuilderWithInner_NullInner_Throws()
    {
        IHostBuilder host = new HostBuilder();

        Assert.Throws<ArgumentNullException>(() => host.UseServiceBuildExtensions<IServiceCollection>(null!));
    }

    [Fact]
    public void UseServiceBuildExtensions_HostBuilderNoArgs_InstallsPipelineAndRunsPreBuildActions()
    {
        var executed = false;
        IHostBuilder hostBuilder = new HostBuilder();

        hostBuilder.UseServiceBuildExtensions();
        hostBuilder.ConfigureServices((_, services) =>
            services.AddPreBuildAction(new DelegatePreBuildAction(_ => executed = true)));

        using var host = hostBuilder.Build();

        Assert.True(executed);
    }

    [Fact]
    public void UseServiceBuildExtensions_HostBuilderWithExplicitInner_DelegatesToProvidedFactory()
    {
        var inner = new RecordingInnerFactory();
        IHostBuilder hostBuilder = new HostBuilder();

        hostBuilder.UseServiceBuildExtensions(inner);
        using var host = hostBuilder.Build();

        Assert.True(inner.CreateServiceProviderCalled);
    }

    [Fact]
    public void UseServiceBuildExtensions_HostApplicationBuilderNoArgs_NullBuilder_Throws()
    {
        HostApplicationBuilder builder = null!;

        Assert.Throws<ArgumentNullException>(() => builder.UseServiceBuildExtensions());
    }

    [Fact]
    public void UseServiceBuildExtensions_HostApplicationBuilderWithInner_NullInner_Throws()
    {
        var builder = Host.CreateApplicationBuilder();

        Assert.Throws<ArgumentNullException>(() => builder.UseServiceBuildExtensions<IServiceCollection>(null!));
    }

    [Fact]
    public void UseServiceBuildExtensions_HostApplicationBuilderNoArgs_InstallsPipelineAndRunsPreBuildActions()
    {
        var executed = false;
        var builder = Host.CreateApplicationBuilder();

        builder.UseServiceBuildExtensions();
        builder.Services.AddPreBuildAction(new DelegatePreBuildAction(_ => executed = true));

        using var host = builder.Build();

        Assert.True(executed);
    }

    [Fact]
    public void UseServiceBuildExtensions_HostApplicationBuilderWithExplicitInner_DelegatesToProvidedFactory()
    {
        var inner = new RecordingInnerFactory();
        var builder = Host.CreateApplicationBuilder();

        builder.UseServiceBuildExtensions(inner);
        using var host = builder.Build();

        Assert.True(inner.CreateServiceProviderCalled);
    }
}
