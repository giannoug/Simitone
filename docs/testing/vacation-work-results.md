# Vacation work batch results

Date: 2026-10-25 (Europe/Budapest). Status: development test checkpoint; manual Windows gameplay remains NOT RUN.

## Where the work is

- Parent branch: https://github.com/giannoug/Simitone/tree/feat/everyday-gameplay
- Tested code upload: `494fc9f3c1f245ec05ecabf65c48d884da4f44d1`.
- Engine branch: https://github.com/giannoug/FreeSO/tree/feat/simitone-everyday-gameplay
- Engine upload: `62ebb08149aaf65375ba2aa1dd5c3f627771d718`.
- Current Windows workflow run: https://github.com/giannoug/Simitone/actions/runs/40179106884
- User instructions: [Windows test guide](windows-test-guide.md), [result form](test-results-template.md), [full matrix](everyday-gameplay-matrix.md), [original-game reference worksheet](household-reference.md).

The GitHub connector creates consolidated upload commits. The local source checkpoint `089e4de0a685246222aa099d69192c71d8d379d0` and engine `e360d1555b567b7417449dcf9c73fda21f5d506b` have the same file contents as the code uploads above, apart from the parent gitlink identifying the equivalent uploaded engine commit. Tree hashes were checked during upload. Default branches, upstream PRs and releases were left unchanged.

## Completed work

| Batch | Result |
|---|---|
| Reproducible baseline | Restored published source after workspace maintenance; rebuilt dependencies and baseline checks. Approved plan/design are saved on GitHub. |
| Windows automation | Feature-branch push now starts Windows build, actual C# checks, script tests and packaging. Manual Linux/macOS options remain available. Build/check/package logs upload even on failure. |
| Windows package | Self-contained win-x64 publish, both dynamic MonoGame backends, runtime/content validation, revision manifest, ZIP and SHA256 manifest. Exact duplicate non-client publish metadata/frontend texture resolved. |
| Save protection | Shared real-provider write path serializes complete IFF into a same-directory temporary file before replacing its destination. Failed neighborhood persistence retains pending avatar edits. Existing bool/exception contracts retained. |
| Travel guards | First home snapshot retains lot/folder ownership. Wrong ownership or missing destination fails before world teardown. Requests are consumed once, first snapshot is not replaced, and prevalidation rejection restores the prior speed. |
| Household/options checks | Invalid family ID boundaries, existing stale/occupied guards, pending display reopen and failed Apply preservation covered; retained behavior passes. Unknown game money/cleanup outcomes remain blank in reference worksheet. |
| Test kit | Explicit disposable copy, unchanged-source hashes, overlap/existing-output/link rejection, empty-directory preservation, quick/full guides and result form. No upload of user data. |
| Independent review | No confirmed Critical/Important issues in source review. Two minor items below remain deferred. |

## Automated verification

Environment: Linux x64, .NET SDK9.0.319, PowerShell7.5.3. No original Sims installation or save assets were supplied. Checks exercise real production classes, bypassing only render/asset initialization in test fixtures; they are not game sessions.

- Eight baseline C# check groups passed after restore.
- Ten extended C# check groups pass: config roundtrip/failure/invalid destination; unsupported saves; original-actor once-only travel cancel; destination filtering; household stale/boundary guards; options draft/apply/rollback/pending state; real provider IFF preservation/roundtrip; actual queued travel ownership/missing-destination/single consumption.
- Windows-only exclusive-file-lock checks are skipped on Linux and are included in the native Windows workflow. The missing-object diagnostics for fixture GUID7 are intentional real-provider lookups, not failed assertions.
- Package helper fixture tests pass for missing runtime/backend/content and prohibited user data.
- Save-copy tests pass for real copy and hashes, empty directories, same/ancestor/descendant/existing output, changed source, and nested symbolic-link rejection. Windows workflow uses a real junction for its equivalent case.
- RED failures observed: actual old IFF house truncation; missing queued ownership validation; earlier missing package/copy checks and empty-directory preservation. The real published tree also caught an incorrect expected graphics asset path; fixed checker/fixture then passed on actual output.
- Windows self-contained cross-publish: exit0, with inherited legacy dependency/analyzer warnings. Both actual package validation and archive integrity checks pass.
- Desktop Release build: exit0, 902 warnings, 0 errors. No Linux gameplay launch.

