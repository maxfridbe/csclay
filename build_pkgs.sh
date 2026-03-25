#!/bin/bash
set -e

OUTPUT_DIR="artifacts"

echo "Cleaning previous artifacts..."
rm -rf $OUTPUT_DIR
mkdir -p $OUTPUT_DIR

# The version is automatically picked up from version.txt via Directory.Build.props
echo "Building and packing NuGet packages with symbols..."
dotnet pack src/CSClay/CSClay.csproj -c Release -p:IncludeSymbols=true -p:SymbolPackageFormat=snupkg -o $OUTPUT_DIR
dotnet pack src/CSClay.Renderers.SkiaSharp/CSClay.Renderers.SkiaSharp.csproj -c Release -p:IncludeSymbols=true -p:SymbolPackageFormat=snupkg -o $OUTPUT_DIR
dotnet pack src/CSClay.Renderers.Raylib/CSClay.Renderers.Raylib.csproj -c Release -p:IncludeSymbols=true -p:SymbolPackageFormat=snupkg -o $OUTPUT_DIR

echo "Packages built successfully:"
ls -l $OUTPUT_DIR
