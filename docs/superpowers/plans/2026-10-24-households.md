# Household Moves Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Evict and move households without stale residents, missing members, or repeated money transfers.

**Architecture:** Use the existing neighborhood provider for family assignment and persistence. Clean stale lot residents through the simulation's normal entity-deletion lifecycle before completing an eviction, adapting only the relevant candidate patch.

**Tech Stack:** Existing C#/FreeSO VM, FAMI, IFF/FSOV; existing regression checks and explicit real-VM cases.

**Spec:** [Approved design](../specs/2026-10-24-everyday-gameplay-design.md). Requires [baseline plan](2026-10-24-baseline-and-saves.md).

## Global Constraints

- Windows gameplay validation is the release requirement.
- Family funds and lot valuation follow the original Sims behavior for the tested edition.
- Preserve inventory and household membership.
- Bulldozing, rezoning, creating neighborhoods, and a new neighborhood manager are outside Release 1.
- Game assets are never committed or redistributed.

## Review Focus

- Duplicate eviction requests must not credit cash twice (Task 2).
- Pets and mixed adult/child households retain their membership (Tasks 1–2).
- A missing/mismatched family or occupied destination must fail before writes (Task 2).
- Cancelled confirmation must change neither family nor lot (Task 2).
- Cleanup failure must not proceed to move-out or show success (Task 2).

## File map

- `Client/Simitone/Simitone.Client/UI/Screens/TS1GameScreen.cs`: `EvictLot`, `MoveInAndPlay`, private lot-cleanup operation.
- `Client/Simitone/Simitone.Client/UI/Panels/UIHouseSelectPanel.cs`: confirmation, single-operation guard, failure feedback.
- `FreeSO/TSOClient/tso.content/TS1/TS1NeighbourProvider.cs`: `MoveOut`, `SetFamilyForHouse`, `SaveNeighbourhood`; modify only for a demonstrated provider defect.
- `FreeSO/TSOClient/tso.simantics/Model/TS1Platform/VMTS1LotState.cs`: resident cleanup belongs here if a reusable engine operation is required.
- `docs/testing/household-reference.md`: observed original-game financial/object behavior.
- `docs/testing/everyday-gameplay-matrix.md`: H01–H07, the required real-VM regression tests; record expected state before making changes.

## Task 1: Pin the behavioral oracle

**Interfaces:** Consumes B01–B06 and the pinned provider. Produces reference cases H01–H03 with concrete starting funds, sale proceeds, object state, purchase charge, family GUIDs, and inventory from the tested edition. These observed values, not guessed formulas, are Task 2's assertions.

- [ ] Review candidate commit `0602ec272c881c4e64977cf4c540257a45b165b4` and its engine dependency separately. Its cleanup of all avatars and its money/object TODO are hypotheses, not the final behavior. Trace `MoveOut(short)`, `SetFamilyForHouse(short,FAMI,bool)`, `VM.RemoveEntity`, entity deletion, and save serialization before choosing cleanup code.
- [ ] Run H01 in the original game on disposable data: evict one occupied residential lot; record original family funds, architecture/object values, resulting family funds, remaining lot objects, house assignment and family list. Run H02 move-in to an empty lot and H03 with an adult, child, and pet. Record the precise arithmetic and inventory changes in `household-reference.md`. A trusted equivalent implementation may replace the original-game comparison only with the exact source revision and code path documented. Without either oracle, do not invent sale-credit behavior; mark this task blocked while other independent planning/tests proceed.
- [ ] Repeat H01–H03 in unmodified Simitone. Assert observed state equals the reference; save/restart and assert again. Record the actual failing step and source revision. If all pass, retain behavior and this regression script; do not rewrite the feature.
- [ ] Commit the reference and regression cases: `git commit -m "test: document household move reference behavior"`.

## Task 2: Fix the reproduced household gaps

**Interfaces:** Preserve `public void EvictLot(FAMI family, short houseID)` and `public void MoveInAndPlay(short house, int family, UIElement switcher)`. Add a private `bool TryCleanUpPeople(short houseID)` only if needed by H01; true means resident cleanup was serialized successfully. Keep normal provider assignment APIs. Do not expose a second source of family membership.

- [ ] Extend H04: double-click/submit eviction twice and verify exactly one credit and one move-out; H05: cancel eviction/move-in and compare every recorded field/file to before; H06: stale family reference, empty source and occupied destination must cause no budget/assignment mutation; H07: interrupt cleanup with a denied write and verify no subsequent move-out/move-in success. Use the actual VM and disposable files, not mocks of a newly invented household store. First demonstrate the failing case in the current build.
- [ ] Adapt only required cleanup/confirmation hunks from the candidate. Validate family identity and source occupancy before charging/crediting; revalidate at confirmation time. Guard in-progress operations. Use the appropriate normal VM deletion lifecycle so avatar queries, threads, and serialized entities agree. Never merely remove UI avatars or clear family GUIDs. Keep lot objects and NPC outcomes consistent with H01, not an unconditional deletion rule inferred from the PR title.
- [ ] Order cleanup/persistence and family updates so a failed prerequisite prevents later mutation; preserve a recoverable original copied save during the operation. If safe multi-file updates require a general transaction redesign, stop and revisit persistence scope rather than shipping a partial solution. If the fix belongs in FreeSO, commit there first and update the parent submodule pointer; do not put a generic engine cleanup extension in a UI file.
- [ ] Run H01–H07, then repeat eviction → move-in → save → restart twice. Assert exact reference funds, unchanged GUID set and inventory, no old residents on the vacated lot, and exactly one instance per household member after move-in. Build the Windows project and rerun the baseline cases. If the package-free checks already exist, run `dotnet run --project Tests/Simitone.Checks/Simitone.Checks.csproj -c Release` too.
- [ ] Commit engine and client fixes separately where applicable, then evidence: `git commit -m "fix: preserve household state across eviction and move-in"`. With missing assets/Windows evidence, code may be reviewed but the milestone remains gameplay-unverified.
