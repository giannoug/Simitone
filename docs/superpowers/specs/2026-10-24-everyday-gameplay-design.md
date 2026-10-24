# Simitone: Everyday Gameplay — Release 1 Design

Date: 24 October 2026
Status: Approved by the user on 24 October 2026. Detailed implementation planning follows; execution method has not yet been selected.

## Intent and agreed choices

Make everyday Sims 1 play more reliable by completing a small set of missing or broken features. The user selected everyday gameplay over Superstar or vacations, the community fork over the original repository, and Windows as the first release target.

This is a desktop gameplay release. Keep Linux compatibility where practical, but Windows gameplay validation is the release requirement. Browser support, Superstar progression, vacation saving, accurate autonomy reconstruction, and complete expansion compatibility are later projects.

## Approaches

1. **Community fork — selected.** Reuse Alex's existing improvements and review outstanding patches. Less duplicate work, but inherited fixes and branch differences need verification.
2. **Original repository with selected patches.** A smaller starting point, with more patch integration and fewer existing improvements. The user did not choose this approach.

The inspected community release branch is `simitone-forked`, commit `e36e83f95d44b9f4b378e7665c4e2ecbaa88b4ab`. Its FreeSO submodule points to Alex's FreeSO fork. The contributor README directs new work toward `alex-main` rather than the release aggregation branch. Builder decision: compare and pin both branches and the engine submodule during baseline work; use a feature branch based on `alex-main` for contributions, bringing across only necessary, reviewed release-branch fixes. Record that resulting baseline before implementing features. No remote push or PR creation is part of this design task.

## What exists today

- `UIMainPanel.cs` provides save, return-to-neighborhood, quit, and a free-will toggle. A complete settings screen should not be assumed present.
- `TS1GameScreen.cs` owns lot transitions, an in-memory `SavedLot`, eviction, and lot/neighborhood saving. It is central to the proposed changes.
- Eviction currently updates family budget and calls `Neighborhood.MoveOut`; the README still describes eviction as unsupported. This is a verification target, not proof of one particular root cause.
- Open upstream PR #68 contains UI, lot visit, options/localization, and eviction-related work. It is unmerged and must be inspected per commit; its claims are not test evidence.
- The roadmap includes both missing functionality and unverified gameplay. Classify each selected item as working, broken, missing, or unverified before writing replacement code.

## Proposed release scope and completion criteria

The following details were proposed by the assistant and included in the design approved by the user.

### 1. Reproducible baseline and save checks

Pin the Simitone revision, FreeSO revision, SDK, build command, and test installation. Build the Windows client before changing behavior. Maintain separate, disposable copies of test neighborhoods. Original Sims installation saves remain untouched; keep the fork's separate save location.

Baseline acceptance: open a residential family, change its state, save, close the application, restart, and confirm family membership, home lot, budget, relationships, and placed objects persist. Repeating save/reload must not duplicate objects or household members. Verify existing save prompts for return and quit, including cancel and discard.

Save handling is a verification and regression workstream, not a proposal to redesign the entire save format. Any reproducible loss or corruption within the selected workflows blocks release. If solving it requires a broader persistence redesign, document the finding and revise this design before expanding scope.

### 2. Eviction and move-in

Evict a household through the neighborhood interface. The family remains available for move-in, its house assignment is cleared, and the vacated lot has no stale resident instances. Move that family back into a valid residential lot and play it successfully. Cancel must leave the household and lot unchanged.

Family funds and lot valuation follow the original Sims behavior for the tested edition. The builder must establish the expected result from a controlled original-game comparison or reliable existing implementation before changing money calculations; preserve inventory and household membership. Repeated eviction/move-in and a full restart must not duplicate funds or Sims.

Bulldozing, rezoning, creating neighborhoods, and a new neighborhood manager are outside Release 1.

### 3. Ordinary community lot visits

Complete the existing normal travel flow for Downtown and Old Town community lots supported by the test edition: choose a destination, arrive with the intended Sim or group, and return home. The home lot and household state survive the round trip; funds, inventory, relationships, selection, and neighborhood context remain consistent with the original game's travel rules.

