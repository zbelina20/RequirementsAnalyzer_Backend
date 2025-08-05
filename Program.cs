// Program.cs
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using RequirementsAnalyzer.API.Configuration;
using RequirementsAnalyzer.API.Data;
using RequirementsAnalyzer.API.Services;
using Serilog;
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);

// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .WriteTo.Console()
    .WriteTo.File("logs/requirements-analyzer-.log", rollingInterval: RollingInterval.Day)
    .CreateLogger();

builder.Host.UseSerilog();

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// Configure Swagger with detailed documentation
builder.Services.AddSwaggerGen(c => {
    c.SwaggerDoc("v1", new OpenApiInfo {
        Title = "Requirements Quality Analyzer API",
        Version = "v1",
        Description = "API for analyzing and enhancing software requirements quality using AI",
        Contact = new OpenApiContact {
            Name = "Requirements Analyzer",
            Email = "zbelina20@student.foi.hr"
        }
    });

    // Include XML comments
    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
    {
        c.IncludeXmlComments(xmlPath);
    }
});

// Configure Perplexity API settings
builder.Services.Configure<PerplexityConfig>(
    builder.Configuration.GetSection(PerplexityConfig.SectionName));

// Validate configuration
var perplexityConfig = builder.Configuration.GetSection(PerplexityConfig.SectionName).Get<PerplexityConfig>();
if (string.IsNullOrEmpty(perplexityConfig?.ApiKey))
{
    Log.Warning("Perplexity API key is not configured. The service will use mock data.");
    Log.Information("To configure API key, use: dotnet user-secrets set \"PerplexityApi:ApiKey\" \"your-key\"");
}

// Add HTTP client for Perplexity API with timeout
builder.Services.AddHttpClient<IPerplexityService, PerplexityService>(client => {
    client.Timeout = TimeSpan.FromSeconds(perplexityConfig?.TimeoutSeconds ?? 30);
});

// Register services
builder.Services.AddScoped<IPerplexityService, PerplexityService>();

// Add CORS for React app
builder.Services.AddCors(options => {
    options.AddPolicy("ReactApp", policy => {
        policy.WithOrigins("http://localhost:3000", "https://localhost:3001")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

// Add health checks
builder.Services.AddHealthChecks()
    .AddCheck<PerplexityHealthCheck>("perplexity");

// Add Entity Framework (commented out database for now)
// Uncomment when you want to add database persistence
/*
builder.Services.AddDbContext<AppDbContext>(options =>
{
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
    options.UseNpgsql(connectionString);
});
*/

// Add memory cache for response caching
builder.Services.AddMemoryCache();

// Add response compression
builder.Services.AddResponseCompression(options => {
    options.EnableForHttps = true;
});

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c => {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Requirements Analyzer API V1");
        c.RoutePrefix = "swagger";
        c.DisplayRequestDuration();
    });
}
else
{
    app.UseExceptionHandler("/error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseResponseCompression();

// Use Serilog request logging
app.UseSerilogRequestLogging();

app.UseCors("ReactApp");
app.UseAuthorization();

// Add health check endpoint
app.MapHealthChecks("/health");

app.MapControllers();

// Log startup information
Log.Information("Requirements Quality Analyzer API starting up...");
Log.Information("Environment: {Environment}", app.Environment.EnvironmentName);
Log.Information("Perplexity API configured: {Configured}", !string.IsNullOrEmpty(perplexityConfig?.ApiKey));

try
{
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}

// Health check for Perplexity service
public class PerplexityHealthCheck : IHealthCheck
{
    private readonly IPerplexityService _perplexityService;
    private readonly ILogger<PerplexityHealthCheck> _logger;

    public PerplexityHealthCheck(IPerplexityService perplexityService, ILogger<PerplexityHealthCheck> logger)
    {
        _perplexityService = perplexityService;
        _logger = logger;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            var isHealthy = await _perplexityService.TestConnectionAsync();

            if (isHealthy)
            {
                return HealthCheckResult.Healthy("Perplexity API is responding");
            }
            else
            {
                return HealthCheckResult.Degraded("Perplexity API is not responding, using fallback data");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Perplexity health check failed");
            return HealthCheckResult.Unhealthy("Perplexity API health check failed", ex);
        }
    }
}