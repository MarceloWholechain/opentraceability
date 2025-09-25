using System.Diagnostics;
using System.Reflection;
using System.Runtime;
using System.Runtime.InteropServices;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "DiagnosticsTool API",
        Version = "v1",
        Description = "A comprehensive diagnostics API for system and application monitoring"
    });
});

// Add health checks
builder.Services.AddHealthChecks();

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "DiagnosticsTool API v1");
        c.RoutePrefix = string.Empty; // Set Swagger UI at the app's root
    });
}

app.UseHttpsRedirection();

// Health check endpoint
app.MapHealthChecks("/health");

// System Information Endpoints
app.MapGet("/diagnostics/system", () =>
{
    return new
    {
        Environment = new
        {
            MachineName = Environment.MachineName,
            OSVersion = Environment.OSVersion.ToString(),
            Platform = RuntimeInformation.OSDescription,
            Architecture = RuntimeInformation.OSArchitecture.ToString(),
            ProcessArchitecture = RuntimeInformation.ProcessArchitecture.ToString(),
            FrameworkDescription = RuntimeInformation.FrameworkDescription,
            ProcessorCount = Environment.ProcessorCount,
            Is64BitOperatingSystem = Environment.Is64BitOperatingSystem,
            Is64BitProcess = Environment.Is64BitProcess,
            UserName = Environment.UserName,
            UserDomainName = Environment.UserDomainName,
            WorkingSet = Environment.WorkingSet,
            SystemPageSize = Environment.SystemPageSize
        },
        Timestamp = DateTime.UtcNow
    };
})
.WithName("GetSystemInfo")
.WithSummary("Get comprehensive system information")
.WithOpenApi();

app.MapGet("/diagnostics/runtime", () =>
{
    var process = Process.GetCurrentProcess();
    var gcInfo = GC.GetGCMemoryInfo();
    
    return new
    {
        Runtime = new
        {
            DotNetVersion = Environment.Version.ToString(),
            FrameworkDescription = RuntimeInformation.FrameworkDescription,
            RuntimeIdentifier = RuntimeInformation.RuntimeIdentifier,
            ProcessId = process.Id,
            ProcessName = process.ProcessName,
            StartTime = process.StartTime,
            UpTime = DateTime.Now - process.StartTime,
            ThreadCount = process.Threads.Count,
            HandleCount = process.HandleCount
        },
        Memory = new
        {
            WorkingSet = process.WorkingSet64,
            PrivateMemorySize = process.PrivateMemorySize64,
            VirtualMemorySize = process.VirtualMemorySize64,
            PagedMemorySize = process.PagedMemorySize64,
            NonpagedSystemMemorySize = process.NonpagedSystemMemorySize64,
            PagedSystemMemorySize = process.PagedSystemMemorySize64,
            PeakWorkingSet = process.PeakWorkingSet64,
            PeakVirtualMemorySize = process.PeakVirtualMemorySize64,
            PeakPagedMemorySize = process.PeakPagedMemorySize64
        },
        GarbageCollection = new
        {
            TotalMemory = GC.GetTotalMemory(false),
            Gen0Collections = GC.CollectionCount(0),
            Gen1Collections = GC.CollectionCount(1),
            Gen2Collections = GC.CollectionCount(2),
            TotalAvailableMemoryBytes = gcInfo.TotalAvailableMemoryBytes,
            HeapSizeBytes = gcInfo.HeapSizeBytes,
            MemoryLoadBytes = gcInfo.MemoryLoadBytes,
            HighMemoryLoadThresholdBytes = gcInfo.HighMemoryLoadThresholdBytes
        },
        Timestamp = DateTime.UtcNow
    };
})
.WithName("GetRuntimeInfo")
.WithSummary("Get detailed runtime and memory information")
.WithOpenApi();

