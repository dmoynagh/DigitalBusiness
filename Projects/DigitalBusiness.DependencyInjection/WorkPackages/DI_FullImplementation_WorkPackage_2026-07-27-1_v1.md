# DI_FullImplementation_WorkPackage_2026-07-27-1_v1

**Project:** DigitalBusiness.DependencyInjectionExtensions (doc prefix: `DI`)
**Topic:** FullImplementation — first-time code implementation of all eight resolved
Topics (`ServiceConfig`, `ServiceBuildExtensions`, `KeyedServices`, `Forwarded`, `Lazy`,
`Factory`, `Owned`, plus the `Overview`-level cross-cutting conventions)
**Document type:** WorkPackage (opened once scope is confirmed → edited in place as tasks
complete → archived on completion, per `DocumentationMethodology_v9.md` §6)
**Status:** Open
**Date-N:** 2026-07-27-1

---

## 0. Bundle contents

Per §7b of the methodology, this is the complete, exhaustive list of what travels into the
dev project for this package:

1. This document (`..._WorkPackage_2026-07-27-1_v1.md`).
2. `DI_FullImplementation_WorkPackage_Implementation_2026-07-27-1_v1.md` — the technical how:
   class shapes, signatures, sequencing, edge cases, extracted Design context.
3. `DI_FullImplementation_WorkPackage_Outcome_2026-07-27-1_v1.md` — ready-to-fill skeleton.
4. **Full Design copies for all eight Topics, attached in full** (not just extracted into
   Implementation), because this package's scope is "implement everything" — extraction
   would either be lossy or would end up reproducing nearly the entire corpus anyway (per
   `DocumentationMethodology_v9.md` §7a's own stated threshold for when a full copy is
   warranted):
   - `DI_Overview_v2.md`
   - `DI_ServiceConfig_Design_v1.md` + `DI_ServiceConfig_Decisions_v1.md`
   - `DI_ServiceBuildExtensions_Design_v2.md` + `DI_ServiceBuildExtensions_Decisions_v2.md`
   - `DI_KeyedServices_Design_v7.md` + `DI_KeyedServices_Decisions_v7.md`
   - `DI_Forwarded_Design_v3.md` + `DI_Forwarded_Decisions_v3.md`
   - `DI_Lazy_Design_v2.md` + `DI_Lazy_Decisions_v2.md`
   - `DI_Factory_Design_v2.md` + `DI_Factory_Decisions_v2.md`
   - `DI_Owned_Design_v4.md` + `DI_Owned_Decisions_v4.md`
   - `DI_Guide_v1.md` (existing Lazy section; other sections are this package's own
     documentation tasks — see §3)
   - `DI_WorkRegister_v11.md` (source of every WR-ID referenced below)

**Not attached:** `DI_Index_v16.md` (pure bookkeeping, no code-relevant content) and
`DocumentationMethodology_v9.md`/`v7.md` (process documents; the dev-side
`documentation-methodology` skill already restates the dev-facing rules).

---

## 1. Scope

Implement, in one pass, the code for **every currently-resolved design decision** across
all eight Topics. This is deliberately the whole corpus at once rather than the usual
per-Topic package, per explicit direction for this package. Every item below is drawn
directly from `DI_WorkRegister_v11.md`, which is the authoritative "what's owed" list — this
Work Package does not invent scope beyond what that register already states.

**Explicitly out of scope** (do not implement, even if related code is touched in passing):

- WR-14 (`Lazy` failure-timing) and WR-28 (`ServiceBuildExtensions` composability limit) —
  both documentation-only per the register; no code owed.
