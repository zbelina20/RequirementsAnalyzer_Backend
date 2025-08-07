namespace RequirementsAnalyzer.API.Configuration
{
    /// <summary>
    /// Configuration settings for Perplexity API integration
    /// </summary>
    public class PerplexityConfig
    {
        public const string SectionName = "PerplexityApi";

        public string ApiKey { get; set; } = string.Empty;
        public string BaseUrl { get; set; } = "https://api.perplexity.ai";

        /// <summary>
        /// Valid Perplexity models as of 2024:
        /// - llama-3.1-sonar-small-128k-online (fast, cost-effective)
        /// - llama-3.1-sonar-large-128k-online (more capable, slower)
        /// - llama-3.1-sonar-huge-128k-online (most capable, slowest)
        /// Check https://docs.perplexity.ai/guides/model-cards for latest models
        /// </summary>
        public string Model { get; set; } = "llama-3.1-sonar-small-128k-online";

        public int MaxTokens { get; set; } = 2000;
        public double Temperature { get; set; } = 0.1;
        public int TimeoutSeconds { get; set; } = 30;
    }
}