# DI_FullImplementation_WorkPackage_Implementation_2026-07-27-1_v1

**Project:** DigitalBusiness.DependencyInjectionExtensions (doc prefix: `DI`)
**Topic:** FullImplementation — technical implementation guidance for
`DI_FullImplementation_WorkPackage_2026-07-27-1_v1.md`
**Document type:** WorkPackage_Implementation (self-contained — extracts the Design context
it needs; a full Design copy also travels in this bundle per §0 of the WorkPackage, since
this package's scope is broad enough that extraction alone would be lossy)

---

## 0. How to read this document

Each section below corresponds to one or more WR-IDs from the WorkPackage's §2/§3. Every
section states: the class/method shapes to implement, the one or two pieces of *why* that
actually change how the code must be written (not general background — see
`DocumentationMethodology_v9.md` §7a's own rule that Decisions reasoning is never copied
wholesale), sequencing notes, and edge cases worth flagging. Cross-references to `§` numbers
without a document name refer to this Implementation document's own sections.

**Cross-cutting conventions that apply everywhere below** (from `DI_Overview_v2.md` §2, not
repeated per-section):

- Every feature is additive, opt-in extension methods/wrapper types — never a global
  behaviour change to the base container.
- Reuse framework vocabulary (`[FromKeyedServices]`, `GetService`/`GetRequiredService`
  naming and exception shapes) over inventing new API surface.
- Customization is always ordinary DI last-registration-wins — never a bespoke override API.
- State that one feature needs to discover from another lives in the `IServiceCollection`
  itself (`ServiceConfig`), never a static/ambient registry.
- Never resolve from a temporary/throwaway `IServiceProvider` at registration or pre-build
  time — this is the exact shape of the project's original F1 disposal defect, and recurs as
  a documented hazard in `ServiceBuildExtensions`' own design (§3 below).
- Default posture favors AOT/trimming safety; faster-but-AOT-unsafe options are opt-in only.

---

## 1. ServiceConfig (WR-31) — build first

### Shape

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
        var existing = services.FirstOrDefault(d => d.ServiceType == typeof(ServiceConfig<T>));
        if (existing?.ImplementationInstance is ServiceConfig<T> config)
            return config.Value;

        var value = factory();
        services.Add(ServiceDescriptor.Singleton(
            typeof(ServiceConfig<T>), new ServiceConfig<T>(value)));
        return value;
    }

    public static T? GetConfig<T>(this IServiceCollection services) where T : class
    {
        var d = services.FirstOrDefault(d => d.ServiceType == typeof(ServiceConfig<T>));
        return (d?.ImplementationInstance as ServiceConfig<T>)?.Value;
    }

    public static bool HasConfig<T>(this IServiceCollection services) where T : class
        => services.Any(d => d.ServiceType == typeof(ServiceConfig<T>));

    internal static void RemoveConfig<T>(this IServiceCollection services) where T : class
    {
        for (int i = services.Count - 1; i >= 0; i--)
            if (services[i].ServiceType == typeof(ServiceConfig<T>))
                services.RemoveAt(i);
    }
}
```

### Why this shape, not something else

`ImplementationInstance`, not a factory delegate, because a factory needs an
`IServiceProvider` to invoke and none exists at registration time — the exact moment config
needs to be readable/writable (Design §2). One type parameter (`T`), not `ServiceConfig<TKey,
TData>`, because `T` is required to be a dedicated type per config — see §2 below for why
that matters for every feature-specific config you write.

### Edge cases

- `GetOrAddConfig<T>` must be genuinely get-or-create — only the *first* caller's factory
  result should ever be stored; do not re-invoke `factory()` on a call that finds an existing
  descriptor.
- No synchronization is required or expected — composition roots are assumed
  single-threaded (Design §6).
- Consider `IServiceCollection`'s actual shape (typically a `List<ServiceDescriptor>` behind
  the interface) for the linear scan's performance — this project's own config sets are
  expected to be small in count, so a linear scan is an accepted, deliberate simplicity
  choice, not an oversight.

---

## 2. Dedicated config types (used by §7's `ForwardedServicesConfig`, §3's `BuildPipelineConfig`)

Any feature needing collection-shaped config data must define its own dedicated subclass —
**never** register the raw collection type (e.g. `Dictionary<object, ForwardEntry>`) directly
as `T` in `ServiceConfig<T>`. Two different features needing the same closed generic shape
would otherwise collide on one shared `ServiceConfig<Dictionary<object, ForwardEntry>>`
descriptor and silently share state (Design §3). This is a one-line, otherwise-empty
subclass per config — see §7's `ForwardedServicesConfig` and §3's `BuildPipelineConfig` below
for the two concrete instances this package needs.

---

## 3. ServiceBuildExtensions (WR-33, WR-34, WR-35) — build second, depends on §1

### Shapes

```csharp
public interface IPreBuildAction { void Execute(IServiceCollection services); }
public interface ICleanupAction { void Execute(IServiceCollection services); }

