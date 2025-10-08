using System.Diagnostics;
using System.Reflection;
using System.Runtime;
using System.Runtime.InteropServices;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json.Converters;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers().AddNewtonsoftJson(o =>
{
    o.SerializerSettings.Converters.Add(new StringEnumConverter());
    o.SerializerSettings.NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore;
});

builder.Services.AddHttpClient("default")
    .ConfigureHttpClient(c =>
    {
        c.Timeout = TimeSpan.FromSeconds(100);
        c.DefaultRequestHeaders.UserAgent.ParseAdd("DiagnosticsTool/1.0");
    });

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "DiagnosticsTool API",
        Version = "v1",
        Description = "Diagnostics endpoints wrapping EPCIS traceability resolver functions with detailed diagnostics output"
    });
});

builder.Services.AddCors(o =>
{
    o.AddPolicy("Default", p => p.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod());
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "DiagnosticsTool API v1");
        c.RoutePrefix = string.Empty;
    });
}

app.UseCors("Default");
app.UseHttpsRedirection();
app.MapControllers();

app.Run();

// Added for integration testing via WebApplicationFactory in test project.
public partial class Program { }