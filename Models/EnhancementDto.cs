namespace RequirementsAnalyzer.API.Models
{
    /// <summary>
    /// Represents an individual enhancement option.
    /// </summary>
    public class EnhancementDto
    {
        /// <summary>
        /// Gets or sets the enhanced requirement text.
        /// </summary>
        public string Text { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the list of changes made.
        /// </summary>
        public List<string> Changes { get; set; } = new();

        /// <summary>
        /// Gets or sets the list of improvements.
        /// </summary>
        public List<string> Improvements { get; set; } = new();

        /// <summary>
        /// Gets or sets the quality score of this enhancement.
        /// </summary>
        public int QualityScore { get; set; }

        /// <summary>
        /// Gets or sets the rationale for this enhancement.
        /// </summary>
        public string? Rationale { get; set; }
    }
}