# DI_ServiceBuildExtensions_WorkPackage_Outcome_2026-07-28-1_v1

**Project:** DigitalBusiness.DependencyInjectionExtensions (doc prefix: `DI`)
**Topic:** ServiceBuildExtensions
**Document type:** WorkPackage_Outcome
**Reports on:** `DI_ServiceBuildExtensions_WorkPackage_2026-07-28-1_v1.md`
**Date:** 2026-07-28
**Status:** Unprocessed

---

## 1. Summary

Implemented the `ServiceBuildExtensions` mechanism in full: `IPreBuildAction`/`ICleanupAction`,
the `ServiceConfig<T>`-backed `BuildPipelineConfig`, `AddBuildPipeline`/`AddPreBuildAction`/
`AddCleanupAction`, the default `RemoveOtherServiceConfigsCleanupAction`,
`BuildPipelineFactory<TContainerBuilder>`, and the unified host install for both `IHostBuilder`
and `HostApplicationBuilder`. All new code lives under
`Projects\DigitalBusiness.DependencyInjectionExtensions\ServiceBuildExtensions\`, matching the
`ServiceConfig` Topic's folder/namespace convention. The one substantive shape deviation from
the Implementation doc's sample code is the `BuildPipelineFactory`/`UseServiceBuildExtensions`
overload split (§2, §3) — needed because the doc's own sample didn't compile safely. Two
`PackageReference`s the project didn't have were added. Full solution build succeeds; the full
test suite passes except for the `DigitalBusiness.JsonDataWrappers.Tests` project, which was
excluded from this package's verification run due to a known, pre-existing, unrelated
stack-overflow bug.

## 2. Task-by-task deviations

- **§3a Core types** (`IPreBuildAction`, `ICleanupAction`, `BuildPipelineConfig`,
  `AddBuildPipeline`, `AddPreBuildAction`/`AddCleanupAction`, `BuildPipelineFactory`): matched
  plan, with one deviation — `BuildPipelineFactory<TContainerBuilder>`'s constructor takes a
  **required** `inner` (no nullable default, no unsafe cast). See §3 below.
- **§3b Default cleanup action** (`RemoveOtherServiceConfigsCleanupAction`): matched plan's code
  sample exactly. One documentation inconsistency surfaced — see §3 below (the doc's prose says
  this action doesn't remove `ServiceConfig<BuildPipelineConfig>`, but its own given code sample
  has no such exclusion).
- **§3c Unified install** (`UseServiceBuildExtensions`, `HostApplicationBuilder` equivalent):
  deviated by design — split into a zero-arg overload and an explicit-inner generic overload for
  each receiver type, instead of one method with a nullable default `inner` parameter. See §3.
- **§3d WR-28 documentation confirmation**: matched plan. Confirmed the single-factory host seam
  is real: `IHostBuilder.UseServiceProviderFactory` and `HostApplicationBuilder.ConfigureContainer`
  each only support one active factory registration (last-registration-wins), with no host API to
  compose multiple factories. This is consistent with — not a violation of — this library's own
  "one override idiom" convention, so no workaround was attempted or needed.
- **Implementation §3 note — `DefaultServiceProviderFactory`/`TContainerBuilder` generic-default
  resolution:** resolved by requiring `inner` and splitting overloads. See §3.
- **Implementation §3 note — `RemoveConfig<T>` vs. direct `services.Remove(descriptor)`:**
  resolved in favor of the direct-`Remove` fallback the Implementation doc itself sanctioned (no
  new public method added to the already-shipped `ServiceConfig` Topic).

## 3. Design or approach changes discovered

1. **`BuildPipelineFactory`'s documented default-`inner` shape doesn't compile safely.** The
   Implementation doc's sample constructs a fallback via
   `(IServiceProviderFactory<TContainerBuilder>)(object)new DefaultServiceProviderFactory()`.
   `DefaultServiceProviderFactory` only implements `IServiceProviderFactory<IServiceCollection>`,
   so for any `TContainerBuilder` other than `IServiceCollection` this is an unchecked cast that
   throws `InvalidCastException` at the moment the factory is actually used. The doc itself
   flagged this as an open question and asked for `UseServiceBuildExtensions()` to remain callable
   with zero type arguments, with the resolution recorded here.

   **Resolution implemented:** `BuildPipelineFactory<TContainerBuilder>`'s constructor now takes a
   required, non-nullable `inner` — no default, no cast. Each install surface
   (`ServiceBuildExtensionsHostExtensions`) is split into two overloads per receiver type:
   - `UseServiceBuildExtensions()` (zero-arg) — wraps `new DefaultServiceProviderFactory()` in
     `BuildPipelineFactory<IServiceCollection>`. This is what makes the zero-type-argument call
     work, safely, without any cast.
   - `UseServiceBuildExtensions<TBuilder>(IServiceProviderFactory<TBuilder> inner)` — explicit,
     for custom containers; `inner` is required so there's nothing to default unsafely.

   Same split applied to the `HostApplicationBuilder` equivalent. This is a public API shape
   change from the doc's literal sample (one generic method with a nullable default parameter →
   two overloads with a required parameter), but preserves every documented behavior and callsite
   ergonomics (`UseServiceBuildExtensions()` with zero arguments still works for the common case).

2. **`RemoveOtherServiceConfigsCleanupAction`'s documented behavior and its own code sample
   disagree.** The Implementation doc's prose states this action "does not remove
   `ServiceConfig<BuildPipelineConfig>` itself," but the code sample it gives for the action has
   no exclusion for that type — its scan matches every closed `ServiceConfig<>` generic
   regardless of `T`, which includes `ServiceConfig<BuildPipelineConfig>`. Implemented exactly as
   the code sample specifies (no added exclusion), since the end-to-end behavior is identical
   either way: `BuildPipelineFactory.CreateServiceProvider`'s own hardcoded, unconditional removal
   step still runs after cleanup actions regardless, so `ServiceConfig<BuildPipelineConfig>` is
   guaranteed to be gone by the time the container builds whether or not the default cleanup
   action happened to remove it first. A test
   (`RemoveOtherServiceConfigsCleanupActionTests.Execute_AlsoRemovesServiceConfigOfBuildPipelineConfigItself`)
   pins down the actual (as-coded) behavior. Flagging this as a documentation inconsistency to fix
   at the source, not a functional bug.

3. **Two missing `PackageReference`s.** Neither the WorkPackage nor Implementation doc mentioned
   that `DigitalBusiness.DependencyInjectionExtensions.csproj` needed new package references.
   `DefaultServiceProviderFactory` lives in the concrete `Microsoft.Extensions.DependencyInjection`
   package (the project only had `.Abstractions`); `IHostBuilder`/`HostApplicationBuilder` need
   `Microsoft.Extensions.Hosting`. Both added at version `10.0.8`, matching the existing
   `.Abstractions` reference. This also required bumping the Tests project's own direct
   `Microsoft.Extensions.DependencyInjection` reference from `10.0.0` to `10.0.8` to avoid a
   `NU1605` package-downgrade error once the source project pulled in `10.0.8` transitively.

4. **Implementation doc's Verification commands name a solution file that doesn't exist.** It
   specifies `dotnet build/test DigitalBusiness.DependencyInjectionExtensions.sln`; no such file
   exists anywhere in the repo (confirmed by filesystem search) — only
   `Solutions\DigitalBusiness.slnx` (the full repo solution) exists. Used that instead, per
   CLAUDE.md's fallback instruction to run the full suite when a package gives no valid
   project-specific filter.

## 4. New open questions or follow-up work

- Should `DI_ServiceBuildExtensions_Implementation` (or the Design doc, if it also contains this
  sample) be corrected so its `BuildPipelineFactory`/`UseServiceBuildExtensions` code sample
  reflects the required-`inner`, split-overload shape rather than the non-compiling
  default-cast version? Recommend yes, to prevent the same open question resurfacing for a future
  reader.
- Should `RemoveOtherServiceConfigsCleanupAction`'s documented behavior (design/decisions text) be
  corrected to say it *does* incidentally match `ServiceConfig<BuildPipelineConfig>` too, with the
  guarantee resting entirely on `BuildPipelineFactory`'s own unconditional step? This is the more
  accurate framing and avoids a future implementer trying to add an exclusion that isn't actually
  needed.
- Recommend `DigitalBusiness.DependencyInjectionExtensions.csproj`'s package references
  (`Microsoft.Extensions.DependencyInjection`, `Microsoft.Extensions.Hosting`) be captured in the
  Design doc's context section for this Topic, so a future dev-side session doesn't have to
  rediscover the gap.
- WR-30 (`ForwardedServicesConfig`) and downstream WR-5/10/25/26 (`Forwarded`) are now unblocked
  by this package's completion, per the WorkPackage's own "Unblocks" note — no change to that
  readiness assessment.
- The known `JsonDataWrappers.JsonDataTypedPathExtensions.SetDeep` stack-overflow bug
  (`task_4a72522a`) is still present and still crashes the test host; filtering out a single named
  test (`SetDeepTyped`) was not sufficient to avoid the crash during this package's verification —
  the entire `DigitalBusiness.JsonDataWrappers.Tests` project had to be excluded. Worth noting for
  whoever eventually fixes that bug: the crash reproduces even when the two `SetDeepTyped`-named
  tests are excluded, so more than those two tests exercise the recursive path.

## 5. Verification status

- **Build:** `dotnet build Solutions\DigitalBusiness.slnx` — succeeded, 0 errors (231 pre-existing
  warnings, all in `DigitalBusiness.JsonDataWrappers`, unrelated to this package).
- **Test:** `dotnet test Solutions\DigitalBusiness.slnx --filter "FullyQualifiedName!~DigitalBusiness.JsonDataWrappers.Tests"`
  — all included projects passed: `DigitalBusiness.DependencyInjectionExtensions.Tests` 46/46
  (15 pre-existing `ServiceConfig` tests + 31 new `ServiceBuildExtensions` tests),
  `DigitalBusiness.Extensibility.Tests` 30/30. `DigitalBusiness.JsonDataWrappers.Tests` excluded
  per the known pre-existing `SetDeep` stack-overflow issue (`task_4a72522a`) — a single-test
  filter did not avoid the crash, so the whole project was excluded as the workaround; not a
  regression introduced by this package.
- **Definition of Done (Work Package §5):** code tasks §3a–3c complete and building — met;
  verification succeeds under the adjusted commands above — met; this Outcome document produced —
  met (reconciliation into the design-side corpus is a separate, design-side step); WorkRegister
  entries marked `done` — not applicable from this repo (no local copy of
  `DI_WorkRegister_v12.md`), proposed for the design side in §6.
- **Effect from the known `JsonDataWrappers.SetDeep` issue:** blocked a clean full-suite run, as
  anticipated by the Implementation doc. Workaround: excluded the whole
  `DigitalBusiness.JsonDataWrappers.Tests` project via
  `--filter "FullyQualifiedName!~DigitalBusiness.JsonDataWrappers.Tests"`, since a filter naming
  only the `SetDeepTyped` tests still crashed (see §4 — more tests than just those two reach the
  recursive path).

## 6. Author's proposed document updates

- [ ] `DI_WorkRegister_v12.md` → v13: mark WR-33, WR-34, WR-35, WR-28 `done`; update §10's
      suggested build order note now that `ServiceBuildExtensions` has shipped
- [ ] `DI_ServiceBuildExtensions_Design_v2.md` → v3: update the `BuildPipelineFactory`/
      `UseServiceBuildExtensions` code sample to the required-`inner`, split-overload shape
      (§3 item 1), and correct `RemoveOtherServiceConfigsCleanupAction`'s documented behavior
      re: `ServiceConfig<BuildPipelineConfig>` (§3 item 2)
- [ ] `DI_ServiceBuildExtensions_Decisions_v2.md` → v3: record the overload-split resolution
      (§3 item 1) as a new Decision, matching the pattern of `ServiceConfig`'s Decisions 6–7
- [ ] `DI_Index_v17.md` → v18: reflect new document versions, archive this package's family
- [ ] `DI_Guide_v1.md`: `ServiceBuildExtensions` section (WR-38) now eligible for real content —
      still a separate, unpackaged item, not part of this reconciliation unless explicitly
      folded in
- [ ] Note the pre-existing `JsonDataWrappers.SetDeep` bug's broader repro surface (§4) wherever
      `task_4a72522a` is tracked
