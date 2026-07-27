# DI_ServiceConfig_WorkPackage_Implementation_2026-07-27-1_v1

**Project:** DigitalBusiness.DependencyInjectionExtensions (doc prefix: `DI`)
**Topic:** ServiceConfig — technical implementation guidance for
`DI_ServiceConfig_WorkPackage_2026-07-27-1_v1.md`
**Document type:** WorkPackage_Implementation (self-contained)

---

## 1. Design context (extracted from `DI_ServiceConfig_Design_v1.md`)

`ServiceConfig<T>` lets a registration extension store a mutable, typed configuration or
data object directly in the `IServiceCollection`, and lets any other code (at registration
time, or later during a build pipeline) retrieve the same instance, read it, and — if `T` is
mutable — update it. It replaces ad hoc, single-use "marker"/"flag" mechanisms with one
small, reusable, strongly-typed pattern:

```csharp
services.GetOrAddConfig<ForwardedServicesConfig>(() => new());
```

**Why `ImplementationInstance`, not a factory delegate:** a factory needs an
`IServiceProvider` to invoke, and none exists at registration time — exactly when config
needs to be readable/writable. `ImplementationInstance` is a plain property read; nothing
executes, so nothing can misbehave by touching a provider that doesn't exist yet. This was
a deliberate rejection of an earlier factory-delegate direction — do not "simplify" this
back toward a factory-based registration.

**Why one type parameter (`T`), not `ServiceConfig<TKey, TData>`:** an earlier direction
considered using a second `TKey` parameter to disambiguate cases where `TData` alone (e.g. a
raw `Dictionary<,>`) wouldn't be unique. Resolved instead by requiring `T` itself to be a
dedicated type per config (a future Topic's own concern, not this package's) — which makes
`T` unique by construction and removes the need for a separate key parameter. This package
only needs to build the one-type-parameter shape; do not add a second generic parameter
"for flexibility."

**Why the `ServiceConfig<T>` wrapper at all, rather than registering `T` bare:** two
reasons, both worth preserving in the implementation: it marks these descriptors as
registration-time metadata distinguishable from ordinary application services (so a future
generic cleanup pass — a different Topic's concern — can find "every config, whatever `T`
is" via the open generic type alone), and it avoids a collision if a consumer happens to
register `T` itself as a real service independently.

---

## 2. Shape to implement

```csharp
public sealed class ServiceConfig<T> where T : class
{
    public T Value { get; }
    public ServiceConfig(T value) => Value = value;
}

public static class ServiceConfigExtensions
{
    public static T GetOrAddConfig<T>(this IServiceCollection services, Func<T> factory)
        where T : class
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
        services.Add(ServiceDescriptor.Singleton(
            typeof(ServiceConfig<T>),
            new ServiceConfig<T>(value)));
        return value;
    }

    public static T? GetConfig<T>(this IServiceCollection services) where T : class
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

    public static bool HasConfig<T>(this IServiceCollection services) where T : class
    {
        ArgumentNullException.ThrowIfNull(services);
        return services.Any(d => d.ServiceType == typeof(ServiceConfig<T>));
    }
}
```

Adjust null-guard style, namespace, and file placement to match this repo's existing
conventions (see `CLAUDE.md` and the existing codebase's own extension-method files) — the
shapes above are the contract to hit, not necessarily the exact file layout.

## 3. Edge cases and things to get right

- **Get-or-create must actually be get-or-create.** `factory()` must only ever be invoked
  on the call that first creates the descriptor. A test should assert the factory is not
  invoked a second time on a repeat `GetOrAddConfig<T>` call for the same `T`.
- **No cross-`T` collisions.** `ServiceConfig<Foo>` and `ServiceConfig<Bar>` must not
  interfere with each other's presence/absence checks — cover this with a test using two
  distinct config types.
- **No synchronization required.** Composition roots are assumed single-threaded (Design
  §6) — do not add locking; it would be unnecessary complexity against the design's stated
  assumption.
- **Linear scan over `services` is intentional, not an oversight.** This project's own
  config sets are expected to be small in count. Don't introduce a separate index/cache
  structure to speed this up — that would be solving a problem this Topic doesn't have.
- **`T : class` constraint** is required — `ServiceConfig<T>` holds `T` as a reference type
  via `ImplementationInstance`; don't loosen this constraint.
- **Do not implement removal/cleanup here** — see the WorkPackage's §1 scope note. If a
  test wants to verify "a config can be removed," that test belongs to whichever future
  package actually builds removal (`ServiceBuildExtensions`), not this one.

## 4. Verification commands

This design corpus doesn't contain the target repository, so exact build/test invocations
can't be stated with certainty here. Run the repository's standard full solution build and
full test commands (see `CLAUDE.md` at the dev repo's root). No package-specific test
filter is needed beyond ensuring the new `ServiceConfig<T>`/`GetOrAddConfig`/`GetConfig`/
`HasConfig` surface has direct test coverage per §3 above.
