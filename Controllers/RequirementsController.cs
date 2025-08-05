// Controllers/RequirementsController.cs
using Microsoft.AspNetCore.Mvc;
using RequirementsAnalyzer.API.DTOs;


namespace RequirementsAnalyzer.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RequirementsController : ControllerBase
    {
        private readonly ILogger<RequirementsController> _logger;

        public RequirementsController(ILogger<RequirementsController> logger)
        {
            _logger = logger;
        }

        [HttpGet("health")]
        public IActionResult Health()
        {
            return Ok(new { status = "healthy", message = "Requirements API is running" });
        }

        [HttpPost("analyze")]
        public IActionResult AnalyzeRequirement([FromBody] AnalyzeRequest request)
        {
            try
            {
                _logger.LogInformation("Analyzing requirement: {Text}", request.Text);

                // Mock analysis for now - replace with actual Perplexity API call
                var mockResponse = new AnalysisResponse {
                    OverallScore = 65,
                    AnalyzedAt = DateTime.UtcNow.ToString("O"),
                    Issues = new List<QualityIssueDto>
                    {
                        new QualityIssueDto
                        {
                            Type = "ambiguity",
                            Severity = "major",
                            Description = "The term 'user-friendly' is ambiguous and not measurable",
                            ProblematicText = "user-friendly",
                            Suggestion = "Replace with specific, measurable criteria like '95% of users can complete the task within 2 minutes'"
                        },
                        new QualityIssueDto
                        {
                            Type = "completeness",
                            Severity = "minor",
                            Description = "Missing error handling specification",
                            ProblematicText = "system should process",
                            Suggestion = "Add specifications for error conditions and system response"
                        }
                    }
                };

                return Ok(mockResponse);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error analyzing requirement");
                return StatusCode(500, new { error = "Internal server error" });
            }
        }

        [HttpPost("enhance")]
        public IActionResult EnhanceRequirement([FromBody] EnhanceRequest request)
        {
            try
            {
                _logger.LogInformation("Enhancing requirement: {Text}", request.Text);

                // Mock enhancement for now - replace with actual Perplexity API call
                var mockResponse = new EnhancementResponse {
                    RecommendedIndex = 0,
                    Enhancements = new List<EnhancementDto>
                    {
                        new EnhancementDto
                        {
                            Text = "The system shall respond to user login requests within 2 seconds and display a confirmation message with 95% success rate for valid credentials.",
                            Changes = new List<string>
                            {
                                "Added specific response time requirement (2 seconds)",
                                "Added measurable success rate criteria (95%)",
                                "Specified the type of user feedback"
                            },
                            Improvements = new List<string>
                            {
                                "Eliminated ambiguous terms",
                                "Added quantifiable metrics",
                                "Improved testability"
                            },
                            QualityScore = 88,
                            Rationale = "Enhanced requirement addresses ambiguity and adds measurable criteria"
                        },
                        new EnhancementDto
                        {
                            Text = "The system shall authenticate users within 2 seconds of credential submission and provide clear success/failure feedback with 99% availability during business hours.",
                            Changes = new List<string>
                            {
                                "Added specific timing constraint",
                                "Added availability requirement",
                                "Clarified feedback mechanism"
                            },
                            Improvements = new List<string>
                            {
                                "Added performance criteria",
                                "Added availability specification",
                                "Improved clarity"
                            },
                            QualityScore = 92,
                            Rationale = "Alternative version with stronger performance and availability requirements"
                        }
                    }
                };

                return Ok(mockResponse);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error enhancing requirement");
                return StatusCode(500, new { error = "Internal server error" });
            }
        }

        [HttpPost("batch-analyze")]
        public IActionResult BatchAnalyze([FromBody] List<string> requirements)
        {
            try
            {
                var results = requirements.Select(req => new AnalysisResponse {
                    OverallScore = Random.Shared.Next(50, 95),
                    AnalyzedAt = DateTime.UtcNow.ToString("O"),
                    Issues = new List<QualityIssueDto>
                    {
                        new QualityIssueDto
                        {
                            Type = "ambiguity",
                            Severity = "minor",
                            Description = "Sample issue for batch processing",
                            ProblematicText = "sample text",
                            Suggestion = "Sample suggestion"
                        }
                    }
                }).ToList();

                return Ok(results);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in batch analysis");
                return StatusCode(500, new { error = "Internal server error" });
            }
        }
    }
}