namespace RequirementsAnalyzer.API.Configuration
{
    /// <summary>
    /// Configuration settings for the Perplexity API integration
    /// </summary>
    public class PerplexityConfig
    {
        /// <summary>
        /// The configuration section name in appsettings.json
        /// </summary>
        public const string SectionName = "PerplexityApi";

        /// <summary>
        /// The API key for authenticating with Perplexity API
        /// </summary>
        public string ApiKey { get; set; } = string.Empty;

        /// <summary>
        /// The base URL for the Perplexity API
        /// </summary>
        public string BaseUrl { get; set; } = "https://api.perplexity.ai";

        /// <summary>
        /// The model to use for completions
        /// </summary>
        public string Model { get; set; } = "llama-3.1-sonar-large-128k-online";

        /// <summary>
        /// Maximum number of tokens to generate
        /// </summary>
        public int MaxTokens { get; set; } = 2000;

        /// <summary>
        /// Temperature for response generation (0.0 to 1.0)
        /// </summary>
        public double Temperature { get; set; } = 0.1;

        /// <summary>
        /// HTTP timeout in seconds
        /// </summary>
        public int TimeoutSeconds { get; set; } = 30;
    }
}