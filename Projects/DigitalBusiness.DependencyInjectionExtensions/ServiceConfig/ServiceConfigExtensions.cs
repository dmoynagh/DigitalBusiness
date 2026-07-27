using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;

namespace DigitalBusiness.DependencyInjectionExtensions.ServiceConfig
{
    /// <summary>
    /// Provides <see cref="IServiceCollection"/> extension methods for attaching typed, mutable
    /// configuration or shared-state instances to the collection itself via
    /// <see cref="ServiceConfig{T}"/>.
    /// </summary>
    public static class ServiceConfigExtensions
    {
        extension(IServiceCollection services)
        {
            /// <summary>
            /// Returns the existing <typeparamref name="T"/> config value if one is already
            /// attached to this collection, otherwise creates one via <paramref name="factory"/>,
            /// attaches it, and returns it. <paramref name="factory"/> is invoked only on the
            /// call that first creates the config.
            /// </summary>
            /// <typeparam name="T">The configuration or shared-state type.</typeparam>
            /// <param name="factory">Invoked to create the value if none is attached yet.</param>
            /// <returns>The existing or newly created <typeparamref name="T"/> value.</returns>
            /// <exception cref="ArgumentNullException"><paramref name="factory"/> returned <see langword="null"/>.</exception>
            public T GetOrAddConfig<T>(Func<T> factory) where T : class
            {
                ArgumentNullException.ThrowIfNull(services);
                ArgumentNullException.ThrowIfNull(factory);

                foreach (var descriptor in services)
                {
                    if (descriptor.ServiceType == typeof(ServiceConfig<T>)
                        && descriptor.ImplementationInstance is ServiceConfig<T> existing)
                    {
                        return existing.Value;
                    }
                }

                var value = factory();
                ArgumentNullException.ThrowIfNull(value, nameof(factory));
                services.Add(ServiceDescriptor.Singleton(
                    typeof(ServiceConfig<T>),
                    new ServiceConfig<T>(value)));
                return value;
            }

            /// <summary>
            /// Returns the <typeparamref name="T"/> config value already attached to this
            /// collection, or <see langword="null"/> if none is attached. Never creates one.
            /// </summary>
            /// <typeparam name="T">The configuration or shared-state type.</typeparam>
            /// <returns>The attached value, or <see langword="null"/> if absent.</returns>
            public T? GetConfig<T>() where T : class
            {
                ArgumentNullException.ThrowIfNull(services);

                foreach (var descriptor in services)
                {
                    if (descriptor.ServiceType == typeof(ServiceConfig<T>)
                        && descriptor.ImplementationInstance is ServiceConfig<T> existing)
                    {
                        return existing.Value;
                    }
                }
                return null;
            }

            /// <summary>
            /// Returns <see langword="true"/> if a <typeparamref name="T"/> config is already
            /// attached to this collection.
            /// </summary>
            /// <typeparam name="T">The configuration or shared-state type.</typeparam>
            public bool HasConfig<T>() where T : class
            {
                ArgumentNullException.ThrowIfNull(services);
                return services.Any(d => d.ServiceType == typeof(ServiceConfig<T>));
            }
        }
    }
}
