$ErrorActionPreference = 'Stop'
$checker = Join-Path $PSScriptRoot '../scripts/Check-WindowsPackage.ps1'
$root = Join-Path ([IO.Path]::GetTempPath()) ('simitone-package-' + [guid]::NewGuid().ToString('N'))
New-Item -ItemType Directory $root | Out-Null
function MustFail([scriptblock]$Action) {
    $failed = $false
    try { & $Action } catch { $failed = $true }
    if (!$failed) { throw 'Invalid package was accepted.' }
}
try {
    MustFail { & $checker -PackageDirectory $root }
    $required = @('Simitone.exe','Simitone.dll','Simitone.deps.json','Simitone.runtimeconfig.json',
        'hostfxr.dll','hostpolicy.dll','coreclr.dll','System.Private.CoreLib.dll','PresentationFramework.dll',
        'Simitone.Client.dll','FSO.UI.dll','FSO.Content.dll','FSO.SimAntics.dll','Eto.dll','Eto.Wpf.dll','SDL2.dll',
        'Monogame/Windows/MonoGame.Framework.dll','Monogame/WindowsGL/MonoGame.Framework.dll',
        'Content/cas.fsov','Content/uigraphics/ngbh_opt.png','Content/DX/Effects/2DWorldBatch.xnb','Content/OGL/Effects/2DWorldBatch.xnb')
    foreach ($file in $required) {
        $path = Join-Path $root $file
        [IO.Directory]::CreateDirectory([IO.Path]::GetDirectoryName($path)) | Out-Null
        [IO.File]::WriteAllText($path, 'fixture')
    }
    & $checker -PackageDirectory $root
    foreach ($file in @('coreclr.dll','Monogame/Windows/MonoGame.Framework.dll','Content/cas.fsov')) {
        $path = Join-Path $root $file
        Remove-Item $path
        MustFail { & $checker -PackageDirectory $root }
        [IO.File]::WriteAllText($path, 'fixture')
    }
    New-Item -ItemType Directory (Join-Path $root 'UserData') | Out-Null
    MustFail { & $checker -PackageDirectory $root }
    'PASS: package checker rejects missing runtime, renderer, content and user data'
} finally { Remove-Item $root -Recurse -Force }