internal sealed class BuildPipelineConfig
{
    public bool RunPreBuildActions { get; set; } = true;
    public bool RunCleanupActions { get; set; } = true;
    public List<IPreBuildAction> PreBuildActions { get; } = new();
    public List<ICleanupAction> CleanupActions { get; } = new();
}

public static void AddBuildPipeline(this IServiceCollection services)
{
    var config = services.GetOrAddConfig<BuildPipelineConfig>(() => new());
    // idempotent via GetOrAddConfig — a second call is a no-op after the first
    if (!config.CleanupActions.Any(a => a is RemoveOtherServiceConfigsCleanupAction))
        config.CleanupActions.Add(new RemoveOtherServiceConfigsCleanupAction());
}

internal sealed class RemoveOtherServiceConfigsCleanupAction : ICleanupAction
{
    public void Execute(IServiceCollection services)
    {
        for (int i = services.Count - 1; i >= 0; i--)
        {
            var t = services[i].ServiceType;
            if (t.IsGenericType && t.GetGenericTypeDefinition() == typeof(ServiceConfig<>))
                services.RemoveAt(i);
        }
    }
}

public sealed class BuildPipelineFactory<TContainerBuilder>
    : IServiceProviderFactory<TContainerBuilder>
{
    private readonly IServiceProviderFactory<TContainerBuilder> _inner;
    private IServiceCollection? _services;

    public BuildPipelineFactory(IServiceProviderFactory<TContainerBuilder> inner)
        => _inner = inner;

    public TContainerBuilder CreateBuilder(IServiceCollection services)
    {
        _services = services;
        return _inner.CreateBuilder(services);
    }

    public IServiceProvider CreateServiceProvider(TContainerBuilder containerBuilder)
    {
        var services = _services
            ?? throw new InvalidOperationException("CreateBuilder must run before CreateServiceProvider.");
        var config = services.GetConfig<BuildPipelineConfig>();
        if (config is not null)
        {
            if (config.RunPreBuildActions)
                foreach (var action in config.PreBuildActions) action.Execute(services);
            if (config.RunCleanupActions)
                foreach (var action in config.CleanupActions) action.Execute(services);

            services.RemoveConfig<BuildPipelineConfig>(); // hardcoded, unconditional, always
        }
        return _inner.CreateServiceProvider(containerBuilder);
    }
}

public static IHostBuilder UseServiceBuildExtensions<TBuilder>(
    this IHostBuilder host,
    IServiceProviderFactory<TBuilder>? inner = null)
    => host.UseServiceProviderFactory(
             new BuildPipelineFactory<TBuilder>(inner ?? new DefaultServiceProviderFactory()))
           .ConfigureServices(services => services.AddBuildPipeline());

