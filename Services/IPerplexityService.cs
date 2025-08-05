using RequirementsAnalyzer.API.DTOs;
using System.Threading.Tasks;

public interface IPerplexityService
{
    // Analyzes a single requirement
    Task<AnalysisResponse> AnalyzeAsync(string text);

    // Enhances a single requirement
    Task<EnhancementResponse> EnhanceAsync(string text);

    // Analyzes a batch of requirements
    Task<List<AnalysisResponse>> BatchAnalyzeAsync(IEnumerable<string> texts);
}
