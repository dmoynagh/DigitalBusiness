namespace DigitalBusiness.DependencyInjectionExtensions.ServiceBuildExtensions
{
    /// <summary>
    /// Backing state for the build pipeline, attached to an
    /// <see cref="Microsoft.Extensions.DependencyInjection.IServiceCollection"/> via
    /// <see cref="DigitalBusiness.DependencyInjectionExtensions.ServiceConfig.ServiceConfig{T}"/>. Its presence in the collection is itself the
    /// "is the pipeline installed" signal — never hand-construct or attach this directly; go
    /// through <see cref="BuildPipelineExtensions"/>.
    /// </summary>
    /// <remarks>
    /// Actions are stored as plain instances rather than as <c>IServiceCollection</c>
    /// registrations because no <see cref="System.IServiceProvider"/> exists yet at pipeline-run
    /// time to resolve them from without building a throwaway provider, which would
    /// double-construct singletons.
    /// </remarks>
    internal sealed class BuildPipelineConfig
    {
        /// <summary>Whether <see cref="PreBuildActions"/> run when the pipeline executes. Defaults to <see langword="true"/>.</summary>
        public bool RunPreBuildActions { get; set; } = true;

        /// <summary>Whether <see cref="CleanupActions"/> run when the pipeline executes. Defaults to <see langword="true"/>.</summary>
        public bool RunCleanupActions { get; set; } = true;

        /// <summary>Pre-build actions, executed in add-order.</summary>
        public List<IPreBuildAction> PreBuildActions { get; } = new();

        /// <summary>Cleanup actions, executed in add-order after every pre-build action has run.</summary>
        public List<ICleanupAction> CleanupActions { get; } = new();
    }
}
