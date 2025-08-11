namespace RequirementsAnalyzer.API.Models
{
    /// <summary>
    /// Represents the response from an enhancement operation.
    /// </summary>
    public class EnhancementResponse
    {
        /// <summary>
        /// Gets or sets the list of enhancement options.
        /// </summary>
        public List<EnhancementDto> Enhancements { get; set; } = new();

        /// <summary>
        /// Gets or sets the index of the recommended enhancement.
        /// </summary>
        public int? RecommendedIndex { get; set; }

        /// <summary>
        /// Gets or sets the enhanced text (for backward compatibility).
        /// </summary>
        public string? EnhancedText { get; set; }

        /// <summary>
        /// Gets or sets the list of improvements made (for backward compatibility).
        /// </summary>
        public List<string>? Improvements { get; set; }

        /// <summary>
        /// Gets or sets the original text before enhancement.
        /// </summary>
        public string? OriginalText { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the enhancement was successful.
        /// </summary>
        public bool IsSuccessful { get; set; }

        /// <summary>
        /// Gets or sets the error message if the enhancement failed.
        /// </summary>
        public string? ErrorMessage { get; set; }
    }
}