// HostApplicationBuilder variant — no ConfigureServices deferral needed there:
public static void UseServiceBuildExtensions<TBuilder>(
    this HostApplicationBuilder builder,
    IServiceProviderFactory<TBuilder> inner)
{
    builder.ConfigureContainer(new BuildPipelineFactory<TBuilder>(inner));
    builder.Services.AddBuildPipeline();
}
```

### Why this shape, not something else

Actions live as **plain instances in lists**, never as `IServiceCollection` registrations —
running descriptor-registered actions would require resolving `IEnumerable<IPreBuildAction>`,
which needs a built provider that doesn't exist yet at this point; a throwaway temporary
provider would construct any singleton an action depends on *twice*, the same shape of bug as
the project's original F1 disposal defect (Design §3).

`BuildPipelineConfig`'s own removal is **hardcoded in the factory, not a toggleable cleanup
action** — there is no scenario where a consumer would want the pipeline's own bookkeeping to
survive, unlike a feature's own config which might legitimately be worth keeping (Design §4).

### Edge cases

- If `BuildPipelineConfig` is absent when `CreateServiceProvider` runs (installed at host
  level but `AddBuildPipeline()` never called), both phases are skipped and the wrapper
  delegates straight through — a safe no-op, not an error (Design §4).
- `AddPreBuildAction`/`AddCleanupAction` extension methods (used by consuming Topics, e.g.
  `Forwarded` in §7) must **throw**, not auto-create `BuildPipelineConfig` via
  `GetOrAddConfig`, if called before `AddBuildPipeline()` — this catches a forgotten install
  loudly at registration time (Design §3). Implement these two extension methods as part of
  this task even though they're not separately WR-numbered — they're the access surface every
  later consumer of the pipeline needs.
- Ordering caveat to document in the Guide (WR-38): `ConfigureServices` delegates run in
  registration order — anything registered before `UseServiceBuildExtensions()` runs without
  `BuildPipelineConfig` present (Design §2).

---

## 4. KeyedServices core (WR-1 through WR-4, WR-6) — independent of §1/§3

### 4.1 CompoundKey (WR-2)

```csharp
internal sealed record CompoundKey(object OuterKey, object InnerKey);

internal static class CompoundKeyHelper
{
    public static object Combine(object outerKey, object innerKey) => new CompoundKey(outerKey, innerKey);
}
```

Record type deliberately, for free structural equality (needed for the container's internal
key-equality checks) — not a string concatenation, which risks collision between differently
split components (`"A"+"BC"` vs `"AB"+"C"`). **Both** the fixed-per-type
`[FromKeyedServices]` path (§4.3) and the dynamic `KeyedFactory<T>` path (§9 below) must route
through this one `Combine` helper — do not let either path construct a `CompoundKey` directly.

### 4.2 Cascading activator + registration extensions (WR-1)

The activator wraps `ActivatorUtilities.CreateInstance` semantics but resolves each
constructor parameter itself: for each parameter, check first for a registration under the
ambient key (the key the *current* type was itself resolved with), fall back to the
unkeyed/default registration if none exists. Parallel registration extensions
(`AddKeyedScoped`/`AddKeyedTransient`/`AddKeyedSingleton` — cascading variants distinct from
the framework's own keyed methods) register the activator as the construction path instead of
the framework's default.

**Per-type construction plan is built once and cached** — which parameters, which types,
keyed-or-fallback per parameter — plus a per-(type, key) cache of which resolution path (keyed
vs. fallback) was actually taken, so repeat resolutions run a direct path with no repeated
existence checks (Design §3). Build the plan through the pluggable
`IConstructorFactoryStrategy` (§4.5) — the activator itself must have **no knowledge of how**
the delegate that constructs an instance was built.

**Cascade termination at a fallback into a plain (non-cascading) registration is intended
behaviour** (Design §2.1) — do not treat this as a bug to work around. A registration's key
is a fixed property of the registration, never inherited from the ambient key that drove the
lookup reaching it — this same fact, unmodified, is what makes a `Forwarded` target continue
under its own key rather than terminating (§7 below) and requires **no special-case code for
`Forwarded` at all** in the activator.

### 4.3 `[FromKeyedServices]` reinterpretation (WR-4)

Only when building via the activator's own path: reinterpret `[FromKeyedServices(literalKey)]`
on a constructor parameter as "combine `literalKey` with the ambient key" via §4.1's
`CompoundKeyHelper.Combine`, rather than resolving `literalKey` alone. Outside the activator's
own construction path (plain framework auto-wiring, direct `new`), this attribute is untouched
by any of this project's code and reverts to its literal, standard meaning — this reversion
is not something to guard against in code; it's a documented registrant responsibility
(Design §5), surfaced instead by `ValidateKeyedCascading()` (§6 below).

### 4.4 Registration-side compound-key extensions (WR-3)

```csharp
public static IServiceCollection AddKeyedScoped<TService, TImplementation>(
    this IServiceCollection services, object outerKey, object innerKey)
    where TImplementation : class, TService
    // registers under CompoundKeyHelper.Combine(outerKey, innerKey) — same helper as §4.1/§4.3
