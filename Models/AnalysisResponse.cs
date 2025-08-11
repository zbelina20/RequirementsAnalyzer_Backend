namespace RequirementsAnalyzer.API.Models
{
    /// <summary>
    /// Represents the response from an analysis operation.
    /// </summary>
    public class AnalysisResponse
    {
        /// <summary>
        /// Gets or sets the analysis result.
        /// </summary>
        public string? Result { get; set; }

        /// <summary>
        /// Gets or sets the list of quality issues found during analysis.
        /// </summary>
        public List<QualityIssueDto>? Issues { get; set; }

        /// <summary>
        /// Gets or sets the list of suggestions for improvement.
        /// </summary>
        public List<string>? Suggestions { get; set; }

        /// <summary>
        /// Gets or sets the overall quality score of the analysis.
        /// </summary>
        public double OverallScore { get; set; } // Changed from Score to OverallScore

        /// <summary>
        /// Gets or sets a value indicating whether the analysis was successful.
        /// </summary>
        public bool IsSuccessful { get; set; }

        /// <summary>
        /// Gets or sets the error message if the analysis failed.
        /// </summary>
        public string? ErrorMessage { get; set; }

        /// <summary>
        /// Gets or sets when the analysis was performed.
        /// </summary>
        public DateTime AnalyzedAt { get; set; } = DateTime.UtcNow; // Add this property
    }
}