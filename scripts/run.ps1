$ErrorActionPreference = 'Stop'
Set-Location (Split-Path -Parent $PSScriptRoot)
dotnet tool restore
if ($LASTEXITCODE -ne 0) { throw 'Không khôi phục được dotnet-ef.' }
dotnet restore AuroraHotel.sln
if ($LASTEXITCODE -ne 0) { throw 'Không khôi phục được NuGet packages.' }
dotnet run --project src/AuroraHotel
