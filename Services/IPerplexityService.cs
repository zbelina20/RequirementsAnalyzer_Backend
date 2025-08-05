using RequirementsAnalyzer.API.DTOs;
using System.Threading.Tasks;

public interface IPerplexityService
{
    Task<AnalysisResponse> AnalyzeRequirementAsync(string requirement);
    Task<EnhancementResponse> EnhanceRequirementAsync(string requirement, List<QualityIssueDto>? issues = null);
    Task<bool> TestConnectionAsync();
    Task<List<AnalysisResponse>> BatchAnalyzeAsync(List<string> requirements);
}
