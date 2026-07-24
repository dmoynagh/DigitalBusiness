# Work Package — JsonDataWrappers Finalization

> **Opened:** 2026-07-22 · **Status:** Open (§2a Complete; §2b, §3 pending) · **Source:**
> `JsonDataWrappers_Working_Archived_2026-07-22.md` v5 (retired 2026-07-22 — see §2a) — all six open items
> (Q1–Q6) resolved; finalized into `JsonDataWrappers_Design.md` and
> `JsonDataWrappers_Decisions.md`. See `CMS_Index.md` for the confirmation trail.
>
> Follows the conventions in `DocumentationMethodology.md`.

---

## 1. Scope

`JsonDataWrappers` is fully designed — the 2026-07-19 review's findings (F1–F10) and the
Working design's six decision points (Q1–Q6) are all resolved. This work package carries that
finished design through to: (a) the library's own permanent documentation record, in place of
the Working doc; (b) the handful of CMS-side documents that reference the library's merge
semantics and will be stale once v2 semantics exist; and (c) the actual modifications to the
existing `DigitalBusiness.JsonDataWrappers` source (46 files, ~4,700 lines) in Visual Studio.

The library keeps its own documentation track, separate from the `CMS_*` corpus (per the
`HandlerPipeline_Design` precedent) — CMS documents reference it, they don't absorb it.

## 1a. Bundle contents

What travels into the dev project together for §3's code tasks, per
`DocumentationMethodology.md` §7b:

- `JsonDataWrappers_WorkPackage_2026-07-22-1.md` (this document)
- `JsonDataWrappers_WorkPackage_Implementation_2026-07-22-1.md` (the *how*)
- `JsonDataWrappers_WorkPackage_Outcome_2026-07-22-1.md` (empty skeleton, to be filled in and
  returned)
- **`JsonDataWrappers_Design.md`** (current version, attached in full) — this package's code
  tasks span nearly the whole of the library's design, broad enough that the Implementation
  guide's extracted context alone would either be lossy or reproduce most of Design anyway;
  see the Implementation guide's own header for this reasoning.
- **Not bundled:** `JsonDataWrappers_Decisions.md`, `JsonDataWrappers_Archive_DesignDecisions.md`
  — reasoning stays in this project; the one piece of it that matters for implementation (the
  `DeepEquals`/`DeepSemanticEquals` split) is already extracted into the Implementation guide.

## 2. Documentation tasks

### 2a. Library's own documentation (separate track) — **Complete 2026-07-22**

- [x] **Create `JsonDataWrappers_Design_v1.md`** — the authoritative, finalized design.
      Merged `JsonDataWrappers_Working_v5.md` into clean form: all decision-in-progress
      markers resolved into plain statements, `[DECIDE]`/`[CONFIRMED]` framing removed,
      §§1–9 kept substantively as written (based on v5, which included the `DeepSemanticEquals`
      correction — not the v3 originally referenced below, since v4/v5 postdated this task's
      original drafting).
- [x] **Create `JsonDataWrappers_Decisions_v1.md`** — the library's own decisions record,
      mirroring `CMS_Decisions.md`'s format (requirement → alternatives considered,
      including rejected → decision → reasoning). Populated from:
      - Architecture options O1–O5 (§3 of the Working doc) — the dual-source wrapper
        selection and its rejected alternatives (POCO, raw BCL, Node-only, custom DOM).
      - Review findings F1–F10 and their resolutions.
      - Q1–Q6, each with its rejected alternatives, **including the Q3 supersession**
        (original `DeepEquals`-unification resolution → `DeepSemanticEquals` correction,
        recorded in place per convention, not rewritten).
- [x] **Create `JsonDataWrappers_Archive_DesignDecisions_v1.md`** — consolidated the
      discussion trail across the Working doc's v1→v5 evolution (now archived as
      `JsonDataWrappers_Working_Archived_2026-07-22.md`) and the review
      (now archived as `JsonDataWrappers_Review_2026-07-19-1_Archived_2026-07-22.md`): the
      full review findings writeup (including the requirement-fit table and strengths worth
      keeping deliberately), the architecture options analysis, and the v1→v5 evolution of
      the six decision points including the Q3 correction. Follows the CQRS/Query precedent.
- [x] **Archive** `JsonDataWrappers_Working.md` (v5) and
      `JsonDataWrappers_Review_2026-07-19-1.md` (v2) — content fully absorbed into
      Design/Decisions/Archive above. Per the archiving convention, both renamed with an
      `_Archived_2026-07-22` suffix, frozen at the version each was archived at:
      `JsonDataWrappers_Working_Archived_2026-07-22.md` (v5),
      `JsonDataWrappers_Review_2026-07-19-1_Archived_2026-07-22.md` (v2). Not deleted —
      renamed, per the no-delete policy; listed with their new names in `CMS_Index.md`.
