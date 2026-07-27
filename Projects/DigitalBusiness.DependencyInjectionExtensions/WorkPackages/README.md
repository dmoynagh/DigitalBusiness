# workpackages — folder conventions

> **Marker version: 3** (2026-07-28). Casing fix, kept in sync with the
> `documentation-methodology` skill (now skill version 2): the dev-side "finished work"
> subfolder is `WorkPackages\Completed\` (capital C), not `completed\` — corrected to match
> the real, already-in-use precedent found in `DigitalBusiness\Projects\DigitalBusiness.
> JsonDataWrappers\WorkPackages\Completed\` during the `DependencyInjectionExtensions`
> walkthrough. Everything else carries over unchanged from marker version 2 (2026-07-27,
> the full resync with the skill covering the P3/P5 additions).

This folder holds Work Package bundles handed off from the design-conversation project.
If you're an AI or developer working in this folder, read this first. This file restates,
for readers without the `documentation-methodology` Claude Code skill, exactly what that
skill states natively — the two are meant to say the same thing.

## The bundle: a family of files sharing one date-N

Files are named `{Project}_{Topic}_{DocType}_{YYYY-MM-DD}-{N}_v{N}.md`. The `{YYYY-MM-DD}-{N}`
(the "date-N") identifies one package; all files carrying the same date-N are one family:

| File | Role |
|---|---|
| `..._WorkPackage_{date-N}_v{N}.md` | Scope, documentation tasks, code tasks (checklists), sequencing, Definition of Done, Bundle contents |
| `..._WorkPackage_Implementation_{date-N}_v{N}.md` | The technical *how*: class shapes, signatures, sequencing, edge cases, extracted Design context. Self-contained on purpose. |
| `..._WorkPackage_Outcome_{date-N}_v{N}.md` | The report back to the design side. Arrives as a skeleton; **you fill it in as you work.** |
| (optional) a full Design copy | Only present when the package's Bundle contents section says so |

The trailing `_v{N}` is a revision counter — increment it in the filename if you produce a
substantially revised copy of a document; never spawn a second, differently-dated Outcome
for the same package.

## Non-negotiable rules

1. **Read the Bundle contents section of the WorkPackage first.** It states exactly what
   context you have. Do not assume access to the wider design corpus, and do not invent
   references to documents that aren't in the bundle or the repo.
2. **Work only within the package's scope.** If you notice an unrelated improvement, record
   it in the Outcome's follow-up section — do not implement it unless asked.
3. **Blockers are surfaced, never silently resolved.** If a task depends on a design decision
   that was never actually made, or the Implementation doc contradicts the code's real state:
   stop that task, record the blocker in the WorkPackage's Notes/Issues section AND the
   Outcome, tell the user, and continue with unblocked tasks if any.
4. **The Outcome is updated as you work, not at the end.** Every deviation from plan, every
   discovery that the design didn't anticipate, every new question — written into the Outcome
   at the moment it happens. An interrupted session must leave a partially-true Outcome, not
   an empty one.
5. **The Definition of Done includes a filled-in Outcome.** Code alone is never "done."
   Before declaring a package complete: full solution build succeeds, full test suite passes,
   every checklist item is ticked or explicitly deferred with a reason, and every Outcome
   section is filled in.
6. **Verification is executed, not assumed.** If the Implementation doc has a Verification
   commands section, run those exact commands. If it doesn't, run the repo's standard build
   and full test commands (see CLAUDE.md) and record in the Outcome that no
   package-specific verification was specified.
7. **Tick checklists in the repo's copy of the WorkPackage as tasks complete.** The design
   project keeps its own authoritative copy; the repo copy is the live working record here.
8. **If a documentation task revs a component Guide, also refresh this repo's copy of it**
   (typically under `docs\guides\`, one file per Guide, each stating the Guide version it
   was taken from). The repo copy exists so any dev-side session — yours or a future one —
   has correct usage guidance without needing corpus access; letting it silently lag behind
   the design side's Guide defeats that purpose. This is a distinct file from the design
   corpus's own Guide and from this marker/skill, which cover *process*, not component usage.

## Outcome document structure (fill every section)

1. **Header** — which Work Package, date, status `Unprocessed` (the design side flips it to
   `Processed`, never you).
2. **Summary** — one paragraph: what was actually implemented.
3. **Task-by-task deviations** — for each task: `matched plan` or a description of how and
   why it deviated.
4. **Design or approach changes discovered** — the most valuable section. Anything
   implementation surfaced that the design didn't anticipate.
5. **New open questions or follow-up work** — candidates for the design side's registers.
6. **Verification status** — build/tests results; whether the Definition of Done was met,
   item by item.
7. **Author's proposed document updates** — your best-guess checklist of which design-side
   documents likely need updating and why ("Design §x needs …", "new Decisions entry
   for …"). Propose; the design side verifies. Use the branch's `git log` and `git diff`
   as source material for sections 3–7 so the report reflects what actually happened.

## Completion mechanics

When the package's dev-side work is fully done (rules 5–6 satisfied):

1. Move the **whole family** — WorkPackage, Implementation, Outcome, any attached Design
   copy — into `WorkPackages\Completed\` as one unit, in one commit.
2. Never move this README; it is a standing marker, not part of any family.
3. A move to `Completed\` signals only that dev-side work is finished. It does NOT mean the
   design side has reconciled the Outcome — that happens elsewhere, on its own schedule.
   Never rename any file here with `_Archived_...`; archiving is a design-side act.

## Git conduct

- Work only on the branch you were started on (typically `wp/{topic}-{date-N}`). Never
  switch to, commit on, merge into, or delete `main`. Never push.
- Commit at logical checkpoints: `"WP {date-N}: <what changed>"`. The first commit on the
  branch is normally the bundle drop (made by the user).
- Merging the branch is always a human act in Visual Studio. **Recommended default:**
  squash-merge to `main` with message `WP {date-N}: {Topic} — <summary>` — one main-commit
  per package, keeping `main`'s history legible. This is a recommendation, not a mandate;
  if the human merging prefers to preserve full checkpoint history via a merge commit
  instead, that's an acceptable alternative. Either way, don't make this call yourself —
  it's the human's decision at merge time.

## A note on multiple packages

More than one package can be live in this folder's root at the same time. Each one is
self-contained (its Implementation doc has what it needs), so there's no need to treat them
as a strict queue.

## Review-mode variant

If asked to **review** work (a branch diff, or changes made manually in Visual Studio)
against a Work Package or other spec: stay read-only (plan mode, if the tool supports it),
map each change to the task or requirement it serves, and report findings only —
correctness risks, untested edge cases, convention violations, work done that nothing asked
for, and any behavior change that a design document likely describes (flag those as
proposed design-side updates). Do not fix anything mid-review unless explicitly told to.

## Coding conventions that always apply

- Every public type/member gets a real `///` XML doc summary as part of writing it.
- Follow the codebase's existing patterns over introducing new ones; a genuinely necessary
  new pattern is a "Design or approach changes discovered" Outcome entry, not a silent choice.
- Write or update tests as part of each code task, not as a trailing afterthought, unless
  the package sequences testing separately.
- Repo-specific commands, projects, and additional conventions: see `CLAUDE.md` at repo root.

## Don't rename or move this file

This README stays at the root of `workpackages\` — it isn't part of any package and never
moves to `Completed\`.
