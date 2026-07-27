namespace DigitalBusiness.DependencyInjectionExtensions.ServiceConfig
{
    /// <summary>
    /// Wraps a typed configuration or shared-state instance so it can be attached to an
    /// <see cref="Microsoft.Extensions.DependencyInjection.IServiceCollection"/> as
    /// registration-time metadata, distinguishable from ordinary application services and
    /// unable to collide with a consumer registering <typeparamref name="T"/> itself.
    /// </summary>
    /// <typeparam name="T">The configuration or shared-state type being wrapped.</typeparam>
    public sealed class ServiceConfig<T> where T : class
    {
        /// <summary>The wrapped configuration or shared-state instance.</summary>
        public T Value { get; }

        /// <summary>Creates a wrapper around the given <paramref name="value"/>.</summary>
        /// <param name="value">The configuration or shared-state instance to wrap.</param>
        public ServiceConfig(T value) => Value = value;
    }
}
