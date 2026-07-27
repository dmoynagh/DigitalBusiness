# DI_ServiceConfig_WorkPackage_2026-07-27-1_v1

**Project:** DigitalBusiness.DependencyInjectionExtensions (doc prefix: `DI`)
**Topic:** ServiceConfig — a general mechanism for attaching typed, mutable configuration
and shared state to the `IServiceCollection` itself
**Document type:** WorkPackage (opened once scope is confirmed → edited in place as tasks
complete → archived on completion, per `DocumentationMethodology_v9.md` §6)
**Status:** Open
**Date-N:** 2026-07-27-1

---

## 0. Bundle contents

Per §7b of the methodology, this is the complete list of what travels into the dev project
for this package:

1. This document.
2. `DI_ServiceConfig_WorkPackage_Implementation_2026-07-27-1_v1.md` — the technical how,
   self-contained (extracts the Design context it needs rather than pointing at documents
   the dev side may not have).
3. `DI_ServiceConfig_WorkPackage_Outcome_2026-07-27-1_v1.md` — ready-to-fill skeleton.

**No full Design copy attached.** `DI_ServiceConfig_Design_v1.md` is short and this
package's scope is exactly that one Topic — the Implementation document's extracted "Design
context" section is not lossy here, unlike the earlier discarded all-topics draft of this
package. If, while working, the Implementation doc's extraction turns out to be missing
something the design doc covers, treat that as a blocker (methodology rule 3) rather than
guessing.

---

## 1. Scope

Implement `ServiceConfig<T>` and its access surface — the entirety of what
`DI_WorkRegister_v11.md` records as owed for this Topic (WR-31; no other WR-ID belongs to
`ServiceConfig`).

**In scope:**
- `ServiceConfig<T>` wrapper type.
- `GetOrAddConfig<T>`, `GetConfig<T>`, `HasConfig<T>` extension methods on
  `IServiceCollection`.

**Explicitly out of scope for this package:**
- Any removal/cleanup mechanism for `ServiceConfig<T>` descriptors. That belongs to
  `ServiceBuildExtensions` (its cleanup-action phase, and its hardcoded removal of
  `BuildPipelineConfig` specifically) — a later package, not this one. Do not add a
  `RemoveConfig<T>` helper here just because it seems like an obvious symmetric addition;
  if it turns out `ServiceBuildExtensions` genuinely needs one, that package can add it
  against the actual consuming code, not speculatively here.
- The read/write-pair pattern (`ReadOnlyDictionary<TKey,TValue>` wrapping a mutable backing
  config) described in Design §5 — that's a *usage pattern* each future feature applies to
  its own config type (e.g. `Forwarded`'s `ForwardedServicesConfig`/`...View`), not a
  separate mechanism this package needs to build. Nothing new is owed here beyond
  `ServiceConfig<T>` itself supporting it, which it already does by construction (any `T`
  can be a `ReadOnlyDictionary` subclass).
- Any concrete feature-specific config type (`ForwardedServicesConfig`,
  `BuildPipelineConfig`) — those belong to their own Topics' packages.

---

## 2. Code tasks

- [x] **WR-31a** — `ServiceConfig<T>` sealed wrapper class (`Value` property,
      constructor taking `T`).
- [x] **WR-31b** — `GetOrAddConfig<T>(this IServiceCollection, Func<T> factory)` — get-or-
      create semantics, registering via `ImplementationInstance`.
- [x] **WR-31c** — `GetConfig<T>(this IServiceCollection)` — read-only peek, `null` if
      absent, creates nothing.
- [x] **WR-31d** — `HasConfig<T>(this IServiceCollection)` — presence-only check.

(Split into four sub-items purely for tracking granularity within this package; all four
are one cohesive unit of work and should be built and tested together, not sequenced.)

## 3. Documentation tasks

None in this package. `DI_Guide_v1.md`'s `ServiceConfig` section (WR-39) is deferred to
whenever the Guide-writing pass happens — per this project's own build-order convention,
Guide sections are written once a Topic's code is done and stable, and per the project-wide
Guide restructuring decision, WR-39 is tracked independently in the Work Register rather
than folded into every Topic's first code package by default. Flag in the Outcome if you
think it'd genuinely be cheap to write now instead.

---

## 4. Definition of Done

- [x] Full solution build succeeds.
- [x] Full test suite passes — with one caveat: see Notes/Issues below. This package's own
      test project (`DigitalBusiness.DependencyInjectionExtensions.Tests`, 8/8) passes both
      in isolation and inside the full solution run. A solution-wide `dotnet test` aborts
      with a stack overflow, but it is pre-existing, unrelated to this package, and
      reproduces in `DigitalBusiness.JsonDataWrappers.Tests` alone with zero files from this
      package on the call stack.
- [x] Tests cover: first-call creation via `GetOrAddConfig`, repeat-call returning the same
      instance without re-invoking the factory, `GetConfig` returning `null` when absent
      and the right value when present, `HasConfig` true/false correctly, and two distinct
      `T` types not colliding with each other.
- [x] Every public type/member carries a real `///` XML doc summary.
- [x] The Outcome document is fully filled in.
- [x] Verification commands (Implementation doc §4) have actually been run.

---

## 5. Notes/Issues

- Both `Projects\DigitalBusiness.DependencyInjectionExtensions.csproj` and `Tests\
  DigitalBusiness.DependencyInjectionExtensions.Tests.csproj` were missing wiring the
  Implementation doc assumed existed: no `Microsoft.Extensions.DependencyInjection*` package
  references anywhere, no `ProjectReference` from the test project to the source project, and
  the test project still had its default `UnitTest1.cs` scaffold. All added/removed as part of
  this package — see Outcome §2/§3.
- Implementation doc §2's code shape uses classic `this IServiceCollection services`
  extension-method syntax; the repo's actual dominant convention (18 files across
  `Extensibility`/`JsonDataWrappers`, including the closest analog `HandlerStartupExtensions.
  cs`) uses the newer `extension(IServiceCollection services) { ... }` block syntax instead.
  Used the block syntax — doc's own caveat permits adjusting file shape to match repo
  convention.
- **Full-suite blocker (pre-existing, out of scope):** `dotnet test Solutions\DigitalBusiness.
  slnx` aborts with "Test host process crashed: Stack overflow" from unbounded recursion in
  `JsonDataTypedPathExtensions.SetDeep` (`DigitalBusiness.JsonDataWrappers`). Reproduces
  running `DigitalBusiness.JsonDataWrappers.Tests` alone, repeatedly, with a different number
  of tests completing before the crash each time (56, 184, ...) — a real recursion bug, not a
  single flaky test. Confirmed via `git status`/`git diff` that nothing in this package's
  changes touches `JsonDataWrappers`. Flagged as a separate out-of-scope task rather than fixed
  here, per the WorkPackages README scope rule.

---

## 6. Producing the Outcome document

A ready-to-fill skeleton, `DI_ServiceConfig_WorkPackage_Outcome_2026-07-27-1_v1.md`, is
included in this bundle. Fill it in as work proceeds, not only at the end.
