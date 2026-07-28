# DI_ServiceBuildExtensions_WorkPackage_Outcome_2026-07-28-1_v1

**Project:** DigitalBusiness.DependencyInjectionExtensions (doc prefix: `DI`)
**Topic:** ServiceBuildExtensions
**Document type:** WorkPackage_Outcome (skeleton — fill in during/after dev work)
**Reports on:** `DI_ServiceBuildExtensions_WorkPackage_2026-07-28-1_v1.md`
**Date:** 2026-07-28
**Status:** Unprocessed

---

## 1. Summary

*(One paragraph: what was actually implemented.)*

## 2. Task-by-task deviations

Reference each task from the Work Package §3 (a/b/c) and Implementation §3–4. For each: state
whether it matched the plan, or deviated — and how/why.

- §3a Core types (`IPreBuildAction`, `ICleanupAction`, `BuildPipelineConfig`,
  `AddBuildPipeline`, `AddPreBuildAction`/`AddCleanupAction`, `BuildPipelineFactory`):
- §3b Default cleanup action (`RemoveOtherServiceConfigsCleanupAction`):
- §3c Unified install (`UseServiceBuildExtensions`, `HostApplicationBuilder` equivalent):
- §3d WR-28 documentation confirmation (composability limitation real/unavoidable):
- Implementation §3 note — `DefaultServiceProviderFactory`/`TContainerBuilder` generic-default
  resolution (flagged as an open compile-shape question in Implementation §3):
- Implementation §3 note — `RemoveConfig<T>` vs. direct `services.Remove(descriptor)`:

## 3. Design or approach changes discovered

*(Anything implementation surfaced that Design/Decisions didn't anticipate — the core value
of this document.)*

## 4. New open questions or follow-up work

*(Candidates for `OpenQuestions` or `DI_WorkRegister_v12.md`. Note explicitly whether this
package's completion changes anything about WR-30's readiness to proceed.)*

## 5. Verification status

- Build: 
- Test: 
- Definition of Done criteria (Work Package §5) met? 
- Any effect from the known `JsonDataWrappers.SetDeep` pre-existing issue (`task_4a72522a`)?

## 6. Author's proposed document updates

*(Best-guess checklist — seeds reconciliation, not binding.)*

- [ ] `DI_WorkRegister_v12.md` → v13: mark WR-33, WR-34, WR-35, WR-28 `done`; update §10's
      suggested build order note now that `ServiceBuildExtensions` has shipped
- [ ] `DI_ServiceBuildExtensions_Design_v2.md` → v3 (only if implementation deviated from
      the documented shape — e.g. the `DefaultServiceProviderFactory` generic-default
      resolution)
- [ ] `DI_ServiceBuildExtensions_Decisions_v2.md` → v3 (only if a genuine new design question
      was answered during implementation, per the same pattern as `ServiceConfig`'s
      Decisions 6–7)
- [ ] `DI_Index_v17.md` → v18: reflect new document versions, archive this package's family
- [ ] `DI_Guide_v1.md`: `ServiceBuildExtensions` section (WR-38) now eligible for real
      content — still a separate, unpackaged item, not part of this reconciliation unless
      explicitly folded in
