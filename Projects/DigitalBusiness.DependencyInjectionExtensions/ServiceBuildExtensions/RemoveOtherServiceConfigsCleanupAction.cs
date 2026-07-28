using DigitalBusiness.DependencyInjectionExtensions.ServiceConfig;
using Microsoft.Extensions.DependencyInjection;
using System.Linq;

namespace DigitalBusiness.DependencyInjectionExtensions.ServiceBuildExtensions
{
    /// <summary>
    /// The default cleanup action registered by <see cref="BuildPipelineExtensions"/>. Removes
    /// every remaining <see cref="ServiceConfig{T}"/> descriptor from the collection, whatever
    /// its <c>T</c> — bookkeeping that has no business surviving into the built container.
    /// </summary>
    /// <remarks>
    /// The scan above matches every <c>T</c>, so it incidentally removes
    /// <see cref="BuildPipelineConfig"/>'s own descriptor too when this action runs. That's
    /// harmless: <see cref="BuildPipelineFactory{TContainerBuilder}"/> also removes it itself,
    /// separately, hardcoded and unconditionally, regardless of whether this action already did
    /// — so a consumer disabling cleanup (or removing this action) for an unrelated reason can
    /// never accidentally leave it behind.
    /// </remarks>
    internal sealed class RemoveOtherServiceConfigsCleanupAction : ICleanupAction
    {
        /// <inheritdoc/>
        public void Execute(IServiceCollection services)
        {
            var toRemove = services
                .Where(descriptor => descriptor.ServiceType.IsGenericType
                    && descriptor.ServiceType.GetGenericTypeDefinition() == typeof(ServiceConfig<>))
                .ToList();

            foreach (var descriptor in toRemove)
                services.Remove(descriptor);
        }
    }
}
