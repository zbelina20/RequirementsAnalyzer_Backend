// Services/PerplexityService.cs
using Microsoft.Extensions.Options;
using RequirementsAnalyzer.API.Configuration;
using RequirementsAnalyzer.API.DTOs;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace RequirementsAnalyzer.API.Services
{
    /// <summary>
    /// Service for integrating with Perplexity AI API for requirements analysis and enhancement
    /// </summary>
    public class PerplexityService : IPerplexityService
    {
        private readonly HttpClient _httpClient;
        private readonly PerplexityConfig _config;
        private readonly ILogger<PerplexityService> _logger;

        /// <summary>
        /// Initializes a new instance of the PerplexityService
        /// </summary>
        public PerplexityService(
            HttpClient httpClient,
            IOptions<PerplexityConfig> config,
            ILogger<PerplexityService> logger)
        {
            _httpClient = httpClient;
            _config = config.Value;
            _logger = logger;

            // Configure HttpClient
            _httpClient.BaseAddress = new Uri(_config.BaseUrl);
            _httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", _config.ApiKey);
            _httpClient.DefaultRequestHeaders.Add("User-Agent", "RequirementsAnalyzer/1.0");
            _httpClient.Timeout = TimeSpan.FromSeconds(_config.TimeoutSeconds);
        }

        /// <summary>
        /// Analyzes a software requirement for quality issues
        /// </summary>
        public async Task<AnalysisResponse> AnalyzeRequirementAsync(string requirement)
        {
            // Check if API key is configured
            if (string.IsNullOrEmpty(_config.ApiKey))
            {
                _logger.LogWarning("No API key configured, using mock analysis");
                return CreateMockAnalysis(requirement);
            }

            try
            {
                _logger.LogInformation("Starting analysis for requirement: {Text}",
                    requirement.Substring(0, System.Math.Min(100, requirement.Length)));

                var prompt = BuildAnalysisPrompt(requirement);
                var response = await CallPerplexityAsync(prompt);

                // Parse the JSON response from Perplexity
                var analysisResult = ParseAnalysisResponse(response);

                _logger.LogInformation("Analysis completed with score: {Score}", analysisResult.OverallScore);
                return analysisResult;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error analyzing requirement with Perplexity API");
                _logger.LogInformation("Falling back to mock analysis");
                return CreateMockAnalysis(requirement);
            }
        }

        /// <summary>
        /// Enhances a software requirement based on quality issues
        /// </summary>
        public async Task<EnhancementResponse> EnhanceRequirementAsync(string requirement, List<QualityIssueDto>? issues = null)
        {
            // Check if API key is configured
            if (string.IsNullOrEmpty(_config.ApiKey))
            {
                _logger.LogWarning("No API key configured, using mock enhancement");
                return CreateMockEnhancement(requirement);
            }

            try
            {
                _logger.LogInformation("Starting enhancement for requirement: {Text}",
                    requirement.Substring(0, System.Math.Min(100, requirement.Length)));

                var prompt = BuildEnhancementPrompt(requirement, issues);
                var response = await CallPerplexityAsync(prompt);

                var enhancementResult = ParseEnhancementResponse(response);

                _logger.LogInformation("Enhancement completed with {Count} versions",
                    enhancementResult.Enhancements.Count);
                return enhancementResult;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error enhancing requirement with Perplexity API");
                _logger.LogInformation("Falling back to mock enhancement");
                return CreateMockEnhancement(requirement);
            }
        }

        /// <summary>
        /// Tests the connection to Perplexity API
        /// </summary>
        public async Task<bool> TestConnectionAsync()
        {
            if (string.IsNullOrEmpty(_config.ApiKey))
            {
                _logger.LogWarning("No API key configured for connection test");
                return false;
            }

            try
            {
                _logger.LogInformation("Testing Perplexity API connection...");

                var testPrompt = "Test connection. Respond with just 'OK' if you can process this message.";
                var response = await CallPerplexityAsync(testPrompt);

                var isConnected = !string.IsNullOrEmpty(response) && response.ToLower().Contains("ok");

                _logger.LogInformation(isConnected ?
                    "Perplexity API connection successful" :
                    "Perplexity API connection test inconclusive");

                return isConnected;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Perplexity API connection test failed");
                return false;
            }
        }

        /// <summary>
        /// Analyzes multiple requirements in batch
        /// </summary>
        public async Task<List<AnalysisResponse>> BatchAnalyzeAsync(List<string> requirements)
        {
            var results = new List<AnalysisResponse>();

            _logger.LogInformation("Starting batch analysis for {Count} requirements", requirements.Count);

            for (int i = 0; i < requirements.Count; i++)
            {
                try
                {
                    _logger.LogInformation("Analyzing requirement {Current}/{Total}", i + 1, requirements.Count);

                    var analysis = await AnalyzeRequirementAsync(requirements[i]);
                    results.Add(analysis);

                    // Add small delay to avoid rate limiting
                    if (i < requirements.Count - 1)
                    {
                        await Task.Delay(500);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error in batch analysis for requirement {Index}: {Requirement}",
                        i + 1, requirements[i].Substring(0, System.Math.Min(50, requirements[i].Length)));
                    results.Add(CreateMockAnalysis(requirements[i]));
                }
            }

            _logger.LogInformation("Batch analysis completed: {Completed}/{Total} requirements processed",
                results.Count, requirements.Count);

            return results;
        }

        private async Task<string> CallPerplexityAsync(string prompt)
        {
            var request = new {
                model = _config.Model,
                messages = new[]
                {
                    new {
                        role = "system",
                        content = "You are an expert in software requirements engineering and quality analysis. " +
                                  "You follow IEEE 830 standards and requirements engineering best practices. " +
                                  "Always respond in valid JSON format as requested. " +
                                  "Be precise, analytical, and provide actionable suggestions."
                    },
                    new { role = "user", content = prompt }
                },
                temperature = _config.Temperature,
                max_tokens = _config.MaxTokens,
                stream = false
            };

            var json = JsonSerializer.Serialize(request, new JsonSerializerOptions {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });

            var content = new StringContent(json, Encoding.UTF8, "application/json");

            _logger.LogDebug("Calling Perplexity API with model: {Model}", _config.Model);

            var response = await _httpClient.PostAsync("/chat/completions", content);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogError("Perplexity API error: {StatusCode} - {Content}",
                    response.StatusCode, errorContent);
                throw new HttpRequestException($"Perplexity API error: {response.StatusCode} - {errorContent}");
            }

            var responseJson = await response.Content.ReadAsStringAsync();
            _logger.LogDebug("Received response from Perplexity API");

            // Parse the response to extract the content
            using var doc = JsonDocument.Parse(responseJson);
            var choices = doc.RootElement.GetProperty("choices");
            if (choices.GetArrayLength() > 0)
            {
                var firstChoice = choices[0];
                var message = firstChoice.GetProperty("message");
                var responseContent = message.GetProperty("content").GetString() ?? "";

                _logger.LogDebug("Perplexity response content length: {Length}", responseContent.Length);
                return responseContent;
            }

            throw new InvalidOperationException("No response content from Perplexity API");
        }

        private string BuildAnalysisPrompt(string requirement)
        {
            return $@"
Analyze this software requirement for quality issues according to IEEE 830 standards and requirements engineering best practices:

REQUIREMENT TO ANALYZE:
""{requirement}""

Evaluate the requirement for these specific quality dimensions:

1. AMBIGUITY: Look for vague terms (user-friendly, fast, efficient, reasonable), unclear pronouns, multiple interpretations, subjective language
2. COMPLETENESS: Check for missing actors, conditions, exceptions, outcomes, preconditions, postconditions  
3. CONSISTENCY: Identify conflicts with standard terminology, inconsistent language patterns
4. VERIFIABILITY: Assess ability to test/measure the requirement objectively with specific criteria
5. TRACEABILITY: Evaluate clear identification, categorization, and reference capability

For each issue found, provide:
- Issue type (ambiguity/completeness/consistency/verifiability/traceability)
- Severity level (critical/major/minor)
- Specific problematic phrases or missing elements
- Clear explanation of why it's problematic
- Specific, actionable suggestion for improvement

Calculate an overall quality score (0-100) based on:
- Number and severity of issues found
- Clarity and precision of language
- Completeness of specification
- Measurability of criteria

Respond in this exact JSON format:
{{
  ""overallScore"": 75,
  ""issues"": [
    {{
      ""type"": ""ambiguity"",
      ""severity"": ""major"",
      ""description"": ""The term 'user-friendly' is subjective and not measurable"",
      ""problematicText"": ""user-friendly"",
      ""suggestion"": ""Replace with specific usability metrics like '95% of users can complete the primary task within 2 minutes without assistance'""
    }}
  ],
  ""analyzedAt"": ""{DateTime.UtcNow:O}""
}}

Be thorough but precise. Focus on actionable improvements.";
        }

        private string BuildEnhancementPrompt(string requirement, List<QualityIssueDto>? issues)
        {
            var issuesText = issues?.Count > 0
                ? string.Join("\n", issues.Select(i => $"- {i.Type} ({i.Severity}): {i.Description}"))
                : "General quality improvements needed";

            return $@"
Rewrite this software requirement to address the identified quality issues and follow requirements engineering best practices:

ORIGINAL REQUIREMENT:
""{requirement}""

IDENTIFIED QUALITY ISSUES:
{issuesText}

Enhancement Guidelines:
- Use active voice and definitive verbs (shall, must, will) instead of weak terms (should, may, might)
- Include measurable, testable criteria with specific numbers, percentages, timeframes
- Specify actors, preconditions, and expected outcomes clearly
- Avoid ambiguous terms (user-friendly, fast, efficient, reasonable, good, bad)
- Include error conditions and exception handling where applicable
- Follow standard requirement patterns: ""The [system/actor] shall [action] [object] [conditions]""
- Use quantifiable metrics (response time < X seconds, success rate > Y%, etc.)
- Specify inputs, outputs, and interfaces clearly

Provide 2-3 enhanced versions, ranked by quality improvement:

{{
  ""enhancements"": [
    {{
      ""text"": ""The authentication system shall validate user credentials within 2 seconds of submission and display appropriate success or error messages with 99.9% availability during business hours (8AM-6PM EST)."",
      ""changes"": [""Added specific timing requirement (2 seconds)"", ""Specified availability metric (99.9%)"", ""Defined business hours precisely"", ""Included error handling specification""],
      ""improvements"": [""Measurable performance criteria"", ""Clear success/failure conditions"", ""Testable availability requirement"", ""Specific operational timeframe""],
      ""qualityScore"": 92,
      ""rationale"": ""Enhanced with specific timing, availability metrics, error handling, and clear operational parameters making it fully testable and unambiguous""
    }}
  ],
  ""recommendedIndex"": 0
}}

Focus on creating requirements that are:
- Specific and measurable
- Testable and verifiable  
- Complete and unambiguous
- Consistent with standard terminology
- Properly scoped and traceable";
        }

        private AnalysisResponse ParseAnalysisResponse(string response)
        {
            try
            {
                // Try to extract JSON from the response if it's wrapped in other text
                var cleanedResponse = ExtractJsonFromResponse(response);

                var options = new JsonSerializerOptions {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                    PropertyNameCaseInsensitive = true
                };

                var result = JsonSerializer.Deserialize<AnalysisResponse>(cleanedResponse, options);

                if (result != null)
                {
                    _logger.LogDebug("Successfully parsed analysis response");
                    return result;
                }
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, "Failed to parse analysis response from Perplexity: {Response}",
                    response.Substring(0, System.Math.Min(500, response.Length)));
            }

            _logger.LogWarning("Using mock analysis due to parsing failure");
            return CreateMockAnalysis("");
        }

        private EnhancementResponse ParseEnhancementResponse(string response)
        {
            try
            {
                var cleanedResponse = ExtractJsonFromResponse(response);

                var options = new JsonSerializerOptions {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                    PropertyNameCaseInsensitive = true
                };

                var result = JsonSerializer.Deserialize<EnhancementResponse>(cleanedResponse, options);

                if (result != null)
                {
                    _logger.LogDebug("Successfully parsed enhancement response");
                    return result;
                }
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, "Failed to parse enhancement response from Perplexity: {Response}",
                    response.Substring(0, System.Math.Min(500, response.Length)));
            }

            _logger.LogWarning("Using mock enhancement due to parsing failure");
            return CreateMockEnhancement("");
        }

        private string ExtractJsonFromResponse(string response)
        {
            // Find JSON object boundaries
            var startIndex = response.IndexOf('{');
            var lastIndex = response.LastIndexOf('}');

            if (startIndex >= 0 && lastIndex > startIndex)
            {
                var extracted = response.Substring(startIndex, lastIndex - startIndex + 1);
                _logger.LogDebug("Extracted JSON from response (length: {Length})", extracted.Length);
                return extracted;
            }

            _logger.LogDebug("No JSON boundaries found, returning response as-is");
            return response;
        }

        private AnalysisResponse CreateMockAnalysis(string requirement)
        {
            var issues = new List<QualityIssueDto>();
            var score = 75; // Default score

            _logger.LogDebug("Creating mock analysis for requirement");

            // Analyze for common quality issues
            var lowerReq = requirement.ToLower();

            // Check for ambiguous terms
            var ambiguousTerms = new[] { "user-friendly", "fast", "efficient", "good", "bad", "easy", "simple", "reasonable", "appropriate" };
            var foundAmbiguous = ambiguousTerms.Where(term => lowerReq.Contains(term)).ToList();

            if (foundAmbiguous.Any())
            {
                issues.Add(new QualityIssueDto {
                    Type = "ambiguity",
                    Severity = "major",
                    Description = $"Contains ambiguous terms that are not measurable: {string.Join(", ", foundAmbiguous)}",
                    ProblematicText = string.Join(", ", foundAmbiguous),
                    Suggestion = "Replace with specific, measurable criteria (e.g., response time < 2 seconds, 95% user success rate)"
                });
                score -= 15;
            }

            // Check for weak modal verbs
            var weakVerbs = new[] { "should", "may", "might", "could", "would" };
            var foundWeak = weakVerbs.Where(verb => lowerReq.Contains(verb)).ToList();

            if (foundWeak.Any())
            {
                issues.Add(new QualityIssueDto {
                    Type = "completeness",
                    Severity = "minor",
                    Description = $"Uses weak modal verbs instead of definitive requirements language: {string.Join(", ", foundWeak)}",
                    ProblematicText = string.Join(", ", foundWeak),
                    Suggestion = "Use definitive language: 'shall', 'must', or 'will' for mandatory requirements"
                });
                score -= 10;
            }

            // Check for missing quantifiable criteria
            var hasNumbers = System.Text.RegularExpressions.Regex.IsMatch(requirement, @"\d+");
            var hasUnits = lowerReq.Contains("second") || lowerReq.Contains("minute") || lowerReq.Contains("percent") || lowerReq.Contains("%");

            if (!hasNumbers && !hasUnits && requirement.Length > 20)
            {
                issues.Add(new QualityIssueDto {
                    Type = "verifiability",
                    Severity = "major",
                    Description = "Lacks quantifiable, measurable criteria for verification",
                    ProblematicText = "entire requirement",
                    Suggestion = "Add specific metrics: timeframes, percentages, counts, or size limits that can be objectively measured"
                });
                score -= 20;
            }

            // Check for passive voice (simple heuristic)
            if (lowerReq.Contains(" be ") || lowerReq.Contains(" been ") || lowerReq.Contains(" being "))
            {
                issues.Add(new QualityIssueDto {
                    Type = "consistency",
                    Severity = "minor",
                    Description = "May contain passive voice constructions",
                    ProblematicText = "passive voice phrases",
                    Suggestion = "Use active voice: specify who performs the action (e.g., 'The system shall...' instead of 'Data shall be processed...')"
                });
                score -= 5;
            }

            // Ensure score doesn't go below 20
            score = System.Math.Max(20, score);

            var mockResult = new AnalysisResponse {
                OverallScore = score,
                Issues = issues,
                AnalyzedAt = DateTime.UtcNow.ToString("O")
            };

            _logger.LogInformation("Mock analysis created: Score={Score}, Issues={IssueCount}", score, issues.Count);
            return mockResult;
        }

        private EnhancementResponse CreateMockEnhancement(string requirement)
        {
            _logger.LogDebug("Creating mock enhancement for requirement");

            // Create enhanced versions based on common improvements
            var enhancements = new List<EnhancementDto>();

            // Version 1: Most comprehensive
            var enhanced1 = requirement
                .Replace("should", "shall")
                .Replace("may", "shall")
                .Replace("might", "shall")
                .Replace("user-friendly", "intuitive with 95% user task completion rate")
                .Replace("fast", "within 2 seconds")
                .Replace("efficient", "with 99% accuracy");

            if (!enhanced1.Contains("shall") && !enhanced1.Contains("must"))
            {
                enhanced1 = "The system shall " + enhanced1.ToLower();
            }

            enhancements.Add(new EnhancementDto {
                Text = enhanced1,
                Changes = new List<string>
                {
                    "Replaced weak modal verbs with 'shall'",
                    "Added specific performance criteria",
                    "Converted to active voice",
                    "Added measurable success metrics"
                },
                Improvements = new List<string>
                {
                    "Definitive requirement language",
                    "Measurable performance criteria",
                    "Clear actor identification",
                    "Testable success conditions"
                },
                QualityScore = 85,
                Rationale = "Enhanced with definitive language, specific metrics, and testable criteria"
            });

            // Version 2: Moderate enhancement
            var enhanced2 = requirement
                .Replace("should", "must")
                .Replace("user-friendly", "accessible to 90% of target users");

            if (!enhanced2.Contains("shall") && !enhanced2.Contains("must") && !enhanced2.StartsWith("The"))
            {
                enhanced2 = "The application must " + enhanced2.ToLower();
            }

            enhancements.Add(new EnhancementDto {
                Text = enhanced2,
                Changes = new List<string>
                {
                    "Replaced 'should' with 'must'",
                    "Added user accessibility metrics",
                    "Improved requirement structure"
                },
                Improvements = new List<string>
                {
                    "Mandatory requirement language",
                    "User-focused success criteria",
                    "Better requirement formatting"
                },
                QualityScore = 78,
                Rationale = "Improved with mandatory language and user-focused metrics"
            });

            var mockResult = new EnhancementResponse {
                Enhancements = enhancements,
                RecommendedIndex = 0
            };

            _logger.LogInformation("Mock enhancement created with {Count} versions", enhancements.Count);
            return mockResult;
        }
    }
}