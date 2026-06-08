#!/usr/bin/env bash
# A+ Language - VS Code Extension Installer (Linux/macOS)
# Run: chmod +x install.sh && ./install.sh

set -e

EXT_DIR="$(cd "$(dirname "$0")" && pwd)"

echo "=== A+ Language Extension Installer ==="
echo ""

# 1. Build the interpreter (if dotnet is available)
echo "[1/3] Checking interpreter..."
if command -v dotnet &> /dev/null; then
    PROJ="$EXT_DIR/runner/A_Plus_Console.csproj"
    if [ -f "$PROJ" ]; then
        echo "  Building interpreter..."
        dotnet build "$PROJ" -c Release 2>/dev/null
        echo "  Build complete." 
    else
        echo "  No .csproj found at $PROJ, skipping build."
    fi
else
    echo "  dotnet not found, skipping build (console-only mode works on any .NET platform)."
fi

# 2. Install npm dependencies
echo "[2/3] Setting up dependencies..."
if [ -f "$EXT_DIR/package.json" ]; then
    if [ ! -d "$EXT_DIR/node_modules" ]; then
        cd "$EXT_DIR"
        npm install --silent 2>/dev/null
        cd "$OLDPWD"
        echo "  npm dependencies installed."
    else
        echo "  Dependencies already installed."
    fi
fi

# 3. Install VS Code extension
echo "[3/3] Installing VS Code extension..."
if command -v code &> /dev/null; then
    code --install-extension "$EXT_DIR" 2>/dev/null
    echo "  Extension installed successfully!"
else
    echo "  'code' command not found."
    echo "  To install manually, copy/symlink $EXT_DIR to ~/.vscode/extensions/a-plus/"
fi

echo ""
echo "=== Done! ===" 
echo "Open a .a or .r.a file in VS Code to start."
echo "Commands: A+: Run (Ctrl+F5), A+: Check, A+: Watch"
