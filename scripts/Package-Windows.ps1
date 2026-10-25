param([Parameter(Mandatory)][string]$OutputDirectory, [ValidateSet('Release','Debug')][string]$Configuration = 'Release')
$ErrorActionPreference = 'Stop'
$repo = Split-Path $PSScriptRoot
$output = [IO.Path]::GetFullPath($OutputDirectory)
if (Test-Path -LiteralPath $output) { throw 'Choose a new output directory; existing files will not be overwritten.' }
$package = Join-Path $output 'Simitone-Windows-x64'
New-Item -ItemType Directory $package | Out-Null
Push-Location $repo
try {
    # Keep inherited legacy-package downgrade diagnostics visible without converting them to restore errors.
    & dotnet publish Client/Simitone/Simitone.Windows/Simitone.Windows.csproj -c $Configuration -r win-x64 --self-contained true -p:EnableWindowsTargeting=true -p:PublishSingleFile=false -p:PublishTrimmed=false '-p:WarningsAsErrors=' -p:WarningsNotAsErrors=NU1605 -m:1 -nr:false -o $package
    if ($LASTEXITCODE -ne 0) { throw "Windows publish failed ($LASTEXITCODE)." }
    & (Join-Path $PSScriptRoot 'Check-WindowsPackage.ps1') -PackageDirectory $package
    $kit = Join-Path $package 'Testing'
    New-Item -ItemType Directory $kit | Out-Null
    Copy-Item -LiteralPath (Join-Path $PSScriptRoot 'Test-SaveCopy.ps1') -Destination $kit
    foreach ($file in @('windows-test-guide.md','test-results-template.md','household-reference.md')) {
        Copy-Item -LiteralPath (Join-Path $repo "docs/testing/$file") -Destination $kit
    }
    $parentRevision = (& git rev-parse HEAD).Trim()
    if ($LASTEXITCODE -ne 0) { throw 'Cannot read parent revision.' }
    $engineRevision = (& git -C FreeSO rev-parse HEAD).Trim()
    if ($LASTEXITCODE -ne 0) { throw 'Cannot read engine revision.' }
    [ordered]@{ Parent = $parentRevision; Engine = $engineRevision; Configuration = $Configuration; Runtime = 'win-x64'; SelfContained = $true; GameplayValidated = $false } |
        ConvertTo-Json | Set-Content (Join-Path $package 'build-info.json') -Encoding utf8NoBOM
    Get-ChildItem $package -File -Recurse | Sort-Object FullName | ForEach-Object {
        '{0}  {1}' -f (Get-FileHash -LiteralPath $_.FullName -Algorithm SHA256).Hash.ToLowerInvariant(), [IO.Path]::GetRelativePath($package, $_.FullName).Replace('\','/')
    } | Set-Content (Join-Path $package 'SHA256SUMS.txt') -Encoding utf8NoBOM
    $zip = Join-Path $output 'Simitone-Windows-x64.zip'
    Compress-Archive -Path $package -DestinationPath $zip
    (Get-FileHash -LiteralPath $zip -Algorithm SHA256).Hash.ToLowerInvariant() + '  Simitone-Windows-x64.zip' | Set-Content ($zip + '.sha256') -Encoding utf8NoBOM
    Write-Output $zip
} finally { Pop-Location }
