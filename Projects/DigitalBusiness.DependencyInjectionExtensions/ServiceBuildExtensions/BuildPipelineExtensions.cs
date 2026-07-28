using DigitalBusiness.DependencyInjectionExtensions.ServiceConfig;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;

namespace DigitalBusiness.DependencyInjectionExtensions.ServiceBuildExtensions
{
    /// <summary>
    /// Provides <see cref="IServiceCollection"/> extension methods for installing and
    /// configuring the build pipeline: registered <see cref="IPreBuildAction"/>s and
    /// <see cref="ICleanupAction"/>s that run against the collection immediately before the DI
    /// container is built.
    /// </summary>
    public static class BuildPipelineExtensions
    {
        extension(IServiceCollection services)
        {
            /// <summary>
            /// Installs the build pipeline on this collection, if not already installed, and
            /// registers the default <see cref="RemoveOtherServiceConfigsCleanupAction"/>
            /// cleanup action. Safe to call more than once — repeat calls are a no-op.
            /// </summary>
            public void AddBuildPipeline()
            {
                ArgumentNullException.ThrowIfNull(services);

                var config = services.GetOrAddConfig<BuildPipelineConfig>(() => new());
                if (!config.CleanupActions.Any(action => action is RemoveOtherServiceConfigsCleanupAction))
                    config.CleanupActions.Add(new RemoveOtherServiceConfigsCleanupAction());
            }

            /// <summary>
            /// Adds a pre-build action to the build pipeline, to run in add-order alongside any
            /// others already registered.
            /// </summary>
            /// <param name="action">The action to add.</param>
            /// <exception cref="InvalidOperationException">
            /// <see cref="AddBuildPipeline"/> (or a host-level installer such as
            /// <c>UseServiceBuildExtensions</c>) has not been called on this collection yet.
            /// </exception>
            public void AddPreBuildAction(IPreBuildAction action)
            {
                ArgumentNullException.ThrowIfNull(services);
                ArgumentNullException.ThrowIfNull(action);

                var config = services.GetConfig<BuildPipelineConfig>()
                    ?? throw new InvalidOperationException(
                        "AddBuildPipeline() (or UseServiceBuildExtensions()) must be called " +
                        "before AddPreBuildAction().");
                config.PreBuildActions.Add(action);
            }

            /// <summary>
            /// Adds a cleanup action to the build pipeline, to run in add-order alongside any
            /// others already registered, after every pre-build action has run.
            /// </summary>
            /// <param name="action">The action to add.</param>
            /// <exception cref="InvalidOperationException">
            /// <see cref="AddBuildPipeline"/> (or a host-level installer such as
            /// <c>UseServiceBuildExtensions</c>) has not been called on this collection yet.
            /// </exception>
            public void AddCleanupAction(ICleanupAction action)
            {
                ArgumentNullException.ThrowIfNull(services);
                ArgumentNullException.ThrowIfNull(action);

                var config = services.GetConfig<BuildPipelineConfig>()
                    ?? throw new InvalidOperationException(
                        "AddBuildPipeline() (or UseServiceBuildExtensions()) must be called " +
                        "before AddCleanupAction().");
                config.CleanupActions.Add(action);
            }
        }
    }
}
