using Microsoft.Extensions.DependencyInjection;

namespace DigitalBusiness.DependencyInjectionExtensions.ServiceBuildExtensions
{
    /// <summary>
    /// An action registered to run against the final <see cref="IServiceCollection"/>
    /// immediately before the DI container is built, before any <see cref="ICleanupAction"/>
    /// runs.
    /// </summary>
    public interface IPreBuildAction
    {
        /// <summary>
        /// Executes this action against <paramref name="services"/>.
        /// </summary>
        /// <param name="services">The service collection about to be built into a container.</param>
        void Execute(IServiceCollection services);
    }
}
