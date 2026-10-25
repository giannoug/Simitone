$ErrorActionPreference = 'Stop'
$checker = Join-Path $PSScriptRoot '../scripts/Check-WindowsPackage.ps1'
$root = Join-Path ([IO.Path]::GetTempPath()) ('simitone-package-' + [guid]::NewGuid().ToString('N'))
New-Item -ItemType Directory $root | Out-Null
function MustFail([scriptblock]$Action, [string]$Reason = 'Invalid package was accepted.') {
    $failed = $false
    try { & $Action } catch { $failed = $true }
    if (!$failed) { throw $Reason }
}
try {
    MustFail { & $checker -PackageDirectory $root }
    $required = @('Simitone.exe','Simitone.dll','Simitone.deps.json','Simitone.runtimeconfig.json',
        'hostfxr.dll','hostpolicy.dll','coreclr.dll','System.Private.CoreLib.dll','PresentationFramework.dll',
        'Simitone.Client.dll','FSO.UI.dll','FSO.Content.dll','FSO.SimAntics.dll','Eto.dll','Eto.Wpf.dll','SDL2.dll','openal.dll',
        'SharpDX.dll','SharpDX.DXGI.dll','SharpDX.Direct2D1.dll','SharpDX.Direct3D11.dll',
        'SharpDX.MediaFoundation.dll','SharpDX.XAudio2.dll','SharpDX.XInput.dll','SharpDX.D3DCompiler.dll',
        'Monogame/Windows/MonoGame.Framework.dll','Monogame/WindowsGL/MonoGame.Framework.dll',
        'Content/cas.fsov','Content/uigraphics/ngbh/ngbh_opt.png','Content/DX/Effects/2DWorldBatch.xnb','Content/OGL/Effects/2DWorldBatch.xnb',
        'Testing/Test-SaveCopy.ps1','Testing/windows-test-guide.md','Testing/test-results-template.md',
        'Testing/household-reference.md','Testing/everyday-gameplay-matrix.md')
    foreach ($file in $required) {
        $path = Join-Path $root $file
        [IO.Directory]::CreateDirectory([IO.Path]::GetDirectoryName($path)) | Out-Null
        [IO.File]::WriteAllText($path, 'fixture')
    }
    & $checker -PackageDirectory $root
    foreach ($file in @('coreclr.dll','Monogame/Windows/MonoGame.Framework.dll','Content/cas.fsov','openal.dll',
        'SharpDX.dll','SharpDX.DXGI.dll','SharpDX.Direct2D1.dll','SharpDX.Direct3D11.dll',
        'SharpDX.MediaFoundation.dll','SharpDX.XAudio2.dll','SharpDX.XInput.dll','SharpDX.D3DCompiler.dll',
        'Testing/Test-SaveCopy.ps1','Testing/windows-test-guide.md','Testing/test-results-template.md',
        'Testing/household-reference.md','Testing/everyday-gameplay-matrix.md')) {
        $path = Join-Path $root $file
        Remove-Item $path
        MustFail { & $checker -PackageDirectory $root } "Package without $file was accepted."
        [IO.File]::WriteAllText($path, 'fixture')
    }
    New-Item -ItemType Directory (Join-Path $root 'UserData') | Out-Null
    MustFail { & $checker -PackageDirectory $root }
    'PASS: package checker rejects missing runtime, graphics/audio dependencies, content, offline kit and user data'
} finally { Remove-Item $root -Recurse -Force }