```

Mirror this shape for `AddKeyedTransient`/`AddKeyedSingleton`. The registrant never
hand-constructs a `CompoundKey` — the type stays `internal` (Design §4.2).

### 4.5 `IConstructorFactoryStrategy` (WR-6)

```csharp
public interface IConstructorFactoryStrategy
{
    Func<IServiceProvider, object> CreateFactory(Type implementationType, object? ambientKey);
}

public sealed class ReflectionConstructorFactoryStrategy : IConstructorFactoryStrategy
{
    // default — ActivatorUtilities.CreateInstance / direct ConstructorInfo.Invoke.
    // Safe under Native AOT and trimming; slowest option; zero-config default.
}

public sealed class ExpressionTreeConstructorFactoryStrategy : IConstructorFactoryStrategy
{
    // ships in the main package, not the default — uses Expression.Lambda(...).Compile(),
    // which needs runtime code generation the default must not depend on.
}
```

The activator asks this DI-registered strategy for a delegate **per (type, key) pair** (not
per-type alone — this supersedes an earlier, superseded per-type-only shape, per
`DI_KeyedServices_Decisions_v7.md` Decision 7 amending Decision 5), caches the result via
§4.2's caching layer, and invokes it. Override mechanics are ordinary DI last-registration-
wins — a consumer registering their own `IConstructorFactoryStrategy` simply takes precedence;
no dedicated override API.

### Edge cases (§4 overall)

- `IEnumerable<T>` cascading parameters are **not** covered by WR-1 — that's WR-8, §5 below,
  with its own required investigation task.
- Do not implement `Lazy<T>`/`Factory<T>`/`KeyedFactory<T>`/`Owned<T>` special-casing here —
  that's WR-7/20/24, §9 below, and depends on those types existing first (see the
  WorkPackage's §4, step 6).

---

## 5. `IEnumerable<T>` cascading semantics (WR-8) — investigate before coding

**Decision to implement:** for a cascading constructor parameter of type `IEnumerable<T>`,
resolve the **keyed set if non-empty, else the unkeyed set** — never merge across keyed and
unkeyed registrations; it's one set or the other (Design §10).

**Mandatory pre-implementation step:** this path uses `GetServices<T>()`, which is where the
framework's documented open/closed-generic instability lives
(`dotnet/runtime#64995`/`#65145`). The `Lazy` Topic dismissed this instability for its own
purposes because it only uses single resolution (`GetService<T>()`) — **that dismissal does
not transfer here**, since this is exactly the `GetServices<T>()` path the issue concerns.
Before writing any code for this task:

1. Reproduce or research the specific conditions `#64995`/`#65145` describe (mixing open- and
   closed-generic registrations for the same base type via `GetServices<T>()`).
2. Determine whether this Topic's actual registration shapes (cascading + plain fallback
   registrations coexisting for the same `T`) hit those conditions.
3. Record the outcome in the Outcome document's Verification status section (§6 of the
   WorkPackage), **whichever way it comes out**.
4. If the instability does apply: stop this task, record it as a blocker in the WorkPackage's
   Notes/Issues (§6) and in the Outcome, and continue with other unblocked tasks. Do not
   attempt a silent workaround.

---

## 6. `ValidateKeyedCascading()` (WR-9, WR-42, WR-40)

### WR-9 — base validation

```csharp
public static void ValidateKeyedCascading(this IServiceCollection services)
```

Development-time-only, opt-in, zero runtime cost when not called (pure diagnostic, no
resolution hook). Scans the final `IServiceCollection` for:

1. Types carrying the `[FromKeyedServices]` reinterpretation (§4.3) that are reachable via a
   non-activator (plain framework) registration path.
2. Cascading registrations whose keyed targets don't exist under any key at all.

