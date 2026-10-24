# Everyday Gameplay Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Ship a Windows-first release with reliable residential saves, eviction/move-in, ordinary community visits, and useful options.

**Architecture:** Keep simulation and persistence in FreeSO and UI/travel coordination in Simitone. Execute four bounded subsystem plans in order, followed by one release check; no blanket PR merges or save-format redesign.

**Tech Stack:** C#, .NET 9, MonoGame 3.8.4, existing IFF/FSOV and INI formats; a package-free .NET console check for new persistence logic.

**Spec:** [Approved design](../specs/2026-10-24-everyday-gameplay-design.md).

## Global Constraints

- Windows gameplay validation is the release requirement.
- Keep Linux compatibility where practical.
- Original Sims installation saves remain untouched; keep the fork's separate save location.
- Game assets are never committed or redistributed.
- Existing copied Simitone saves should continue to load; no promise of exporting them back to the original game.
- Browser support, Superstar progression, vacation saving, accurate autonomy reconstruction, and complete expansion compatibility are later projects.
- No remote push or PR creation is part of this design task.

## Review Focus

1. Read-only/full save destination: retain the live game and report failure rather than quit. Baseline plan, Task 2.
2. Repeated eviction/move-in: no repeated cash credit or duplicate Sims. Households plan, Task 2.
3. Travel dialog cancellation/double click: exactly one response and a usable home view. Travel plan, Tasks 1–2.
4. Invalid settings or failed config write: no silent success and no partial Apply. Options plan, Tasks 1–2.
5. Existing saves and 3D at high DPI: retain compatibility and reachable primary controls. Release task below.

## Evidence and baseline

Inspected release: `alexjyong/Simitone`, `simitone-forked`, `e36e83f95d44b9f4b378e7665c4e2ecbaa88b4ab`; engine `alexjyong/FreeSO`, `7e76c1960252ddd033d8937d5edb47d326f5b69c`.

Contributor branch: `alex-main`, `95fc53ee123389dcd3226757acdad15ce8bfda77`; engine `f86bbd34b2112296ed786741f640a0535886d180`. These are substantially different. Do not switch to `alex-main` and assume release features survive.

Candidate upstream PR #68 is open at `72aae7486026b0933606cc4f51da10bd0cd45764`. Its eviction patch contains a money/object-value TODO; its audio options descriptors are empty. Review the exact code, not just the title. `IniConfig.Save()` currently catches exceptions without returning a result; fix this once in that class with an error-returning safe-write method and reuse it.

There is no .NET executable in this planning host, no Windows gameplay runner, and no supplied game assets. No builds or gameplay tests were run during planning. Do not report these as passing. Plan verification commands below are instructions for execution.

## Simplification decisions — Ponytail

- Fix the existing shared Save method and its two leave callers; no SaveFlow enum/coordinator or wrapper.
- Keep SavedLot and add ownership fields beside it; no public HomeLotTravelState class.
- Keep the options draft and Apply code inside the new dialog; no OptionsCommit service or reflection-based framework.
- Reuse the existing INI serializer with a standard-library file replacement, instead of custom readback/rollback machinery in the UI.
- Add one package-free runnable persistence check when it has a real consumer; no three-package test stack or suites for trivial wrappers.
- Retain real-VM, money, cancellation, save-failure, restart, DPI and compatibility checks. Simpler code does not remove the approved requirements.

## File map and execution order

| Order | Plan | Independently reviewable result |
|---|---|---|
| 1 | [Baseline and saves](2026-10-24-baseline-and-saves.md) | Documented branch baseline, test entry point, and safe save/exit flow |
| 2 | [Households](2026-10-24-households.md) | Correct eviction/move-in with restart evidence |
| 3 | [Ordinary visits](2026-10-24-ordinary-visits.md) | Destination/cancel UI and a correct home round trip |
| 4 | [Options](2026-10-24-options.md) | Apply/Cancel for audio and display settings, with persistence |
| 5 | Release task below | One tested Windows package and an honest limitations list |

Each subsystem plan owns its file map and interfaces. Use the same feature branch after the baseline is established; execute sequentially because the flows share `TS1GameScreen`. If investigation shows an already working feature, record the evidence, retain the code, and add only regression coverage that would catch a real failure. Do not force a fake red test for working behavior.

## Task 5: Release the scoped workflows

**Files:** Create `docs/testing/release-1-results.md`; update `docs/testing/everyday-gameplay-matrix.md` and `README.md`; use existing `.github/workflows/build.yml` and Windows packaging.

**Interfaces:** Consumes the four subsystem plans' tests and manual cases. Produces a revision-pinned results report, package path/hash, and known limitations. No publishing or upstream merge is included.

- [ ] Run `dotnet run --project Tests/Simitone.Checks/Simitone.Checks.csproj -c Release`; require exit 0 and all release-required checks executed.
- [ ] Run `dotnet build Client/Simitone/Simitone.Windows/Simitone.Windows.csproj -c Release`; require exit 0. Build the existing Desktop project too if retained by the baseline; log Linux execution as untested unless actually run.
- [ ] Package with the baseline's verified Windows command, then launch that exact package on Windows using the declared legitimate edition. Do not validate only a developer output folder. If the fork's `build.ps1 -Publish` is retained, its native launcher requires the documented C++ toolchain; resolve that in the baseline rather than silently replacing the package format.
- [ ] On disposable data, complete residential save/restart, eviction/move-in twice, Downtown and Old Town trips twice, and settings Apply/Cancel/restart. Verify membership, home lot, cash, relationships, inventory, and objects against recorded before/after values. Run in 2D; repeat primary control smoke cases in 3D and at Windows 150% DPI. A previously copied Simitone save must still open and save without a format conversion.
- [ ] Test 800×600 and 1920×1080 windows, plus the smallest fullscreen mode exposed by the supported adapter. All Apply/Cancel/Save/Return/Quit controls must be visible or scrollable and clickable. Verify modal clicks do not reach the lot beneath them.
- [ ] Confirm source game save hashes did not change. Record Complete/Legacy edition, OS, renderer, SDK, both repository SHAs, package hash, results, and outstanding failures. Untested editions remain unclaimed; no Windows tester means validation pending, not a verified release.
- [ ] Update README with only proven behavior and remaining exclusions. Commit documentation and any release-only fixes with `git commit -m "docs: record everyday gameplay release validation"` after reviewing the staged diff.

## Planning self-review

Coverage: baseline/save prompts → Plan 1; membership/funds/pets → Plan 2; transit/cancel/context → Plan 3; volume/display/config/modal → Plan 4; legacy saves/3D/DPI/package → Task 5. Use existing save and travel code directly; only extract a helper when a real second caller needs it. Engine fixture construction and rendering remain execution concerns, with explicit manual evidence rather than invented passing tests. No product code, dependency installation, or tests are part of this planning commit.
