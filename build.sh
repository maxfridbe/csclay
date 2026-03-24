#!/bin/bash
set -e

# Configuration
PROJECT_NAME="Clay"
SRC_DIR="src/Clay"
OUTPUT_DIR="artifacts"

echo "Building $PROJECT_NAME..."

# Clean previous artifacts
rm -rf $OUTPUT_DIR
mkdir -p $OUTPUT_DIR

# 1. Build and Test
echo "Running tests..."
dotnet test tests/Clay.Tests/Clay.Tests.csproj -c Release

# 2. Pack NuGet Package
echo "Creating NuGet package..."
dotnet pack $SRC_DIR/$PROJECT_NAME.csproj -c Release -o $OUTPUT_DIR

# 3. Build Demo (Optional check)
echo "Building demo..."
dotnet build examples/Clay.Demo/Clay.Demo.csproj -c Release

echo "Build complete! Artifacts are in $OUTPUT_DIR/"
ls -l $OUTPUT_DIR
