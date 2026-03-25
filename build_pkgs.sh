#!/bin/bash
set -e

VERSION=$1
if [ -z "$VERSION" ]; then
  echo "Error: No version specified."
  echo "Usage: ./build_pkgs.sh <version>"
  exit 1
fi

OUTPUT_DIR="artifacts"

echo "Cleaning previous artifacts..."
rm -rf $OUTPUT_DIR
mkdir -p $OUTPUT_DIR

echo "Building and packing NuGet packages version $VERSION with symbols..."
dotnet pack src/CSClay/CSClay.csproj -c Release -p:Version=$VERSION -p:IncludeSymbols=true -p:SymbolPackageFormat=snupkg -o $OUTPUT_DIR
dotnet pack src/CSClay.Renderers.SkiaSharp/CSClay.Renderers.SkiaSharp.csproj -c Release -p:Version=$VERSION -p:IncludeSymbols=true -p:SymbolPackageFormat=snupkg -o $OUTPUT_DIR
dotnet pack src/CSClay.Renderers.Raylib/CSClay.Renderers.Raylib.csproj -c Release -p:Version=$VERSION -p:IncludeSymbols=true -p:SymbolPackageFormat=snupkg -o $OUTPUT_DIR

echo "Packages built successfully:"
ls -l $OUTPUT_DIR