### WR-42 — extend for nested special types

Add a third check to the same scan: constructor parameters where one special-cased type
(`Lazy<>`/`Factory<>`/`KeyedFactory<>`/`Owned<>`) is nested inside another of the same set
(e.g. `Owned<Lazy<T>>`). This is a documented, intended ambient-key-propagation boundary
(`DI_KeyedServices_Design_v7.md` §13) — the diagnostic surfaces it as a visible warning, it
does not change the underlying (unchanged) resolution behaviour. Depends on WR-9 existing
first (same scan, extended).

### WR-40 — optional pipeline self-registration

```csharp
public static void ValidateKeyedCascading(this IServiceCollection services, bool selfRegisterAsPreBuildAction)
```

When `services.HasConfig<BuildPipelineConfig>()` is true, allow `ValidateKeyedCascading()` to
self-register as an `IPreBuildAction` instead of requiring a separate manual call — purely
additive; calling it directly with no pipeline present is unaffected. Depends on WR-9, WR-31
(§1), and WR-33 (§3).

---

## 7. Forwarded — `ForwardedServicesConfig` (WR-30) — build before the rest of Forwarded

```csharp
internal sealed record ForwardEntry(Type ServiceType, object TargetKey, ServiceLifetime? ExplicitLifetime);
internal sealed class ForwardedServicesConfig : Dictionary<object, ForwardEntry> { }
```

Built on `ServiceConfig` (§1/§2) directly — `services.GetOrAddConfig<ForwardedServicesConfig>(() => new())`
— **not** a bespoke `ForwardMarker` type (an earlier, superseded direction). Anything needing
"is key X a forward, and to what" (Base mode's scan, Enhanced mode's pre-build pass, cycle
detection) looks the key up in this one dictionary directly — nothing inspects or reflects on
the real forwarding descriptor's factory delegate (Design §2). This is the sole source of
truth for forward identification; build it first, since WR-5/WR-10/WR-25/WR-26 all read/write
it.

---

## 8. Forwarded — Base and Enhanced modes (WR-5, WR-10, WR-25, WR-26)

### WR-5 — `AddKeyedForward`, Base mode

```csharp
public static IServiceCollection AddKeyedForward<TService>(
    this IServiceCollection services,
    object forwardKey, object targetKey, ServiceLifetime? explicitLifetime = null)
```

- Explicit lifetime given → trust immediately, register the forward with that lifetime, no
  scan against the target.
- No lifetime given → scan the `IServiceCollection` for the target's `ServiceDescriptor` **at
  the point this method is called**. Found → match its lifetime. Not found → **throw
  immediately** (`InvalidOperationException`), don't guess.
- The actual forwarding registration is an ordinary keyed factory-delegate registration:
  `services.AddKeyedTransient(forwardKey, (sp, k) => sp.GetRequiredKeyedService<TService>(targetKey))`
  wrapped at whatever lifetime was determined above (not literally always-Transient — that
  was an earlier, superseded approach; see `DI_Forwarded_Decisions_v3.md` Decision 4's
  superseded banner and Decision 8 for the corrected, lifetime-matched approach).
- Record the forward in `ForwardedServicesConfig` (§7) as part of this same call.
- Piggyback the cycle-detection walk (WR-10) onto this same call in Base mode — see below.

### WR-10 — cycle detection

A forward's out-degree is always exactly one, so the reachable set from any starting key is
always a simple chain, never a general graph — a linked-list walk over
`ForwardedServicesConfig`, not general graph cycle detection.

- **Base mode:** at `AddKeyedForward` call time, walk from the new target through
  `ForwardedServicesConfig` entries. If the walk revisits the original forward key, **throw
  immediately** (`InvalidOperationException`) — the cycle is never registered. This piggybacks
  on the same call as WR-5's lifetime-matching scan, not a separate pass.
- **Enhanced mode:** the walk happens once, during the pre-build pass (WR-26), over every
  entry in `ForwardedServicesConfig` — one linear pass catching every cycle.

### WR-25 — Enhanced mode detection

