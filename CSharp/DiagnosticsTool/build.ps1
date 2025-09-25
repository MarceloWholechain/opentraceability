# PowerShell build script for DiagnosticsTool

param(
    [string]$Configuration = "Release",
    [switch]$Docker,
    [switch]$Push,
    [string]$Tag = "latest"
)

Write-Host "Building DiagnosticsTool..." -ForegroundColor Green

# Check if .NET 9 SDK is installed
$dotnetVersion = dotnet --version
Write-Host "Using .NET SDK version: $dotnetVersion" -ForegroundColor Yellow

if ($dotnetVersion -notlike "9.*") {
    Write-Warning "This project requires .NET 9 SDK. Current version: $dotnetVersion"
    Write-Host "Download from: https://dotnet.microsoft.com/download/dotnet/9.0" -ForegroundColor Cyan
}

try {
    # Restore dependencies
    Write-Host "Restoring NuGet packages..." -ForegroundColor Blue
    dotnet restore
    if ($LASTEXITCODE -ne 0) { throw "Restore failed" }

    # Build the project
    Write-Host "Building project..." -ForegroundColor Blue
    dotnet build -c $Configuration --no-restore
    if ($LASTEXITCODE -ne 0) { throw "Build failed" }

    # Run tests if any exist
    if (Test-Path "*.Tests.csproj") {
        Write-Host "Running tests..." -ForegroundColor Blue
        dotnet test -c $Configuration --no-build
        if ($LASTEXITCODE -ne 0) { throw "Tests failed" }
    }

    # Publish the application
    Write-Host "Publishing application..." -ForegroundColor Blue
    dotnet publish -c $Configuration -o ./publish --no-build
    if ($LASTEXITCODE -ne 0) { throw "Publish failed" }

    Write-Host "✅ Build completed successfully!" -ForegroundColor Green
    Write-Host "Published to: ./publish" -ForegroundColor Yellow

    # Docker build if requested
    if ($Docker) {
        Write-Host "Building Docker image..." -ForegroundColor Blue
        
        $imageName = "diagnosticstool:$Tag"
        docker build -t $imageName -f Dockerfile ..
        
        if ($LASTEXITCODE -eq 0) {
            Write-Host "✅ Docker image built successfully: $imageName" -ForegroundColor Green
            
            if ($Push) {
                Write-Host "Pushing Docker image..." -ForegroundColor Blue
                docker push $imageName
                if ($LASTEXITCODE -eq 0) {
                    Write-Host "✅ Docker image pushed successfully!" -ForegroundColor Green
                } else {
                    Write-Error "Failed to push Docker image"
                }
            }
        } else {
            Write-Error "Failed to build Docker image"
        }
    }

} catch {
    Write-Error "Build failed: $_"
    exit 1
}

Write-Host ""
Write-Host "🚀 To run the application:" -ForegroundColor Cyan
Write-Host "   dotnet run" -ForegroundColor White
Write-Host ""
Write-Host "🐳 To run with Docker:" -ForegroundColor Cyan
Write-Host "   docker-compose up -d" -ForegroundColor White
Write-Host ""
Write-Host "📖 API Documentation available at:" -ForegroundColor Cyan
Write-Host "   http://localhost:5000 (Docker)" -ForegroundColor White
Write-Host "   https://localhost:5001 (Local)" -ForegroundColor White
