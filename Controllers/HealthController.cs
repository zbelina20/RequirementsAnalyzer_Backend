using Microsoft.AspNetCore.Mvc;
using Npgsql;

namespace RequirementsAnalyzer.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class HealthController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<HealthController> _logger;

        public HealthController(IConfiguration configuration, ILogger<HealthController> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        /// <summary>
        /// Basic health check endpoint
        /// </summary>
        [HttpGet]
        public IActionResult GetHealth()
        {
            return Ok(new {
                status = "healthy",
                message = "Requirements Analyzer API is running",
                timestamp = DateTime.UtcNow,
                environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Development"
            });
        }

        /// <summary>
        /// Database connection health check
        /// </summary>
        [HttpGet("database")]
        public async Task<IActionResult> GetDatabaseHealth()
        {
            try
            {
                var connectionString = _configuration.GetConnectionString("DefaultConnection");
                if (string.IsNullOrEmpty(connectionString))
                {
                    return StatusCode(500, new {
                        status = "unhealthy",
                        message = "Database connection string not configured",
                        timestamp = DateTime.UtcNow
                    });
                }

                using var connection = new NpgsqlConnection(connectionString);
                await connection.OpenAsync();

                using var command = new NpgsqlCommand("SELECT current_database(), version()", connection);
                using var reader = await command.ExecuteReaderAsync();

                if (await reader.ReadAsync())
                {
                    var database = reader.GetString(0);
                    var version = reader.GetString(1);

                    return Ok(new {
                        status = "healthy",
                        message = "Database connection successful",
                        database = database,
                        version = version.Split(',')[0].Trim(),
                        timestamp = DateTime.UtcNow
                    });
                }

                return StatusCode(500, new {
                    status = "unhealthy",
                    message = "Database query failed",
                    timestamp = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Database health check failed");
                return StatusCode(500, new {
                    status = "unhealthy",
                    message = $"Database connection failed: {ex.Message}",
                    timestamp = DateTime.UtcNow
                });
            }
        }
    }
}