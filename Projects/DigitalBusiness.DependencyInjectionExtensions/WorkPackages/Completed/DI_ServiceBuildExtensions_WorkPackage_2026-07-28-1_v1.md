# DI_ServiceBuildExtensions_WorkPackage_2026-07-28-1_v1

**Project:** DigitalBusiness.DependencyInjectionExtensions (doc prefix: `DI`)
**Topic:** ServiceBuildExtensions
**Document type:** WorkPackage (live — edited in place as tasks complete)
**Fulfills:** `DI_WorkRegister_v12.md` §8 — WR-33, WR-34, WR-35, WR-28

## Related documents

- Design: `DI_ServiceBuildExtensions_Design_v2.md`
- Decisions: `DI_ServiceBuildExtensions_Decisions_v2.md` (Decisions 1, 2, 5, 6, 7, 8, 9, 10, 11)
- Implementation: `DI_ServiceBuildExtensions_WorkPackage_Implementation_2026-07-28-1_v1.md`
- Outcome (skeleton): `DI_ServiceBuildExtensions_WorkPackage_Outcome_2026-07-28-1_v1.md`
- Depends on: `DI_ServiceConfig_Design_v2.md` / `DI_ServiceConfig_Decisions_v2.md` (WR-31,
  **done** — this package's `BuildPipelineConfig` is a `ServiceConfig<T>`, registered/read
  only via `GetOrAddConfig<T>`/`GetConfig<T>`, per that Decisions doc's Decision 7)
- Unblocks: WR-30 (`ForwardedServicesConfig`), and downstream WR-5/10/25/26 (`Forwarded`)

## Bundle contents (§7b)

Travels into the dev project as one bundle:
- This Work Package
- `DI_ServiceBuildExtensions_WorkPackage_Implementation_2026-07-28-1_v1.md`
- `DI_ServiceBuildExtensions_WorkPackage_Outcome_2026-07-28-1_v1.md` (skeleton, to be filled in)

No full Design copy is attached — the Implementation document's "Design context" section
(per methodology §7a) extracts the relevant slice of `DI_ServiceBuildExtensions_Design_v2.md`
directly, since this package's scope is narrow enough that extraction is not lossy.

---

## 1. Scope

Build the `ServiceBuildExtensions` mechanism end to end: the two-phase build pipeline
(`IPreBuildAction`/`ICleanupAction`), its config-driven install (`BuildPipelineConfig`,
backed by `ServiceConfig<T>`), the decorator that actually runs it
(`BuildPipelineFactory<TContainerBuilder>`), and the unified host-level install
(`UseServiceBuildExtensions()`). This is the last unblocked, self-contained piece of
infrastructure before `Forwarded`'s Enhanced mode (WR-25/26) and `ForwardedServicesConfig`
(WR-30) can be built.

**Why now:** WR-31 (`ServiceConfig`) shipped and unblocked this Topic; per
`DI_WorkRegister_v12.md` §10's suggested build order, `ServiceBuildExtensions` is next.

**Out of scope:** `ForwardedServicesConfig` (WR-30) and `Forwarded`'s own Base/Enhanced-mode
behaviour (WR-5/10/25/26) — those are separate, later packages that consume this Topic's
output but aren't part of it. WR-28 (composability limitation) is documentation-only and
included here for completeness, not as a code task.

## 2. Documentation tasks

- [x] None required beyond what's already current. `DI_ServiceBuildExtensions_Design_v2.md`
      and `DI_ServiceBuildExtensions_Decisions_v2.md` are already at their target state —
      this package implements what they already specify. Any deviation surfaced during dev
      work is the Outcome's job to report (§6a), not a pre-emptive doc edit here.
- [x] `DI_Guide_v1.md`'s `ServiceBuildExtensions` section (WR-38) remains **unpackaged** —
      explicitly out of scope for this package; do not write it here. It can only be written
      well once this package's Outcome confirms the shipped shape. (Confirmed still out of
      scope; not written as part of this package.)

## 3. Code tasks

