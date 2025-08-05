using Microsoft.AspNetCore.Mvc;
using RequirementsAnalyzer.API.DTOs;
using RequirementsAnalyzer.API.Services;
using System.ComponentModel.DataAnnotations;

namespace RequirementsAnalyzer.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RequirementsController : ControllerBase
    {
        private readonly ILogger<RequirementsController> _logger;
        private readonly IPerplexityService _perplexityService;

        public RequirementsController(
            ILogger<RequirementsController> logger,
            IPerplexityService perplexityService)
        {
            _logger = logger;
            _perplexityService = perplexityService;
        }

        /// <summary>
        /// Health check endpoint to verify API and external service connectivity
        /// </summary>
        [HttpGet("health")]
        public async Task<IActionResult> Health()
        {
            try
            {
                var isPerplexityConnected = await _perplexityService.TestConnectionAsync();

                return Ok(new {
                    status = "healthy",
                    message = "Requirements API is running",
                    perplexityConnected = isPerplexityConnected,
                    timestamp = DateTime.UtcNow,
                    version = "1.0.0"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Health check failed");
                return StatusCode(500, new {
                    status = "unhealthy",
                    message = "Health check failed",
                    error = ex.Message,
                    timestamp = DateTime.UtcNow
                });
            }
        }

        /// <summary>
        /// Analyze a single requirement for quality issues
        /// </summary>
        /// <param name="request">The requirement analysis request</param>
        /// <returns>Analysis results with quality score and identified issues</returns>
        [HttpPost("analyze")]
        public async Task<IActionResult> AnalyzeRequirement([FromBody] AnalyzeRequest request)
        {
            try
            {
                // Validate input
                if (string.IsNullOrWhiteSpace(request.Text))
                {
                    return BadRequest(new {
                        error = "Requirement text is required",
                        message = "Please provide a non-empty requirement text to analyze"
                    });
                }

                if (request.Text.Length > 5000)
                {
                    return BadRequest(new {
                        error = "Requirement text too long",
                        message = "Requirement text must be less than 5000 characters"
                    });
                }

                _logger.LogInformation("Analyzing requirement: {Text}", request.Text.Substring(0, Math.Min(100, request.Text.Length)));

                var result = await _perplexityService.AnalyzeRequirementAsync(request.Text);

                _logger.LogInformation("Analysis completed with score: {Score}, issues: {IssueCount}",
                    result.OverallScore, result.Issues.Count);

                return Ok(result);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Invalid input for requirement analysis");
                return BadRequest(new { error = "Invalid input", message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error analyzing requirement");
                return StatusCode(500, new {
                    error = "Internal server error",
                    message = "An error occurred while analyzing the requirement. Please try again."
                });
            }
        }

        /// <summary>
        /// Enhance a requirement based on identified quality issues
        /// </summary>
        /// <param name="request">The requirement enhancement request</param>
        /// <returns>Enhanced requirement suggestions with improvements</returns>
        [HttpPost("enhance")]
        public async Task<IActionResult> EnhanceRequirement([FromBody] EnhanceRequest request)
        {
            try
            {
                // Validate input
                if (string.IsNullOrWhiteSpace(request.Text))
                {
                    return BadRequest(new {
                        error = "Requirement text is required",
                        message = "Please provide a non-empty requirement text to enhance"
                    });
                }

                if (request.Text.Length > 5000)
                {
                    return BadRequest(new {
                        error = "Requirement text too long",
                        message = "Requirement text must be less than 5000 characters"
                    });
                }

                _logger.LogInformation("Enhancing requirement: {Text}", request.Text.Substring(0, Math.Min(100, request.Text.Length)));

                var result = await _perplexityService.EnhanceRequirementAsync(request.Text, request.Issues);

                _logger.LogInformation("Enhancement completed with {Count} versions", result.Enhancements.Count);

                return Ok(result);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Invalid input for requirement enhancement");
                return BadRequest(new { error = "Invalid input", message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error enhancing requirement");
                return StatusCode(500, new {
                    error = "Internal server error",
                    message = "An error occurred while enhancing the requirement. Please try again."
                });
            }
        }

        /// <summary>
        /// Analyze multiple requirements in batch
        /// </summary>
        /// <param name="request">Batch analysis request with multiple requirements</param>
        /// <returns>Analysis results for all requirements</returns>
        [HttpPost("batch-analyze")]
        public async Task<IActionResult> BatchAnalyze([FromBody] BatchAnalyzeRequest request)
        {
            try
            {
                // Validate input
                if (request == null || request.Requirements == null || !request.Requirements.Any())
                {
                    return BadRequest(new {
                        error = "Requirements list is required",
                        message = "Please provide a non-empty list of requirements to analyze"
                    });
                }

                if (request.Requirements.Count > 50)
                {
                    return BadRequest(new {
                        error = "Too many requirements",
                        message = "Batch analysis is limited to 50 requirements at a time"
                    });
                }

                // Validate individual requirements
                var invalidRequirements = request.Requirements
                    .Select((req, index) => new { req, index })
                    .Where(x => string.IsNullOrWhiteSpace(x.req) || x.req.Length > 5000)
                    .ToList();

                if (invalidRequirements.Any())
                {
                    return BadRequest(new {
                        error = "Invalid requirements found",
                        message = $"Requirements at positions {string.Join(", ", invalidRequirements.Select(x => x.index))} are invalid (empty or too long)"
                    });
                }

                _logger.LogInformation("Starting batch analysis for {Count} requirements", request.Requirements.Count);

                var results = await _perplexityService.BatchAnalyzeAsync(request.Requirements);

                _logger.LogInformation("Batch analysis completed for {Count} requirements", results.Count);

                return Ok(results);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Invalid input for batch analysis");
                return BadRequest(new { error = "Invalid input", message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in batch analysis");
                return StatusCode(500, new {
                    error = "Internal server error",
                    message = "An error occurred while analyzing the requirements. Please try again."
                });
            }
        }

        /// <summary>
        /// Get analysis statistics and metrics
        /// </summary>
        [HttpGet("stats")]
        public IActionResult GetAnalysisStats()
        {
            try
            {
                var stats = new {
                    supportedIssueTypes = new[] { "ambiguity", "completeness", "consistency", "verifiability", "traceability" },
                    supportedSeverityLevels = new[] { "critical", "major", "minor" },
                    maxRequirementLength = 5000,
                    maxBatchSize = 50,
                    analysisFeatures = new[]
                    {
                        "IEEE 830 compliance checking",
                        "Ambiguous term detection",
                        "Modal verb analysis",
                        "Measurability assessment",
                        "Completeness validation"
                    },
                    enhancementCapabilities = new[]
                    {
                        "Active voice conversion",
                        "Measurable criteria addition",
                        "Specification completion",
                        "Standard pattern application"
                    }
                };

                return Ok(stats);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving analysis stats");
                return StatusCode(500, new {
                    error = "Internal server error",
                    message = "Unable to retrieve analysis statistics"
                });
            }
        }
    }

    // Additional DTOs for batch operations
    public class BatchAnalyzeRequest
    {
        [Required]
        [MinLength(1, ErrorMessage = "At least one requirement is required")]
        [MaxLength(50, ErrorMessage = "Maximum 50 requirements allowed")]
        public List<string> Requirements { get; set; } = new();
    }
}