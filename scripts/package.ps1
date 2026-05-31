$ErrorActionPreference = "Stop"

$Root = Resolve-Path (Join-Path $PSScriptRoot "..")
$Dist = Join-Path $Root "dist"
$Zip = Join-Path $Dist "one-click-clear-screen.zip"

if (Test-Path $Dist) {
    Remove-Item -LiteralPath $Dist -Recurse -Force
}

New-Item -ItemType Directory -Path $Dist | Out-Null

$Files = @(
    "manifest.json",
    "README.md",
    "LICENSE",
    "src/background.js",
    "src/content.js"
)

$Temp = Join-Path $Dist "package"
New-Item -ItemType Directory -Path $Temp | Out-Null

foreach ($File in $Files) {
    $Source = Join-Path $Root $File
    $Target = Join-Path $Temp $File
    $TargetDir = Split-Path $Target -Parent
    if (-not (Test-Path $TargetDir)) {
        New-Item -ItemType Directory -Path $TargetDir | Out-Null
    }
    Copy-Item -LiteralPath $Source -Destination $Target
}

Compress-Archive -Path (Join-Path $Temp "*") -DestinationPath $Zip
Remove-Item -LiteralPath $Temp -Recurse -Force

Write-Host "Created $Zip"
