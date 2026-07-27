# CLAUDE.md — DigitalBusiness

## What this repo is
`DigitalBusiness` is a mono-repo holding several related, independently-versioned projects
side by side under `Projects\`:

- `DigitalBusiness` — the core library.
- `DigitalBusiness.Extensibility`
- `DigitalBusiness.Json`
- `DigitalBusiness.JsonDataWrappers` — already has an active Work Package pipeline; see its
  own `Projects\DigitalBusiness.JsonDataWrappers\WorkPackages\` for a live example.
- `DigitalBusiness.DependencyInjectionExtensions` — extends the standard
  Microsoft.Extensions.DependencyInjection container additively (never a fork or
  replacement): `Lazy`, `Factory`/`KeyedFactory`, `Owned`, `KeyedServices` (cascading keys),
  `Forwarded`, `ServiceBuildExtensions` (build pipeline), and `ServiceConfig` (typed
  collection-attached config). See the design corpus (a separate Claude project) for the
  full Overview/Design/Decisions per Topic if something here needs more context.

Solution: `Solutions\DigitalBusiness.slnx`
Tests live in a parallel `Tests\` folder at repo root, one subfolder per project (e.g.
`Tests\DigitalBusiness.JsonDataWrappers.Tests\`), not nested inside each project's own
folder. `DigitalBusiness.DependencyInjectionExtensions.Tests\` follows the same pattern.

## Build & test
<!-- TODO: confirm exact solution path/flags once /init runs against the real repo -->
- Build:  `dotnet build Solutions\DigitalBusiness.slnx`
- Test:   `dotnet test Solutions\DigitalBusiness.slnx`
- Format: `dotnet format Solutions\DigitalBusiness.slnx`

Always build AND run the full test suite before declaring any task complete. A task with
failing tests is not complete. Unless a Work Package's Implementation doc gives a
project-specific test filter, run the full suite — this is a shared solution, and a change
in one project can affect another.

## How work arrives here
Development is driven by Work Package bundles under **each project's own**
`Projects\{ProjectName}\WorkPackages\` folder — not one shared `WorkPackages\` at the repo
root. When working on `DigitalBusiness.DependencyInjectionExtensions`, that's
`Projects\DigitalBusiness.DependencyInjectionExtensions\WorkPackages\`.

**READ that project's `WorkPackages\README.md` FIRST** — it defines the bundle family
(WorkPackage + Implementation + Outcome), the Outcome document obligation, and the
`WorkPackages\Completed\` folder rules (capital C — matches the existing
`JsonDataWrappers` convention). Follow it exactly. Do not act on work outside the current
package's scope without being asked, and do not act on another project's Work Packages
unless specifically asked to. If the `documentation-methodology` skill is installed, it
states the same rules natively — read the marker file anyway, since it's the fallback and
the two must always agree.

## This library's own conventions (from the DI design corpus)
Applies to `DigitalBusiness.DependencyInjectionExtensions` specifically:
- **Opt-in, never global.** Every feature activates only via this library's own extension
  methods/types. An application that doesn't use a feature is unaffected.
- **Reuse framework vocabulary over inventing new API surface** —
  `GetService`/`GetRequiredService` naming, `[FromKeyedServices]`, exception shapes matching
  the framework's own, `ServiceDescriptor.ImplementationInstance` over bespoke mechanisms.
- **One override idiom:** ordinary DI last-registration-wins is the customization mechanism
  everywhere — no bespoke override APIs per feature.
- **A dedicated type beats a structurally-generic container** wherever two features might
  otherwise collide (`CompoundKey` over string-concatenated keys; `ServiceConfig<T>`
  requiring a dedicated `T` per feature over a shared `Dictionary<Type, object>`).
- **Never resolve from a provider that doesn't fully exist yet** — anything acting at
  registration or pre-build time avoids spinning up a temporary `IServiceProvider`.

## Coding conventions (repo-wide)
- Every public type/member gets a real `///` XML doc summary as part of writing it.
- `<GenerateDocumentationFile>` is enabled; treat missing-doc warnings as errors to fix.
- Follow existing patterns in the codebase over introducing new ones; if a new pattern
  seems necessary, flag it in the Outcome rather than deciding silently.
- Nullable reference types enabled; no `#pragma` suppressions without a comment saying why.
- Write or update tests as part of each code task, not as a trailing afterthought, unless
  the package sequences testing separately.

## Git conventions
- Work only on the branch you were started on (`wp/{topic}-{date-N}`). Never switch to,
  commit on, merge into, or delete `main`. Never push.
- Commit at logical checkpoints: `"WP {date-N}: <what changed>"`.
- Merging is always a human act in Visual Studio (squash-merge is the recommended default;
  see the relevant project's `WorkPackages\README.md`).

## Component guides
Once written, stable usage guides for each `DependencyInjectionExtensions` Topic live under
`docs\guides\` (synced copies of the design corpus's `DI_Guide_v1.md` sections). Consult the
relevant section before using a Topic's feature; the Guide states the version it was last
checked against.
