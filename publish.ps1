param([string]$Runtime = 'win-x64')
$ErrorActionPreference = 'Stop'
$taskProject = Join-Path $PSScriptRoot 'src\PhotoFileFilter.App\PhotoFileFilter.App.csproj'
$taskOutput = Join-Path $PSScriptRoot "artifacts\publish\$Runtime"
dotnet publish $taskProject -c Release -r $Runtime --self-contained true -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true -p:DebugType=None -p:DebugSymbols=false -o $taskOutput
if ($LASTEXITCODE -ne 0) { throw 'Publish failed.' }
Copy-Item -LiteralPath (Join-Path $PSScriptRoot 'LICENSE') -Destination $taskOutput -Force
Copy-Item -LiteralPath (Join-Path $PSScriptRoot 'NOTICE') -Destination $taskOutput -Force
Copy-Item -LiteralPath (Join-Path $PSScriptRoot 'README.md') -Destination $taskOutput -Force
Write-Host "Application: $taskOutput\PhotoFileFilter.exe"
