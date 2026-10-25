[CmdletBinding(DefaultParameterSetName='Copy')]
param(
    [Parameter(Mandatory,ParameterSetName='Copy')][string]$SourceDirectory,
    [Parameter(Mandatory,ParameterSetName='Copy')][string]$DestinationDirectory,
    [Parameter(Mandatory,ParameterSetName='Verify')][string]$VerifyManifest
)
$ErrorActionPreference = 'Stop'
$comparison = if ($IsWindows) { [StringComparison]::OrdinalIgnoreCase } else { [StringComparison]::Ordinal }
function FullPath([string]$Path) { [IO.Path]::TrimEndingDirectorySeparator([IO.Path]::GetFullPath($Path)) }
function Within([string]$Child, [string]$Parent) { $Child.Equals($Parent,$comparison) -or $Child.StartsWith($Parent + [IO.Path]::DirectorySeparatorChar,$comparison) }
function RejectLinks([string]$Path) {
    $current = $Path
    while ($current) {
        if (Test-Path -LiteralPath $current) {
            if ((Get-Item -Force -LiteralPath $current).Attributes -band [IO.FileAttributes]::ReparsePoint) { throw "Linked paths are not supported: $current" }
        }
        $current = [IO.Path]::GetDirectoryName($current)
    }
}
function Hashes([string]$Source) {
    RejectLinks $Source
    if (!(Test-Path -LiteralPath (Join-Path $Source 'Neighborhood.iff') -PathType Leaf) -or !(Test-Path -LiteralPath (Join-Path $Source 'Houses') -PathType Container)) { throw 'Select a UserData folder containing Neighborhood.iff and Houses.' }
    $items = @(Get-ChildItem -LiteralPath $Source -Force -Recurse)
    foreach ($item in $items) { if ($item.Attributes -band [IO.FileAttributes]::ReparsePoint) { throw "Linked entries are not supported: $($item.FullName)" } }
    @($items | Where-Object { !$_.PSIsContainer } | Sort-Object FullName | ForEach-Object {
        [ordered]@{ Path = [IO.Path]::GetRelativePath($Source,$_.FullName).Replace('\','/'); SHA256 = (Get-FileHash -LiteralPath $_.FullName -Algorithm SHA256).Hash }
    })
}
function CheckSource($Manifest) {
    $source = FullPath $Manifest.SourceDirectory
    $actual = @(Hashes $source)
    $expected = @($Manifest.Files)
    if ($actual.Count -ne $expected.Count) { throw 'Source file list changed.' }
    for ($i = 0; $i -lt $actual.Count; $i++) {
        if ($actual[$i].Path -cne $expected[$i].Path -or $actual[$i].SHA256 -cne $expected[$i].SHA256) { throw "Source changed: $($actual[$i].Path)" }
    }
}
if ($PSCmdlet.ParameterSetName -eq 'Verify') {
    $manifest = Get-Content -Raw -LiteralPath $VerifyManifest | ConvertFrom-Json
    if ($manifest.Version -ne 1) { throw 'Unsupported source hash manifest.' }
    CheckSource $manifest
    Write-Output 'PASS: source save files are unchanged'
    return
}
$source = FullPath $SourceDirectory
$destination = FullPath $DestinationDirectory
if ((Within $source $destination) -or (Within $destination $source)) { throw 'Source and destination must be separate, non-overlapping folders.' }
RejectLinks $destination
if (Test-Path -LiteralPath $destination) { throw 'Destination already exists; choose a new disposable folder.' }
$install = [IO.Path]::GetDirectoryName($source)
if ((Test-Path -LiteralPath (Join-Path $install 'GameData')) -and (Within $destination $install)) { throw 'Destination must be outside the original game installation.' }
$files = @(Hashes $source)
$manifest = [ordered]@{ Version = 1; SourceDirectory = $source; Files = $files }
[IO.Directory]::CreateDirectory($destination) | Out-Null
foreach ($directory in Get-ChildItem -LiteralPath $source -Directory -Force -Recurse) {
    RejectLinks $directory.FullName
    [IO.Directory]::CreateDirectory((Join-Path $destination ([IO.Path]::GetRelativePath($source,$directory.FullName)))) | Out-Null
}
foreach ($file in $files) {
    $target = Join-Path $destination $file.Path
    [IO.Directory]::CreateDirectory([IO.Path]::GetDirectoryName($target)) | Out-Null
    [IO.File]::Copy((Join-Path $source $file.Path),$target,$false)
    if ((Get-FileHash -LiteralPath $target -Algorithm SHA256).Hash -cne $file.SHA256) { throw "Source changed while copying: $($file.Path). Do not use the incomplete copy." }
}
CheckSource $manifest
$manifestPath = Join-Path $destination 'source-hashes.json'
$manifest | ConvertTo-Json -Depth 5 | Set-Content -LiteralPath $manifestPath -Encoding utf8NoBOM
Write-Output "Disposable copy created: $destination"
Write-Output "Verify the original files later with: pwsh -File `"$PSCommandPath`" -VerifyManifest `"$manifestPath`""
