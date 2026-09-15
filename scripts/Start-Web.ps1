param([switch]$Production)

$ErrorActionPreference = 'Stop'
$projectRoot = Split-Path -Parent $PSScriptRoot
Set-Location -LiteralPath (Join-Path $projectRoot 'frontend')

# เครื่องนี้อาจมี Node.js รุ่นเก่าใน PATH จึงตรวจเวอร์ชันก่อนเริ่ม
$nodeCommand = Get-Command node -ErrorAction SilentlyContinue
$nodeExecutable = if ($nodeCommand) { $nodeCommand.Source } else { $null }
$nodeVersion = if ($nodeExecutable) { [version]((& $nodeExecutable --version).TrimStart('v')) } else { [version]'0.0.0' }
if ($nodeVersion -lt [version]'20.9.0') {
    $bundledNode = Join-Path $env:USERPROFILE '.cache\codex-runtimes\codex-primary-runtime\dependencies\node\bin\node.exe'
    if (Test-Path -LiteralPath $bundledNode) { $nodeExecutable = $bundledNode }
    else { throw 'Please install Node.js 20.9 or newer, then run this script again.' }
}
$env:PATH = (Split-Path -Parent $nodeExecutable) + [IO.Path]::PathSeparator + $env:PATH

if (-not (Test-Path -LiteralPath 'node_modules\next\dist\bin\next')) {
    $pnpmCommand = Get-Command pnpm.cmd -ErrorAction SilentlyContinue
    $bundledPnpm = Join-Path $env:USERPROFILE '.cache\codex-runtimes\codex-primary-runtime\dependencies\bin\fallback\pnpm.cmd'
    if ($pnpmCommand) { & $pnpmCommand.Source install --frozen-lockfile }
    elseif (Test-Path -LiteralPath $bundledPnpm) { & $bundledPnpm install --frozen-lockfile }
    elseif (Get-Command npm.cmd -ErrorAction SilentlyContinue) { npm.cmd install }
    else { throw 'Please install pnpm or npm to download the frontend packages.' }
    if ($LASTEXITCODE -ne 0) { throw 'Installing frontend packages failed.' }
}

if ($Production) {
    & $nodeExecutable node_modules\next\dist\bin\next build
    if ($LASTEXITCODE -ne 0) { throw 'Next.js build failed.' }
    & $nodeExecutable node_modules\next\dist\bin\next start --hostname 127.0.0.1 --port 3000
} else {
    & $nodeExecutable node_modules\next\dist\bin\next dev --hostname 127.0.0.1 --port 3000
}
exit $LASTEXITCODE
