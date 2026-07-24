# Work Package — Merge Semantics Simplification (2026-07-24-1)

> **Opened:** 2026-07-24 · **Status:** Open — code tasks (§3) not started.
> **Source:** `JsonDataWrappers_Decisions_v4.md` D9 (supersession) and
> `JsonDataWrappers_Design_v3.md` §6.1/§6.4/§5.2/§9 — confirmed with David 2026-07-24. v1
> merge semantics is retired entirely; the library moves from a versioned `IJsonMergeSemantics`
> (v1/v2) split to a single, unversioned methodology behaving as v2 did. Breaking changes are
> explicitly sanctioned — v1 was never used by any consumer, so there is nothing to preserve
> replay-fidelity for, and no dependent project to coordinate a migration with.
>
> Follows the conventions in `DocumentationMethodology.md`. Paired with
> `JsonDataWrappers_WorkPackage_Outcome_2026-07-24-1.md` (empty skeleton, to be filled in by
> whoever does the dev work).
>
> **No separate Implementation guide this time** — unlike
> `JsonDataWrappers_WorkPackage_Implementation_Archived_2026-07-24_2026-07-22-1_v4.md` (which
> was built by reading the actual `DigitalBusiness_JsonDataWrappers.zip` source directly), §3
> below is written from Design/Decisions only — this session didn't have the current source in
> context. The scope is narrow enough (one subsystem) that this should be sufficient, but
> whoever picks this up should confirm exact file locations/signatures against the actual
> source before editing, per the same caution the original Implementation guide gave: **match
> on the method signature, not on any line number or file name guessed here.**

---

## 1. Scope

Two parts, both required:

**(a) Library code** — collapse the merge subsystem from a versioned abstraction
(`IJsonMergeSemantics`, `V1`/`V2` implementations, a `Semantics` option, a version parameter
on `ToPatch`) to a single, unversioned implementation matching what v2 already did. This is
the only part of this package that is blocking — see §4.

**(b) Documentation reconciliation** — `JsonDataWrappers_Guide.md` still describes the
versioned v1/v2 shape (it was last reconciled against the *previous* Work Package's Outcome,
before this decision). It is **deliberately not updated yet** — Guide describes verified,
built behaviour, and the code doesn't reflect this decision until §3 below lands. Update it
during this package's own reconciliation step, not before.

Not in this package's scope: the CMS-side document updates originally tracked as "§2b" —
David reports the CMS's own docs/design have already been updated to build against
v2-shaped logic, which this decision now generalizes to "the only logic." No CMS-side action
is being tracked as blocked on this project anymore (see `JsonDataWrappers_Index.md`).

## 2. Documentation tasks — already complete

- [x] `JsonDataWrappers_Design.md` → **v3**. §6.1 (`JsonMerge`/`JsonMergeOptions` shapes,
      merge rules), §6.4 (round-trip law, no caveat), §5.2 (`ToPatch()` signature), §5.4/§9
      (informative CMS mappings), R7 (requirement text) all updated to the single-methodology
      shape.
- [x] `JsonDataWrappers_Decisions.md` → **v4**. D9 superseded in place (same pattern as D8):
      original v1/v2 resolution shown first, then the 2026-07-24 revision and its reasoning
      (v1 never shipped, nothing to replay, breaking changes safe pre-1.0). Out-of-scope
      table's RFC-patch-format row updated (no longer "versioned").
- [ ] `JsonDataWrappers_Guide.md` — **deferred to this package's reconciliation** (see §1(b)
      and §6 below for exactly what needs to change).

## 3. Code tasks

### 3a. Remove the versioned semantics abstraction

- Delete `IJsonMergeSemantics` and its implementations (the current source has at least a
  `JsonMergeSemanticsV1`-equivalent and `JsonMergeSemanticsV2` — referenced as
  `JsonMergeSemanticsV2.Instance` in the current Guide; confirm exact type names in source).