`AddKeyedForward` checks `services.HasConfig<BuildPipelineConfig>()` (§1/§3) to decide its own
behaviour — **detected, not configured**; no separate mode flag anywhere. When present, every
forward without an explicit lifetime defers **unconditionally** to the pre-build pass (WR-26)
— no eager scan attempt first, even if the target already happens to be present at
registration time (this guarantees correctness against the *final* collection state,
independent of registration order).

### WR-26 — Enhanced mode's `IPreBuildAction`

```csharp
internal sealed class ForwardedServicesPreBuildAction : IPreBuildAction
{
    public void Execute(IServiceCollection services) { /* see below */ }
}
```

Runs the cycle-detection walk (WR-10, Enhanced-mode branch) once over every
`ForwardedServicesConfig` entry, then is the **sole final authority** on every forwarded
service's lifetime, explicit or not:

- An explicit lifetime that already matches the target → descriptor left untouched, no
  unnecessary rewrite.
- An explicit lifetime that's wrong, or none given → corrected/set here.
- Target still not found anywhere in the final collection → throw (mirrors Base mode's throw,
  just deferred to the point where "final" is actually knowable).

Register this action via `AddPreBuildAction` (the extension method built as part of §3) —
only reachable when `BuildPipelineConfig` exists, i.e. only in Enhanced mode.

### Edge cases (§8 overall)

- Forwarding does not participate in cascading key propagation — a forward is a static,
  registration-time redirect, not a per-resolution activator concern (Design §6). No
  activator code changes are owed by this Topic at all.
- Standard scoping/captive-dependency discipline still applies on top of forwarding — nothing
  new is introduced by forwarding itself; do not add bespoke captive-dependency checks.

---

## 9. Lazy (WR-11, WR-12, WR-13) — independent

### WR-11/WR-12 — generic `Lazy<T>` resolution

```csharp
internal sealed class DiLazy<T> : Lazy<T>
{
    public DiLazy(IServiceProvider provider, LazyThreadSafetyMode mode)
        : base(() => provider.GetRequiredService<T>(), mode) { }
}

public static IServiceCollection AddLazyResolution(
    this IServiceCollection services,
    LazyThreadSafetyMode mode = LazyThreadSafetyMode.ExecutionAndPublication)
```

Register `DiLazy<>` as an **open-generic type-to-type mapping against `Lazy<>` itself**,
**Transient**. `Lazy<T>` is unsealed (the BCL itself derives `Lazy<T,TMetadata>` from it for
MEF) — this is what makes the subclass trick work at all; do not attempt the equivalent for a
sealed type (contrast `Factory<T>`, §10, which needs a different trick because `Func<T>` is
sealed). Transient registration is what supplies the *currently resolving scope's* provider
to the constructor — capture that provider in the deferred closure so `.Value` resolves from
the correct scope later, with the same disposal behaviour as direct injection (Decisions
1–2).

**Repeat-call behaviour:** a second call to `AddLazyResolution()` is a no-op if the mode
argument matches the already-configured default; **throws `InvalidOperationException`** if it
specifies a conflicting mode. Track the configured mode (e.g. via a small internal marker
config, or by inspecting the existing registration) to make this check possible.

### WR-13 — explicit-key lazy resolution

```csharp
public static IServiceCollection AddLazyResolution<TService>(
    this IServiceCollection services, object key)
```

Registers the same generic `Lazy<T>` wrapper as a **keyed** type-to-type mapping against
`Lazy<>`, under `key`, for the specific closed `TService`. A consumer requests it with the
framework's own unmodified `[FromKeyedServices(key)]` on a `Lazy<TService>` parameter —
ordinary keyed resolution finds this registration by (type, key) ahead of the generic
unkeyed fallback. **Do not** implement this as a second reinterpretation of
`[FromKeyedServices]` — outside the cascading activator's own construction path, the
attribute must remain completely untouched by any code in this project (Design §8).

### Edge cases

- Do not implement `Lazy<T>` special-casing inside the cascading activator here — that's
  WR-7 (§9 below in this document — see §11), which needs this type to exist first.
- No dedicated diagnostic surface for `Lazy<T>` resolvability (Design §7) — don't add one.

---

## 10. Factory (WR-15, WR-16, WR-19) — independent

