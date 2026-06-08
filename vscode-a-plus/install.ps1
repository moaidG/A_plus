# A+ Language - VS Code Extension Installer (Windows)
# Run this script to install or update the A+ extension

param(
    [switch]$force = $false,
    [switch]$build = $true
)

$ErrorActionPreference = "Stop"
$extDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$runnerDir = Join-Path $extDir "runner"

Write-Host "=== A+ Language Extension Installer ===" -ForegroundColor Cyan
Write-Host ""

# 1. Build the interpreter
if ($build) {
    Write-Host "[1/3] Building interpreter..." -ForegroundColor Yellow
    $proj = Join-Path $runnerDir "A_Plus_Console.csproj"
    if (Test-Path $proj) {
        dotnet build $proj -c Release 2>&1 | Out-Null
        if ($LASTEXITCODE -ne 0) {
            Write-Host "  Build failed! Run 'dotnet build' manually in $runnerDir" -ForegroundColor Red
            exit 1
        }
        Write-Host "  Build succeeded." -ForegroundColor Green
    } else {
        Write-Host "  Skipping build (project not found at $proj)" -ForegroundColor Yellow
    }
}

# 2. Install npm dependencies if needed
Write-Host "[2/3] Checking dependencies..." -ForegroundColor Yellow
$nodeModules = Join-Path $extDir "node_modules"
if (-not (Test-Path $nodeModules)) {
    Push-Location $extDir
    npm install --silent 2>&1 | Out-Null
    Pop-Location
    Write-Host "  npm dependencies installed." -ForegroundColor Green
} else {
    Write-Host "  Dependencies already installed." -ForegroundColor Green
}

# 3. Install/update VS Code extension
Write-Host "[3/3] Installing VS Code extension..." -ForegroundColor Yellow
$installArgs = @("--install-extension", $extDir)
if ($force) { $installArgs += "--force" }
$code = Get-Command "code" -ErrorAction SilentlyContinue
if (-not $code) {
    Write-Host "  'code' command not found. Please install VS Code and add it to PATH." -ForegroundColor Red
    Write-Host "  You can manually install by copying $extDir to ~/.vscode/extensions/" -ForegroundColor Yellow
    exit 1
}
& code @installArgs 2>&1 | Out-Null
if ($LASTEXITCODE -eq 0) {
    Write-Host "  Extension installed successfully!" -ForegroundColor Green
} else {
    Write-Host "  Extension installation had issues. Try VS Code > Extensions > ... > Install from VSIX" -ForegroundColor Yellow
}

Write-Host ""
Write-Host "=== Done! ===" -ForegroundColor Cyan
Write-Host "Open a .a or .r.a file in VS Code to start using A+."
Write-Host "Run 'A+: Run' (Ctrl+F5) to execute the current file."
Write-Host "Run 'A+: Check' to syntax-check the current file."
Write-Host "Run 'A+: Watch' for hot-reload."