**Repo convention (carried over from `ServiceConfig`'s Outcome, `DI_WorkRegister_v12.md`
§9a):** create a `ServiceBuildExtensions\` subfolder (namespace
`DigitalBusiness.DependencyInjectionExtensions.ServiceBuildExtensions`), matching the
one-subfolder-per-Topic convention `ServiceConfig` established. Write all new extension
methods using the repo's `extension(IServiceCollection services) { ... }` block syntax, not
the classic `this IServiceCollection` parameter style.

### 3a. Core types (WR-33)
- [x] `IPreBuildAction` interface — `void Execute(IServiceCollection services)`
- [x] `ICleanupAction` interface — `void Execute(IServiceCollection services)` (distinct
      type from `IPreBuildAction`, not a shared/tagged interface — Decision 7)
- [x] `BuildPipelineConfig` (internal sealed class) — `RunPreBuildActions`/
      `RunCleanupActions` bools (default `true`), `PreBuildActions`/`CleanupActions` lists
      of plain instances (not `IServiceCollection` registrations — Decision 8/9)
- [x] `AddBuildPipeline()` extension method — idempotent via `GetOrAddConfig<T>`'s
      get-or-create semantics; registers one default `ICleanupAction`
      (`RemoveOtherServiceConfigsCleanupAction`, §3b) unconditionally on first call
- [x] `AddPreBuildAction`/`AddCleanupAction` extension methods — **throw**
      `InvalidOperationException` if called before `AddBuildPipeline()` (i.e. no
      `BuildPipelineConfig` present); must not silently auto-create one via `GetOrAddConfig`
- [x] `BuildPipelineFactory<TContainerBuilder>` — decorator over
      `IServiceProviderFactory<TContainerBuilder>`. `CreateBuilder` delegates straight
      through. `CreateServiceProvider`: reads `BuildPipelineConfig` via `GetConfig<T>` (not
      hand-constructed); if absent, delegates straight through as a no-op; if present, runs
      pre-build actions in list order (if `RunPreBuildActions`), then cleanup actions in
      list order (if `RunCleanupActions`), then unconditionally/non-toggleably removes
      `ServiceConfig<BuildPipelineConfig>` itself if present, then delegates to the inner
      factory's `CreateServiceProvider`. **Deviation:** constructor takes a required
      (non-nullable) `inner`, no default — see Outcome §3/§4.

### 3b. Default cleanup action (WR-35)
- [x] `RemoveOtherServiceConfigsCleanupAction` (internal, implements `ICleanupAction`) —
      finds and removes every remaining `ServiceConfig<>` closed-generic descriptor via
      `d.ServiceType.IsGenericType && d.ServiceType.GetGenericTypeDefinition() ==
      typeof(ServiceConfig<>)`, regardless of `T`. Registered by `AddBuildPipeline()` (§3a);
      toggleable by removing it from `CleanupActions` or setting `RunCleanupActions = false`.
      **Deviation:** as coded (matching the Implementation doc's own sample), this action's
      scan does *not* specially exclude `ServiceConfig<BuildPipelineConfig>` — see Outcome §3.

### 3c. Unified install (WR-34)
- [x] `UseServiceBuildExtensions(this IHostBuilder host)` / `UseServiceBuildExtensions<TBuilder>(this
      IHostBuilder host, IServiceProviderFactory<TBuilder> inner)` — wraps the container factory
      in `BuildPipelineFactory<TBuilder>` via `UseServiceProviderFactory`, and registers
      `AddBuildPipeline()` via a `ConfigureServices` callback, in one call. **Deviation:** split
      into a zero-arg and an explicit-inner overload rather than one method with a nullable
      default parameter — see Outcome §3/§4.
- [x] `HostApplicationBuilder` equivalent — pairs `ConfigureContainer(...)` with a direct
      `builder.Services.AddBuildPipeline()` call (no `ConfigureServices` deferral needed
      there, per Design §2). Same zero-arg/explicit-inner overload split as above.

### 3d. Documentation-only (WR-28)
- [x] No code task. Confirmed during implementation that the single-factory host seam
      limitation (Decision 5) is real and unavoidable via the host APIs — only one
      `UseServiceProviderFactory` registration can be active per host builder
      (last-registration-wins), matching this library's own "one override idiom" convention.
      No workaround attempted; see Outcome §3.

## 4. Sequencing

1. §3a core types first (everything else depends on `BuildPipelineConfig` existing and
   `BuildPipelineFactory` being able to read it).
2. §3b (default cleanup action) can be written alongside §3a — it's referenced by
   `AddBuildPipeline()` but has no dependency the other direction.
3. §3c (unified install) last — depends on `BuildPipelineFactory` (§3a) existing.
4. §3d is a documentation confirmation only; no sequencing dependency.

No task here depends on a decision that hasn't actually been made — Decisions 1–11 in
`DI_ServiceBuildExtensions_Decisions_v2.md` cover every task above. If implementation
surfaces a gap Decisions doesn't actually answer, stop and flag it as a blocker in the
Outcome rather than guessing.

## 5. Definition of done

- [x] All code tasks in §3a–3c complete and building
- [x] Verification commands succeed, adjusted per Outcome §3/§6 (the Implementation doc's
      named `.sln` doesn't exist in this repo; full solution build/test used instead, with the
      JsonDataWrappers.Tests project excluded from the test run due to a known pre-existing
      crash unrelated to this package)
- [x] An Outcome document has been produced (this bundle); reconciliation into the design-side
      corpus documents happens on that side's own schedule, not part of dev-side completion
- [ ] `DI_WorkRegister_v12.md` §8 entries WR-33, WR-34, WR-35, WR-28 marked `done` — this repo
      doesn't hold a copy of that document; proposed for the design side, see Outcome §7

## 6. Notes / issues encountered

- Project file (`DigitalBusiness.DependencyInjectionExtensions.csproj`) only referenced
  `Microsoft.Extensions.DependencyInjection.Abstractions`; needed `Microsoft.Extensions.DependencyInjection`
  (for `DefaultServiceProviderFactory`) and `Microsoft.Extensions.Hosting` (for `IHostBuilder`/
  `HostApplicationBuilder`) added, neither of which either doc anticipated. See Outcome §3.
- The Implementation doc's own sample code for `BuildPipelineFactory<TContainerBuilder>`'s
  default-`inner` cast doesn't compile safely (unchecked cast, throws for any
  `TContainerBuilder` other than `IServiceCollection`); the doc itself flagged this as
  unresolved and delegated the resolution here. See Outcome §3/§4.
- `ServiceConfigExtensions` has no `RemoveConfig<T>`, per the Implementation doc's own
  §9a-sanctioned fallback of removing the matching descriptor directly. `BuildPipelineFactory`
  now does so via the internal `ServiceConfigExtensions.FindConfigDescriptors<T>()` helper (see
  the last bullet below, from the second review's follow-up pass) rather than a separately
  hand-rolled scan.
- The Implementation doc's Verification commands section names
  `DigitalBusiness.DependencyInjectionExtensions.sln`, which does not exist anywhere in this
  repo; used `Solutions\DigitalBusiness.slnx` (the full repo solution) instead, per CLAUDE.md.
- The known pre-existing `JsonDataWrappers.JsonDataTypedPathExtensions.SetDeep` stack-overflow
  bug (`task_4a72522a`) still crashes the test host; a single-test filter (`SetDeepTyped`) was
  not sufficient to avoid it, so the whole `DigitalBusiness.JsonDataWrappers.Tests` project was
  excluded from this package's verification run. See Outcome §5.
- An independent second-pass review (GPT-5.3 Codex, per this project's dual-AI review approach)
  found four implementation-quality defects — a misleading XML doc comment on
  `RemoveOtherServiceConfigsCleanupAction`, `BuildPipelineFactory` removing only the first
  matching `ServiceConfig<BuildPipelineConfig>` descriptor instead of all matches, no guard
  against calling `CreateServiceProvider` before `CreateBuilder`, and unguarded action-list
  iteration that would throw if an action registered another action mid-run. All four fixed,
  with three new tests added; full suite re-verified green. See Outcome §3 item 5.
- Same reviewer's follow-up pass accepted three of the four outright and held one nit open: the
  removal in `BuildPipelineFactory` still duplicated `HasConfig<T>`'s match predicate inline
  instead of sharing it. Extracted an internal `ServiceConfigExtensions.FindConfigDescriptors<T>()`
  helper, now used by both. Small, internal-only touch to the already-shipped `ServiceConfig`
  Topic, in direct service of this package. See Outcome §3 item 5.

## 7. Outcome reporting

Once dev-side work is complete, fill in
`DI_ServiceBuildExtensions_WorkPackage_Outcome_2026-07-28-1_v1.md` (bundled alongside this
package) per `DocumentationMethodology_v9.md` §6a: summary, task-by-task deviations,
design/approach changes discovered, new open questions or follow-up work, verification
status, and a proposed document-updates checklist. Bring it back into this project for
reconciliation. If only this Work Package file survives the trip to the dev side, recreate
the Outcome using that same filename and structure — don't invent a different name or shape.
