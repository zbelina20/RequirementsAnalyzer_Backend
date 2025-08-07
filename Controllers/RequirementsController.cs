using Microsoft.AspNetCore.Mvc;
using RequirementsAnalyzer.API.DTOs;
using RequirementsAnalyzer.API.Services;

namespace RequirementsAnalyzer.API.Controllers
{
    /// <summary>
    /// Controller for managing requirements analysis and enhancement operations
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class RequirementsController : ControllerBase
    {
        private readonly ILogger<RequirementsController> _logger;
        private readonly IPerplexityService _perplexityService;

        /// <summary>
        /// Initializes a new instance of the RequirementsController
        /// </summary>
        /// <param name="logger">Logger instance</param>
        /// <param name="perplexityService">Perplexity service for AI operations</param>
        public RequirementsController(
            ILogger<RequirementsController> logger,
            IPerplexityService perplexityService)
        {
            _logger = logger;
            _perplexityService = perplexityService;
        }

        /// <summary>
        /// Health check endpoint to verify API and Perplexity service status
        /// </summary>
        /// <returns>Health status information</returns>
        [HttpGet("health")]
        public async Task<IActionResult> Health()
        {
            var isPerplexityConnected = await _perplexityService.TestConnectionAsync();
            var status = isPerplexityConnected ? "healthy" : "degraded";

            return Ok(new {
                status = status,
                message = "Requirements API is running",
                perplexityConnected = isPerplexityConnected,
                timestamp = DateTime.UtcNow
            });
        }

        /// <summary>
        /// Analyzes a requirement for quality issues
        /// </summary>
        /// <param name="request">The requirement analysis request</param>
        /// <returns>Analysis results with quality score and issues</returns>
        [HttpPost("analyze")]
        public async Task<IActionResult> AnalyzeRequirement([FromBody] AnalyzeRequest request)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(request.Text))
                {
                    return BadRequest(new { error = "Requirement text is required" });
                }

                _logger.LogInformation("Analyzing requirement with {Length} characters", request.Text.Length);

                var result = await _perplexityService.AnalyzeRequirementAsync(request.Text);

                _logger.LogInformation("Analysis completed - Score: {Score}, Issues: {IssueCount}",
                    result.OverallScore, result.Issues.Count);

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error analyzing requirement");
                return StatusCode(500, new { error = "Internal server error", message = ex.Message });
            }
        }

        /// <summary>
        /// Enhances a requirement based on identified quality issues
        /// </summary>
        /// <param name="request">The requirement enhancement request</param>
        /// <returns>Enhanced requirement versions</returns>
        [HttpPost("enhance")]
        public async Task<IActionResult> EnhanceRequirement([FromBody] EnhanceRequest request)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(request.Text))
                {
                    return BadRequest(new { error = "Requirement text is required" });
                }

                _logger.LogInformation("Enhancing requirement with {IssueCount} identified issues",
                    request.Issues?.Count ?? 0);

                var result = await _perplexityService.EnhanceRequirementAsync(request.Text, request.Issues);

                _logger.LogInformation("Enhancement completed - {EnhancementCount} versions generated",
                    result.Enhancements.Count);

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error enhancing requirement");
                return StatusCode(500, new { error = "Internal server error", message = ex.Message });
            }
        }

        /// <summary>
        /// Batch analyzes multiple requirements
        /// </summary>
        /// <param name="request">The batch analysis request</param>
        /// <returns>Analysis results for all requirements</returns>
        [HttpPost("batch-analyze")]
        public async Task<IActionResult> BatchAnalyze([FromBody] BatchAnalyzeRequest request)
        {
            try
            {
                if (request.Requirements == null || !request.Requirements.Any())
                {
                    return BadRequest(new { error = "At least one requirement is required" });
                }

                if (request.Requirements.Any(string.IsNullOrWhiteSpace))
                {
                    return BadRequest(new { error = "All requirements must contain text" });
                }

                _logger.LogInformation("Starting batch analysis of {Count} requirements",
                    request.Requirements.Count);

                var results = new List<AnalysisResponse>();

                foreach (var requirement in request.Requirements)
                {
                    try
                    {
                        var result = await _perplexityService.AnalyzeRequirementAsync(requirement);
                        results.Add(result);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Failed to analyze requirement: {Requirement}",
                            requirement[..Math.Min(50, requirement.Length)]);

                        // Add a failed analysis result
                        results.Add(new AnalysisResponse {
                            OverallScore = 0,
                            Issues = new List<QualityIssueDto>
                            {
                                new QualityIssueDto
                                {
                                    Type = "processing_error",
                                    Severity = "critical",
                                    Description = "Failed to process this requirement",
                                    ProblematicText = requirement[..Math.Min(20, requirement.Length)] + "...",
                                    Suggestion = "Please review the requirement format and try again"
                                }
                            },
                            AnalyzedAt = DateTime.UtcNow.ToString("O")
                        });
                    }
                }

                _logger.LogInformation("Batch analysis completed - {SuccessCount}/{TotalCount} successful",
                    results.Count(r => r.OverallScore > 0), request.Requirements.Count);

                return Ok(results);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in batch analysis");
                return StatusCode(500, new { error = "Internal server error", message = ex.Message });
            }
        }
    }

    /// <summary>
    /// Request model for batch requirement analysis
    /// </summary>
    public class BatchAnalyzeRequest
    {
        /// <summary>
        /// List of requirements to analyze
        /// </summary>
        public List<string> Requirements { get; set; } = new();
    }
}