### WR-15/WR-16 — `Factory<T>`

```csharp
public class Factory<T>
{
    private readonly IServiceProvider _provider;
    public Factory(IServiceProvider provider) => _provider = provider;

    public T? GetService() => _provider.GetService<T>();
    public T GetRequiredService() => _provider.GetRequiredService<T>();
}

services.AddTransient(typeof(Factory<>), typeof(Factory<>));
```

`Factory<T>` is a **library-owned open-generic type registered against itself** — not a
subclass-of-sealed-type trick (impossible; `Func<T>` is sealed and delegates can't be
subclassed at all, Design §2) and not an implicit-conversion-to-`Func<T>` (deliberately
rejected — would silently collapse `GetService`/`GetRequiredService` back to one throwing
behaviour). Same scope-capture mechanics as `Lazy<T>` — Transient registration supplies the
resolving scope's provider to the constructor.

**No `.AsFunc()` extension** — out of scope for this package (WorkPackage §1).

### WR-19 — `KeyedFactory<T>`

```csharp
public class KeyedFactory<T>
{
    private readonly IServiceProvider _provider;
    private readonly object? _ambientKey; // null unless constructed via the cascading activator

    public KeyedFactory(IServiceProvider provider, object? ambientKey = null)
    {
        _provider = provider;
        _ambientKey = ambientKey;
    }

    public T? GetService(object key) =>
        _ambientKey is null
            ? _provider.GetKeyedService<T>(key)
            : _provider.GetKeyedService<T>(CompoundKeyHelper.Combine(_ambientKey, key));

    public T GetRequiredService(object key) =>
        _ambientKey is null
            ? _provider.GetRequiredKeyedService<T>(key)
            : _provider.GetRequiredKeyedService<T>(CompoundKeyHelper.Combine(_ambientKey, key));
}
```

One adapting type, not two — behaviour depends on **how it was constructed** (whether an
ambient key was supplied), not a separate overload or sibling type. Outside the cascading
activator, `_ambientKey` is always `null` and this behaves as the plain explicit-key case
(equivalent to `Lazy`'s explicit-key parity feature, WR-13). Reuses §4.1's
`CompoundKeyHelper.Combine` — do not build a second combining path here. No fallback layering
beyond a single `CompoundKey` lookup — it either resolves or it doesn't (Design §4).

**Scope note:** parameterization is limited to key-based data only — no generic
`Func<TArg, T>`-style factory (WorkPackage §1, explicitly out of scope).

---

## 11. Owned (WR-22, WR-23, WR-41, WR-43) — independent; build together as one constructor

```csharp
public class Owned<T> : IDisposable, IAsyncDisposable
{
    private readonly AsyncServiceScope _scope;
    public T Value { get; }

    public Owned(IServiceScopeFactory scopeFactory)
    {
        var scope = scopeFactory.CreateAsyncScope();
        try
        {
            Value = scope.ServiceProvider.GetRequiredService<T>();
            _scope = scope;
        }
        catch
        {
            scope.Dispose(); // sync dispose is safe here — nothing else was resolved yet
            throw;
        }
    }

    public void Dispose() => _scope.Dispose();
    public ValueTask DisposeAsync() => _scope.DisposeAsync();
}

services.AddTransient(typeof(Owned<>), typeof(Owned<>));
```

**These four WR-IDs are not independently schedulable — implement as one constructor, one
pass:**

- **WR-22** — the open-generic registration, `CreateAsyncScope()`-based child scope, eager
  resolution of `T` within it. Eager, not lazy — resolves `T` immediately at construction
  (Design §2); do not add any laziness option.
- **WR-23/WR-41** — `IDisposable`/`IAsyncDisposable` both back onto the same
  `AsyncServiceScope` field; `CreateAsyncScope()` is used **unconditionally**, not as an
  opt-in variant — the cost is negligible either way (Design §2.1). Do not build a plain
  `IServiceScope`/`CreateScope()` code path as an alternative.
- **WR-43** — the `try`/`catch`/`scope.Dispose()`/`throw` shown above is the actual fix, not
  optional hardening — without it, any resolution failure for `T` inside the constructor
  silently leaks the child scope, since no `Owned<T>` instance is ever produced to call
  `Dispose()` on (Design §2.2).

### Known, accepted caveat (document, don't engineer around)

If `T` (or something it transitively captured) implements `IAsyncDisposable` **without** also
implementing `IDisposable`, calling the synchronous `Dispose()` still throws
(`InvalidOperationException`) — inherited directly from `AsyncServiceScope`'s own behaviour,
not specific to `Owned<T>`. Document this in the Guide (WR-45); do not add code to detect or
work around it.

### Not in scope

No scope pooling/reuse (Design §3, accepted cost) — do not build this even as an internal
optimization.

---

## 12. Special-casing for `Lazy<T>`/`Factory<T>`/`KeyedFactory<T>`/`Owned<T>` in the activator (WR-7, WR-20, WR-24) — one implementation unit, build last among the four Topics above

Once `Lazy<T>` (§9), `Factory<T>`/`KeyedFactory<T>` (§10), and `Owned<T>` (§11) exist, extend
the cascading activator (§4.2) to recognize all four types **uniformly** as an immediate
constructor parameter type:

- **`Lazy<T>` / `Factory<T>`** — the activator captures the ambient key immediately at
  construction time, then defers the actual keyed-then-fallback lookup for `T` into the
  deferred closure (`Lazy<T>`'s `.Value` accessor / `Factory<T>`'s `GetService()`/
  `GetRequiredService()` call) — composing cascading and deferred resolution together rather
  than one overriding the other.
- **`KeyedFactory<T>`** — the activator captures the ambient key and passes it into the
  constructed instance's `_ambientKey` field (§10's `KeyedFactory<T>` constructor overload
  that takes `ambientKey`), rather than driving a keyed-then-fallback lookup directly — the
  combination with a caller-supplied variant key happens later, at each `GetService(key)`
  call (§10).