- [x] **Create `JsonDataWrappers_Guide_v1.md`** — new document type, added to
      `DocumentationMethodology.md` this session. Audience-filtered "how to use this"
      document: what the library does, quick start, core concepts, task-oriented usage,
      limits/considerations, compatibility, a dedicated CMS-consumer-integration section
      (carrying forward Design §9's integration mapping — this is what lets the library's
      Design/Decisions/Archive move to their own namespace/project while the CMS still has
      what it needs), a public-surface-at-a-glance table, and a pointer to Design/Decisions
      for anyone who needs the reasoning. Excludes all decision narrative — that stays in
      Decisions/Archive.

### 2b. CMS-side cross-references

- [ ] **Decision required first (blocking — see §4):** does the CMS's branch/merge/ChangeLog
      machinery target semantics **v1** or **v2** by default once implementation begins? This
      is a genuine CMS-level design call, not yet made — the tasks below depend on its answer.
- [ ] **Update `CMS_Foundation_Design.md` §4a.5** (Merge Semantics table) — add a note
      that merge semantics are now versioned (v1/v2) at the library level, point to
      `JsonDataWrappers_Design.md` §6.1 for the full contract, and state which version the CMS
      targets by default per the decision above.
- [ ] **Update `CMS_Decisions.md`** — the branch merge semantics decision entry — add a
      cross-reference note that the null-vs-absent question raised there was resolved at the
      library level via semantics v2 (`"$$null"` marker), rather than duplicating the reasoning.
- [ ] **Update `CMS_ChangeLog_Design.md`** — the `changes` payload description should
      note it is produced by `JsonDataWrappers` `ToPatch(...)`, is versioned, and state which
      semantics version new ChangeLog entries record going forward (same decision as above —
      old entries remain valid under whatever version they were written under).
- [ ] **Update `CMS_Glossary.md`** — add/update entries for: merge semantics
      v1/v2, the `"$$null"` marker, and confirm the existing `JsonDataWrappers`-adjacent
      entries (e.g. `HandleChanges`) don't need adjustment.
- [ ] **Check `CMS_DependencySystem_Design.md`** — scan for any explicit reference to
      merge semantics v1 as sole/default; update only if found (none located in this session's
      search — verify before assuming clean).

## 3. Code tasks (`DigitalBusiness.JsonDataWrappers`, Visual Studio)

All items below modify the existing reviewed source (not a rewrite). Grouped by design
section for traceability back to `JsonDataWrappers_Design_v1.md`.

**Null model (§4.1, F1/Q1 — already confirmed 2026-07-21, not yet coded):**
- [ ] `Set(key, value)` always writes, including explicit JSON null; remove the nullable
      `Set` overloads where a null argument meant "remove."
- [ ] Removal only via `Remove(key)` / `Remove(path)` / `RemoveAt(index)`.
- [ ] Add `IsUnset` alongside existing `IsNull`, Element-backed-only distinction documented.

**Converter registration (§4.2, F2/Q2 — already confirmed 2026-07-21, not yet coded):**
- [ ] Explicit `JsonDataConverters.Register<T>(...)` + `Freeze()` model.
- [ ] `[JsonConverter(typeof(...))]` attribute fallback path, consulted only when no explicit
      registration exists; explicit registration always wins.

**Committed-state integrity rule (§4.3, F3):**
- [ ] No code change — add XML-doc note on Node-backed `AsReadOnly()` clarifying it's a guard,
      not a security boundary, and stating the Element-backed-for-anything-diffed-or-audited rule.

**Renames (§4.4, F6/F9/Q6 — confirmed this session):**
- [ ] `ToJsonElementJsonData()` → `ToElementBacked()`
- [ ] `ToJsonNodeJsonData(bool?)` → `ToNodeBacked(bool? readOnly = null)`
- [ ] `ToEditableJsonData()` → `ToEditable()`
- [ ] `EnsureArray<T>(int)` → `GetOrCreateArray<T>(int)`

**Array enumeration (§4.4, Q5 — confirmed this session):**
- [ ] `JsonDataArray<T>` enumerator: `TryGet` semantics, default-on-null/mismatch, enumerated
      length always matches source array length.
- [ ] Add `GetRequired(int index)` throwing form.

**Dead code (F8):**
- [ ] Delete `JsonDataPrimativeExtensions.cs` from the project (compile-excluded; source-level
      deletion is fine per the corpus's own no-delete policy, which applies to design document
      download links, not superseded source under version control).

