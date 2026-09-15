param([string]$Runtime = 'win-x64')
$ErrorActionPreference = 'Stop'
$taskProject = Join-Path $PSScriptRoot 'PhotoFileFilter.csproj'
$taskOutput = Join-Path $PSScriptRoot "artifacts\publish\$Runtime"
dotnet publish $taskProject -c Release -r $Runtime --self-contained true -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true -p:DebugType=None -p:DebugSymbols=false -o $taskOutput
if ($LASTEXITCODE -ne 0) { throw 'Publish failed.' }
Write-Host "Application: $taskOutput\PhotoFileFilter.exe"
