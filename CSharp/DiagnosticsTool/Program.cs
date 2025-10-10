using DiagnosticsTool;

var builder = WebApplication.CreateBuilder(args);

Startup startup = new(builder.Configuration);
startup.ConfigureServices(builder.Services);

var app = builder.Build();

startup.Configure(app, app.Environment);

app.Run();

// Added for integration testing via WebApplicationFactory in test project.
public partial class Program { }