**New surface (§4.5):**
- [ ] Add `Properties` — `(string Name, JsonData Value)` pair enumeration on objects.
- [ ] Add `EnumerateLeaves(JsonDiffOptions? options = null)` — depth-first leaf enumeration
      honouring path exclusions; shared by diff, branch snapshot capture, text search.

**Diff subsystem — new (§5, F5/R5):**
- [ ] Implement `JsonDiff.Diff(...)` — cross-source structural diff producing `JsonDiffResult`
      (`Entries`, `ChangedPaths`, `ToPatch(semantics)`, `IsEmpty`).
- [ ] Kind-change handling as a single `Changed` entry (not remove+add).
- [ ] Array index-wise leaf comparison for `Entries`/`ChangedPaths`; whole-array replacement
      collapse in `ToPatch`.
- [ ] Number comparison: `Numeric` default for `JsonDiff`, via a new `DeepSemanticEquals`
      method (not by changing `DeepEquals` — that decision was revised 2026-07-22; see
      `JsonDataWrappers_Decisions.md` D8). **`DeepEquals` itself needs no code change** —
      it keeps its existing BCL-delegating, exact-text behaviour unchanged, avoiding a
      silent redefinition of a name with established BCL meaning. `Structural` remains
      available as explicit opt-in on `DeepSemanticEquals`.
- [ ] Path-prefix exclusions (`cmsSystem` rule).
- [ ] Read-only over both inputs, no mutation.

**Merge subsystem — new (§6, F5/R6/R7):**
- [ ] Implement `JsonMerge.Apply(...)` / `ApplyInPlace(...)` with `JsonMergeOptions`
      (`Semantics`, `Scope`).
- [ ] `IJsonMergeSemantics` — v1 (object merge, array replace, `null`/`"$$delete"` = delete).
- [ ] **v2 (confirmed this session)** — `"$$delete"` deletes, JSON `null` no longer overloaded
      as delete, new `"$$null"` sentinel sets explicit null. `ToPatch(V2)` emits `"$$null"`
      wherever the diffed target holds explicit null at a changed path.
- [ ] Scope enforcement — silent ignore of out-of-scope patch paths.
- [ ] Round-trip law (§6.4) as a property-based test target: holds unconditionally under v2;
      holds under v1 except the stated explicit-null-to-absent exception.

**Testing obligations (§8):**
- [ ] Cross-source matrix (Element×Element, Node×Node, Element×Node, Node×Element) for every
      diff/merge/equality behaviour.
- [ ] Round-trip law property tests, v1 and v2, including the v1 explicit-null caveat case.
- [ ] Merge semantics v1 and v2 conformance tests against their respective tables, including
      scope enforcement.
- [ ] Converter registration tests: duplicate-throws, freeze-throws, per-assembly scan failure
      attribution, `CRef` round-trip (mirroring `CRefJsonConverterTests`).
- [ ] Null model tests: `Set` writes null, `Remove` removes, byte-faithful authoring of the
      `publishFrom: null` reference case.
- [ ] Serialization round-trip tests for `JsonData`/`JsonData<T>` inside DTOs.
- [ ] `TypedJsonDataJsonConverter<>` and serialized-value extension round-trip tests (F10 —
      not previously covered).

**XML doc comments (new — API reference lives in source, not a corpus document):**
- [ ] Confirm `<GenerateDocumentationFile>true</GenerateDocumentationFile>` is set in
      `DigitalBusiness.JsonDataWrappers.csproj` (add it if not already present) so missing/
      malformed doc comments on public members surface as build warnings.
- [ ] Every public type/method/property touched by this Work Package's other code tasks gets
      a real `///` summary as part of that change — not deferred to a follow-up pass. This
      applies in particular to the wholly new surface (`JsonDiff`, `JsonMerge`,
      `DeepSemanticEquals`, `Properties`, `EnumerateLeaves`, `IJsonMergeSemantics` and its
      two versions) where no prior XML documentation exists to carry forward.
