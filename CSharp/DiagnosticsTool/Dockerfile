# Use the official .NET 9 runtime as the base image for the final stage
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

# Use the official .NET 9 SDK for building the application
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# Copy the project file and restore dependencies
COPY ["DiagnosticsTool/DiagnosticsTool.csproj", "DiagnosticsTool/"]
RUN dotnet restore "DiagnosticsTool/DiagnosticsTool.csproj"

# Copy the entire source code
COPY . .
WORKDIR "/src/DiagnosticsTool"

# Build the application
RUN dotnet build "DiagnosticsTool.csproj" -c $BUILD_CONFIGURATION -o /app/build

# Publish the application
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "DiagnosticsTool.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# Final stage: Create the runtime image
FROM base AS final
WORKDIR /app

# Create a non-root user for security
RUN adduser --disabled-password --gecos "" --uid 1001 appuser

# Copy the published application
COPY --from=publish /app/publish .

# Change ownership of the app directory to the appuser
RUN chown -R appuser:appuser /app

# Switch to the non-root user
USER appuser

# Set environment variables
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production

# Add health check
HEALTHCHECK --interval=30s --timeout=3s --start-period=5s --retries=3 \
    CMD curl -f http://localhost:8080/health || exit 1

# Define the entry point
ENTRYPOINT ["dotnet", "DiagnosticsTool.dll"]
