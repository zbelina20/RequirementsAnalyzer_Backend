namespace RequirementsAnalyzer.API.Models
{
    /// <summary>
    /// Represents a quality issue found during analysis.
    /// </summary>
    public class QualityIssueDto
    {
        /// <summary>
        /// Gets or sets the type of quality issue.
        /// </summary>
        public string? Type { get; set; }

        /// <summary>
        /// Gets or sets the description of the issue.
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// Gets or sets the severity level of the issue.
        /// </summary>
        public string? Severity { get; set; }

        /// <summary>
        /// Gets or sets the suggested fix for the issue.
        /// </summary>
        public string? SuggestedFix { get; set; }

        /// <summary>
        /// Gets or sets the problematic text that caused this issue.
        /// </summary>
        public string? ProblematicText { get; set; }

        /// <summary>
        /// Gets or sets the suggestion for fixing this issue.
        /// </summary>
        public string? Suggestion { get; set; }
    }
}