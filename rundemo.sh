#!/bin/bash
set -e

echo "Building and running CSClay Raylib Demo..."
dotnet run --project examples/CSClay.Demo/CSClay.Demo.csproj
