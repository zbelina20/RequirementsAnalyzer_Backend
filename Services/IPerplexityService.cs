using RequirementsAnalyzer.API.DTOs;
using RequirementsAnalyzer.API.Models;
using System.Threading.Tasks;

/// <summary>
/// Service interface for interacting with the Perplexity API for requirements analysis
/// </summary>
public interface IPerplexityService
{
    /// <summary>
    /// Analyzes a requirement for quality issues using AI
    /// </summary>
    /// <param name="requirement">The requirement text to analyze</param>
    /// <returns>Analysis response with quality score and identified issues</returns>
    Task<AnalysisResponse> AnalyzeRequirementAsync(string requirement);

    /// <summary>
    /// Enhances a requirement based on identified quality issues
    /// </summary>
    /// <param name="requirement">The original requirement text</param>
    /// <param name="issues">Optional list of quality issues to address</param>
    /// <returns>Enhancement response with improved requirement versions</returns>
    Task<EnhancementResponse> EnhanceRequirementAsync(string requirement, List<QualityIssueDto>? issues = null);

    /// <summary>
    /// Tests the connection to the Perplexity API
    /// </summary>
    /// <returns>True if the API is accessible, false otherwise</returns>
    Task<bool> TestConnectionAsync();

    /// <summary>
    /// Performs batch analysis on multiple requirements
    /// </summary>
    /// <param name="requirements">List of requirement texts to analyze</param>
    /// <returns>List of analysis responses for each requirement</returns>
    Task<List<AnalysisResponse>> BatchAnalyzeAsync(List<string> requirements);
}
