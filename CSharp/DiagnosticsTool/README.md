# DiagnosticsTool API

A comprehensive cross-platform ASP.NET Core Web API for system and application diagnostics, built with .NET 9.

## Features

- **System Information**: Get detailed OS, hardware, and environment information
- **Runtime Diagnostics**: Monitor .NET runtime, memory usage, and garbage collection
- **Assembly Information**: Inspect loaded assemblies and their metadata
- **Performance Metrics**: Real-time CPU usage and performance monitoring
- **Environment Variables**: Access to system and application environment variables
- **Health Checks**: Built-in health monitoring endpoints
- **Swagger/OpenAPI**: Interactive API documentation
- **Docker Support**: Full containerization with multi-stage builds

## API Endpoints

### Core Endpoints
- `GET /` - API root with basic information
- `GET /health` - Health check endpoint
- `GET /swagger` - Interactive API documentation

### Diagnostic Endpoints
- `GET /diagnostics/system` - Comprehensive system information
- `GET /diagnostics/runtime` - .NET runtime and memory diagnostics
- `GET /diagnostics/assembly` - Assembly and dependency information
- `GET /diagnostics/environment` - Environment variables and paths
- `GET /diagnostics/performance` - Real-time performance metrics
- `POST /diagnostics/gc` - Force garbage collection and get memory stats

## Quick Start

### Running Locally

1. Ensure you have .NET 9 SDK installed
2. Navigate to the project directory:
   ```bash
   cd DiagnosticsTool
   ```
3. Run the application:
   ```bash
   dotnet run
   ```
4. Open your browser to `https://localhost:5001` or `http://localhost:5000`

### Docker Deployment

#### Build and Run with Docker

```bash
# Build the Docker image
docker build -t diagnosticstool -f Dockerfile ..

# Run the container
docker run -p 5000:8080 -p 5001:8081 diagnosticstool
```

#### Using Docker Compose

```bash
# Start all services (API + Nginx proxy)
docker-compose up -d

# View logs
docker-compose logs -f

# Stop services
docker-compose down
```

#### Docker Compose Services

- **diagnosticstool**: Main API service (ports 5000:8080, 5001:8081)
- **nginx**: Reverse proxy with load balancing and caching (ports 80, 443)

## Configuration

### Environment Variables

- `ASPNETCORE_ENVIRONMENT`: Set to `Development`, `Staging`, or `Production`
- `ASPNETCORE_URLS`: Configure listening URLs (default: `http://+:8080;https://+:8081`)

### Docker Environment

The Docker container runs as a non-root user (`appuser`) for security and includes:
- Health checks every 30 seconds
- Optimized multi-stage build process
- Security headers via Nginx proxy
- Gzip compression
- Static file caching

## Security Features

- Non-root container execution
- Security headers (X-Frame-Options, X-XSS-Protection, etc.)
- Content Security Policy
- Gzip compression
- Static file caching with long expiration

## Monitoring and Health Checks

The application includes built-in health checks accessible at `/health`. Docker containers automatically monitor this endpoint and restart if unhealthy.

## Development

### Project Structure

```
DiagnosticsTool/
├── DiagnosticsTool.csproj    # Project file (.NET 9)
├── Program.cs                # Main application with API endpoints
├── Dockerfile               # Multi-stage Docker build
├── docker-compose.yml       # Container orchestration
├── nginx.conf              # Reverse proxy configuration
├── .dockerignore           # Docker build exclusions
└── README.md               # This file
```

### Building

```bash
# Restore dependencies
dotnet restore

# Build the project
dotnet build

# Run tests (if any)
dotnet test

# Publish for deployment
dotnet publish -c Release
```

## API Documentation

Once running, visit `/swagger` for interactive API documentation with:
- Request/response examples
- Parameter descriptions
- Try-it-out functionality
- Schema definitions

## Cross-Platform Support

This application is designed to run on:
- **Linux** (Ubuntu, Alpine, RHEL, etc.)
- **Windows** (Windows Server, Windows 10/11)
- **macOS** (Intel and Apple Silicon)

Docker images are based on Microsoft's official .NET 9 runtime images for optimal compatibility and security updates.

## License

This project is part of the OpenTraceability suite and follows the same licensing terms.
