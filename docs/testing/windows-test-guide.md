# Windows test guide

This is an experimental test package. Automated checks do not prove gameplay compatibility. Use a legitimate game installation; no original game assets are included. Complete Collection is the baseline; Steam Legacy startup support is present but its gameplay compatibility is unverified.

## Download and open

Open https://github.com/giannoug/Simitone/actions, choose the latest successful **Build Simitone** run for **feat/everyday-gameplay**, and download its **Simitone-Windows-x64** artifact. Extract the artifact, then the inner ZIP. The package includes the .NET runtime; no separate SDK is needed. Keep all folders next to Simitone.exe. Read build-info.json for parent and engine revisions. The adjacent .sha256 describes the inner ZIP; in PowerShell use `Get-FileHash .\Simitone-Windows-x64.zip -Algorithm SHA256` to compare it.

## Use disposable saves

Close The Sims and Simitone first. This Windows build uses **Documents\Simitone\UserData** and **Documents\Simitone\config.ini**. It does not use arbitrary copies elsewhere and has no -testdata flag. If you already have Simitone saves/configuration to preserve, use a separate Windows test account. Do not overwrite or rename your existing save folder as part of this guide.

Install PowerShell 7 only if using the helper; run it as `pwsh`, not the older Windows PowerShell. For a first-time test account, before starting Simitone, create the test copy in its empty Documents\Simitone\UserData directory. Change the source below to the actual original UserData folder. For Complete Collection it is usually under the game installation; for Steam Legacy it is under Saved Games\Electronic Arts\The Sims 25\UserData. Choose the correct neighborhood before copying. No destination may already exist.

```powershell
$source = 'C:\Your game installation\UserData'
$documents = [Environment]::GetFolderPath('MyDocuments')
$copy = Join-Path $documents 'Simitone\UserData'
pwsh -NoProfile -File .\Testing\Test-SaveCopy.ps1 -SourceDirectory $source -DestinationDirectory $copy
```

The helper only copies and hashes files; it does not change the application's save directory. It rejects overlap, existing destinations, links and output inside a detected original install. If it fails, do not use the incomplete copy. Do not choose an original installation as a test output.

Launch **Simitone.exe**. Select your real game installation for read-only assets, and retain existing copied Simitone saves if asked. Do not reset the copy after testing begins. The app may offer other neighborhoods; stay in the copied neighborhood for the first pass. Extra neighborhoods will be copied to the test account's Simitone folder.

## Quick pass

1. Record build-info.json, game edition, Windows version and display scaling in test-results-template.md.
2. Open a household in 2D. Note funds, members and one object. Save, quit, reopen and compare.
3. Open Options. Edit volumes/display then Cancel: changes should not apply. Apply a small audio change. Type a width while paused: the game should remain paused. Apply a supported display size and restart: it should be used then.
4. Summon a cab for Downtown; cancel selection and check usable controls, original selected Sim and simulation speed. Repeat for Old Town if supported by your installation.
5. Make one permitted community trip, buy/interact, return, save and restart. Check funds, inventory, members, relationships and original home/neighborhood. Repeat once. Do not save away from home.
6. Close the game and verify original files are unchanged:

```powershell
pwsh -NoProfile -File .\Testing\Test-SaveCopy.ps1 -VerifyManifest (Join-Path $copy 'source-hashes.json')
```

If a step fails, stop that flow, keep its copy, record the exact last action and report the error. Do not repeatedly retry a broken world load. A failed load after world teardown is not yet recoverable in this build.

## Full pass after the quick checks

Use the repository's everyday-gameplay-matrix.md for B01–B08, H01–H07, T01–T06 and O01–O06. Fill household-reference.md using a separate original-game disposable copy before assessing eviction money/cleanup. Test adult/child/pet households, cancel/double confirmations, occupied destinations, repeat moves/trips, non-default neighborhoods, important inventory/relationships, existing copied saves, 3D, 150% DPI, 800×600 and 1920×1080. Check all buttons remain reachable and modal clicks do not reach the lot underneath.

On a disposable test account only, a denied save destination should show failure and keep the session open rather than claim success or exit. Restore write access before retrying. Per-file replacement does not make several save files one transaction; keep a separate untouched copy.

## Report

Use test-results-template.md. Include build-info.json and the exact error/screenshot. If an error produces a text log or crash report, inspect it for names/paths before sharing. The application is not promised to create a log for every failure; do not upload saves or asset folders. GitHub Actions provides separate build/check/package logs for automated failures. No helper uploads anything automatically.
