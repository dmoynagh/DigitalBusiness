using Microsoft.Extensions.DependencyInjection;

namespace DigitalBusiness.DependencyInjectionExtensions.ServiceBuildExtensions
{
    /// <summary>
    /// An action registered to run against the final <see cref="IServiceCollection"/>
    /// immediately before the DI container is built, after every <see cref="IPreBuildAction"/>
    /// has run. Typically used to remove registration-time bookkeeping that has no business
    /// surviving into the built container.
    /// </summary>
    public interface ICleanupAction
    {
        /// <summary>
        /// Executes this action against <paramref name="services"/>.
        /// </summary>
        /// <param name="services">The service collection about to be built into a container.</param>
        void Execute(IServiceCollection services);
    }
}
