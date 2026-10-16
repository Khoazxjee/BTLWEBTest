$ErrorActionPreference = 'Stop'
$projectRoot = Split-Path -Parent $PSScriptRoot
Write-Host 'Initialize a NEW hosting demo database. Existing accounts or rooms will never be overwritten.'
$databaseName = Read-Host 'Exact database name from the hosting panel'
$connectionSecret = Read-Host 'Full SQL connection string (hidden input)' -AsSecureString
$passwordSecret = Read-Host 'New ADMIN demo password, 16+ characters with upper/lower case, digit and symbol (hidden input)' -AsSecureString
$environmentNames = @('ConnectionStrings__HotelDb', 'Bootstrap__DatabaseName', 'Bootstrap__DemoPassword', 'ASPNETCORE_ENVIRONMENT', 'DOTNET_ENVIRONMENT')
$savedEnvironment = @{}
foreach ($variableName in $environmentNames) { $savedEnvironment[$variableName] = [Environment]::GetEnvironmentVariable($variableName, 'Process') }
function Convert-SecretToText([Security.SecureString]$Secret) {
    $pointer = [Runtime.InteropServices.Marshal]::SecureStringToBSTR($Secret)
    try { [Runtime.InteropServices.Marshal]::PtrToStringBSTR($pointer) }
    finally { [Runtime.InteropServices.Marshal]::ZeroFreeBSTR($pointer) }
}
try {
    [Environment]::SetEnvironmentVariable('ConnectionStrings__HotelDb', (Convert-SecretToText $connectionSecret), 'Process')
    [Environment]::SetEnvironmentVariable('Bootstrap__DatabaseName', $databaseName, 'Process')
    [Environment]::SetEnvironmentVariable('Bootstrap__DemoPassword', (Convert-SecretToText $passwordSecret), 'Process')
    [Environment]::SetEnvironmentVariable('ASPNETCORE_ENVIRONMENT', 'Production', 'Process')
    [Environment]::SetEnvironmentVariable('DOTNET_ENVIRONMENT', 'Production', 'Process')
    & dotnet run --project (Join-Path $projectRoot 'src/AuroraHotel') -c Release --no-launch-profile -- --initialize-demo
    if ($LASTEXITCODE -ne 0) { throw 'Database initialization failed. Read the error above.' }
    Write-Host 'Done. Accounts: admin@aurora.local, letan@aurora.local, minh@aurora.local, linh@aurora.local.'
    Write-Host 'Each account has a different password. Read src/AuroraHotel/App_Data/hosting-demo-accounts.txt locally; never upload it.'
} finally {
    foreach ($variableName in $environmentNames) { [Environment]::SetEnvironmentVariable($variableName, $savedEnvironment[$variableName], 'Process') }
    $connectionSecret.Dispose()
    $passwordSecret.Dispose()
}
