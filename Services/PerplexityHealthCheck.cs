using Microsoft.Extensions.Diagnostics.HealthChecks;
using RequirementsAnalyzer.API.Services;

namespace RequirementsAnalyzer.API.Services
{
    /// <summary>
    /// Health check service for Perplexity API connectivity
    /// </summary>
    public class PerplexityHealthCheck : IHealthCheck
    {
        private readonly IPerplexityService _perplexityService;
        private readonly ILogger<PerplexityHealthCheck> _logger;

        /// <summary>
        /// Initializes a new instance of the PerplexityHealthCheck
        /// </summary>
        /// <param name="perplexityService">The Perplexity service instance</param>
        /// <param name="logger">The logger instance</param>
        public PerplexityHealthCheck(IPerplexityService perplexityService, ILogger<PerplexityHealthCheck> logger)
        {
            _perplexityService = perplexityService;
            _logger = logger;
        }

        /// <summary>
        /// Performs the health check for Perplexity API
        /// </summary>
        /// <param name="context">The health check context</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Health check result</returns>
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
}