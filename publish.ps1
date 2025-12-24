$ErrorActionPreference = "Stop"

$publishDir = Join-Path $PSScriptRoot "publish\portable"
if (Test-Path $publishDir) {
  Remove-Item $publishDir -Recurse -Force
}

Write-Host "Publishing TreeOverlay (self-contained win-x64)..."
dotnet publish "TreeOverlay\TreeOverlay.csproj" -c Release -r win-x64 --self-contained true -o $publishDir /p:PublishSingleFile=true

Write-Host "Portable build output: $publishDir"