app.MapGet("/diagnostics/assembly", () =>
{
    var assembly = Assembly.GetExecutingAssembly();
    var entryAssembly = Assembly.GetEntryAssembly();
    
    return new
    {
        ExecutingAssembly = new
        {
            FullName = assembly.FullName,
            Location = assembly.Location,
            CodeBase = assembly.EscapedCodeBase,
            ImageRuntimeVersion = assembly.ImageRuntimeVersion,
            Version = assembly.GetName().Version?.ToString(),
            CreationTime = File.GetCreationTime(assembly.Location),
            LastWriteTime = File.GetLastWriteTime(assembly.Location)
        },
        EntryAssembly = entryAssembly != null ? new
        {
            FullName = entryAssembly.FullName,
            Location = entryAssembly.Location,
            Version = entryAssembly.GetName().Version?.ToString()
        } : null,
        LoadedAssemblies = AppDomain.CurrentDomain.GetAssemblies()
            .Select(a => new
            {
                Name = a.GetName().Name,
                Version = a.GetName().Version?.ToString(),
                Location = a.IsDynamic ? "Dynamic" : a.Location
            })
            .OrderBy(a => a.Name)
            .ToList(),
        Timestamp = DateTime.UtcNow
    };
})
.WithName("GetAssemblyInfo")
.WithSummary("Get information about loaded assemblies")
.WithOpenApi();

app.MapGet("/diagnostics/environment", () =>
{
    return new
    {
        EnvironmentVariables = Environment.GetEnvironmentVariables()
            .Cast<System.Collections.DictionaryEntry>()
            .ToDictionary(e => e.Key.ToString()!, e => e.Value?.ToString()),
        CommandLineArgs = Environment.GetCommandLineArgs(),
        CurrentDirectory = Environment.CurrentDirectory,
        SystemDirectory = Environment.SystemDirectory,
        TempPath = Path.GetTempPath(),
        LogicalDrives = Environment.GetLogicalDrives(),
        SpecialFolders = new
        {
            ApplicationData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            CommonApplicationData = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
            LocalApplicationData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            ProgramFiles = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles),
            System = Environment.GetFolderPath(Environment.SpecialFolder.System),
            UserProfile = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile)
        },
        Timestamp = DateTime.UtcNow
    };
})
.WithName("GetEnvironmentInfo")
.WithSummary("Get environment variables and system paths")
.WithOpenApi();

app.MapGet("/diagnostics/performance", () =>
{
    var process = Process.GetCurrentProcess();
    var stopwatch = Stopwatch.StartNew();
    
    // Simple CPU usage calculation
    var startTime = DateTime.UtcNow;
    var startCpuUsage = process.TotalProcessorTime;
    Thread.Sleep(100); // Small delay for measurement
    var endTime = DateTime.UtcNow;
    var endCpuUsage = process.TotalProcessorTime;
    
    var cpuUsedMs = (endCpuUsage - startCpuUsage).TotalMilliseconds;
    var totalMsPassed = (endTime - startTime).TotalMilliseconds;
    var cpuUsageTotal = cpuUsedMs / (Environment.ProcessorCount * totalMsPassed);
    
    stopwatch.Stop();
    
    return new
    {
        Performance = new
        {
            CpuUsagePercent = Math.Round(cpuUsageTotal * 100, 2),
            TotalProcessorTime = process.TotalProcessorTime.TotalMilliseconds,
            UserProcessorTime = process.UserProcessorTime.TotalMilliseconds,
            PrivilegedProcessorTime = process.PrivilegedProcessorTime.TotalMilliseconds,
            ResponseTimeMs = stopwatch.ElapsedMilliseconds
        },
        Timestamp = DateTime.UtcNow
    };
})
.WithName("GetPerformanceInfo")
.WithSummary("Get current performance metrics")
.WithOpenApi();

app.MapPost("/diagnostics/gc", () =>
{
    var before = GC.GetTotalMemory(false);
    GC.Collect();
    GC.WaitForPendingFinalizers();
    GC.Collect();
    var after = GC.GetTotalMemory(false);
    
    return new
    {
        Message = "Garbage collection forced",
        MemoryBefore = before,
        MemoryAfter = after,
        MemoryFreed = before - after,
        Timestamp = DateTime.UtcNow
    };
})
.WithName("ForceGarbageCollection")
.WithSummary("Force garbage collection and return memory statistics")
.WithOpenApi();

app.MapGet("/", () => "DiagnosticsTool API is running. Visit /swagger for API documentation.")
    .WithName("Root")
    .WithSummary("API root endpoint")
    .WithOpenApi();

app.Run();