Native Windows run40179106884 also completed successfully: Windows build, eleven C# check groups (including exclusive file locks), both helper suites (including a Windows junction), self-contained publish and artifact upload. This does not establish game launch or gameplay compatibility.

## Locally verified package

The local package is an additional reproducible result, not a game-tested release. Prefer the GitHub artifact from the successful run for the remote revision because its build-info.json contains the remote SHAs.

- ZIP: Simitone-Windows-x64.zip, 362011392 bytes (about345MiB).
- SHA256: `061bee89e60a0582459288d93f33f41ba3798d1593b265fc0537521a8f6aca4b`.
- Local manifest revisions: parent089e4de0 / enginee360d155 (source equivalent to uploads above).
- ZIP CRC check passes for 2879 entries; file manifests verified without mismatches. Required backends, test kit and runtime are present; no UserData directory included.
- The kit does not include or upload original assets or saves. A self-contained runtime does not remove the need for a legitimate installed game and Windows graphics support.

## Pending user tests and limits

All manual B/H/T/O matrix cases remain NOT RUN: original save hashes, actual startup/asset selection, save/quit/reopen, repeated move-in/eviction and accounting, complete Downtown/Old Town/group/pet trips, return/restart state, inventory/relationships/motives, non-default neighborhoods, source compatibility, audio playback, resolution/restart, modal controls, 3D and150% DPI.

Save protection is per-file, not atomic across every house/character/neighborhood file. A later failure can leave earlier files replaced. The IFF serializer may also update in-memory chunk bookkeeping while serializing. Do not infer crash recovery from byte preservation of individual destination files.

InitializeLot destroys the old world before loading the next one. The new ownership/missing-file guards run before that, but an actual error after teardown can still end the session. No unproved retry or multi-world rollback was added. The original transfer rules remain unchanged.

The Windows application still uses Documents/Simitone. A copy elsewhere does not redirect it. Users with existing Simitone data should use a separate Windows test account; the guide does not ask them to overwrite or rename that data.

Unknown original-game eviction proceeds, object/NPC cleanup and membership semantics are deliberately not guessed. Browser implementation, Superstar, vacation saving and new expansion systems remain outside this batch.

## Decisions made

1. Retained approval/Native execution after maintenance, reconstructed documents from the conversation and reverified source. Cost: wording differs from the lost local draft; scope and decisions are unchanged.
2. Kept per-file save protection rather than introducing a broad transaction framework. Cost: cross-file partial updates remain possible.
3. Deferred larger world rollback/retry until it can be demonstrated with the real flow. Cost: post-teardown load failures may still end a session.
4. Kept inherited NU1605 explicit-RID diagnostics as warnings, clearing the SDK warning-as-error setting for packaging instead of changing the legacy dependency graph. Cost: compatibility warnings remain and native runtime testing is required.
5. Used the current fixed Windows save root and a separate test account for existing data. Cost: extra setup for users with existing saves; no untested save-root flag was introduced.

## Deferred minor review items

- The packaged full-pass guide points to the repository matrix; everyday-gameplay-matrix.md is not copied into the offline Testing directory.
- The checker does not explicitly require OpenAL/SharpDX files. Actual output contains openal.dll/SharpDX dependencies; the check could detect more artificially incomplete packages.

## Native Windows result

Run40179106884 for494fc9f completed successfully on2026-10-25. Job121126113800 passed every build/check/helper/package/upload step. Native logs confirm locked-file preservation, provider checks, queued travel checks and the copy helper's link/overlap cases; no C# cases were skipped on Windows.

Download from the run's **Artifacts** section: **Simitone-Windows-x64-494fc9f3c1f245ec05ecabf65c48d884da4f44d1** (artifact12593715455, 360622072bytes). GitHub artifact archive digest: `sha256:91284f4ff8f97083299ec3725878d8e76b6c48e1a143a14d61f53c402ec3a562`. Retained until2026-11-08. This outer artifact contains the inner package ZIP and its own .sha256 file; do not compare its hash with the separate locally built ZIP hash above.

Logs artifact12594065262 is available on the same run. The earlier run40178886407 uses the superseded graphics-check path; use40179106884 for the final package. All user gameplay matrix cases remain NOT RUN.
