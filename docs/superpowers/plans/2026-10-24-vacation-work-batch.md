# Vacation Work Batch Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Deliver independently verified fixes and a Windows test package/guide.
**Architecture:** Reuse existing provider/screen/checker, run shared edits sequentially, publish engine before parent. Per-file protection is not a multi-file transaction.
**Tech Stack:** C#/.NET 9, MonoGame, PowerShell 7, GitHub Actions, existing package-free checks.
**Spec:** ../specs/2026-10-24-vacation-work-batch-design.md (approved).

## Global Constraints

No new framework/store/transaction/travel-session abstraction, guessed game money/cleanup, default-branch changes, force pushes, upstream PRs or releases. No original assets or saves in uploads. Windows gameplay remains a release gate. User authorized Native execution without routine check-ins.

## Review Focus

Failed neighborhood writes retain dirty avatars; failed/mismatched travel does not consume the snapshot or retry each frame; failed Apply retains an earlier display tuple; copy rejects overlap/links; packaging rejects missing runtime/backend/content. Tests below own these cases.

### Task 1: Baseline/provenance

**Files:** docs/testing/release-1-results.md, git metadata.
**Interfaces:** published parent1409435/enginee417cd4, source-equivalent local checkpoint.
- [ ] Verify isolated feature worktree and recursive dependencies; preserve unrelated files. Reconcile uploaded/local revision descriptions.
- [ ] Run existing checker (eight PASS groups; Windows lock skip on Linux). Record actual results and commit documents.

### Task 2: Windows checks/package

**Files:** .github/workflows/build.yml; scripts/{Package-Windows,Check-WindowsPackage}.ps1; Tests/Check-WindowsPackage.ps1; Windows csproj.
**Interfaces:** package script takes -OutputDirectory and Release/Debug -Configuration; publish win-x64/self-contained, ZIP, SHA256 and build-info.json. Checker takes -PackageDirectory and throws for missing runtime/backend/content or user data.
- [ ] Write/run package fixture test: missing checker/empty package fails, required fixture passes, missing coreclr/backend/content and UserData fail.
- [ ] Implement checker and publish script; inspect real output. Resolve exact duplicate publish sources and dynamic backend copy rules. Keep legacy dependency diagnostics visible; do not disable arbitrary build errors.
- [ ] Enable push checks on feat/everyday-gameplay and manual platform choices. Restore client graph, run native production/script checks, upload ZIP/hash and logs on failure.
- [ ] Validate syntax, actual publish and package; commit. Publish workflow early where possible and inspect Actions status or record exact enablement/access prerequisite.

### Task 3: Individual save preservation

**Files:** FreeSO/TSOClient/tso.content/TS1/TS1NeighbourProvider.cs; Tests/Simitone.Checks/ProviderChecks.cs; Program.cs.
**Interfaces:** preserve bool SaveHouse(int,IffFile), bool SaveNeighbourhood(bool), void SaveNewNeighbour(GameObject); private static void WriteIff(string,IffFile) reused by actual callers.
- [ ] Add real failing IffChunk serializer fixture; run to observe old House01.iff truncation. Also check Neighborhood.iff, temp cleanup, STR roundtrip and conditional Windows locks.
- [ ] Serialize to same-directory CreateNew temporary file, dispose then File.Move overwrite, finally cleanup. Clear DirtyAvatars only after required neighborhood writes succeed.
- [ ] Run full checker and relevant build; commit engine then parent pointer/checks. Document cross-file limitations.

### Task 4: Home travel guards

**Files:** TS1GameScreen.cs; Tests/Simitone.Checks/TravelChecks.cs.
**Interfaces:** retain SavedLot, add SavedHomeLot/SavedHomeUserPath; private bool TrySwitchLot(out string) called by Update consumes queued request and validates before world teardown.
- [ ] Trace actual engine transfer and destructive loader. Add actual queued-operation checks: wrong folder/home, missing destination, missing live home, repeated processing. Observe missing validation failure.
- [ ] Capture first home only, reject wrong ownership before load, consume requests once, clear after successful restore or explicit exit. Reject missing destination before capture/world work; preserve transfer rules and restore prior speed after prevalidation rejection.
- [ ] Keep unproved exception catch/retry/world rollback deferred. Run complete checker; commit bounded fixes and pending real-game evidence.

### Task 5: Household/options evidence

**Files:** checker Program.cs, UI screen/dialog only for demonstrated defects, docs/testing/household-reference.md.
- [ ] Exercise invalid family IDs, stale/occupied requests, options pending display on reopen and failed Apply. First-green checks document retained behavior; do not force a fake red.
- [ ] Fix only reproduced shared-path defects RED→GREEN. No guessed money or resident deletion.
- [ ] Record unknown financial/member/inventory/lot reference fields and edition/revision; run checker and commit evidence.

### Task 6: Safe test kit

**Files:** scripts/Test-SaveCopy.ps1; Tests/Test-SaveCopy.ps1; docs/testing/{windows-test-guide,test-results-template}.md.
**Interfaces:** -SourceDirectory/-DestinationDirectory creates a NEW copy and source-hashes.json; -VerifyManifest compares unchanged source. No automatic upload or silent game redirection.
- [ ] Write/run tests: missing helper fails; valid copy/hash, equal/ancestor/descendant/existing destinations, source change and nested link/junction rejection; empty directories retained.
- [ ] Implement standard filesystem/hash operations, reject before writes, run on Linux PowerShell and Windows Actions. Explain fixed save root and separate Windows test account for existing data.
- [ ] Write quick/full checklist, launch/log/revision instructions and results form; include in package; commit.

### Task 7: Review/publication

**Files:** docs/testing/vacation-work-results.md; README links.
- [ ] Run checker/scripts, Windows publish/package and retained Desktop build. Inspect file set; do not claim native gameplay without launch evidence.
- [ ] Dispatch fresh whole-branch reviewer. Fix material findings with RED→GREEN checks and affected suites; ledger decisions/minors.
- [ ] Publish engine then parent to existing personal branches, verify source tree and gitlink, inspect Windows Actions/run/artifact or exact external prerequisite. Do not force or merge default branches.
- [ ] Report completed/pending/blocked tasks and one test guide/package. Preserve useful work remotely before ending.

## Self-review

All eight approved batches map to tasks1–7. Tasks2/6 share kit/package paths; final packaging includes the kit. Tasks3–5 share provider/screen and are sequential. Source-only checks do not count as gameplay. This recovered plan retains the approved scope and Native method.