- Inline the single behaviour directly into `JsonMerge`'s merge logic:
  - `"$$delete"` → delete the property.
  - JSON `null` (literal) or `"$$null"` (sentinel) → set the property to explicit JSON null.
    Both are equivalent at apply time (D9's carried-forward judgment call) — keep whichever
    single code path already handled this under v2, don't reintroduce a null-means-delete
    branch.
  - Object → merge (append/replace); Array → replace wholesale; other Value → replace.
- `JsonMergeOptions`: remove the `Semantics` property. Only `Scope` remains.

### 3b. Update `JsonDiffResult.ToPatch`

- Change the signature from `ToPatch(IJsonMergeSemantics semantics)` to a parameterless
  `ToPatch()`.
- Internal patch generation keeps emitting `"$$delete"` for removals and `"$$null"` for
  explicit-null targets at changed paths (the same emission logic v2 already had) — this
  doesn't change, only the now-removed version selection around it.

### 3c. Update call sites

- Grep the solution for `IJsonMergeSemantics`, `JsonMergeSemanticsV1`, `JsonMergeSemanticsV2`,
  `.Semantics =`, and `ToPatch(` with an argument — update every call site (library internals,
  sample/handler code, anything under `Architecture/` docs-as-code if applicable) to the
  parameterless/`Scope`-only shape.

### 3d. Test suite

- Remove v1-specific conformance tests and the round-trip-law "v1 explicit-null caveat" test
  case entirely.
- Update remaining round-trip-law property tests to assert the law holds unconditionally —
  no caveat branch, no version parameter in the test's own `ToPatch()` calls.
- Collapse "merge semantics v1 and v2 conformance" test groups into one conformance suite
  against the single behaviour table (delete/set-null/merge/replace + scope enforcement +
  literal-null-vs-`$$null` equivalence).
- Confirm the cross-source matrix (Element×Element, Node×Node, Element×Node, Node×Element)
  and other unrelated suites are unaffected — this package touches only the merge subsystem.

### 3e. Dead-reference sweep

- Confirm no remaining references anywhere in source, tests, or XML doc comments to
  `IJsonMergeSemantics`, a `Version` discriminator, or "v1"/"v2" as merge-semantics concepts.
  (`JsonDiff`'s own `Numbers` option — `Numeric`/`Structural` — is unrelated and untouched.)

### 3f. XML doc comments

- `JsonMerge.Apply`/`ApplyInPlace`, `JsonMergeOptions`, and `JsonDiffResult.ToPatch` should
  have their `///` summaries updated to describe the single, unversioned contract — carry
  forward existing wording where still accurate rather than rewriting from scratch.

## 4. Sequencing

1. Design/Decisions (§2) are already done — code implements the confirmed design, not the
   reverse, per the corpus's documentation-first discipline. No blocking dependency before
   starting §3.
2. Within §3: remove the abstraction and inline single behaviour (3a) before touching
   `ToPatch` (3b), since 3b's simplification follows directly from 3a. Call-site sweep (3c)
   and test updates (3d) follow naturally once 3a/3b compile. Dead-reference sweep (3e) and
   doc comments (3f) are cleanup passes done last, before final build/test.
3. Guide reconciliation (§1(b)) happens after this package's Outcome is filled in and brought
   back — same sequence as the previous package.

## 5. Definition of done

- [ ] All items in §3 checked off; solution builds; full test suite passes with no
      regressions outside the merge subsystem's own (now-collapsed) test groups.
- [ ] No remaining source, test, or doc-comment references to `IJsonMergeSemantics` or merge
      semantics versioning (§3e).
- [ ] `JsonDataWrappers_WorkPackage_Outcome_2026-07-24-1.md` produced (see §7) and reconciled:
      `Guide.md` updated per §6 below; `Design.md`/`Decisions.md` confirmed to need no further
      changes (or amended if the actual implementation surfaces something this package's
      Design/Decisions update didn't anticipate).
- [ ] `JsonDataWrappers_Index.md` updated to reflect this package's archival and any new
      document versions.
- [ ] This package archived as a trio (Work Package + Outcome; no separate Implementation
      guide to archive this time), per the corpus's naming convention.

## 6. What Guide.md will need at reconciliation (preview, not to be actioned early)

- Quick Start's `diff.ToPatch(JsonMergeSemanticsV2.Instance)` → `diff.ToPatch()`.
- §3 Core Concepts' "Merge semantics are versioned" bullet — rewritten to describe a single,
  unversioned methodology; drop the "so a patch created under one version can still be
  replayed correctly later" justification (no longer applicable).
- §4 "Turning a diff into a patch, and applying it" — drop the "use v2 for anything new... use
  v1 only to replay" guidance; show the parameterless call.
- §5 Limits — remove the "Merge semantics v1 cannot express 'set to explicit null'" bullet
  entirely (v1 no longer exists to have this limitation).
- §8 Public surface table — remove the `IJsonMergeSemantics — V1/V2` row; update the
  `JsonDiff.Diff`/`ToPatch` row to show the parameterless call.
- §9 "why merge semantics are versioned" — rewritten (they no longer are).

## 7. Notes / issues encountered

*(Fill in during dev work — anything the code surfaces that Design/Decisions didn't
anticipate, per §7 below.)*

## 8. Outcome reporting

**For whoever does the dev work** — a developer, or a coding AI working from this dev project
alone, with no access to the documentation corpus this Work Package came from.

A companion file, **`JsonDataWrappers_WorkPackage_Outcome_2026-07-24-1_v1.md`**, was created
alongside this Work Package. Fill it in as work proceeds, or at minimum before considering
this package done:

1. **Summary** — one paragraph on what was actually implemented.
2. **Task-by-task deviations** — for each checklist item in §3 above: did it match the plan,
   or did something change? If it changed, say how and why.
3. **Design or approach changes discovered** — anything this Work Package didn't anticipate:
   an API shape that didn't fit as described, an edge case, a simplification or complication
   found while building it. This is the most important section.
4. **New open questions or follow-up work** — anything surfaced that needs a decision or
   further work but isn't part of finishing this package.
5. **Verification status** — build result, test results, whether §5's Definition of Done
   criteria are met.
6. **Proposed document updates** — best guess at what needs to change in `Guide.md` beyond
   the §6 preview above, and whether `Design.md`/`Decisions.md` need any further correction.

Once filled in, this Outcome document is brought back into the documentation project for
reconciliation — don't update Guide/Design/Decisions from within the dev project directly.