- [ ] Existing public members touched by a rename (§4 above) keep their XML doc comments
      intact through the rename (IDE rename-refactor should already preserve these — confirm
      it did, don't assume).

## 4. Sequencing

1. **Blocking decision first:** the CMS-side merge-semantics-version default (§2b) must be
   settled before any CMS-side document task in §2b proceeds. The library's own documentation
   (§2a) and all code tasks (§3) are **not** blocked by this — they can proceed immediately,
   since v1 and v2 both exist in the library regardless of which the CMS defaults to.
2. Library documentation (§2a) should land before or alongside the code changes (§3) per the
   corpus's documentation-first discipline — the code implements what the finalized design
   states, not the reverse.
3. Within code tasks (§3): null model and converter registration are pre-existing confirmed
   decisions with no dependency on anything else — safe to start immediately. Renames and
   array enumeration are independent, low-risk, mechanical. The diff and merge subsystems
   depend on each other (`ToPatch` is diff output feeding merge input) and should be built
   together, with the round-trip law tests as the shared acceptance criterion.
4. CMS-side document updates (§2b) should follow the blocking decision, and can happen in
   parallel with the library code work — they don't depend on code being finished, only on
   the design (already finalized) and the CMS default decision.

## 5. Definition of done

- [ ] All items in §2a and §2b checked off; retired documents listed as such in
      `CMS_Index.md`, not deleted.
- [ ] All items in §3 checked off; solution builds; new tests pass alongside existing ones.
- [ ] **`JsonDataWrappers_WorkPackage_Outcome_2026-07-22-1.md` produced** (see §7) and its
      proposed document updates reconciled into Design/Decisions/Guide/OpenQuestions/
      WorkRegister as needed — this package is not Complete on code alone.
- [ ] `CMS_Index.md` updated to reflect every new/updated/retired document from
      this package, and this package's own status changed to **Complete**.
- [ ] This package **archived**: renamed
      `JsonDataWrappers_WorkPackage_Archived_{archiveDate}_2026-07-22-1.md`, frozen at its
      final version, alongside its now-Processed Outcome document
      (`JsonDataWrappers_WorkPackage_Outcome_Archived_{archiveDate}_2026-07-22-1.md`).
- [ ] No `[DECIDE]`-equivalent markers left open anywhere in the affected documents.

## 6. Notes / issues encountered

- **2026-07-22 — DeepEquals scope correction.** The diff-subsystem checklist item on number
  comparison originally called for unifying `DeepEquals`'s own default to `Numeric`. On
  review (raised by David: two BCL methods, `JsonElement.DeepEquals`/`JsonNode.DeepEquals`,
  have an established exact-text meaning that this would have silently redefined under the
  same name), this was revised: `DeepEquals` stays untouched; a new, distinctly-named method,
  `DeepSemanticEquals`, carries the `Numeric` comparison instead. `JsonDiff` calls the new
  method rather than `DeepEquals`. Updated in `JsonDataWrappers_Decisions.md` D8 and
  `JsonDataWrappers_Implementation_2026-07-22-1.md` §8a (at the time, this was recorded in
  `JsonDataWrappers_Working.md` §5.3, before that document was archived — see the §2a note
  below). No task-count change to this checklist — same item, corrected approach.
- **2026-07-22 — §2a complete.** `JsonDataWrappers_Design_v1.md`,
  `JsonDataWrappers_Decisions_v1.md`, and `JsonDataWrappers_Archive_DesignDecisions_v1.md`
  created; `JsonDataWrappers_Working.md` (v5) and `JsonDataWrappers_Review_2026-07-19-1.md`
  (v2) archived — renamed with the `_Archived_2026-07-22` suffix
  (`JsonDataWrappers_Working_Archived_2026-07-22.md`,
  `JsonDataWrappers_Review_2026-07-19-1_Archived_2026-07-22.md`), content absorbed above,
  files kept on disk per the no-delete policy. §2b (CMS-side
  cross-references, still blocked on the merge-semantics-version default) and §3 (code tasks)
  remain open.

## 7. Outcome reporting

**For whoever does the dev work** — a developer, or a coding AI working from this dev
project alone, with no access to the documentation corpus this Work Package came from.

A companion file, **`JsonDataWrappers_WorkPackage_Outcome_2026-07-22-1_v1.md`**, was copied
into this project alongside this Work Package. Fill it in as work proceeds, or at minimum
before considering this package done:

1. **Summary** — one paragraph on what was actually implemented.
2. **Task-by-task deviations** — for each checklist item in §2/§3 above: did it match the
   plan, or did something change? If it changed, say how and why.
3. **Design or approach changes discovered** — anything this Work Package's design/
   Implementation guide didn't anticipate: an API that didn't fit as described, an edge case
   that changes the approach, a simplification or complication found while building it. This
   is the most important section — it's what gets fed back into the design documentation.
4. **New open questions or follow-up work** — anything surfaced that needs a decision or
   further work but isn't part of finishing this package.
5. **Verification status** — build result, test results, whether §5's Definition of Done
   criteria are met.
6. **Proposed document updates** — your best guess at what needs to change in
   `JsonDataWrappers_Design.md`, `JsonDataWrappers_Decisions.md`, or
   `JsonDataWrappers_Guide.md` as a result. This doesn't need to be perfect or final — it's a
   starting point for the documentation project to verify and act on, not something you need
   to get exactly right yourself.

Once filled in, this Outcome document is brought back into the documentation project (the
one this Work Package came from) for reconciliation — don't try to update the Design/
Decisions/Guide documents yourself from within this dev project; they're not present here,
and reconciling them is deliberately a separate step done with the full corpus in view.
