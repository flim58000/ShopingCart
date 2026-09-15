$ErrorActionPreference = 'Stop'
$projectRoot = Split-Path -Parent $PSScriptRoot
Set-Location -LiteralPath $projectRoot
dotnet run --project ShopingCart.csproj --launch-profile http
exit $LASTEXITCODE
