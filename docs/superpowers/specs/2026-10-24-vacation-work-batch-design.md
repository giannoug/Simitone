# Vacation work batch design

Approved by the user on 2026-10-24, followed by explicit instructions to proceed. Recovered after workspace maintenance on 2026-10-25; scope and decisions are unchanged.

## Intent and approach

Prepare as much independently verifiable work as possible before the user can test on Windows. Finish the current saves, households, ordinary visits and options workflows, then address evidence-backed defects in those same flows. Prefer this over broad expansion features or browser implementation, which add behavior that cannot yet be checked. Deliver a Windows test package, results and an ordered guide, without claiming a validated gameplay release.

Published baseline: giannoug/Simitone feat/everyday-gameplay at 140943564d7f7cc1cec1541373672db8483c366e; giannoug/FreeSO feat/simitone-everyday-gameplay at e417cd456acffbf1560c854d236460e1f2b2d75a. The published commits consolidate the previously tested local checkpoint. Eight package-free check groups passed; native Windows/gameplay remains untested.

## Constraints

Retain C#/.NET 9/MonoGame, existing provider/serializer/screen and package-free checks. No new frameworks, save transaction service, household store or travel-session model. Publish only to the user's existing feature branches, engine first; no default branch changes, force pushes, upstream PRs or releases. Do not redistribute original game assets or saves. Do not upload user data automatically.

The user delegated ordinary implementation decisions and the amount of safely verifiable work. Implement source-level fixes with real production-path regression evidence before Windows testing, but do not count them as original-game or rendered gameplay results. Original financial, NPC/resident cleanup, pet/group and inventory semantics still require a trustworthy original-game reference. No guessed accounting or blanket avatar deletion.

## Ordered batches

1. Reconcile local/published provenance and dependency availability; preserve unrelated files.
2. Enable feature-branch Windows Actions checks plus manual builds. Execute real checks natively, including exclusive file locking. Keep manual other-platform builds available. If fork Actions need enablement outside available tools, record that external prerequisite and continue.
3. Publish Windows x64 self-contained ZIP through existing .NET apphost, not a custom installer. Inspect dynamic MonoGame loader and content rules. Include backend/runtime/content files, revision manifest, checksums and test kit. Reject incomplete packages; exclude saves/config/logs/original assets. Use workflow artifacts, not releases. Static checks are not a game launch.
4. Protect individual IFF writes from truncation by same-directory temporary serialization and replacement. Cover real provider failures and round trips. SaveHouse then SaveNeighbourhood spans several files; per-file protection is not cross-file atomicity. Preserve dirty edits until required writes succeed. Stop short of a general recovery redesign.
5. Validate first home snapshot ownership, queue consumption and missing destinations before destructive world initialization. Keep SavedLot and home lot/UserPath fields in TS1GameScreen; clear only on successful restore or explicit discard. Retain existing transfer rules. InitializeLot destroys the old world first, so retaining a snapshot is not proof that retry/rollback works; larger recovery changes stay deferred without real-flow evidence.
6. Extend household boundary/cancel/stale checks and options pending-display/reopen/failure/input checks. Fix only demonstrated defects at shared entry points. Preserve money and membership rules; provide a blank reference worksheet for unknown original-game outcomes. Rendered/DPI tests stay pending.
7. Supply PowerShell helper for explicit disposable copy and unchanged-source hashes, plus quick/full Windows checklists and a results form. Reject overlapping/existing destinations and links/reparse points before writing; never overwrite/move/delete original saves. Preserve directory structure, including empty folders. The helper must not silently redirect the game. Explain the application's actual fixed Documents/Simitone save root and use a separate Windows test account if existing Simitone data needs isolation.
8. Run appropriate checks, independent whole-branch review, fix material findings, publish engine then parent, verify source hashes, report actual Actions/package status and remaining user tests. Stop once useful verifiable work is exhausted.

## Delegated decisions and acceptance

The builder chooses exact regression cases, ordinary layout details, necessary private helpers, publish flags after inspection and commit division. Execute shared screen/provider edits sequentially. Reduce a subtask when evidence cannot support it rather than expanding the scope. The user should receive one revision/package, a quick checklist and full remaining tests. Work proceeds in active sessions; unattended execution across days is not promised.
