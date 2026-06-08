param(
    [Parameter(Position = 0, ValueFromRemainingArguments = $true)]
    [string[]]$ArgsList
)

$ErrorActionPreference = "Stop"

$csproj = Get-ChildItem "$PSScriptRoot\runner\*.csproj" | Select-Object -First 1 -ExpandProperty FullName
if (-not $csproj) {
    Write-Error "No .csproj file found in runner"
    exit 1
}

function Show-Usage {
    Write-Error "Usage: .\make.ps1 A+"
    Write-Error "       .\make.ps1 package <script.a> [name] -os [exe|apk|ios|web|linux|macos|all]"
}

if (-not $ArgsList -or $ArgsList.Count -eq 0) {
    Show-Usage
    exit 1
}

$target = $ArgsList[0]
$remaining = @()
if ($ArgsList.Count -gt 1) {
    $remaining = $ArgsList[1..($ArgsList.Count - 1)]
}

if ($target -ieq "A+") {
    dotnet run -f net10.0 -c Release --project $csproj -- make A+
    exit $LASTEXITCODE
}

if ($target -ieq "package") {
    dotnet run -f net10.0 -c Release --project $csproj -- package @remaining
    exit $LASTEXITCODE
}

Show-Usage
exit 1
