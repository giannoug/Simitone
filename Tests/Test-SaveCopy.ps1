$ErrorActionPreference = 'Stop'
$helper = Join-Path $PSScriptRoot '../scripts/Test-SaveCopy.ps1'
$root = Join-Path ([IO.Path]::GetTempPath()) ('simitone-copy-' + [guid]::NewGuid().ToString('N'))
New-Item -ItemType Directory $root | Out-Null
function MustFail([scriptblock]$Action) { $failed = $false; try { & $Action } catch { $failed = $true }; if (!$failed) { throw 'Unsafe copy operation was accepted.' } }
try {
    $source = Join-Path $root 'UserData'
    New-Item -ItemType Directory (Join-Path $source 'Houses') | Out-Null
    New-Item -ItemType Directory (Join-Path $source 'Characters') | Out-Null
    [IO.File]::WriteAllText((Join-Path $source 'Neighborhood.iff'),'source-neighborhood')
    [IO.File]::WriteAllText((Join-Path $source 'Houses/House01.iff'),'source-house')
    MustFail { & $helper -SourceDirectory $source -DestinationDirectory $source }
    MustFail { & $helper -SourceDirectory $source -DestinationDirectory (Join-Path $source 'child') }
    MustFail { & $helper -SourceDirectory $source -DestinationDirectory $root }
    $copy = Join-Path $root 'disposable'
    & $helper -SourceDirectory $source -DestinationDirectory $copy
    if (!(Test-Path -LiteralPath (Join-Path $copy 'Characters') -PathType Container)) { throw 'Empty save directories were not copied.' }
    if ([IO.File]::ReadAllText((Join-Path $copy 'Houses/House01.iff')) -ne 'source-house') { throw 'Copy content differs.' }
    MustFail { & $helper -SourceDirectory $source -DestinationDirectory $copy }
    & $helper -VerifyManifest (Join-Path $copy 'source-hashes.json')
    [IO.File]::WriteAllText((Join-Path $source 'Houses/House01.iff'),'changed')
    MustFail { & $helper -VerifyManifest (Join-Path $copy 'source-hashes.json') }
    $kind = if ($IsWindows) { 'Junction' } else { 'SymbolicLink' }
    New-Item -ItemType $kind -Path (Join-Path $source 'linked') -Target $root | Out-Null
    MustFail { & $helper -SourceDirectory $source -DestinationDirectory (Join-Path $root 'unsafe') }
    Remove-Item -LiteralPath (Join-Path $source 'linked')
    'PASS: disposable copy, empty directories, unchanged-source hashes, overlap and link rejection'
} finally { Remove-Item $root -Recurse -Force }
