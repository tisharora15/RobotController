using robot_controller_api.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Prometheus;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);

// ── Configuration ──────────────────────────────────────────────────────────
// Connection string read from env var (override in Docker/CI) or appsettings
var connectionString = Environment.GetEnvironmentVariable("DB_CONNECTION_STRING")
    ?? builder.Configuration.GetConnectionString("DefaultConnection")
    ?? "Host=localhost;Database=sit331;Username=postgres;Password=prime";

// ── Services ───────────────────────────────────────────────────────────────
builder.Services.AddControllers();

// Swagger/OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new()
    {
        Title   = "Robot Controller API",
        Version = "v1",
        Description = "REST API for controlling robots on grid maps."
    });
});

// Entity Framework + Postgres
builder.Services.AddDbContext<RobotContext>(options =>
    options.UseNpgsql(connectionString));

// Dependency injection
builder.Services.AddScoped<IRobotCommandDataAccess, RobotCommandEF>();
builder.Services.AddScoped<IMapDataAccess, MapEF>();

// Health checks
builder.Services.AddHealthChecks()
    .AddNpgSql(
        connectionString,
        name: "postgres",
        failureStatus: HealthStatus.Unhealthy,
        tags: new[] { "db", "postgres" })
    .AddCheck("self", () => HealthCheckResult.Healthy("API is running"), tags: new[] { "api" });

// CORS (useful for front-end integration)
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
        policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod());
});

var app = builder.Build();

// ── Middleware ─────────────────────────────────────────────────────────────
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Robot Controller API v1");
    c.RoutePrefix = string.Empty; // Swagger at root
});

app.UseCors("AllowAll");
app.UseHttpsRedirection();

// Prometheus metrics endpoint  (/metrics)
app.UseMetricServer();
app.UseHttpMetrics();

app.MapControllers();

// Health endpoints
app.MapHealthChecks("/health", new HealthCheckOptions
{
    ResponseWriter = async (context, report) =>
    {
        context.Response.ContentType = "application/json";
        var result = JsonSerializer.Serialize(new
        {
            status  = report.Status.ToString(),
            checks  = report.Entries.Select(e => new
            {
                name    = e.Key,
                status  = e.Value.Status.ToString(),
                duration = e.Value.Duration.TotalMilliseconds
            })
        });
        await context.Response.WriteAsync(result);
    }
});

app.MapHealthChecks("/health/ready", new HealthCheckOptions
{
    Predicate = check => check.Tags.Contains("db")
});

app.MapHealthChecks("/health/live", new HealthCheckOptions
{
    Predicate = check => check.Tags.Contains("api")
});

app.Run();

// Make Program accessible to test project
public partial class Program { }