param([Parameter(Mandatory)][string]$PackageDirectory)
$ErrorActionPreference = 'Stop'
$required = @('Simitone.exe','Simitone.dll','Simitone.deps.json','Simitone.runtimeconfig.json',
    'hostfxr.dll','hostpolicy.dll','coreclr.dll','System.Private.CoreLib.dll','PresentationFramework.dll',
    'Simitone.Client.dll','FSO.UI.dll','FSO.Content.dll','FSO.SimAntics.dll','Eto.dll','Eto.Wpf.dll','SDL2.dll',
    'Monogame/Windows/MonoGame.Framework.dll','Monogame/WindowsGL/MonoGame.Framework.dll',
    'Content/cas.fsov','Content/uigraphics/ngbh_opt.png','Content/DX/Effects/2DWorldBatch.xnb','Content/OGL/Effects/2DWorldBatch.xnb')
foreach ($file in $required) {
    $path = Join-Path $PackageDirectory $file
    if (!(Test-Path -LiteralPath $path -PathType Leaf) -or (Get-Item -LiteralPath $path).Length -eq 0) { throw "Package is missing required file: $file" }
}
foreach ($name in @('UserData','config.ini','GameData','logs')) {
    if (Test-Path -LiteralPath (Join-Path $PackageDirectory $name)) { throw "Package contains user/game data: $name" }
}
