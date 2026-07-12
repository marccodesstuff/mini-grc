$ErrorActionPreference = 'SilentlyContinue'
$renames = @(
    'src\MiniGrc.Agent',
    'src\MiniGrc.Api',
    'src\MiniGrc.Application',
    'src\MiniGrc.ComponentLib',
    'src\MiniGrc.Domain',
    'src\MiniGrc.Infrastructure',
    'src\MiniGrc.Web',
    'tests\MiniGrc.E2E',
    'tests\MiniGrc.UnitTests'
)
foreach ($src in $renames) {
    if (-not (Test-Path $src)) { continue }
    $dst = $src -replace 'MiniGrc','Augur'
    if (Test-Path $dst) { Remove-Item $dst -Recurse -Force }
    for ($i = 0; $i -lt 10; $i++) {
        try {
            Move-Item -Path $src -Destination $dst -Force -ErrorAction Stop
            Write-Host "renamed $src -> $dst"
            break
        } catch {
            Start-Sleep -Seconds 1
        }
    }
}
