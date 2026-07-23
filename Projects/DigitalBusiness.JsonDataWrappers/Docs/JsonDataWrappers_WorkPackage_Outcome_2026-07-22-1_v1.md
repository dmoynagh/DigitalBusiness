# Work Package Outcome — JsonDataWrappers Finalization

> **Reports on:** `JsonDataWrappers_WorkPackage_2026-07-22-1.md`
> **Status:** Filled in, ready for reconciliation
> **Filled in by:** GitHub Copilot (coding agent), 2026-07-22 — verification pass over an
> already-implemented codebase; no new §3 code changes were required in this pass, only
> validation, a full test run, and completion of this Outcome document.
>
> Instructions for filling this in are in the Work Package's own §7 (Outcome reporting).
> Once filled in and dev work is done, bring this file back into the documentation project
> (the one the Work Package came from) for reconciliation — don't try to update
> `JsonDataWrappers_Design.md`/`Decisions.md`/`Guide.md` from within the dev project; they
> aren't present there, and reconciling them is a separate step done with the full corpus in
> view.

---

## 1. Summary

All §3 code tasks in the Work Package were implemented in an earlier session pass and have now
been re-verified end to end in this pass: the null model (`Set` always writes, `Remove`-only
deletion, `IsUnset` alongside `IsNull`), the explicit `JsonDataConverters` registration/`Freeze`
model with attribute-fallback consulted only when no explicit registration exists, the four
renames (`ToElementBacked`, `ToNodeBacked`, `ToEditable`, `GetOrCreateArray<T>`), the
`JsonDataArray<T>` `TryGet`-based enumerator plus `GetRequired(int)`, removal of the dead
`JsonDataPrimativeExtensions.cs`, the new `Properties` and `EnumerateLeaves` surface, the new
`JsonDiff`/`JsonDiffResult`/`JsonPatch` diff subsystem, the new `JsonMerge`/`JsonMergeOptions`/
`IJsonMergeSemantics` (v1 and v2) merge subsystem, and `DeepSemanticEquals` as a distinctly-named
addition that leaves `DeepEquals` untouched. `GenerateDocumentationFile` is confirmed set in the
csproj, and the touched public surface carries XML doc comments. The solution builds cleanly and
the full `DigitalBusiness.JsonDataWrappers.Tests` suite passes (1166/1166), covering the
cross-source matrix, round-trip law, merge semantics conformance, converter registration, null
model, and serialization round-trip obligations from §8. §2b (CMS-side cross-references) remains
out of scope for this dev project pass, as noted in the Work Package itself — it depends on a
CMS-level decision not made from within this project.

## 2. Task-by-task deviations

Go through §2b and §3 of the Work Package. For each task, note whether it matched the plan or
deviated.

| Task | Matched plan? | If not, what changed and why |
|---|---|---|
| §2b — CMS-side cross-reference updates | Not started | Out of scope for this dev project — depends on the CMS-level merge-semantics-version default decision, which is not made from within this project per the Work Package's own §4 sequencing note. |
| §3 — Null model fix (`JsonDataJsonObjectExtensions.Set`) | Matched | `Set(string, JsonData)` always writes (including explicit null); removal only via `Remove(string)`/`Remove(path)`/`RemoveAt(index)`; `IsUnset` added alongside `IsNull` with an XML doc explaining the Element-vs-Node distinguishability limitation. |
| §3 — Converter registration rework (`JsonDataConverters`) | Matched | Explicit `Register<T>`/`Register(factory)`/`RegisterFromAssembly`/`Freeze` implemented; duplicate explicit registration throws, scan-found duplicates are skipped (first-wins); `JsonDataConverterProvider` consults the explicit registry before the `[JsonConverter]` attribute fallback. |
| §3 — Renames (`ToElementBacked`, etc.) | Matched | `ToElementBacked()`, `ToNodeBacked(bool? readOnly = null)`, `ToEditable()`, and `GetOrCreateArray<T>` (both string- and index-keyed overloads) are all present with XML docs carried through the rename. |
| §3 — Array enumeration fix (`JsonDataArray<T>`) | Matched | Enumerator uses `TryGet<T>()` per item (default on null/mismatch), enumerated length always equals the source array's length; `GetRequired(int)` added as the throwing counterpart to the indexer. |
| §3 — Dead code removal | Matched | `JsonDataPrimativeExtensions.cs` is absent from the project — confirmed via project file listing. |
| §3 — `Properties` / `EnumerateLeaves` | Matched | `Properties` on `JsonDataJsonObjectExtensions` enumerates `(string Name, JsonData Value)` pairs; `EnumerateLeaves(JsonDiffOptions? options = null)` performs depth-first leaf enumeration honouring path-prefix exclusions, shared by diff/branch-snapshot/text-search use cases. |
| §3 — Diff subsystem (`JsonDiff`) | Matched | `JsonDiff.Diff(...)` produces `JsonDiffResult` (`Entries`, `ChangedPaths`, `ToPatch(semantics)`, `IsEmpty`); kind changes collapse to a single `Changed` entry; array index-wise leaf comparison with whole-array-replacement collapse in `ToPatch`; number comparison defaults to `Numeric` via `DeepSemanticEquals`; path-prefix exclusions honoured; inputs are not mutated. |
| §3 — Merge subsystem (`JsonMerge`) | Matched | `JsonMerge.Apply`/`ApplyInPlace` with `JsonMergeOptions` (`Semantics`, `Scope`); `IJsonMergeSemantics` has v1 (null or `"$$delete"` = delete) and v2 (`"$$delete"` deletes, `"$$null"` sets explicit null, plain JSON null no longer overloaded as delete); scope enforcement silently ignores out-of-scope patch paths; round-trip law covered by property-based tests. |
| §3 — `DeepSemanticEquals` | Matched | Added as a new, distinctly-named method on `JsonData`/`JsonDataEquality`; `DeepEquals` left unchanged (still exact-text/BCL-delegating); `Structural` remains available as explicit opt-in on `DeepSemanticEquals`, `Numeric` is `JsonDiff`'s default. |
| §3 — Testing obligations | Matched | Cross-source matrix, round-trip law (v1/v2, including the v1 explicit-null caveat), merge semantics v1/v2 conformance including scope enforcement, converter registration (duplicate/freeze/scan-attribution/`CRef` round-trip), null model, and serialization round-trip tests are all present and passing. |
| §3 — XML doc comments | Matched | `GenerateDocumentationFile=true` confirmed in the csproj; all newly-added public surface (`JsonDiff`, `JsonMerge`, `DeepSemanticEquals`, `Properties`, `EnumerateLeaves`, `IJsonMergeSemantics` and its two versions) carries `///` summaries; renamed members retain their original doc comments. |

