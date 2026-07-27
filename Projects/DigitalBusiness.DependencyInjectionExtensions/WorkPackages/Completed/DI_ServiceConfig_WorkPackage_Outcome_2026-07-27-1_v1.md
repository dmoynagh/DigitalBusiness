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

**Addendum (external review follow-up):** an independent review of the branch diff (GPT-5.3
Codex, run via Visual Studio's git chat) surfaced a real gap — `GetOrAddConfig<T>` didn't guard
against `factory` returning `null` — which has now been fixed, plus 5 new tests covering it and
the existing `ArgumentNullException` guards, which had no direct test coverage before. See §3/§4
for the two behaviors the review also raised that were deliberately *not* changed (they match
the Implementation doc's own given code) and are recorded as open design questions instead.

**Addendum 2 (test coverage for the two documented-only behaviors):** on request, added 2 more
tests that pin down (without changing) the two behaviors §4 records as open questions —
`MultipleRegistrations_GetOrAddConfig_ReturnsFirstRegisteredDescriptor` and
`HasConfig_TrueButGetConfig_Null_WhenRegisteredWithoutAnInstance`. 15/15 tests now pass.

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
- **Factory-returns-null gap (found by external review, fixed).** `GetOrAddConfig<T>` called
  `factory()` and wrapped the result directly into `new ServiceConfig<T>(value)` with no null
  check. `T : class` expresses non-null intent but nothing enforced it at runtime — a factory
  returning `null` would have silently produced a `ServiceConfig<T>` whose `Value` is `null`.
  Fixed with `ArgumentNullException.ThrowIfNull(value, nameof(factory))` immediately after the
  factory call, matching the existing guard style in the same method. New test:
  `GetOrAddConfig_FactoryReturnsNull_Throws`.
- **Two behaviors the external review flagged were deliberately left unchanged** — both match
  the Implementation doc's own given code, so changing them would be a design decision, not a
  bug fix. Recorded as open questions in §4 rather than resolved silently.
- **One external-review finding was checked and is incorrect.** It flagged `folder-structure.txt`
  as scope creep against this package. `git log main..HEAD -- folder-structure.txt` shows it was
  touched by the pre-existing `1a9cd5a "wip"` commit already on this branch before this package's
  work started, not by anything in `0337962` or this addendum. No action taken; noted here so the
  record is accurate if this Outcome is read alongside that review.

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
- **Duplicate-registration precedence (candidate for `DI_WorkRegister_v11.md`/Design doc):**
  `GetOrAddConfig`'s linear scan returns the *first* matching `ServiceConfig<T>` descriptor
  (registration order) if more than one somehow exists. That's the opposite of the repo's stated
  "last registration wins" override convention (CLAUDE.md). Arguably correct here — the point of
  get-or-create is one canonical instance, not an overridable registration — but it's non-obvious
  and this package didn't get an explicit design answer on whether "first wins" is the intended,
  permanent contract or coincidental. Left unchanged (matches Implementation doc's given code);
  surfaced via external review. Now covered by
  `MultipleRegistrations_GetOrAddConfig_ReturnsFirstRegisteredDescriptor`, which pins down
  current behavior so a future refactor can't silently change it without a failing test.
- **`HasConfig<T>`/`GetConfig<T>` shape mismatch on non-instance registrations (candidate for
  Design doc or Guide, once written):** if a consumer registers `ServiceConfig<T>` directly via
  ordinary DI (bypassing `GetOrAddConfig` — e.g. a type-based or factory-based registration),
  `HasConfig<T>` (checks `ServiceType` only) can return `true` while `GetConfig<T>` (requires
  `ImplementationInstance is ServiceConfig<T>`) returns `null`. Both methods match the
  Implementation doc's given shape exactly. Worth a documented caveat in the eventual
  `ServiceConfig` Guide section (WR-39) that `ServiceConfig<T>` is meant to be registered only
  via `GetOrAddConfig`, not by hand. Now covered by
  `HasConfig_TrueButGetConfig_Null_WhenRegisteredWithoutAnInstance`, which pins down current
  behavior so a future refactor can't silently change it without a failing test.

---

## 5. Verification status

- [x] Full solution build: **Passed**, re-verified after the addendum.
      `dotnet build Solutions\DigitalBusiness.slnx` — 0 errors, 226 warnings, all pre-existing
      `CS1591` missing-doc-comment warnings in `DigitalBusiness.JsonDataWrappers` (unrelated to
      this package; that project already had `GenerateDocumentationFile` enabled before this WP).
- [~] Full test suite: **Partial / blocked by a pre-existing, unrelated bug.**
      `DigitalBusiness.DependencyInjectionExtensions.Tests` (this package's tests): **15/15
      passed**, re-verified after both addenda (8 original + 5 for null-guard/factory-null
      coverage + 2 pinning down the documented precedence/shape-mismatch behaviors).
      `DigitalBusiness.Extensibility.Tests`: 30/30 passed. A solution-wide
      `dotnet test Solutions\DigitalBusiness.slnx` aborts with
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
