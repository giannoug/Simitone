# Everyday gameplay: baseline handoff

Status: tools installed; baseline build in progress, gameplay validation pending. Date: 24 October 2026.

The implementation worktree is on `feat/everyday-gameplay`, based on the community fork's contributor branch. The approved design and all five implementation plans are included in this branch. No game behavior has changed.

## Pinned sources

| Source | Revision | Role |
|---|---|---|
| alexjyong/Simitone alex-main | 95fc53ee123389dcd3226757acdad15ce8bfda77 | Contribution base |
| alexjyong/Simitone simitone-forked | e36e83f95d44b9f4b378e7665c4e2ecbaa88b4ab | Existing release improvements to compare |
| Base FreeSO submodule | f86bbd34b2112296ed786741f640a0535886d180 | Initialized at the pinned revision |
| Release FreeSO submodule | 7e76c1960252ddd033d8937d5edb47d326f5b69c | Candidate engine changes, not yet imported |
| Upstream PR 68 | 72aae7486026b0933606cc4f51da10bd0cd45764 | Candidate selective fixes, not merged |

The base submodule URL is riperiperi/FreeSO; the release uses alexjyong/FreeSO. Changing only the pointer without checking the URL and required engine APIs is insufficient.

## Branch comparison and import decisions

| Area | Existing release work | Next action |
|---|---|---|
| Startup and game paths | Windows entry point now uses Simitone.Shared and new installation dialogs | Trace the whole dependency change; preserve Complete/Legacy separate-save handling without copying a single entry-point file in isolation |
| Project structure | Shared/Desktop/Launcher projects, resources and altered DLL placement | Import only what the selected Windows startup/package needs; build-test the resulting combination |
| Free will | Existing toggle and persisted engine setting/startup initialization | Preserve the matching UI, startup and engine changes together; no new autonomy algorithm |
| Neighborhood return | CurrentNeighborhoodMode and return-view handling in TS1GameScreen | Preserve; distinguish region IDs from the provider's UserData folder |
| Save path | Existing save implementation and save-and-exit callers | Test B01–B08, then change the shared Save return result directly if needed |
| Options | Four startup audio settings already map to HITVM; no complete settings UI | Reuse the mapping; add only the approved small dialog and reliable INI result |
| Unrelated additions | Weather, updater, catalog/eyedropper and other graphics work | Exclude unless a concrete selected baseline dependency requires a small supporting hunk |

No imports have been applied yet: the required baseline build is being checked before changes. The table is an inspection-based import decision list, not a claim of tested compatibility.

## Initial execution results (before tool installation)

- `dotnet --info`: exit 127, `dotnet: command not found`.
- `dotnet build Client/Simitone/Simitone.Windows/Simitone.Windows.csproj -c Release`: exit 127 for the same reason; compilation never started.
- `git submodule status`: `-f86bbd34b2112296ed786741f640a0535886d180 FreeSO`; the engine is not initialized.
- No .NET SDK/compiler was found in PATH or the inspected runtime locations.
- This is a Linux host. No Windows test machine or licensed game assets have been supplied in this session. No edition, renderer, startup, persistence or gameplay result is claimed.

## Resume on a prepared Windows workspace

1. Use this implementation branch and its approved plans. Provide the existing project's .NET 9 SDK/toolchain; record `dotnet --info` rather than guessing an SDK version.
2. Run `git submodule update --init --recursive` at the pinned base, then restore and build before importing release improvements. The existing workflow documents optional legacy content-generation steps; record any actual prerequisite failure and its resolution.
3. Run `dotnet restore Client/Simitone/Simitone.sln`, then `dotnet build Client/Simitone/Simitone.Windows/Simitone.Windows.csproj -c Release`.
4. Complete the selective baseline imports above, pin both resulting revisions, rebuild and document the actual package/content-copy procedure.
5. Use a legitimate Complete Collection or Legacy Collection installation and disposable Simitone saves. Run B01–B08 before advancing to household fixes. Record edition/OS/DPI/renderer, expected values, actual results and original-save hashes.

## Tool installation update

The user explicitly requested installation of required tools, superseding the earlier plan restriction on installing the missing SDK.

- Installed the official Linux x64 .NET SDK **9.0.319**, including .NET and ASP.NET Core runtimes **9.0.21**, at `/workspace/scratch/b178c25d06ff/tooling/dotnet`.
- `dotnet --info` and `--list-sdks` succeed. A clean net9.0 console program compiled and ran with exit 0 and output `Hello, World!`.
- The official installer's extraction initially reported unmapped archive-owner IDs in the container. The SDK files were copied; compiler/runtime verification succeeded. Use `TAR_OPTIONS=--no-same-owner` for any subsequent extraction here.
- Initialized FreeSO and all its recursive submodules at their pinned revisions. SDL2, OpenAL, ICU and OpenSSL runtime libraries are already present.
- The global `/usr/local/bin` is not writable, so tools use a session-local PATH rather than changing system installation directories.

Use this environment for subsequent commands in this host:

```bash
export DOTNET_ROOT=/workspace/scratch/b178c25d06ff/tooling/dotnet
export PATH="$DOTNET_ROOT:$PATH"
export DOTNET_CLI_TELEMETRY_OPTOUT=1
dotnet --info
dotnet build Client/Simitone/Simitone.Windows/Simitone.Windows.csproj -c Release -p:EnableWindowsTargeting=true -m:1 -nr:false
```

The first parallel restore returned failure without compiler diagnostics. The single-node build has restored 33 projects and compiled 14; full completion is not yet claimed. The SDK/toolchain itself passed the independent console compile/run check. No product code has changed. Windows gameplay testing and licensed game files remain separate requirements. No push or publication has been performed.
