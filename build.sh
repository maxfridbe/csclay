#!/bin/bash
set -e

# Configuration
PROJECT_NAME="CSClay"
SRC_DIR="src/CSClay"
OUTPUT_DIR="artifacts"

echo "Building $PROJECT_NAME..."

# Clean previous artifacts
rm -rf $OUTPUT_DIR
mkdir -p $OUTPUT_DIR

# 1. Build and Test
echo "Running tests..."
dotnet test CSClay.slnx -c Release

# 2. Pack NuGet Packages
echo "Creating NuGet packages..."
dotnet pack src/CSClay/CSClay.csproj -c Release -o $OUTPUT_DIR
dotnet pack src/CSClay.Renderers.SkiaSharp/CSClay.Renderers.SkiaSharp.csproj -c Release -o $OUTPUT_DIR
dotnet pack src/CSClay.Renderers.Raylib/CSClay.Renderers.Raylib.csproj -c Release -o $OUTPUT_DIR

# 3. Build Demo (Optional check)
echo "Building demo..."
dotnet build examples/CSClay.Demo/CSClay.Demo.csproj -c Release

echo "Build complete! Artifacts are in $OUTPUT_DIR/"
ls -l $OUTPUT_DIR
