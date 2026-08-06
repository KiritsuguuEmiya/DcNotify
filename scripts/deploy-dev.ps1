#Requires -Version 5.1
param(
    [ValidateSet('Debug', 'Release')]
    [string]$Configuration = 'Debug',

    [string]$SourceDir = '',

    [string]$RepoRoot = (Resolve-Path (Join-Path $PSScriptRoot '..')).Path
)

function Resolve-BuildOutputDir {
    param([string]$Root, [string]$Cfg, [string]$Explicit)

    if ($Explicit -and (Test-Path (Join-Path $Explicit 'DcNotify.dll'))) {
        return (Resolve-Path $Explicit).Path
    }

    $candidates = @(
        (Join-Path $Root "DcNotify/bin/x64/$Cfg"),
        (Join-Path $Root "DcNotify/bin/$Cfg")
    )

    foreach ($candidate in $candidates) {
        if (Test-Path (Join-Path $candidate 'DcNotify.dll')) {
            return (Resolve-Path $candidate).Path
        }
    }

    throw "Build output not found. Run: dotnet build DcNotify.sln -c $Cfg"
}

function Copy-BuildOutput {
    param([string]$From, [string]$To)

    New-Item -ItemType Directory -Path $To -Force | Out-Null
    Copy-Item (Join-Path $From 'DcNotify.dll') $To -Force
    Copy-Item (Join-Path $From 'DcNotify.deps.json') $To -Force
    Copy-Item (Join-Path $From 'DcNotify.json') $To -Force

    $runtimes = Join-Path $From 'runtimes'
    if (Test-Path $runtimes) {
        Copy-Item $runtimes $To -Recurse -Force
    }
}

$src = Resolve-BuildOutputDir -Root $RepoRoot -Cfg $Configuration -Explicit $SourceDir

$targets = @(
    (Join-Path $env:APPDATA 'XIVLauncher/devPlugins/DcNotify'),
    (Join-Path $RepoRoot "DcNotify/bin/$Configuration")
)

foreach ($target in $targets) {
    Copy-BuildOutput -From $src -To $target
}

$version = (Get-Item (Join-Path $src 'DcNotify.dll')).VersionInfo.FileVersion
Write-Output "Deployed $Configuration v$version from $src"
foreach ($target in $targets) {
    Write-Output "  -> $target"
}
Write-Output "Reload in-game: /xlplugins -> DcNotify -> reload (or restart Dalamud)."
