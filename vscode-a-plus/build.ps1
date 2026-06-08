# Build script for A+ Language Runner
# Creates standalone a+ executable

$project = Join-Path $PSScriptRoot "runner"
$output = Join-Path $PSScriptRoot "dist"

Write-Host "Building A+ Runner..." -ForegroundColor Cyan

# Clean
Remove-Item -Recurse -Force "$project\bin", "$project\obj" -ErrorAction SilentlyContinue
if (Test-Path $output) { Remove-Item -Recurse -Force $output }
New-Item -ItemType Directory -Path $output -Force | Out-Null

# Publish Windows self-contained single-file executable
dotnet publish $project -f net10.0-windows -c Release -r win-x64 `
  --self-contained true -p:PublishSingleFile=true -p:AssemblyName=a+ -o $output 2>&1

if ($LASTEXITCODE -eq 0) {
    # Copy as a+ (no extension) for convenience
    Copy-Item "$output\a+.exe" "$output\a+" -Force
    Write-Host "Windows build done!" -ForegroundColor Green
} else {
    Write-Host "Windows build failed!" -ForegroundColor Red
}

# Publish cross-platform (console-only, no WPF)
dotnet publish $project -f net10.0 -c Release -o "$output\net10.0" 2>&1

# Copy stdlib to output (for both targets)
$stdlibSrc = Join-Path $project "stdlib"
$stdlibDst = Join-Path $output "stdlib"
if (Test-Path $stdlibSrc) {
    Remove-Item -Recurse -Force $stdlibDst -ErrorAction SilentlyContinue
    Remove-Item -Recurse -Force "$output\net10.0\stdlib" -ErrorAction SilentlyContinue
    Copy-Item -Recurse $stdlibSrc $stdlibDst -Force
    Copy-Item -Recurse $stdlibSrc "$output\net10.0\stdlib" -Force -ErrorAction SilentlyContinue
    Write-Host "stdlib copied to output" -ForegroundColor Green
}

# Copy compiler/runtime sources needed by the package command in the standalone runner
$languageSrc = Join-Path $project "A_Language"
$languageDst = Join-Path $output "A_Language"
if (Test-Path $languageSrc) {
    Remove-Item -Recurse -Force $languageDst -ErrorAction SilentlyContinue
    Remove-Item -Recurse -Force "$output\net10.0\A_Language" -ErrorAction SilentlyContinue
    Copy-Item -Recurse $languageSrc $languageDst -Force
    Copy-Item -Recurse $languageSrc "$output\net10.0\A_Language" -Force -ErrorAction SilentlyContinue
    Write-Host "A_Language sources copied to output" -ForegroundColor Green
}

# Copy MobileTemplate to output
$templateSrc = Join-Path $project "MobileTemplate"
$templateDst = Join-Path $output "MobileTemplate"
if (Test-Path $templateSrc) {
    Remove-Item -Recurse -Force $templateDst -ErrorAction SilentlyContinue
    Remove-Item -Recurse -Force "$output\net10.0\MobileTemplate" -ErrorAction SilentlyContinue
    Copy-Item -Recurse $templateSrc $templateDst -Force
    Copy-Item -Recurse $templateSrc "$output\net10.0\MobileTemplate" -Force -ErrorAction SilentlyContinue
    Remove-Item -Recurse -Force "$templateDst\bin", "$templateDst\obj", "$output\net10.0\MobileTemplate\bin", "$output\net10.0\MobileTemplate\obj" -ErrorAction SilentlyContinue
    Get-ChildItem $templateDst -Recurse -Filter "*.tmp" -ErrorAction SilentlyContinue | Remove-Item -Force -ErrorAction SilentlyContinue
    Get-ChildItem "$output\net10.0\MobileTemplate" -Recurse -Filter "*.tmp" -ErrorAction SilentlyContinue | Remove-Item -Force -ErrorAction SilentlyContinue
    Write-Host "MobileTemplate copied to output" -ForegroundColor Green
}

Write-Host ""
Write-Host "Done! a+ executable created in: $output" -ForegroundColor Green
Write-Host ""
Write-Host "Usage: $output\a+ <file.a>  or  $output\a+ <folder>"