- Any eager-validation mode for `Lazy<T>` (Design §7's "candidate future addition, not
  implemented now").
- Bounded/LRU caching for the per-(type,key) cache (Design §3's documented assumption/future
  addition).
- Scope pooling for `Owned<T>` (Design §3's documented accepted cost).
- Recursive nested-special-type key threading (`Owned<Lazy<T>>` etc. threading the ambient
  key to the innermost resolution) — explicitly rejected scope, Decision 24/Owned Decision
  "future work" note. Only the diagnostic (WR-42) is in scope, not the mechanism itself.
- A Roslyn analyzer for `[FromKeyedServices]` misuse (Design §12 — "possible future
  addition," not this package).
- `.AsFunc()` extension for `Factory<T>` (Factory Design §2 — explicitly deferred, no current
  need).

If, while working a task, you find yourself tempted to build any of the above "while you're
in there" — don't. Record it as a follow-up in the Outcome (§5) instead, per rule 2 of the
dev-side methodology.

---

## 2. Code tasks

Grouped by Topic, each row carrying its Work Register ID so the Outcome can report back
against the same identifiers the design side already tracks. **Build order across groups
matters — see §4 (Sequencing) before starting; do not work strictly top-to-bottom through
this table without reading §4 first.**

### 2.1 ServiceConfig (foundational — build first)

- [ ] **WR-31** — `ServiceConfig<T>` type + `GetOrAddConfig<T>`/`GetConfig<T>`/`HasConfig<T>`
      extension methods, registering/reading via `ImplementationInstance`.

### 2.2 ServiceBuildExtensions (build second — depends on WR-31)

- [ ] **WR-33** — `IPreBuildAction`/`ICleanupAction` interfaces + `BuildPipelineConfig` +
      `BuildPipelineFactory<TContainerBuilder>` decorator.
- [ ] **WR-34** — `UseServiceBuildExtensions()` unified host-level install
      (`IHostBuilder` and `HostApplicationBuilder` variants).
- [ ] **WR-35** — Default generic cleanup action (removes every remaining
      `ServiceConfig<>` closed-generic descriptor), registered by `AddBuildPipeline()`.

### 2.3 KeyedServices (core cascading mechanism — independent of 2.1/2.2 except WR-40/WR-42)

- [ ] **WR-1** — Cascading activator + parallel registration extensions
      (`AddScoped`/`AddTransient`/etc. wrappers).
- [ ] **WR-2** — `CompoundKey` record + shared combining helper (resolution side).
- [ ] **WR-3** — Registration-side compound-key extensions
      (`AddKeyedScoped(outerKey, innerKey, ...)` etc.).
- [ ] **WR-4** — `[FromKeyedServices]` reinterpretation in the cascading activator.
- [ ] **WR-6** — `IConstructorFactoryStrategy` interface + reflection default +
      expression-tree strategy.
- [ ] **WR-7** — `Lazy<T>`/`Factory<T>`/`KeyedFactory<T>`/`Owned<T>` special-casing in the
      cascading activator (**one implementation unit with WR-20 and WR-24** — do these three
      together, in one pass over the activator, not as three separate changes).
- [ ] **WR-8** — `IEnumerable<T>` cascading semantics (keyed-set-if-non-empty-else-unkeyed).
      **Includes a required investigation task** (see §4a) before writing any code.
- [ ] **WR-9** — `ValidateKeyedCascading()` composition-time validation extension.
- [ ] **WR-42** — Extend `ValidateKeyedCascading()` to flag nested special-cased-type
      parameters (e.g. `Owned<Lazy<T>>`). Depends on WR-9.
- [ ] **WR-40** — `ValidateKeyedCascading()` optional self-registration as an
      `IPreBuildAction` when `HasConfig<BuildPipelineConfig>()`. Depends on WR-9, WR-31,
      WR-33.

### 2.4 Lazy (independent — no dependency on 2.1/2.2)

- [ ] **WR-11** — `AddLazyResolution()` — open-generic `Lazy<T>` subclass registration,
      thread-safety mode option, repeat-call conflict handling.
- [ ] **WR-12** — Transient wrapper capturing resolving-scope provider.
- [ ] **WR-13** — `AddLazyResolution<TService>(key)` — keyed type-to-type mapping for
      explicit-key lazy resolution.

### 2.5 Factory (independent — no dependency on 2.1/2.2; WR-20 shares a unit with 2.3)

- [ ] **WR-15** — `Factory<T>` open-generic registration + `GetService()`/
      `GetRequiredService()`.
- [ ] **WR-16** — Captured-scope provider (mirrors `Lazy`'s Transient pattern).
- [ ] **WR-19** — `KeyedFactory<T>` — open-generic registration, call-time key argument,
      ambient-vs-plain adaptive behaviour.
- [ ] **WR-20** — `KeyedFactory<T>` special-casing in the cascading activator — **build
      together with WR-7/WR-24, see 2.3**.

### 2.6 Owned (independent — no dependency on 2.1/2.2; WR-24 shares a unit with 2.3)

- [ ] **WR-22** — `Owned<T>` open-generic registration + `CreateAsyncScope()`-based child
      scope + eager resolution of `T`.
- [ ] **WR-23** — `IDisposable` implementation disposing the child scope.
- [ ] **WR-41** — `IAsyncDisposable` implementation — **build together with WR-22/23, same
      `AsyncServiceScope` field, not independently schedulable.**
- [ ] **WR-43** — Constructor exception safety (dispose the child scope if resolving `T`
      throws) — **build together with WR-22, same constructor.**
- [ ] **WR-24** — `Owned<T>` special-casing in the cascading activator — **build together
      with WR-7/WR-20, see 2.3**.

### 2.7 Forwarded (depends on ServiceConfig; Enhanced mode depends on ServiceBuildExtensions)

- [ ] **WR-30** — `ForwardedServicesConfig` via `GetOrAddConfig<T>`. **Build this before
      WR-5, WR-10, WR-25, WR-26.** Depends on WR-31.
- [ ] **WR-5** — `AddKeyedForward(forwardKey, targetKey[, lifetime])` — Base mode.
- [ ] **WR-10** — Forwarding cycle detection (linked-list walk over
      `ForwardedServicesConfig`). Depends on WR-30.
- [ ] **WR-25** — Enhanced mode support in `AddKeyedForward` (detect
      `HasConfig<BuildPipelineConfig>()`). Depends on WR-33/34, WR-31.
- [ ] **WR-26** — Enhanced mode's `IPreBuildAction` (final lifetime resolution/correction).
      Depends on WR-33, WR-30.

---

## 3. Documentation tasks

`DI_Guide_v1.md` is a project-wide, sectioned Guide (§9 of the Work Register). These sections
are new content owed for Topics that are fully resolved but not yet documented for a
consuming audience. Write each against the *actual shipped API* once its code task above is
done, not against the Design doc's illustrative snippets alone — if implementation diverged
from Design in any way, the Guide should reflect what was actually built (and the divergence
belongs in the Outcome, §7).

- [ ] **WR-21** — `KeyedServices` section of `DI_Guide_v1.md`.
- [ ] **WR-36** — `Factory` section of `DI_Guide_v1.md`.
- [ ] **WR-37** — `Forwarded` section of `DI_Guide_v1.md`.
- [ ] **WR-38** — `ServiceBuildExtensions` section of `DI_Guide_v1.md`.
- [ ] **WR-39** — `ServiceConfig` section of `DI_Guide_v1.md`.
- [ ] **WR-45** — `Owned` section of `DI_Guide_v1.md` (replacing the existing interim
      placeholder in `DI_Guide_v1.md` §3).

Every public type/member also gets a real `///` XML doc summary as part of writing it — this
is a standing dev-side convention (`documentation-methodology` skill), not a separate
checklist item here.

---

## 4. Sequencing

This package deliberately spans every Topic, so build order is not "top to bottom through
§2" — follow this instead, per `DI_WorkRegister_v11.md` §10:

1. **ServiceConfig (§2.1)** first — everything else that touches config depends on it.
2. **ServiceBuildExtensions (§2.2)** second — depends on ServiceConfig.
3. **`Forwarded`'s `ForwardedServicesConfig` (WR-30)** — depends on ServiceConfig; must land
   before the rest of `Forwarded` (§2.7).
4. **`Forwarded`'s own behaviour (WR-5, WR-10, WR-25, WR-26)** — depends on WR-30, and
   (for Enhanced mode specifically) ServiceBuildExtensions.
5. **`KeyedServices`, `Lazy`, `Factory`, `Owned`** (§2.3–2.6) have **no dependency on the
   ServiceConfig/ServiceBuildExtensions/Forwarded chain** and can proceed independently, at
   any point, in parallel with steps 1–4 if the dev environment supports parallel work.
6. Within `KeyedServices` (§2.3): **WR-7, WR-20, WR-24 are one implementation unit** (the
   activator's special-casing for all four deferred/on-demand types) — do this as a single
   pass over the activator once `Lazy<T>` (§2.4), `Factory<T>`/`KeyedFactory<T>` (§2.5), and
   `Owned<T>` (§2.6) themselves exist, since the activator special-cases all four types by
   reference to their actual shapes. **Sequence-within-sequence:** the base `Lazy<T>`,
   `Factory<T>`/`KeyedFactory<T>`, and `Owned<T>` types (WR-11/12/13, WR-15/16/19,
   WR-22/23/41/43) must each exist before WR-7/20/24 can special-case them — but those three
   Topics' base types have no dependency on each other and can be built in any order or in
   parallel.
7. WR-9 (`ValidateKeyedCascading()`) before WR-42 (its nested-type extension) and before
   WR-40 (its optional pipeline self-registration, which also needs WR-31/33).
8. Guide sections (§3) are written last, per-Topic, once that Topic's own code tasks are
   actually done — a Guide describing not-yet-shipped behaviour would immediately be wrong.

### 4a. Required investigation before WR-8

Per `DI_KeyedServices_Design_v7.md` §10 and §12's research context: before implementing
`IEnumerable<T>` cascading semantics, verify whether `dotnet/runtime#64995`/`#65145` (the
documented open/closed-generic `GetServices<T>()` instability) actually affects this Topic's
specific usage pattern. The `Lazy` Topic investigated and dismissed this issue for its own
(single-resolution, `GetService<T>()`) purposes — **that dismissal does not transfer to
`GetServices<T>()`**, and must be independently checked. Record the investigation's outcome
in the Outcome document (§6, Verification status) regardless of which way it comes out — if
the instability *does* apply, stop this task and record it as a blocker (per dev-side rule 3)
rather than working around it silently.

---

## 5. Definition of Done

- [ ] Full solution build succeeds.
- [ ] Full test suite passes.
- [ ] Every checklist item in §2 and §3 is ticked, or explicitly deferred in the Outcome with
      a stated reason.
- [ ] Every item in §1's "explicitly out of scope" list remains unimplemented (verify by
      diff/review, not just by memory of not having written it).
- [ ] WR-8's investigation task (§4a) has a recorded outcome, whichever way it went.
- [ ] Every public type/member introduced carries a real `///` XML doc summary.
- [ ] Tests exist for each code task, written alongside that task (not as a trailing pass at
      the end).
- [ ] `DI_Guide_v1.md`'s repo copy is refreshed with all six new sections (§3) plus removal
      of the `Owned` interim placeholder once WR-45 lands.
- [ ] The Outcome document (`..._Outcome_2026-07-27-1_v1.md`) is fully filled in — every
      section, not just a subset.
- [ ] Verification commands (Implementation doc, final section) have actually been run, not
      assumed.

---

## 6. Notes/Issues

*(Populated during dev-side work, per rule 3/4 of the dev-side methodology — blockers and
discoveries go here as they happen, mirrored into the Outcome.)*

---

## 7. Producing the Outcome document

A ready-to-fill skeleton, `DI_FullImplementation_WorkPackage_Outcome_2026-07-27-1_v1.md`, is
included in this bundle (§0). Fill it in as work proceeds, not only at the end — every
deviation, discovery, and new question goes in at the moment it happens. If only this
WorkPackage document survives into the dev project and the skeleton is somehow lost, produce
a new file with that exact name, `Status: Unprocessed`, and the seven sections listed in
`documentation-methodology`'s skill file (Header, Summary, Task-by-task deviations, Design or
approach changes discovered, New open questions or follow-up work, Verification status,
Author's proposed document updates).
