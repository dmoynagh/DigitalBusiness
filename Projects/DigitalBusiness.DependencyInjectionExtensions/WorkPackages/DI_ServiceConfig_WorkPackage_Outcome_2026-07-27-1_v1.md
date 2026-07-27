# DI_ServiceConfig_WorkPackage_Outcome_2026-07-27-1_v1

**Project:** DigitalBusiness.DependencyInjectionExtensions (doc prefix: `DI`)
**Reports on:** `DI_ServiceConfig_WorkPackage_2026-07-27-1_v1.md`
**Date:** 2026-07-27
**Status:** Unprocessed

> Fill this in as work proceeds, not only at the end.

---

## 1. Summary

Implemented `ServiceConfig<T>` and its `IServiceCollection` extension surface
(`GetOrAddConfig<T>`, `GetConfig<T>`, `HasConfig<T>`) exactly as specified in WR-31a–d, in a new
`ServiceConfig\` subfolder under `Projects\DigitalBusiness.DependencyInjectionExtensions\`
(namespace `DigitalBusiness.DependencyInjectionExtensions.ServiceConfig`). Since this project was
completely empty before this package (no `.cs` files, no package references, no test-project
wiring), the work also included the csproj plumbing both the source and test projects needed to
even compile/run: `Microsoft.Extensions.DependencyInjection.Abstractions` on the source project,
`Microsoft.Extensions.DependencyInjection` + a `ProjectReference` on the test project, and removal
of the test project's default scaffold. All logic matches Implementation doc §2/§3 (linear scan,
factory invoked only once, no locking, no removal helper); the only deliberate deviation from the
doc's literal code is using the repo's current `extension(...)` block syntax instead of classic
`this IServiceCollection` parameters, which the doc's own caveat permits. Full solution build is
clean (0 errors); this package's own 8 tests pass in isolation and inside the full run. A
solution-wide `dotnet test` aborts due to a pre-existing, unrelated stack-overflow bug in
`DigitalBusiness.JsonDataWrappers` — flagged separately, not fixed here (out of scope).

---

## 2. Task-by-task deviations

| WR-ID | Matched plan? | Deviation (if any) |
|---|---|---|
| WR-31a (`ServiceConfig<T>` type) | Matched plan | — |
| WR-31b (`GetOrAddConfig<T>`) | Matched plan (logic) | Written using `extension(IServiceCollection services)` block syntax rather than the doc's literal `this IServiceCollection services` parameter style, to match the repo's dominant convention (see §3). |
| WR-31c (`GetConfig<T>`) | Matched plan (logic) | Same syntax deviation as WR-31b. |
| WR-31d (`HasConfig<T>`) | Matched plan (logic) | Same syntax deviation as WR-31b. |

---

## 3. Design or approach changes discovered

- **Extension-method syntax.** The Implementation doc's §2 code shape uses classic `public
  static T Method<T>(this IServiceCollection services, ...)` extension methods. The repo has
  since moved to the newer C# `extension(IServiceCollection services) { ... }` block syntax:
  18 files across `DigitalBusiness.Extensibility` and `DigitalBusiness.JsonDataWrappers` use it
  (including `HandlerStartupExtensions.cs`, the closest existing analog — it also extends
  `IServiceCollection`), versus only 3 files repo-wide (none DI-related) still using the classic
  style. Used the block syntax here. The doc's own text ("shapes above are the contract to hit,
  not necessarily the exact file layout") anticipates exactly this kind of adjustment, so this
  isn't flagged as a blocker — but future Implementation docs for this project could save a
  step by writing shapes in the block-syntax style directly.
- **Folder/namespace precedent set, not found.** This project had zero source files before this
  package, so there was no internal convention to follow. Created a `ServiceConfig\` subfolder
  (namespace `DigitalBusiness.DependencyInjectionExtensions.ServiceConfig`), matching how
  `Extensibility`/`JsonDataWrappers` use one subfolder per feature area. Since CLAUDE.md lists
  several more Topics coming to this same project (`Lazy`, `Factory`/`KeyedFactory`, `Owned`,
  `KeyedServices`, `Forwarded`, `ServiceBuildExtensions`), this choice is effectively setting
  precedent for all of them — worth the design side's explicit sign-off rather than treating it
  as settled by this package alone.
- **csproj/test-project setup was entirely missing, not just "adjust to match conventions."**
  The Implementation doc's phrasing ("adjust namespace/file placement...") reads as if only
  minor placement details were open. In fact neither the source project's csproj nor the test
  project's csproj had *any* of the wiring needed to compile or run this package's code: no
  `Microsoft.Extensions.DependencyInjection*` package references anywhere in the project, and
  critically, the test project (`DigitalBusiness.DependencyInjectionExtensions.Tests.csproj`)
  had **no `ProjectReference` to the source project at all** — it could never have exercised
  this code as originally set up. Worth noting for future packages in this project: don't
  assume the test-project scaffold is wired correctly just because it exists.
- **CLAUDE.md's repo-wide doc-file claim doesn't match repo reality.** CLAUDE.md states
  `<GenerateDocumentationFile>` is enabled repo-wide. In fact only `DigitalBusiness.
  JsonDataWrappers.csproj` had it set; `DigitalBusiness`, `DigitalBusiness.Extensibility`,
  `DigitalBusiness.Json`, and this project's own csproj (before this package) did not. Added it
  to this project's csproj to align with CLAUDE.md's explicit instruction — but the other three
  projects remain out of alignment with what CLAUDE.md claims, which the design/repo-maintenance
  side may want to reconcile (either enable it everywhere, or correct CLAUDE.md's wording).
- **No contradiction found in the Design context (Implementation doc §1) itself.** Its rationale
  (`ImplementationInstance` over factory-delegate, single `T` over `T, TKey`, wrapper-type over
  bare `T`) doesn't conflict with anything in the repo — confirmed via a repo-wide grep for
  `ServiceConfig`, which returned no prior hits.

---

## 4. New open questions or follow-up work

- **Removal/cleanup:** nothing about implementing `GetOrAddConfig`/`GetConfig`/`HasConfig`
  surfaced any need for a `RemoveConfig<T>` (or similar) on `ServiceConfig` itself — the linear
  `services.Remove(descriptor)` pattern would work unaided whenever `ServiceBuildExtensions`
  needs it. No changes owed to this package from that direction.
- **Out-of-scope bug found, not fixed:** `DigitalBusiness.JsonDataWrappers`'s
  `JsonDataTypedPathExtensions.SetDeep` has unbounded recursion that crashes the test host with
  a stack overflow — reproduces running `DigitalBusiness.JsonDataWrappers.Tests` alone,
  repeatedly, unrelated to this package. Flagged as a separate task
  (`task_4a72522a`, "Fix stack overflow in JsonDataTypedPathExtensions.SetDeep") rather than
  fixed here, per scope rules. This currently blocks a clean solution-wide `dotnet test` run for
  *any* work in this repo, not just this package — worth prioritizing.
- **CLAUDE.md vs. actual csproj state on `GenerateDocumentationFile`** — see §3. Candidate for
  either a CLAUDE.md wording fix or a follow-up task to enable it on the remaining projects.
- **Extension-method syntax** in future Implementation docs for this project could be written
  directly in the `extension(...)` block style to match the repo's actual convention and save
  this deviation note each time — candidate for a documentation-methodology or template tweak,
  not a code change.

---

## 5. Verification status

- [x] Full solution build: **Passed.** `dotnet build Solutions\DigitalBusiness.slnx` — 0
      errors, 226 warnings, all pre-existing `CS1591` missing-doc-comment warnings in
      `DigitalBusiness.JsonDataWrappers` (unrelated to this package; that project already had
      `GenerateDocumentationFile` enabled before this WP).
- [~] Full test suite: **Partial / blocked by a pre-existing, unrelated bug.**
      `DigitalBusiness.DependencyInjectionExtensions.Tests` (this package's tests): 8/8 passed,
      both run alone and inside the full solution run. `DigitalBusiness.Extensibility.Tests`:
      30/30 passed. A solution-wide `dotnet test Solutions\DigitalBusiness.slnx` aborts with
      "Test host process crashed: Stack overflow" originating in `DigitalBusiness.
      JsonDataWrappers`'s `JsonDataTypedPathExtensions.SetDeep`; reproduced 3/3 times running
      `DigitalBusiness.JsonDataWrappers.Tests` in isolation, with a different number of tests
      completing before the crash each run (56, 184, and one run with 0 completing) — a real
      recursion bug, not test flakiness. `git status`/`git diff` confirm this package's changes
      never touch `JsonDataWrappers`. Flagged as a separate task rather than fixed here.
- [x] Definition of Done (WorkPackage §4): all items ticked in the repo's copy of the
      WorkPackage except "Full test suite passes," which carries the same caveat as above.

---

## 6. Author's proposed document updates

- `DI_ServiceConfig_WorkPackage_Implementation` (or a project-level template/README): consider
  rewriting the §2 code-shape example in the repo's current `extension(...)` block syntax so
  future packages don't need to independently rediscover and re-flag this convention shift.
- Whatever design-side document tracks this project's folder/namespace layout (if any):
  record `ServiceConfig\` as the first Topic subfolder precedent, so later Topics
  (`Lazy`, `Factory`/`KeyedFactory`, `Owned`, `KeyedServices`, `Forwarded`,
  `ServiceBuildExtensions`) follow the same one-subfolder-per-Topic shape rather than each
  re-deciding it.
- CLAUDE.md's "Coding conventions (repo-wide)" section claims `GenerateDocumentationFile` is
  enabled repo-wide; only 2 of 5 `Projects\*` csproj files (`JsonDataWrappers`, now also
  `DependencyInjectionExtensions`) actually have it. Either fix the wording to state it's
  per-project, or file a follow-up to enable it on `DigitalBusiness`, `DigitalBusiness.
  Extensibility`, and `DigitalBusiness.Json`.
- Not a document update, but worth registering somewhere the design side tracks repo health:
  the `JsonDataTypedPathExtensions.SetDeep` stack-overflow bug (see §4) blocks a clean
  solution-wide test run for everyone, independent of any particular Topic's package.
