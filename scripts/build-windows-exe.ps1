$ErrorActionPreference = "Stop"

$Root = Resolve-Path (Join-Path $PSScriptRoot "..")
$Source = Join-Path $Root "windows\OneClickClearScreen.cs"
$Dist = Join-Path $Root "dist"
$Output = Join-Path $Dist "OneClickClearScreen.exe"

if (-not (Test-Path $Dist)) {
    New-Item -ItemType Directory -Path $Dist | Out-Null
}

if (Test-Path $Output) {
    Remove-Item -LiteralPath $Output -Force
}

Add-Type `
    -Path $Source `
    -ReferencedAssemblies @("System.Windows.Forms", "System.Drawing") `
    -OutputAssembly $Output `
    -OutputType WindowsApplication

Write-Host "Created $Output"