- **`Owned<T>`** — unlike the other three, `Owned<T>` doesn't defer at all: the captured
  ambient key drives a keyed-then-fallback resolution of `T`, performed **eagerly**, from
  within `Owned<T>`'s own new child scope, at the point `Owned<T>` itself is constructed by
  the activator.

**This is exactly one pass over the activator's parameter-handling logic, recognizing all
four types together** — do not implement these as three separate, sequential changes to the
same code path; the WorkPackage groups WR-7/20/24 as one unit for exactly this reason.

### Explicitly not in scope: nested special-type key threading

The activator recognizes these four types only as an **immediate** constructor parameter
type — it has (and must have) no handling for one nested inside another (e.g.
`Owned<Lazy<T>>`). This is intended, documented scope, not a gap to close in this pass
(`DI_KeyedServices_Design_v7.md` §13, Decision 24) — the *only* code owed for this case is
the `ValidateKeyedCascading()` diagnostic extension, §6/WR-42 above. Do not add recursive
unwrapping logic here.

---

## 13. Verification commands

This design corpus does not itself contain the target repository, so this package cannot
state exact build/test invocations (project file names, target framework, test runner) with
certainty. Per the dev-side methodology's rule 6: **if this Implementation document's
Verification commands section can't state package-specific commands, it must say so
explicitly and point at the repo's standing commands** — which it does here:

- Run the repository's standard full solution build command (see `CLAUDE.md` at the dev
  repo's root for the exact command).
- Run the repository's standard full test command (see `CLAUDE.md`).
- **Package-specific smoke check, in lieu of a named test filter:** because this package
  spans every Topic, there is no single meaningful test-name filter narrower than "the full
  suite" — confirm instead that the full suite includes coverage for each of the eight
  Topics' new public surface (`ServiceConfig<T>`, `BuildPipelineFactory<>`, the cascading
  activator + `CompoundKey`, `AddKeyedForward` Base and Enhanced modes, `AddLazyResolution()`
  (both generic and explicit-key), `Factory<T>`/`KeyedFactory<T>`, and `Owned<T>` (both sync
  and async disposal, and the constructor-exception-safety path specifically)) before
  declaring the Definition of Done met.
- If `CLAUDE.md` specifies a package-specific verification step this document doesn't
  anticipate, follow that instead — this section is a fallback, not an override.
