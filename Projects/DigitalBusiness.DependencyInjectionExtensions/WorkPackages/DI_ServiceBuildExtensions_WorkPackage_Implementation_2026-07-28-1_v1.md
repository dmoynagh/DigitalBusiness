# DI_ServiceBuildExtensions_WorkPackage_Implementation_2026-07-28-1_v1

**Project:** DigitalBusiness.DependencyInjectionExtensions (doc prefix: `DI`)
**Topic:** ServiceBuildExtensions
**Document type:** WorkPackage_Implementation (shares date-N with its Work Package; archived
alongside it)
**Belongs to:** `DI_ServiceBuildExtensions_WorkPackage_2026-07-28-1_v1.md`

This document is self-contained: it does not assume access to the design corpus. Where
Decisions reasoning matters for *how* to code something, that one point is restated here in
its own words rather than pointed at.

---

## 1. Design context (extracted, not copy-pasted)

`ServiceBuildExtensions` is a container-agnostic build pipeline: registered logic that runs
against the final `IServiceCollection`, in two ordered phases, immediately before the
container is actually built. The standard container exposes no first-class "run this right
before build" hook, so this mechanism uses a decorator over
`IServiceProviderFactory<TContainerBuilder>` — the Generic Host's own extensibility seam
(the same one third-party container integrations like Autofac's own hosting package use) —
rather than wrapping the container's own build method directly. This keeps the mechanism
container-swappable: it occupies a seam any container's own integration would occupy anyway.

Two ordered phases, both driven by one config object (`BuildPipelineConfig`) rather than by
separate registration surfaces:

1. **Pre-build actions** (`IPreBuildAction`) — mutate or inspect the collection.
2. **Cleanup actions** (`ICleanupAction`) — remove things that have no business surviving
   into the built container (leftover `ServiceConfig<T>` bookkeeping descriptors).

The two phases are strictly separated (all pre-build actions run to completion, then all
cleanup actions run) specifically so a cleanup action can never remove something a pre-build
action still needs to read — a future `Forwarded` pre-build action, for example, will need to
read a `ForwardedServicesConfig` that must not be removed until after it's done reading it.

`BuildPipelineConfig` is itself a `ServiceConfig<T>` (from the now-shipped `ServiceConfig`
Topic) — its presence in the collection *is* the "is the pipeline installed" signal; there is
no separate boolean anywhere for that. **Register and retrieve it only via
`GetOrAddConfig<T>()`/`GetConfig<T>()`** — never hand-construct or directly manipulate the
`ServiceConfig<BuildPipelineConfig>` descriptor. This is a hard constraint carried over from
`ServiceConfig`'s own Decision 7: `GetOrAddConfig<T>` is that Topic's only supported
registration path, and bypassing it can produce disagreement between `HasConfig`/`GetConfig`.

Actions themselves are **plain instances stored in `BuildPipelineConfig`'s own lists**, not
`IServiceCollection` registrations. The reasoning matters for how you write
`BuildPipelineFactory`: at the point the pipeline runs, no `IServiceProvider` has been built
yet (that's the entire reason this mechanism exists), so there is nothing to resolve
`IEnumerable<IPreBuildAction>` from without building a throwaway provider — which would
construct any singleton dependency twice (once in the throwaway, once in the real provider),
recreating the same disposal-ownership bug class this project fixed early in its history.
Don't reach for DI resolution here; iterate the plain lists directly.

`BuildPipelineConfig`'s own removal from the collection is **not** one of the toggleable
cleanup actions — it's a separate, hardcoded, unconditional step the decorator itself
performs after both phases run, whenever the config was present. This is deliberate: a
consumer disabling cleanup for an unrelated reason (e.g. to keep one feature's config around)
must never accidentally also preserve `BuildPipelineConfig`, which never has a legitimate
reason to survive into the built provider.

## 2. Repo conventions to follow (carried over from `ServiceConfig`'s Outcome)

- New subfolder: `ServiceBuildExtensions\`, namespace
  `DigitalBusiness.DependencyInjectionExtensions.ServiceBuildExtensions` — matches the
  one-subfolder-per-Topic layout `ServiceConfig` established, itself matching the existing
  `Extensibility`/`JsonDataWrappers` layout.
- Write extension methods using C#'s `extension(IServiceCollection services) { ... }` block
  syntax, not the classic `this IServiceCollection services` parameter style — the repo has
  moved to block syntax (18 files across `Extensibility`/`JsonDataWrappers`; only 3 files
  repo-wide, none DI-related, still use the old style).

## 3. Class shapes

```csharp
namespace DigitalBusiness.DependencyInjectionExtensions.ServiceBuildExtensions;

public interface IPreBuildAction
{
    void Execute(IServiceCollection services);
}

public interface ICleanupAction
{
    void Execute(IServiceCollection services);
}

internal sealed class BuildPipelineConfig
{
    public bool RunPreBuildActions { get; set; } = true;
    public bool RunCleanupActions { get; set; } = true;
    public List<IPreBuildAction> PreBuildActions { get; } = new();
    public List<ICleanupAction> CleanupActions { get; } = new();
}

internal sealed class RemoveOtherServiceConfigsCleanupAction : ICleanupAction
{
    public void Execute(IServiceCollection services)
    {
        // Remove every remaining ServiceConfig<> closed-generic descriptor, whatever its T.
        var toRemove = services
            .Where(d => d.ServiceType.IsGenericType
                && d.ServiceType.GetGenericTypeDefinition() == typeof(ServiceConfig<>))
            .ToList();
        foreach (var descriptor in toRemove)
            services.Remove(descriptor);
    }
}

public static class BuildPipelineExtensions
{
    extension(IServiceCollection services)
    {
        public void AddBuildPipeline()
        {
            var config = services.GetOrAddConfig<BuildPipelineConfig>(() => new());
            // Idempotent: GetOrAddConfig's get-or-create means a repeat call is a no-op
            // for everything except the first-time default cleanup action registration —
            // guard against double-adding it if AddBuildPipeline() is called more than once.
            if (!config.CleanupActions.Any(a => a is RemoveOtherServiceConfigsCleanupAction))
                config.CleanupActions.Add(new RemoveOtherServiceConfigsCleanupAction());
        }

        public void AddPreBuildAction(IPreBuildAction action)
        {
            var config = services.GetConfig<BuildPipelineConfig>()
                ?? throw new InvalidOperationException(
                    "AddBuildPipeline() (or UseServiceBuildExtensions()) must be called " +
                    "before AddPreBuildAction().");
            config.PreBuildActions.Add(action);
        }

        public void AddCleanupAction(ICleanupAction action)
        {
            var config = services.GetConfig<BuildPipelineConfig>()
                ?? throw new InvalidOperationException(
                    "AddBuildPipeline() (or UseServiceBuildExtensions()) must be called " +
                    "before AddCleanupAction().");
            config.CleanupActions.Add(action);
        }
    }
}

public sealed class BuildPipelineFactory<TContainerBuilder>
    : IServiceProviderFactory<TContainerBuilder>
{
    private readonly IServiceProviderFactory<TContainerBuilder> _inner;
    private IServiceCollection _services = null!;

    public BuildPipelineFactory(IServiceProviderFactory<TContainerBuilder>? inner = null)
    {
        _inner = inner ?? (IServiceProviderFactory<TContainerBuilder>)
            (object)new DefaultServiceProviderFactory();
    }

    public TContainerBuilder CreateBuilder(IServiceCollection services)
    {
        _services = services; // captured for CreateServiceProvider
        return _inner.CreateBuilder(services);
    }

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

            // Hardcoded, unconditional, not a toggleable cleanup action.
            _services.RemoveConfig<BuildPipelineConfig>();
        }
        return _inner.CreateServiceProvider(containerBuilder);
    }
}

public static class ServiceBuildExtensionsHostExtensions
{
    extension(IHostBuilder host)
    {
        public IHostBuilder UseServiceBuildExtensions<TBuilder>(
            IServiceProviderFactory<TBuilder>? inner = null)
        {
            host.UseServiceProviderFactory(new BuildPipelineFactory<TBuilder>(inner));
            host.ConfigureServices((_, services) => services.AddBuildPipeline());
            return host;
        }
    }
}
```

**Note on `DefaultServiceProviderFactory` default:** the constructor above needs a default
`IServiceProviderFactory<TContainerBuilder>` when `inner` is null, but
`DefaultServiceProviderFactory` only implements
`IServiceProviderFactory<IServiceCollection>` — it isn't generic over `TContainerBuilder`.
Confirm during implementation whether the actual repo target framework's overload set allows
`TContainerBuilder` to be constrained to `IServiceCollection` for the parameterless-default
case, or whether `UseServiceBuildExtensions()` (no explicit type argument, `TBuilder =
IServiceCollection` inferred from a null-defaulted `DefaultServiceProviderFactory`) is the
only shape that actually compiles. If the shape above doesn't compile as written, this is
expected — resolve it in favor of whatever keeps `UseServiceBuildExtensions()` callable with
zero type arguments for the common case (matching `host.UseServiceBuildExtensions();` in
Design §2), and note the actual resolution in the Outcome as a deviation.

**`RemoveConfig<T>`:** if `ServiceConfig`'s shipped API doesn't already expose a
`RemoveConfig<T>()` extension method, use `services.Remove(descriptor)` directly against the
matching `ServiceConfig<BuildPipelineConfig>` descriptor (found via `GetConfig<T>`'s own
descriptor-lookup, not a hand-rolled scan) — per `DI_WorkRegister_v12.md` §9a's confirmation
that a plain `Remove` is sufficient and no dedicated removal mechanism is owed by
`ServiceConfig` itself.

## 4. Edge cases to handle

- `UseServiceBuildExtensions()` called, but `AddBuildPipeline()` never runs (shouldn't happen
  given §3c wires both together, but if some other codepath removes/bypasses it): decorator
  finds no `BuildPipelineConfig`, delegates straight through — must be a silent no-op, not an
  error.
- `AddPreBuildAction`/`AddCleanupAction` called before `AddBuildPipeline()`/
  `UseServiceBuildExtensions()`: throw `InvalidOperationException` immediately, don't
  auto-create a config nothing will ever execute.
- `AddBuildPipeline()` called more than once: idempotent: no duplicate default cleanup action,
  no exception, existing `PreBuildActions`/`CleanupActions` untouched.
- Pre-build and cleanup lists each execute strictly in the order items were added,
  independently of each other.

## 5. Verification commands

At minimum, the repo's standing build and test commands (see `CLAUDE.md` for the current
canonical invocations); if this Topic warrants a specific filter, use it. As of this
package, no `ServiceBuildExtensions`-specific smoke check beyond the full test suite is
known to be needed — if implementation adds one (e.g. a host-integration test exercising
`UseServiceBuildExtensions()` end to end), list its exact filtered command here before
closing this package.

```
dotnet build DigitalBusiness.DependencyInjectionExtensions.sln
dotnet test DigitalBusiness.DependencyInjectionExtensions.sln
```

**Known pre-existing issue (per `DI_WorkRegister_v12.md` §9a):** a stack-overflow bug in
`DigitalBusiness.JsonDataWrappers`'s `JsonDataTypedPathExtensions.SetDeep` blocks a clean
solution-wide `dotnet test` run, tracked dev-side as `task_4a72522a`. This is not this
corpus's decision to fix and is not part of this package's scope — if it's still unresolved
when this package runs, note in the Outcome whether it affected verification and how (e.g. a
project-scoped test filter used as a workaround).
