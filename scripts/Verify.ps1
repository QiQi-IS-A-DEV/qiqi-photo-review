param([ValidateSet('Debug', 'Release')][string]$Configuration = 'Release')

$ErrorActionPreference = 'Stop'
$root = Split-Path $PSScriptRoot -Parent
Push-Location $root
try {
    dotnet restore PhotoFileFilter.sln
    if ($LASTEXITCODE -ne 0) { throw 'Restore failed.' }
    dotnet build PhotoFileFilter.sln -c $Configuration --no-restore
    if ($LASTEXITCODE -ne 0) { throw 'Build failed.' }
    dotnet run --project tests/PhotoFileFilter.Tests/PhotoFileFilter.Tests.csproj -c $Configuration --no-build
    if ($LASTEXITCODE -ne 0) { throw 'Validation failed.' }
}
finally {
    Pop-Location
}
