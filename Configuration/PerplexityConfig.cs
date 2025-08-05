namespace RequirementsAnalyzer.API.Configuration
{
    public class PerplexityConfig
    {
        public const string SectionName = "PerplexityApi";

        public string ApiKey { get; set; } = string.Empty;
        public string BaseUrl { get; set; } = "https://api.perplexity.ai";
        public string Model { get; set; } = "llama-3.1-sonar-large-128k-online";
        public int MaxTokens { get; set; } = 2000;
        public double Temperature { get; set; } = 0.1;
        public int TimeoutSeconds { get; set; } = 30;
    }
}