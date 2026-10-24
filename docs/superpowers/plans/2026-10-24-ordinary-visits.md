# Ordinary Lot Visits Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Let Sims select a valid community destination, cancel safely, and return to their original home/neighborhood without losing live state.

**Architecture:** Preserve VM travel commands, the existing SavedLot field, and cross-lot motive/inventory handling. Correct snapshot lifetime in the existing screen and adapt the candidate visit panel without exposing residential visits.

**Tech Stack:** C#, MonoGame UI, `VMMarshal`, `VMNetDialogResponseCmd`, existing neighborhood zoning.

**Spec:** [Approved design](../specs/2026-10-24-everyday-gameplay-design.md). Requires the baseline and household plans; see [release order](2026-10-24-everyday-gameplay.md).

## Global Constraints

- Complete the existing normal travel flow for Downtown and Old Town community lots supported by the test edition.
- Do not add new residential visiting or open-world behavior.
- Vacation, Studio Town, and Magic Town expansion-specific travel and saving away from home are excluded.
- Windows gameplay validation is the release requirement.
- Game assets are never committed or redistributed.

## Review Focus

- Cancel/double selection sends exactly one response and restores controls (Task 1).
- A second destination cannot replace the original home snapshot (Task 2).
- A failed home restore keeps the snapshot available for retry (Task 2).
- Travel from a non-default neighborhood returns to that neighborhood (Task 2).
- Group travel, pets and relationship/inventory changes follow existing original-game rules (Task 2).

## File map

- `Client/Simitone/Simitone.Client/UI/Panels/Desktop/UITS1NeighborhoodVisitLotPanel.cs`: scoped destination view adapted from PR.
- `Client/Simitone/Simitone.Client/UI/Panels/UINeighbourhoodSelectionPanel.cs` and `UIHouseSelectPanel.cs`: travel-only exclusion of residential/editor controls.
- `Client/Simitone/Simitone.Client/UI/Panels/UILotControl.cs`: dialog actor, response-once, selection/cancel cleanup.
- `Client/Simitone/Simitone.Client/UI/Screens/TS1GameScreen.cs`: travel execution and restoring home view/context.
- `FreeSO/TSOClient/tso.simantics/Primitives/VMGenericTS1Call.cs` and `FreeSO/TSOClient/tso.content/TS1/TS1NeighbourProvider.cs`: inspect travel data; change only for reproduced defects.
- `docs/testing/everyday-gameplay-matrix.md`: T01–T06.

## Task 1: Destination dialog and cancellation

**Interfaces:** Retain `Action<int> OnHouseSelect` and `VMNetDialogResponseCmd` with positive destination → `ResponseCode=1`, cancel → `ResponseCode=0`, `ResponseText` as destination ID (0 for cancel). Preserve the originating actor for the dialog lifetime; do not read a newly selected active Sim at submission.

- [ ] Write T01: summon a cab to Downtown/Old Town, show only permitted community destinations with Visit/Cancel and no eviction/editor controls. T02: cancel and assert no travel, no file changes, usable original lot view and resumed intended simulation state. T03: double-click selection/cancel and verify one command and one panel teardown. Reproduce current failures before editing.
- [ ] Adapt commit `869257436854780f5044dfabf8b450267bead66d` and relevant selector hunks only. Provide explicit Cancel/Escape handling; record the origin actor and guard dialog completion. Use existing zoning/provider results; do not hardcode a universal lot ID range for Old Town. Restore background, frontend and lot-control visibility on every exit path; remove handlers with the panel. Keep unrelated expansion dialogs' behavior unchanged.
- [ ] Run T01–T03 on Windows in both supported destination types and at 150% DPI. Manual command evidence must identify dialog ID/actor/destination/response; use debug logging if necessary without creating a public debug UI. Re-run baseline tests and commit `git commit -m "fix: complete ordinary community travel dialogs"`.

## Task 2: Retain and restore the original home

**Interfaces:** Keep the private `VMMarshal SavedLot` and `ushort CurrentNeighborhoodMode` fields in `TS1GameScreen`. Add only `short SavedHomeLot` and `string SavedHomeUserPath` beside SavedLot to retain its ownership. The current region/view ID is distinct from `NeighSelectionMode` and from the provider's UserData folder. Do not introduce a public travel-session class or duplicate snapshot storage.

- [ ] Write and run T04: household from a non-default neighborhood goes out and returns; T05: permitted single/group trip, purchase/interact, return, save/restart, and compare cash, inventory, relationships and membership; T06: repeat trips twice, cancel a second destination, and simulate restore failure. Assert the first home snapshot is retained until a successful restore and there is no duplicate household spawn. A mismatched home/folder must fail without consuming SavedLot. Record the current build's real-VM failure, not just an identity-token test.
- [ ] In the existing queued switch block, capture SavedLot and its home/folder only on the first home departure. Restore it only when the target matches SavedHomeLot and the provider's UserPath matches SavedHomeUserPath; report mismatches without consuming the snapshot. Clear SavedLot and its ownership only after successful InitializeLot, or when explicitly discarding the live game. Keep CurrentNeighborhoodMode for the home view; do not reset the provider to neighborhood zero. Preserve the existing engine's transfer of acquired motives, relationships, inventory and family state. Selected avatar must belong to the permitted travelling group. Introduce a private helper only if a second real caller needs the same multi-line operation.
- [ ] Build the Windows project and rerun T04–T06 and B08. Require real save/restart comparisons, not just an in-memory snapshot check. If the package-free checks already exist, run `dotnet run --project Tests/Simitone.Checks/Simitone.Checks.csproj -c Release` too. 2D gameplay evidence is mandatory; 3D remains a smoke case.
- [ ] Commit the small existing-file changes and regression evidence: `git commit -m "fix: retain home state across community lot visits"`. Missing Windows/game assets means gameplay-unverified, never an implied pass.
