# workpackages — folder conventions

This folder holds Work Package bundles handed off from the design-conversation project.
If you're an AI or developer working in this folder, read this first.

## The rule

- **Files here, at this root level, are current work.** Treat them as active scope.
- **Files under `completed\` are finished work.** Reference only. Do not treat them as
  pending tasks, do not re-implement them, and do not include them when scanning for current
  work — unless a person explicitly asks you to look at a specific file in there.

## What one package looks like

A single Work Package normally arrives as a family of files sharing the same date-tagged
identifier, e.g.:

```
{Project}_{Topic}_WorkPackage_2026-07-22-1_v1.md
{Project}_{Topic}_WorkPackage_Implementation_2026-07-22-1_v1.md
{Project}_{Topic}_WorkPackage_Outcome_2026-07-22-1_v1.md
```

Sometimes a full design-context document is bundled in alongside these too — if so, it's
listed in the WorkPackage's own "Bundle contents" section. Treat the whole set as one unit.

## What to do with each file

- **`WorkPackage`** — the checklist: what to build, in what order, definition of done.
- **`WorkPackage_Implementation`** — the technical how: class/method shapes, sequencing, edge
  cases. Work from this rather than re-deriving intent from scratch.
- **`WorkPackage_Outcome`** — fill this in as you go, or once the work is done: a summary,
  task-by-task deviations from plan, any design/approach changes discovered, new open
  questions, and verification status. This is what gets sent back to the design-conversation
  project — it's the whole point of the loop back.

## Finishing a package

1. Make sure the WorkPackage's own Definition of Done is actually met (code done, Outcome
   filled in).
2. Send/pass the Outcome document back to the design-conversation project.
3. Move the whole family for that package — WorkPackage, Implementation, Outcome, and any
   bundled Design copy — into `completed\`.

Do this move as soon as the dev-side work is done. **Don't wait** for confirmation that the
design-conversation project has reconciled the Outcome back into its own documents first —
that's a separate step on a separate timeline. This folder's `completed\` marker only means
"the dev side is done with this," not "the design corpus has processed it."

## A note on multiple packages

More than one package can be live in this folder's root at the same time. Each one is
self-contained (its Implementation doc has what it needs), so there's no need to treat them
as a strict queue.

## Don't rename or move this file

This README stays at the root of `workpackages\` — it isn't part of any package and never
moves to `completed\`.
