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
    /// Does not remove <see cref="BuildPipelineConfig"/>'s own descriptor — that removal is a
    /// separate, hardcoded, unconditional step performed by <see cref="BuildPipelineFactory{TContainerBuilder}"/>
    /// itself, so a consumer disabling cleanup for an unrelated reason can never accidentally
    /// also preserve it.
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
