using RequirementsAnalyzer.API.DTOs;

namespace RequirementsAnalyzer.API.Services
{
    public class PerplexityService : IPerplexityService
    {
        public Task<AnalysisResponse> AnalyzeAsync(string text)
        {
            throw new NotImplementedException();
        }

        public Task<List<AnalysisResponse>> BatchAnalyzeAsync(IEnumerable<string> texts)
        {
            throw new NotImplementedException();
        }

        public Task<EnhancementResponse> EnhanceAsync(string text)
        {
            throw new NotImplementedException();
        }
    }
}
