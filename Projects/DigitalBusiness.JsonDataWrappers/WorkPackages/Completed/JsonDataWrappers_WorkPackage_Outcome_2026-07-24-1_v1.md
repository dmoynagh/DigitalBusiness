# Work Package Outcome — Merge Semantics Simplification (2026-07-24-1)

> **Reports on:** `JsonDataWrappers_WorkPackage_2026-07-24-1.md`
> **Status:** Complete — all code tasks (§3) implemented, build and tests verified.
> **Filled in by:** GitHub Copilot (coding agent), 2026-07-24

---

## 1. Summary

The versioned merge-semantics abstraction (`IJsonMergeSemantics`, `JsonMergeSemanticsV1`,
`JsonMergeSemanticsV2`) was removed entirely and the single v2-equivalent behaviour was
inlined directly into `JsonMerge`. `JsonMergeOptions` now exposes only `Scope`.
`JsonDiffResult.ToPatch` is now parameterless. All call sites (library and tests) were
updated accordingly, the versioned test groups in `JsonDiffAndMergeTests.cs` were collapsed
into a single conformance suite, and a repository-wide sweep confirmed no remaining
references to the removed types. The solution builds cleanly and the full merge/diff test
suite (11 tests) passes.

## 2. Task-by-task deviations

| Task | Matched plan? | If not, what changed and why |
|---|---|---|
| §3a — Remove versioned semantics abstraction | Yes | Deleted `IJsonMergeSemantics.cs`, `JsonMergeSemanticsV1.cs`, `JsonMergeSemanticsV2.cs`. Inlined marker checks as `JsonMerge.IsDeleteMarker`/`IsSetNullMarker` private helpers, with `DeleteMarker`/`SetNullMarker` promoted to public `const string` fields on `JsonMerge` so `JsonDiffResult` can reference the same literal values instead of re-declaring them. |
| §3b — Update `JsonDiffResult.ToPatch` | Yes | Signature changed to parameterless `ToPatch()`. Internal marker creation now calls private `CreateDeleteMarker()`/`CreateSetNullMarker()` helpers built from `JsonMerge.DeleteMarker`/`JsonMerge.SetNullMarker` rather than delegating to a semantics object. |
| §3c — Update call sites | Yes | Only call sites found were in `JsonDiffAndMergeTests.cs`; no other library, sample, or `Architecture/` docs-as-code references existed. |
| §3d — Test suite updates | Yes | Removed the separate v1/v2 round-trip tests (`ToPatch_ThenMerge_ReproducesTarget_V1`/`_V2`) and collapsed to one `ToPatch_ThenMerge_ReproducesTarget`. Renamed `Merge_DeleteMarkerV2_RemovesProperty` → `Merge_DeleteMarker_RemovesProperty` and `Merge_SetNullMarkerV2_SetsExplicitNull` → `Merge_SetNullMarker_SetsExplicitNull`, dropping the `Semantics =` option argument from all calls. The v1-only test case that existed for the null/delete-ambiguity caveat was already absent from this codebase's test file (only one v1 round-trip test existed, testing add/remove, not the null caveat) — nothing further to remove there. |
| §3e — Dead-reference sweep | Yes | Repo-wide `Select-String` sweep for `JsonMergeSemantics`, `IJsonMergeSemantics`, `\.Semantics`, `ToPatch\(` confirmed zero remaining references outside the now-updated files. |
| §3f — XML doc comments | Yes | `JsonMerge` class summary, `JsonDiffResult.ToPatch` summary, and `JsonPatch` struct summary all rewritten to describe the single unversioned contract (delete/null markers, object-merge/array-replace rule), dropping all v1/v2 wording and the round-trip caveat language. |

## 3. Design or approach changes discovered

- The Work Package's plan text presumed `IJsonMergeSemantics` exposed marker-creation methods
  (`CreateDeleteMarker`/`CreateSetNullMarker`) that `JsonDiffResult` called directly — this
  matched the actual source exactly, so no adaptation was needed there.
- To avoid duplicating the `"$$delete"`/`"$$null"` string literals in two files, the constants
  were centralized as public `const string DeleteMarker`/`SetNullMarker` on `JsonMerge`, with
  `JsonDiffResult` referencing them via `JsonMerge.DeleteMarker`/`JsonMerge.SetNullMarker`.
  This wasn't explicitly specified in the Work Package but keeps a single source of truth and
  is a minor, non-breaking internal improvement.
- No `JsonDataWrappers_Guide.md` file exists anywhere in this dev project/repo, so §1(b) and
  §6's Guide reconciliation preview could not be actioned from within this project — flagged
  in §4 below.

## 4. New open questions or follow-up work

- `JsonDataWrappers_Guide.md` does not exist in this repository. The §6 preview describes
  specific edits to make to it (Quick Start, §3/§4/§5/§8/§9), but there is no file here to
  apply them to. Whoever owns the documentation corpus should either locate/create that file
  in the appropriate documentation project, or confirm this repo is not where it lives, before
  this package can be considered doc-reconciled per §1(b).

## 5. Verification status

- [x] Solution builds.
- [x] Full test suite passes, no regressions outside the merge subsystem's own test groups
      (`JsonDiffAndMergeTests`: 11/11 passed).
- [x] Round-trip law property test passes with no version/caveat branch remaining
      (`ToPatch_ThenMerge_ReproducesTarget`).
- [x] No remaining references to `IJsonMergeSemantics` or merge-semantics versioning anywhere
      in source, tests, or doc comments (confirmed via repo-wide grep sweep).

## 6. Proposed document updates

- [ ] `JsonDataWrappers_Guide.md` — **not found in this repository**; §6 preview in the Work
      Package could not be applied here. Needs to be located/created in the documentation
      project and reconciled there.
- [ ] `JsonDataWrappers_Design.md` / `JsonDataWrappers_Decisions.md` — no further correction
      needed; the implementation matches what was already documented in Design v3/Decisions v4
      ahead of this package.

---

Once filled in, bring this document back into the documentation project (the one this Work
Package came from) for reconciliation.
