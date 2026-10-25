# Everyday gameplay implementation checkpoint

Historical results recorded before publication. See vacation-work-results.md for the later batch. Published baseline: parent 1409435 and engine e417cd4; these preserve the earlier tested trees with consolidated upload history.

Date: 2026-10-24. Status: **implementation checkpoint, not a validated release**.

The approved Windows gameplay release gate has not run. There is no original-game installation, disposable save fixture, or Windows runtime in this workspace. All B01–B08, H01–H07, T01–T06 and O01–O06 manual matrix cases remain NOT RUN. No gameplay, edition compatibility, rendered layout, DPI, source-save hash, package launch, inventory, relationship or money round-trip result is implied by the checks below.

## Implemented

- Selected community startup/installation, stock-lot, free-will, DPI and neighborhood-return fixes; no blanket release merge.
- Config saves serialize to a temporary file before replacement, return errors to new callers, and preserve the prior file after serialization failure. Existing Save callers remain compatible.
- Lot saves reject missing lots and away-from-home sessions. Save-and-return/quit stop if saving fails; away exit prompts describe discarding the trip.
- Stale eviction and invalid/occupied household move-in requests are rejected before changing money or membership. Original sale arithmetic is retained.
- Audio/display options use a detached draft, validate before applying, roll back on write failure, and leave Cancel without side effects. Audio applies on successful persistence. Display settings apply at restart, survive later config saves, and do not change live renderer dimensions. Audio-only Apply leaves ordinary resize persistence working. Speed controls ignore number entry while options remain open.
- Ordinary Downtown/Old Town travel uses the existing map in visit-only mode: no neighborhood management panel; Old Town requires community zoning. Unmapped/residential destinations are rejected. Cancel/Escape and repeated selection complete once. Teardown restores prior controls and sends the response for the originating actor, using ForwardCommand because SendCommand overwrites ActorUID.
- Windows CI now runs the package-free checks before artifact upload. CI itself has not run remotely.

## Revisions

Parent code: `128cfee7f7d11201e464d8f61f49272f0c62fa53` on `feat/everyday-gameplay`, based on `95fc53ee123389dcd3226757acdad15ce8bfda77`.
Engine: `39c2bcb5fe0a8bec509ccf23b9b2f58b226afb16` on `feat/simitone-everyday-gameplay`, based on community engine `4ec98c574aae8243f8c83d014ae18115452d1c51`. Both engine commits must accompany the parent; they have not been pushed.

## Automated evidence

SDK 9.0.319, Linux x64. The package-free console executable exercises the real config classes, real household provider/family data, real selector/command completion and options Apply logic. Test-only reflection bypasses asset/render constructors; it is not gameplay coverage. A recording network driver observes commands at the existing VM boundary.

RED failures observed before fixes: failed serialization truncated the old config; unsupported Save accessed missing VM data; stale eviction credited funds before failing; travel selection opened the management panel; SendCommand replaced the original dialog actor; audio-only Apply froze resize persistence; options speed input reached sound/simulation code without a modal guard.

Windows Release cross-build: exit 0, 0 errors, 669 warnings (compatibility/analyzer/Windows DPI diagnostics). Windows executable not launched.

Desktop Release build: exit 0, 0 errors, 792 warnings after restoring its dependencies. Linux gameplay was not launched.

Latest check execution: **8 PASS groups, 1 PENDING**, exit 0:

- Exact config round trip and preservation of unrelated values.
- Serialization-failure preservation and temporary-file cleanup.
- Invalid destinations report failure without breaking legacy Save callers.
- No-lot and away saves stop before accessing/writing game state.
- Travel cancellation restores visibility and submits once for the original caller.
- Visit selector rejects residential/unmapped lots and completes once.
- Stale eviction and invalid move-in preserve money, membership and occupancy.
- Options draft validation, successful persistence, failure rollback, restart-only display handling, audio-only resize persistence and input isolation.

**PENDING:** Windows exclusive-file-lock replacement test (automatically runs on Windows).

Reproduce with .NET 9: build Tests/Simitone.Checks/Simitone.Checks.csproj in Release, then run its net9.0 executable. On this Linux host builds require EnableWindowsTargeting=true, single-node MSBuild (-m:1 -nr:false). Full build logs remain with the execution ledger.

## Remaining work

- Full eviction proceeds/cleanup and repeated move-in/eviction need the approved original-game behavioral reference. No new money formula or resident/NPC deletion was guessed.
- Home snapshot ownership/lifetime and failed-restore handling remain unchanged. The approved plan requires real VM trip/restore failure evidence; synthetic identity tests cannot establish it. The imported region-return fix alone does not prove a round trip.
- Windows startup/path selection, modal controls, 800×600 and 1920×1080 layouts, 150% DPI, audio playback, restart behavior, trip cancellation/unpause and rendered 2D/3D checks need Windows and lawful game assets.
- Explicit Windows -path overrides now update both runtime paths. Detected installations use locator edition data; unknown custom paths follow the existing selector's non-Steam default. Configure a custom Legacy installation through a supported detected path before claiming compatibility.
- No Windows package has been launched, no publishing/push/merge occurred. Browser work, Superstar, vacation saves and expanded compatibility remain excluded.

Independent whole-branch review found three Important issues (explicit-path precedence, numeric input/speed interaction, audio-only display persistence); all were fixed and rechecked by the reviewer. No Critical or remaining Important issue was reported in the inspected fixes. This does not replace gameplay validation.

## Execution decisions

- Continue asset-free implementation after the user authorized tool installation and proceeding; cost: Windows/rendered compatibility remains unverified.
- Four Eto aliases resolve imported Windows startup namespace conflicts; runtime dialogs still need Windows validation.
- Keep restart display choices in one pending field with a serialization hook because existing renderers read live config dimensions; cost: this engine change must accompany the parent.
- Reuse the existing neighborhood selector with an optional visit-only mode; cost: permitted map selection submits directly, without a separate Visit confirmation.
- Do not invent eviction accounting or certify snapshot restoration without the plan's original-game/real-VM evidence; cost: those features remain incomplete.