Test destination selection, cancellation, single-Sim and family/group travel where the original game allows it, repeated trips, and home save/reload after return. Do not add new residential visiting or open-world behavior. Vacation, Studio Town, and Magic Town expansion-specific travel and saving away from home are excluded. Reject or disable unsupported save-away actions with a clear message rather than claiming they work.

Editing community lots is a separate backlog item unless a defect in that code directly blocks the selected travel workflow.

### 4. Useful options and essential UI repairs

Reuse and verify the existing free-will control. Add a settings panel for existing engine audio volume controls and display size/window mode settings that the Windows client can support. Provide Apply and Cancel; persist accepted settings across restart. Clearly label changes requiring restart. Do not expose unsupported settings. Do not reimplement the free-will algorithm.

Reuse original game strings where available; add fallback text for new controls. Full translation coverage is not a release requirement. Preserve existing save, neighborhood, and quit actions. Ensure modal dialogs block clicks underneath and cancel leaves state unchanged. Essential UI fixes are limited to controls needed for the selected workflows; no general redesign, weather, or unrelated graphics work.

## Technical boundaries

Keep Simitone responsible for UI and travel coordination, and FreeSO responsible for simulation, neighborhood data, and engine persistence. Inspect the pinned engine implementation before placing fixes in the client. Make the smallest change at the correct layer; record both repository revisions if an engine change is necessary. Avoid a blanket merge of PR #68 or unrelated fork branches.

Use the existing configuration system for settings. Keep current save formats where possible. If an unavoidable format change appears, stop and revise the compatibility design before implementation. Existing copied Simitone saves should continue to load; no promise of exporting them back to the original game.

## Delivery order

1. Baseline: branch comparison, gap inventory, build instructions, Windows build, and save/reload checks.
2. Eviction/move-in: reviewed patch reuse, missing corrections, and persistence checks.
3. Lot visits: destination UI and round-trip state checks.
4. Options: supported controls, persistence, and essential dialog repairs.
5. Release validation: run the complete selected workflow matrix and produce an exact known-limitations list.

Each milestone should be independently reviewable. If a feature already passes the criteria, keep it and add meaningful regression coverage rather than rewriting it. Do not estimate the later milestones precisely until baseline and reproduction results exist.

## Validation and available resources

Use focused automated regression tests for reproduced state bugs, plus manual Windows gameplay tests for rendering, input, dialogs, travel, and persistence. A successful build alone is not gameplay validation. Test 2D as the baseline and smoke-test existing 3D for the changed UI and flows; rebuilding the renderer is not in scope.

The fork documents Complete Collection and Legacy Collection. Proposed validation policy: nominate whichever legitimate edition is available as the initial test edition, record it explicitly, and claim compatibility only for editions actually tested. Both editions are desirable, not an implicit requirement to procure both. Game assets are never committed or redistributed. Runtime game files and a Windows test machine have not been supplied or verified in this session. Their availability is an execution dependency, not an assumption of completed tests. Planning and code inspection can proceed; a release cannot be called verified until Windows gameplay evidence exists. If no Windows runner or tester is available, deliver code and a pending manual test checklist, not a claimed finished release.

For each fix record reproduction steps, expected and actual results, relevant revisions, automated checks, and manual check status. Release blockers: save corruption, household loss/duplication, incorrect money transfer, reproducible crashes, or broken primary controls in the scoped workflows.

## Decisions left to the builder

Exact class extraction, test framework, fixture construction, control layout within the existing UI, supported volume channels, and patch selection are implementation decisions. Confirm available settings against the pinned engine. Prefer existing project patterns. Unknown original-game behavior must be investigated and recorded, not guessed. If investigation changes user-visible scope, return for a design update.

## Sources inspected

- Community fork README: https://github.com/alexjyong/Simitone/blob/simitone-forked/README.md
- Contribution branch: https://github.com/alexjyong/Simitone/tree/alex-main
- Community client: `Client/Simitone/Simitone.Client/UI/Screens/TS1GameScreen.cs` and `UI/Panels/UIMainPanel.cs` at the pinned release revision.
- Existing candidate work: https://github.com/riperiperi/Simitone/pull/68
- Original roadmap: https://github.com/riperiperi/Simitone/issues/8

## Review

This document is a proposed release design, not a completed implementation plan. After user approval, use the writing-plans workflow to produce concrete tasks, exact files, dependencies, and verification steps. No product code has been changed.
