// Program.cs - Updated with project services
using Microsoft.EntityFrameworkCore;
using RequirementsAnalyzer.API.Configuration;
using RequirementsAnalyzer.API.Services;

var builder = WebApplication.CreateBuilder(args);

// Configure logging to reduce double logging
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.SetMinimumLevel(LogLevel.Information);

// Filter out excessive logging from Microsoft
builder.Logging.AddFilter("Microsoft.AspNetCore", LogLevel.Warning);
builder.Logging.AddFilter("Microsoft.EntityFrameworkCore", LogLevel.Warning);

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c => {
    c.SwaggerDoc("v1", new() {
        Title = "Requirements Analyzer API",
        Version = "v1",
        Description = "API for analyzing and enhancing software requirements quality with project management"
    });

    // Enable XML documentation
    var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
    {
        c.IncludeXmlComments(xmlPath);
    }
});

// Configure Perplexity API settings
builder.Services.Configure<PerplexityConfig>(
    builder.Configuration.GetSection(PerplexityConfig.SectionName));

// Add HTTP client for Perplexity API
builder.Services.AddHttpClient<IPerplexityService, PerplexityService>();

// Add project service (in-memory implementation for now)
builder.Services.AddScoped<IProjectService, ProjectService>();

// Add CORS for React app - support both development ports
builder.Services.AddCors(options => {
    options.AddPolicy("ReactApp", policy => {
        policy.WithOrigins("http://localhost:3000", "http://localhost:3001")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

// Add Entity Framework (commented out database for now)
// Uncomment when you want to add database
/*
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));
*/

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c => {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Requirements Analyzer API v1");
        c.RoutePrefix = "swagger";
    });

    // Add development exception page
    app.UseDeveloperExceptionPage();
}

// Enable CORS before other middleware
app.UseCors("ReactApp");

// Add request logging middleware for debugging
app.Use(async (context, next) => {
    var logger = context.RequestServices.GetRequiredService<ILogger<Program>>();
    logger.LogInformation("{Method} {Path} from {RemoteIP}",
        context.Request.Method,
        context.Request.Path,
        context.Connection.RemoteIpAddress);

    await next();
});

app.UseAuthorization();
app.MapControllers();

// Add a simple root endpoint
app.MapGet("/", () => new {
    message = "Requirements Analyzer API is running",
    version = "1.0.0",
    timestamp = DateTime.UtcNow,
    swagger = "/swagger",
    features = new[] { "Project Management", "Requirements Analysis", "Quality Enhancement" }
});

// Improved startup logging
var logger = app.Services.GetRequiredService<ILogger<Program>>();
logger.LogInformation("Requirements Analyzer API starting up...");
logger.LogInformation("Environment: {Environment}", app.Environment.EnvironmentName);

// Get the configured URLs
var urls = builder.Configuration["urls"] ?? "http://localhost:5074";
logger.LogInformation("API available at: {Urls}", urls);
logger.LogInformation("Swagger UI: {Url}/swagger", urls.Split(';')[0]);
logger.LogInformation("Features: Project Management, Requirements Analysis, Quality Enhancement");

app.Run();