using Microsoft.Extensions.Options;
using RequirementsAnalyzer.API.Configuration;
using RequirementsAnalyzer.API.DTOs;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace RequirementsAnalyzer.API.Services
{
    public class PerplexityService : IPerplexityService
    {
        private readonly HttpClient _httpClient;
        private readonly PerplexityConfig _config;
        private readonly ILogger<PerplexityService> _logger;

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

        public async Task<AnalysisResponse> AnalyzeRequirementAsync(string requirement)
        {
            try
            {
                var prompt = BuildAnalysisPrompt(requirement);
                var response = await CallPerplexityAsync(prompt);

                // Parse the JSON response from Perplexity
                var analysisResult = ParseAnalysisResponse(response);
                return analysisResult;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error analyzing requirement with Perplexity API");
                // Return mock data as fallback
                return CreateMockAnalysis(requirement);
            }
        }

        public async Task<EnhancementResponse> EnhanceRequirementAsync(string requirement, List<QualityIssueDto>? issues = null)
        {
            try
            {
                var prompt = BuildEnhancementPrompt(requirement, issues);
                var response = await CallPerplexityAsync(prompt);

                var enhancementResult = ParseEnhancementResponse(response);
                return enhancementResult;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error enhancing requirement with Perplexity API");
                // Return mock data as fallback
                return CreateMockEnhancement(requirement);
            }
        }

        public async Task<bool> TestConnectionAsync()
        {
            try
            {
                var testPrompt = "Test connection. Respond with 'OK' if you can process this message.";
                var response = await CallPerplexityAsync(testPrompt);
                return !string.IsNullOrEmpty(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Perplexity API connection test failed");
                return false;
            }
        }

        public async Task<List<AnalysisResponse>> BatchAnalyzeAsync(List<string> requirements)
        {
            var results = new List<AnalysisResponse>();

            foreach (var requirement in requirements)
            {
                try
                {
                    var analysis = await AnalyzeRequirementAsync(requirement);
                    results.Add(analysis);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error in batch analysis for requirement: {Requirement}", requirement);
                    results.Add(CreateMockAnalysis(requirement));
                }
            }

            return results;
        }

        private async Task<string> CallPerplexityAsync(string prompt)
        {
            var request = new {
                model = _config.Model,
                messages = new[]
                {
                    new { role = "system", content = "You are an expert in software requirements engineering and quality analysis. Always respond in valid JSON format." },
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

            _logger.LogInformation("Calling Perplexity API with model: {Model}", _config.Model);

            var response = await _httpClient.PostAsync("/chat/completions", content);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogError("Perplexity API error: {StatusCode} - {Content}", response.StatusCode, errorContent);
                throw new HttpRequestException($"Perplexity API error: {response.StatusCode}");
            }

            var responseJson = await response.Content.ReadAsStringAsync();

            // Parse the response to extract the content
            using var doc = JsonDocument.Parse(responseJson);
            var choices = doc.RootElement.GetProperty("choices");
            if (choices.GetArrayLength() > 0)
            {
                var firstChoice = choices[0];
                var message = firstChoice.GetProperty("message");
                return message.GetProperty("content").GetString() ?? "";
            }

            throw new InvalidOperationException("No response content from Perplexity API");
        }

        private string BuildAnalysisPrompt(string requirement)
        {
            return $@"
Analyze this software requirement for quality issues according to IEEE 830 standards and requirements engineering best practices:

REQUIREMENT: '{requirement}'

Evaluate for these specific issues:
1. AMBIGUITY: Vague terms (user-friendly, fast, efficient), unclear pronouns, multiple interpretations
2. COMPLETENESS: Missing actors, conditions, exceptions, or outcomes  
3. CONSISTENCY: Conflicts with standard terminology or patterns
4. VERIFIABILITY: Ability to test/measure the requirement objectively
5. TRACEABILITY: Clear identification and categorization

For each issue found, provide:
- Issue type and severity (critical/major/minor)
- Specific problematic phrases
- Why it's problematic
- Suggested improvement

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
      ""suggestion"": ""Replace with specific usability metrics like '95% of users can complete the task within 2 minutes without assistance'""
    }}
  ],
  ""analyzedAt"": ""{DateTime.UtcNow:O}""
}}";
        }

        private string BuildEnhancementPrompt(string requirement, List<QualityIssueDto>? issues)
        {
            var issuesText = issues?.Count > 0
                ? string.Join("\n", issues.Select(i => $"- {i.Type}: {i.Description}"))
                : "General quality improvements needed";

            return $@"
Rewrite this software requirement to address the identified quality issues and follow requirements engineering best practices:

ORIGINAL REQUIREMENT: '{requirement}'

ISSUES TO ADDRESS:
{issuesText}

Requirements enhancement guidelines:
- Use active voice and specific verbs (shall, must, will)
- Include measurable, testable criteria where applicable
- Specify actors, preconditions, and expected outcomes clearly
- Avoid ambiguous terms like 'user-friendly', 'fast', 'efficient', 'reasonable'
- Include error conditions and exception handling
- Follow standard requirement patterns: ""The [system/actor] shall [action] [object] [conditions]""
- Use quantifiable metrics (time, percentage, count, size)
- Specify inputs, outputs, and interfaces clearly

Provide 2-3 alternative enhanced versions, ranked by quality improvement:

{{
  ""enhancements"": [
    {{
      ""text"": ""The system shall authenticate user credentials within 2 seconds and display a success message with 99.9% availability during peak hours (8AM-6PM)."",
      ""changes"": [""Added specific timing requirement"", ""Specified availability metric"", ""Clarified success criteria""],
      ""improvements"": [""Measurable performance criteria"", ""Clear success conditions"", ""Testable availability requirement""],
      ""qualityScore"": 85,
      ""rationale"": ""Enhanced with specific timing, availability metrics, and clear success conditions making it fully testable""
    }}
  ],
  ""recommendedIndex"": 0
}}";
        }

        private AnalysisResponse ParseAnalysisResponse(string response)
        {
            try
            {
                // Try to extract JSON from the response if it's wrapped in other text
                var cleanedResponse = ExtractJsonFromResponse(response);

                return JsonSerializer.Deserialize<AnalysisResponse>(cleanedResponse, new JsonSerializerOptions {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                    PropertyNameCaseInsensitive = true
                }) ?? CreateMockAnalysis("");
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, "Failed to parse analysis response from Perplexity: {Response}", response);
                return CreateMockAnalysis("");
            }
        }

        private EnhancementResponse ParseEnhancementResponse(string response)
        {
            try
            {
                var cleanedResponse = ExtractJsonFromResponse(response);

                return JsonSerializer.Deserialize<EnhancementResponse>(cleanedResponse, new JsonSerializerOptions {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                    PropertyNameCaseInsensitive = true
                }) ?? CreateMockEnhancement("");
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, "Failed to parse enhancement response from Perplexity: {Response}", response);
                return CreateMockEnhancement("");
            }
        }

        private string ExtractJsonFromResponse(string response)
        {
            // Find JSON object boundaries
            var startIndex = response.IndexOf('{');
            var lastIndex = response.LastIndexOf('}');

            if (startIndex >= 0 && lastIndex > startIndex)
            {
                return response.Substring(startIndex, lastIndex - startIndex + 1);
            }

            return response; // Return as-is if no JSON boundaries found
        }

        private AnalysisResponse CreateMockAnalysis(string requirement)
        {
            var issues = new List<QualityIssueDto>();

            // Analyze for common issues in mock mode
            if (requirement.ToLower().Contains("user-friendly") ||
                requirement.ToLower().Contains("fast") ||
                requirement.ToLower().Contains("efficient"))
            {
                issues.Add(new QualityIssueDto {
                    Type = "ambiguity",
                    Severity = "major",
                    Description = "Contains vague, non-measurable terms",
                    ProblematicText = "user-friendly/fast/efficient",
                    Suggestion = "Replace with specific, measurable criteria"
                });
            }

            if (requirement.ToLower().Contains("should") ||
                requirement.ToLower().Contains("may") ||
                requirement.ToLower().Contains("might"))
            {
                issues.Add(new QualityIssueDto {
                    Type = "completeness",
                    Severity = "minor",
                    Description = "Uses weak modal verbs instead of definitive requirements",
                    ProblematicText = "should/may/might",
                    Suggestion = "Use 'shall', 'must', or 'will' for definitive requirements"
                });
            }

            var score = Math.Max(30, 90 - (issues.Count * 15));

            return new AnalysisResponse {
                OverallScore = score,
                Issues = issues,
                AnalyzedAt = DateTime.UtcNow.ToString("O")
            };
        }

        private EnhancementResponse CreateMockEnhancement(string requirement)
        {
            var enhancements = new List<EnhancementDto>
            {
                new EnhancementDto
                {
                    Text = $"The system shall {requirement.ToLower().Replace("should", "").Replace("the system", "").Trim()} within specified performance criteria.",
                    Changes = new List<string> { "Replaced weak verbs with 'shall'", "Added performance criteria placeholder" },
                    Improvements = new List<string> { "More definitive language", "Measurable criteria framework" },
                    QualityScore = 75,
                    Rationale = "Enhanced with definitive language and measurable criteria framework"
                }
            };

            return new EnhancementResponse {
                Enhancements = enhancements,
                RecommendedIndex = 0
            };
        }
    }
}