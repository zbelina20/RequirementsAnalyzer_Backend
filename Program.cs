using Microsoft.EntityFrameworkCore;
using RequirementsAnalyzer.API.Data;
using RequirementsAnalyzer.API.Services;
using RequirementsAnalyzer.API.Configuration;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
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

// Add Entity Framework with PostgreSQL
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.Configure<PerplexityConfig>(
    builder.Configuration.GetSection("PerplexityApi"));

// Add services
builder.Services.AddScoped<IPerplexityService, PerplexityService>();
builder.Services.AddScoped<IProjectService, ProjectService>();
builder.Services.AddHttpClient<IPerplexityService, PerplexityService>();

// Add CORS for your React frontend
builder.Services.AddCors(options => {
    options.AddPolicy("AllowReactApp", policy => {
        policy.WithOrigins("http://localhost:3000")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c => {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Requirements Analyzer API v1");
        c.RoutePrefix = string.Empty; // This serves Swagger UI at the app's root
    });
}

app.UseHttpsRedirection();
app.UseCors("AllowReactApp");
app.UseAuthorization();
app.MapControllers();

// Auto-migrate database on startup
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();

    try
    {
        context.Database.Migrate();
        logger.LogInformation("Database migration completed successfully.");
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "An error occurred while migrating the database.");
    }
}

// Test configuration on startup (optional)
using (var scope = app.Services.CreateScope())
{
    var config = scope.ServiceProvider.GetRequiredService<Microsoft.Extensions.Options.IOptions<PerplexityConfig>>();
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();

    if (string.IsNullOrEmpty(config.Value.ApiKey))
    {
        logger.LogWarning("Perplexity API key is not configured. Please set PerplexityApi:ApiKey in user secrets or appsettings.");
    }
    else
    {
        logger.LogInformation("Perplexity API key is configured. Model: {Model}", config.Value.Model);
    }
}

app.Run();