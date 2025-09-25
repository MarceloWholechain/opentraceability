#!/bin/bash

# Bash build script for DiagnosticsTool

set -e

CONFIGURATION="Release"
DOCKER=false
PUSH=false
TAG="latest"

# Parse command line arguments
while [[ $# -gt 0 ]]; do
    case $1 in
        -c|--configuration)
            CONFIGURATION="$2"
            shift 2
            ;;
        -d|--docker)
            DOCKER=true
            shift
            ;;
        -p|--push)
            PUSH=true
            shift
            ;;
        -t|--tag)
            TAG="$2"
            shift 2
            ;;
        -h|--help)
            echo "Usage: $0 [options]"
            echo "Options:"
            echo "  -c, --configuration  Build configuration (Debug|Release) [default: Release]"
            echo "  -d, --docker         Build Docker image"
            echo "  -p, --push           Push Docker image (requires -d)"
            echo "  -t, --tag            Docker image tag [default: latest]"
            echo "  -h, --help           Show this help message"
            exit 0
            ;;
        *)
            echo "Unknown option: $1"
            exit 1
            ;;
    esac
done

echo "🔨 Building DiagnosticsTool..."

# Check if .NET 9 SDK is installed
DOTNET_VERSION=$(dotnet --version)
echo "📋 Using .NET SDK version: $DOTNET_VERSION"

if [[ ! $DOTNET_VERSION == 9.* ]]; then
    echo "⚠️  This project requires .NET 9 SDK. Current version: $DOTNET_VERSION"
    echo "📥 Download from: https://dotnet.microsoft.com/download/dotnet/9.0"
fi

# Restore dependencies
echo "📦 Restoring NuGet packages..."
dotnet restore

# Build the project
echo "🔧 Building project..."
dotnet build -c $CONFIGURATION --no-restore

# Run tests if any exist
if ls *.Tests.csproj 1> /dev/null 2>&1; then
    echo "🧪 Running tests..."
    dotnet test -c $CONFIGURATION --no-build
fi

# Publish the application
echo "📤 Publishing application..."
dotnet publish -c $CONFIGURATION -o ./publish --no-build

echo "✅ Build completed successfully!"
echo "📁 Published to: ./publish"

# Docker build if requested
if [ "$DOCKER" = true ]; then
    echo "🐳 Building Docker image..."
    
    IMAGE_NAME="diagnosticstool:$TAG"
    docker build -t $IMAGE_NAME -f Dockerfile ..
    
    echo "✅ Docker image built successfully: $IMAGE_NAME"
    
    if [ "$PUSH" = true ]; then
        echo "📤 Pushing Docker image..."
        docker push $IMAGE_NAME
        echo "✅ Docker image pushed successfully!"
    fi
fi

echo ""
echo "🚀 To run the application:"
echo "   dotnet run"
echo ""
echo "🐳 To run with Docker:"
echo "   docker-compose up -d"
echo ""
echo "📖 API Documentation available at:"
echo "   http://localhost:5000 (Docker)"
echo "   https://localhost:5001 (Local)"
