# CLAUDE.md — DigitalBusiness.DependencyInjectionExtensions

## What this repo is
`DigitalBusiness.DependencyInjectionExtensions` — extends the standard
Microsoft.Extensions.DependencyInjection container additively (never a fork or replacement):
`Lazy`, `Factory`/`KeyedFactory`, `Owned`, `KeyedServices` (cascading keys), `Forwarded`,
`ServiceBuildExtensions` (build pipeline), and `ServiceConfig` (typed collection-attached
config). See the design corpus (a separate Claude project) for the full Overview/Design/
Decisions per Topic if something here needs more context than this file gives.

<!-- TODO: fill in with your actual solution/project file names -->
Solution: `DigitalBusiness.DependencyInjectionExtensions.slnx` (.NET version: __ )
Main project: `Projects/DigitalBusiness.DependencyInjectionExtensions`
Tests: `Projects/DigitalBusiness.DependencyInjectionExtensions.Tests`

## Build & test
<!-- TODO: confirm these match the real project once /init or first session runs -->
- Build:  `dotnet build DigitalBusiness.DependencyInjectionExtensions.slnx`
- Test:   `dotnet test DigitalBusiness.DependencyInjectionExtensions.slnx`
- Format: `dotnet format DigitalBusiness.DependencyInjectionExtensions.slnx`

Always build AND run the full test suite before declaring any task complete. A task with
failing tests is not complete.

## How work arrives here
Development is driven by Work Package bundles under `WorkPackages\`.
**READ `WorkPackages\README.md` FIRST** — it defines the bundle family (WorkPackage +
Implementation + Outcome), the Outcome document obligation, and the `completed\` folder
rules. Follow it exactly. Do not act on work outside the current package's scope without
being asked. If the `documentation-methodology` skill is installed, it states the same
rules natively — read the marker file anyway, since it's the fallback and the two must
always agree.

## This library's own conventions (from the design corpus)
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

## Coding conventions
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
  see `WorkPackages\README.md`).

## Component guides
Once written, stable usage guides for each Topic live under `docs\guides\` (synced copies
of the design corpus's `DI_Guide_v1.md` sections). Consult the relevant section before using
a Topic's feature; the Guide states the version it was last checked against.
