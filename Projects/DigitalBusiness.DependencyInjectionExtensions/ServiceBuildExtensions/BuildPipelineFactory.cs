using DigitalBusiness.DependencyInjectionExtensions.ServiceConfig;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;

namespace DigitalBusiness.DependencyInjectionExtensions.ServiceBuildExtensions
{
    /// <summary>
    /// Decorates an <see cref="IServiceProviderFactory{TContainerBuilder}"/> so that, immediately
    /// before the container is built, every registered <see cref="IPreBuildAction"/> runs, then
    /// every registered <see cref="ICleanupAction"/> runs, then the pipeline's own
    /// <see cref="BuildPipelineConfig"/> bookkeeping is removed. If no
    /// <see cref="BuildPipelineConfig"/> is attached to the collection (the pipeline was never
    /// installed via <see cref="BuildPipelineExtensions.AddBuildPipeline"/>), this is a silent
    /// passthrough to the inner factory.
    /// </summary>
    /// <typeparam name="TContainerBuilder">The container builder type of the decorated factory.</typeparam>
    public sealed class BuildPipelineFactory<TContainerBuilder> : IServiceProviderFactory<TContainerBuilder>
    {
        private readonly IServiceProviderFactory<TContainerBuilder> _inner;
        private IServiceCollection _services = null!;

        /// <summary>
        /// Creates a decorator around <paramref name="inner"/>.
        /// </summary>
        /// <param name="inner">The factory to delegate <see cref="CreateBuilder"/>/<see cref="CreateServiceProvider"/> to.</param>
        public BuildPipelineFactory(IServiceProviderFactory<TContainerBuilder> inner)
        {
            ArgumentNullException.ThrowIfNull(inner);
            _inner = inner;
        }

        /// <inheritdoc/>
        public TContainerBuilder CreateBuilder(IServiceCollection services)
        {
            ArgumentNullException.ThrowIfNull(services);
            _services = services;
            return _inner.CreateBuilder(services);
        }

        /// <inheritdoc/>
        public IServiceProvider CreateServiceProvider(TContainerBuilder containerBuilder)
        {
            var config = _services.GetConfig<BuildPipelineConfig>();
            if (config is not null)
            {
                if (config.RunPreBuildActions)
                    foreach (var action in config.PreBuildActions)
                        action.Execute(_services);

                if (config.RunCleanupActions)
                    foreach (var action in config.CleanupActions)
                        action.Execute(_services);

                // Hardcoded and unconditional — not a toggleable cleanup action, so disabling
                // cleanup for an unrelated reason can never also preserve this bookkeeping.
                var descriptor = _services.FirstOrDefault(
                    d => d.ServiceType == typeof(ServiceConfig<BuildPipelineConfig>));
                if (descriptor is not null)
                    _services.Remove(descriptor);
            }

            return _inner.CreateServiceProvider(containerBuilder);
        }
    }
}
