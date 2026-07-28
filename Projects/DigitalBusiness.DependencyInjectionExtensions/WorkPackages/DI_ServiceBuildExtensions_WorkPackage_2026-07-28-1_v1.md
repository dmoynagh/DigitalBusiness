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

- [ ] None required beyond what's already current. `DI_ServiceBuildExtensions_Design_v2.md`
      and `DI_ServiceBuildExtensions_Decisions_v2.md` are already at their target state —
      this package implements what they already specify. Any deviation surfaced during dev
      work is the Outcome's job to report (§6a), not a pre-emptive doc edit here.
- [ ] `DI_Guide_v1.md`'s `ServiceBuildExtensions` section (WR-38) remains **unpackaged** —
      explicitly out of scope for this package; do not write it here. It can only be written
      well once this package's Outcome confirms the shipped shape.

## 3. Code tasks

**Repo convention (carried over from `ServiceConfig`'s Outcome, `DI_WorkRegister_v12.md`
§9a):** create a `ServiceBuildExtensions\` subfolder (namespace
`DigitalBusiness.DependencyInjectionExtensions.ServiceBuildExtensions`), matching the
one-subfolder-per-Topic convention `ServiceConfig` established. Write all new extension
methods using the repo's `extension(IServiceCollection services) { ... }` block syntax, not
the classic `this IServiceCollection` parameter style.

### 3a. Core types (WR-33)
- [ ] `IPreBuildAction` interface — `void Execute(IServiceCollection services)`
- [ ] `ICleanupAction` interface — `void Execute(IServiceCollection services)` (distinct
      type from `IPreBuildAction`, not a shared/tagged interface — Decision 7)
- [ ] `BuildPipelineConfig` (internal sealed class) — `RunPreBuildActions`/
      `RunCleanupActions` bools (default `true`), `PreBuildActions`/`CleanupActions` lists
      of plain instances (not `IServiceCollection` registrations — Decision 8/9)
- [ ] `AddBuildPipeline()` extension method — idempotent via `GetOrAddConfig<T>`'s
      get-or-create semantics; registers one default `ICleanupAction`
      (`RemoveOtherServiceConfigsCleanupAction`, §3b) unconditionally on first call
- [ ] `AddPreBuildAction`/`AddCleanupAction` extension methods — **throw**
      `InvalidOperationException` if called before `AddBuildPipeline()` (i.e. no
      `BuildPipelineConfig` present); must not silently auto-create one via `GetOrAddConfig`
- [ ] `BuildPipelineFactory<TContainerBuilder>` — decorator over
      `IServiceProviderFactory<TContainerBuilder>`. `CreateBuilder` delegates straight
      through. `CreateServiceProvider`: reads `BuildPipelineConfig` via `GetConfig<T>` (not
      hand-constructed); if absent, delegates straight through as a no-op; if present, runs
      pre-build actions in list order (if `RunPreBuildActions`), then cleanup actions in
      list order (if `RunCleanupActions`), then unconditionally/non-toggleably removes
      `ServiceConfig<BuildPipelineConfig>` itself if present, then delegates to the inner
      factory's `CreateServiceProvider`

### 3b. Default cleanup action (WR-35)
- [ ] `RemoveOtherServiceConfigsCleanupAction` (internal, implements `ICleanupAction`) —
      finds and removes every remaining `ServiceConfig<>` closed-generic descriptor via
      `d.ServiceType.IsGenericType && d.ServiceType.GetGenericTypeDefinition() ==
      typeof(ServiceConfig<>)`, regardless of `T`. Registered by `AddBuildPipeline()` (§3a);
      toggleable by removing it from `CleanupActions` or setting `RunCleanupActions = false`.
      Does **not** itself remove `ServiceConfig<BuildPipelineConfig>` — that removal is the
      hardcoded, separate step in `BuildPipelineFactory` (§3a), not this action.

### 3c. Unified install (WR-34)
- [ ] `UseServiceBuildExtensions<TBuilder>(this IHostBuilder host,
      IServiceProviderFactory<TBuilder>? inner = null)` — wraps `inner ?? new
      DefaultServiceProviderFactory()` in `BuildPipelineFactory<TBuilder>` via
      `UseServiceProviderFactory`, and registers `AddBuildPipeline()` via a
      `ConfigureServices` callback, in one call
- [ ] `HostApplicationBuilder` equivalent — pairs `ConfigureContainer(...)` with a direct
      `builder.Services.AddBuildPipeline()` call (no `ConfigureServices` deferral needed
      there, per Design §2)

### 3d. Documentation-only (WR-28)
- [ ] No code task. Confirm during implementation that the single-factory host seam
      limitation (Decision 5) is real and unavoidable via the host APIs, and note this in
      the Outcome rather than attempting a workaround.

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

- [ ] All code tasks in §3a–3c complete and building
- [ ] Verification commands in the Implementation document all succeed
- [ ] An Outcome document has been produced and its proposed updates reconciled into the
      affected corpus documents (Design/Decisions/Guide/OpenQuestions/WorkRegister as needed)
- [ ] `DI_WorkRegister_v12.md` §8 entries WR-33, WR-34, WR-35, WR-28 marked `done`

## 6. Notes / issues encountered

*(populated during execution)*

## 7. Outcome reporting

Once dev-side work is complete, fill in
`DI_ServiceBuildExtensions_WorkPackage_Outcome_2026-07-28-1_v1.md` (bundled alongside this
package) per `DocumentationMethodology_v9.md` §6a: summary, task-by-task deviations,
design/approach changes discovered, new open questions or follow-up work, verification
status, and a proposed document-updates checklist. Bring it back into this project for
reconciliation. If only this Work Package file survives the trip to the dev side, recreate
the Outcome using that same filename and structure — don't invent a different name or shape.
