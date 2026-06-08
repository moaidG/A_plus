# A+ Language Runner - Build standalone EXE
# Run: powershell -ExecutionPolicy Bypass .\publish.ps1

$project = Join-Path $PSScriptRoot "vscode-a-plus\runner"
$dist    = Join-Path $PSScriptRoot "dist"

Write-Host "Building A+ Standalone Runner..." -ForegroundColor Cyan

if (Test-Path $dist) { Remove-Item -Recurse -Force $dist }
New-Item -ItemType Directory -Path $dist -Force | Out-Null

# Build Windows standalone
dotnet publish $project -f net10.0-windows -c Release -r win-x64 `
  --self-contained true -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true -p:AssemblyName=a+ -o $dist 2>&1

if ($LASTEXITCODE -eq 0) {
    Copy-Item "$dist\a+.exe" "$dist\a+" -Force
    Write-Host "Windows build done!" -ForegroundColor Green
} else {
    Write-Host "Windows build failed!" -ForegroundColor Red
}

# Build cross-platform (console-only)
dotnet publish $project -f net10.0 -c Release -o "$dist\net10.0" 2>&1

# Copy stdlib
$stdlibSrc = Join-Path $project "stdlib"
$stdlibDst = Join-Path $dist "stdlib"
if (Test-Path $stdlibSrc) {
    Copy-Item -Recurse $stdlibSrc $stdlibDst -Force
    Copy-Item -Recurse $stdlibSrc "$dist\net10.0\stdlib" -Force -ErrorAction SilentlyContinue
}

# Copy MobileTemplate
$templateSrc = Join-Path $project "MobileTemplate"
$templateDst = Join-Path $dist "MobileTemplate"
if (Test-Path $templateSrc) {
    Copy-Item -Recurse $templateSrc $templateDst -Force
    Copy-Item -Recurse $templateSrc "$dist\net10.0\MobileTemplate" -Force -ErrorAction SilentlyContinue
}

Write-Host "`nDone! a+.exe created in: $dist" -ForegroundColor Green
Write-Host "`nUsage:"
Write-Host "  $dist\a+.exe <file.a>"
Write-Host "  $dist\a+.exe <folder>"
Write-Host "`nAdd to PATH for global use:"
Write-Host "  [Environment]::SetEnvironmentVariable('Path', [Environment]::GetEnvironmentVariable('Path','User') + ';$dist', 'User')"
