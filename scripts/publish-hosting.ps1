param([ValidateSet('win-x86', 'win-x64')][string]$Runtime = 'win-x86')
$ErrorActionPreference = 'Stop'
$projectRoot = Split-Path -Parent $PSScriptRoot
$artifactRoot = Join-Path $projectRoot 'artifacts'
$publishId = Get-Date -Format 'yyyyMMdd-HHmmss-fff'
$publishFolder = Join-Path $artifactRoot "hosting-$Runtime-$publishId"
$archivePath = Join-Path $artifactRoot "AuroraHotel-hosting-$Runtime-$publishId.zip"
New-Item -ItemType Directory -Path $publishFolder -Force | Out-Null
& dotnet publish (Join-Path $projectRoot 'src/AuroraHotel/AuroraHotel.csproj') -c Release -r $Runtime --self-contained false -p:EnvironmentName=Production -p:DebugType=None -p:DebugSymbols=false -o $publishFolder
if ($LASTEXITCODE -ne 0) { throw 'dotnet publish failed; no hosting archive created.' }
$unexpected = Get-ChildItem -LiteralPath $publishFolder -Recurse -File | Where-Object {
    $_.Name -in @('appsettings.Local.json', 'appsettings.Development.json', 'demo-accounts.txt', 'demo-password.txt') -or
    $_.FullName -match '[\\/]App_Data[\\/]|[\\/]uploads[\\/]' -or $_.Extension -in @('.publishsettings', '.pubxml', '.pfx', '.pem')
}
if ($unexpected) { throw 'Runtime data or a local secret file was included. Archive creation refused.' }
if (!(Test-Path -LiteralPath (Join-Path $publishFolder 'web.config'))) { throw 'IIS web.config was not generated.' }
Add-Type -AssemblyName System.IO.Compression.FileSystem
[System.IO.Compression.ZipFile]::CreateFromDirectory($publishFolder, $archivePath)
Write-Host "Hosting ZIP: $archivePath"
Write-Host 'The ZIP contains compiled application files at its root, without local credentials or database data.'
