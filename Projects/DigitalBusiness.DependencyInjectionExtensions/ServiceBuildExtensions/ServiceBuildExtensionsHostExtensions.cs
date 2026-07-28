using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;

namespace DigitalBusiness.DependencyInjectionExtensions.ServiceBuildExtensions
{
    /// <summary>
    /// Provides one-call install of <see cref="BuildPipelineFactory{TContainerBuilder}"/> for
    /// <see cref="IHostBuilder"/> and <see cref="HostApplicationBuilder"/>: wraps the container's
    /// <see cref="IServiceProviderFactory{TContainerBuilder}"/> and registers
    /// <see cref="BuildPipelineExtensions.AddBuildPipeline"/> in one step.
    /// </summary>
    public static class ServiceBuildExtensionsHostExtensions
    {
        extension(IHostBuilder host)
        {
            /// <summary>
            /// Installs the build pipeline over the host's default
            /// (<see cref="IServiceCollection"/>-based) container factory.
            /// </summary>
            /// <returns>The same host builder, for chaining.</returns>
            public IHostBuilder UseServiceBuildExtensions()
            {
                ArgumentNullException.ThrowIfNull(host);
                return host.UseServiceBuildExtensions(new DefaultServiceProviderFactory());
            }

            /// <summary>
            /// Installs the build pipeline over a custom container's
            /// <see cref="IServiceProviderFactory{TContainerBuilder}"/>.
            /// </summary>
            /// <typeparam name="TBuilder">The custom container's builder type.</typeparam>
            /// <param name="inner">The container's own factory, to delegate to after the pipeline runs.</param>
            /// <returns>The same host builder, for chaining.</returns>
            public IHostBuilder UseServiceBuildExtensions<TBuilder>(IServiceProviderFactory<TBuilder> inner)
            {
                ArgumentNullException.ThrowIfNull(host);
                ArgumentNullException.ThrowIfNull(inner);

                host.UseServiceProviderFactory(new BuildPipelineFactory<TBuilder>(inner));
                host.ConfigureServices((_, services) => services.AddBuildPipeline());
                return host;
            }
        }

        extension(HostApplicationBuilder builder)
        {
            /// <summary>
            /// Installs the build pipeline over the builder's default
            /// (<see cref="IServiceCollection"/>-based) container factory.
            /// </summary>
            public void UseServiceBuildExtensions()
            {
                ArgumentNullException.ThrowIfNull(builder);
                builder.UseServiceBuildExtensions(new DefaultServiceProviderFactory());
            }

            /// <summary>
            /// Installs the build pipeline over a custom container's
            /// <see cref="IServiceProviderFactory{TContainerBuilder}"/>.
            /// </summary>
            /// <typeparam name="TBuilder">The custom container's builder type.</typeparam>
            /// <param name="inner">The container's own factory, to delegate to after the pipeline runs.</param>
            public void UseServiceBuildExtensions<TBuilder>(IServiceProviderFactory<TBuilder> inner) where TBuilder : notnull
            {
                ArgumentNullException.ThrowIfNull(builder);
                ArgumentNullException.ThrowIfNull(inner);

                builder.Services.AddBuildPipeline();
                builder.ConfigureContainer(new BuildPipelineFactory<TBuilder>(inner));
            }
        }
    }
}
