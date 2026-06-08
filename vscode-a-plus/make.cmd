@echo off
setlocal
set "bat_dir=%~dp0"
set "csproj=%bat_dir%runner\A_Plus_Console.csproj"
if not exist "%csproj%" (
  for /f "delims=" %%i in ('dir /s /b "%bat_dir%runner\*.csproj" 2^>nul') do set csproj=%%~fi
)
if not exist "%csproj%" (echo Error: No .csproj found in runner & exit /b 1)
if /I "%~1"=="A+" (
  dotnet run -f net10.0 --project "%csproj%" -- make A+
  exit /b %ERRORLEVEL%
)
if /I "%~1"=="package" (
  dotnet run -f net10.0 --project "%csproj%" -- %*
  exit /b %ERRORLEVEL%
)
echo Usage: make A+          Create a full starter project
echo        make package ^<script.a^> [name] -os [exe^|apk^|ios^|web^|linux^|macos^|all]
exit /b 1
