param([Parameter(Mandatory)][string]$PackageDirectory)
$ErrorActionPreference = 'Stop'
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
    $path = Join-Path $PackageDirectory $file
    if (!(Test-Path -LiteralPath $path -PathType Leaf) -or (Get-Item -LiteralPath $path).Length -eq 0) { throw "Package is missing required file: $file" }
}
foreach ($name in @('UserData','config.ini','GameData','logs')) {
    if (Test-Path -LiteralPath (Join-Path $PackageDirectory $name)) { throw "Package contains user/game data: $name" }
}
