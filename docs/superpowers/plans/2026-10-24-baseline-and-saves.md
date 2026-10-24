# Baseline and Saves Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Establish the contribution baseline and prevent save failure or unsupported save-away actions from silently losing live progress.

**Architecture:** Preserve the current save serializers and fix the shared save method and its callers directly in `TS1GameScreen`. No separate save-flow abstraction.

**Tech Stack:** C#, .NET 9, MonoGame 3.8.4, current FreeSO submodule; a package-free .NET check executable for logic added later.

**Spec:** [Approved design](../specs/2026-10-24-everyday-gameplay-design.md). Execution entry point and complete constraints: [release plan](2026-10-24-everyday-gameplay.md).

## Global Constraints

- Windows gameplay validation is the release requirement.
- Original Sims installation saves remain untouched; keep the fork's separate save location.
- Game assets are never committed or redistributed.
- Keep current save formats where possible.
- No remote push or PR creation is part of this design task.

## Review Focus

- Saving to a read-only/full destination must not close the game or discard the lot (Task 2).
- Closing while away must not offer a misleading save-and-quit operation (Task 2).
- Cancel and discard must have distinct behavior, including no VM write on Cancel (Task 2).
- Existing Simitone save files must reopen (Task 1 and release validation).
- Original installation data must remain byte-identical after tests (Task 1 and release validation).

## File map

- `docs/testing/baseline.md`: exact branch/engine/SDK and build commands, edition, patch ledger.
- `docs/testing/everyday-gameplay-matrix.md`: numbered manual cases and observed gap status.
- `Client/Simitone/Simitone.Client/UI/Screens/TS1GameScreen.cs`: actual saving, failure alert, quit/return wiring.
- `Client/Simitone/Simitone.Client/UI/Panels/UIMainPanel.cs`: disable unsupported Save action.
- `.github/workflows/build.yml`: run the package-free checks added with their first real consumer in the options task. No new test framework or solution restructuring.

## Task 1: Verified baseline and residential save round trip

**Interfaces:** Consumes the exact two branch/engine revisions in the release plan. Produces `baseline.md` and numbered cases B01–B06. Later tasks use the resulting baseline, not a moving remote branch.

- [ ] At execution, use the worktree skill before feature changes. Start from pinned `alex-main` and carry the approved spec/plans into the worktree. Initialize submodules. Compare the pinned release branch, including `.gitmodules`, FreeSO pointer, Windows installation/save-path handling, free-will startup/toggle, neighborhood return context, project layout, and build workflow. Preserve those selected release improvements on the contribution branch; record each imported commit/hunk and exclusion in `baseline.md`. Weather and unrelated catalog/rendering changes are excluded. Do not copy all release files over `alex-main`.
- [ ] Read the engine save-provider implementations at the resulting pinned revision. Confirm separate user data, working Windows asset-copy steps, and preserved free-will/config initialization. Run `dotnet --info`, `dotnet restore Client/Simitone/Simitone.sln`, and `dotnet build Client/Simitone/Simitone.Windows/Simitone.Windows.csproj -c Release`. Record the exact SDK and commands, including any required existing content-copy steps. Exit 0 is build success; startup is a separate case. Do not install a new SDK or change target frameworks merely to avoid a missing execution dependency.
- [ ] Reuse any existing runnable regression mechanism found in the chosen baseline. Otherwise defer check scaffolding until the options persistence task adds its first real check; that task defines a two-file console executable with no added packages. Keep real-VM gameplay checks separate from checks that can run without game assets.
- [ ] On Windows, choose the available legitimate edition, back up existing Simitone data, and use a disposable copy in the existing separate save location. Record SHA-256 hashes of original game saves before and after. Run B01 initial load; B02 change a placed object and save; B03 close/restart and verify membership, home number, budget, relationship, inventory, and object state; B04 repeat twice with no duplication; B05 open an older copied Simitone save; B06 return/quit prompts with Yes/No/Cancel. Mark each working/broken/missing/unverified with exact observations. No assets/runner means these cases remain unverified.
- [ ] Commit baseline imports and evidence separately from later feature fixes: `git commit -m "build: establish everyday gameplay baseline"`. If the Windows baseline does not build or run, fix and document that before proceeding. A required broad persistence rewrite returns to design review.

## Task 2: Save/exit decision and unsupported-away guard

**Interfaces:** Change the existing `TS1GameScreen.Save()` return type from `void` to `bool`; true means the existing save operation completed. Add `public bool CanSave => vm != null && !Downtown`. Keep all save logic in this method; no SaveChoice enum, delegate coordinator, or compatibility wrapper. Grep every caller before changing it, including any selected PR cleanup code.

- [ ] Extend B06 into a precise failing reproduction: denied save-and-quit must show an error and keep the same live lot open; successful save-and-quit leaves only after saving. Cancel invokes neither save nor leave; Discard invokes leave without save. Run the current Windows executable against disposable copied data and record the failing outcome before changing it. This checks the actual UI/VM path, not a newly invented callback helper.

```csharp
// Intended change in each existing save-and-leave handler:
if (!Save()) return;
GameFacade.Game.Exit(); // ReturnToNeighbourhood uses ExitLot instead.
```

- [ ] In `Save()`, check CanSave before mutation/write, use the existing serializer, return true at completion, and catch expected I/O/access failures to show a modal error and return false. Guard save-and-return/quit callers as shown; standalone Save may ignore the result because Save reports its own error. Cancel must clear the active prompt reference and permit reopening. While away, disable direct Save and remove the Save choice from close/return prompts; offer discard/cancel with explicit text that the trip cannot be saved. Keep return-home travel available. An unexpected serialization exception must never result in leave or a success message.
- [ ] Run `dotnet build Client/Simitone/Simitone.Windows/Simitone.Windows.csproj -c Release` and B06. B07: deny the save destination, select save-and-quit, verify error and the same lot remains open; recover permissions and save successfully. B08: while on a community lot, every Save entry is blocked and no lot/neighborhood file changes; Cancel leaves the trip running. Check `IniConfig.Save` is not confused with lot-save success.
- [ ] Commit only this flow fix and regression evidence: `git commit -m "fix: keep live lots open when saving fails"`. Document any B01–B05 serializer failure separately with a reproduction and focused regression before changing the responsible engine code. Partial multi-file write recovery is not solved by returning a bool; failures suggesting corruption block release and trigger the spec's persistence-scope review.