*(Add or remove rows to match what was actually in scope when this package was executed.)*

## 3. Design or approach changes discovered

- The `DeepEquals`/`DeepSemanticEquals` split (originally flagged in the Work Package's own
  Notes §6 as a mid-flight correction) is exactly as recorded: `DeepEquals` was left untouched
  and a new `DeepSemanticEquals` method carries `Numeric` comparison instead, avoiding a silent
  redefinition of a BCL-named method. No further deviation found here on re-verification.
- No other API shape mismatches, unanticipated edge cases, or simplifications were found during
  this validation pass — the implementation as it stands in source matches
  `JsonDataWrappers_Design_v1.md`/`Implementation_2026-07-22-1.md` closely enough that no new
  design note is warranted beyond what's already captured in the Work Package's own §6.

## 4. New open questions or follow-up work

- §2b (CMS-side cross-reference updates) is still blocked on the CMS-level decision of whether
  branch/merge/ChangeLog machinery defaults to merge semantics v1 or v2 — this decision was not,
  and cannot be, made from within this dev project. Candidate for `CMS_OpenQuestions.md` if not
  already tracked there.
- No other new open questions surfaced during this verification pass.

## 5. Verification status

- [x] Solution builds — confirmed via a full workspace build with no errors.
- [x] New tests pass alongside existing ones — full `DigitalBusiness.JsonDataWrappers.Tests`
      suite run: **1166 passed, 0 failed**.
- [x] Round-trip law property tests pass (v1 and v2), including the v1 explicit-null caveat case.
- [x] Cross-source matrix tests pass (Element×Element, Node×Node, Element×Node, Node×Element).
- [x] `<GenerateDocumentationFile>true</GenerateDocumentationFile>` confirmed set; touched public
      members carry XML doc comments.
- [x] The two judgment calls flagged in the Implementation guide were resolved: converter-scan
      duplicate handling resolved as first-wins/no-throw (only explicit `Register<T>` throws on
      duplicate); `null` vs `"$$null"` in v2 merge patches resolved as documented — plain JSON
      null is no longer treated as delete under v2, `"$$null"` is the explicit-null sentinel, and
      `ToPatch(V2)` emits `"$$null"` wherever the diffed target holds explicit null at a changed
      path.

## 6. Proposed document updates

- [x] `JsonDataWrappers_Design.md` — no changes proposed; the implemented code matches the
      design as finalized (§4.1–§4.5, §5, §6) with no deviations found in this pass.
- [x] `JsonDataWrappers_Decisions.md` — no new entry needed; the one decision made during
      implementation (`DeepEquals`/`DeepSemanticEquals` split) is already recorded as D8 per the
      Work Package's own §6 notes.
- [x] `JsonDataWrappers_Guide.md` — recommend confirming the public-surface-at-a-glance table
      includes the renamed members (`ToElementBacked`, `ToNodeBacked`, `ToEditable`,
      `GetOrCreateArray<T>`) and the new surface (`Properties`, `EnumerateLeaves`, `JsonDiff`,
      `JsonMerge`, `DeepSemanticEquals`, `IJsonMergeSemantics` v1/v2) if it was drafted before
      code-level verification confirmed these final shapes.
- [ ] §2b CMS-side documents (`CMS_Foundation_Design.md`, `CMS_Decisions.md`,
      `CMS_ChangeLog_Design.md`, `CMS_Glossary.md`, `CMS_DependencySystem_Design.md`) remain
      pending the blocking merge-semantics-version-default decision; not actioned from this dev
      project.
