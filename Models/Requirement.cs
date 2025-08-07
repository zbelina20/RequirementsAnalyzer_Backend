using System.ComponentModel.DataAnnotations;

namespace RequirementsAnalyzer.API.Models
{
    /// <summary>
    /// Represents a software requirement with quality analysis results
    /// </summary>
    public class Requirement
    {
        /// <summary>
        /// Unique identifier for the requirement
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// The original text of the requirement
        /// </summary>
        [Required]
        public string OriginalText { get; set; } = string.Empty;

        /// <summary>
        /// Enhanced version of the requirement (if generated)
        /// </summary>
        public string? EnhancedText { get; set; }

        /// <summary>
        /// Quality score from 0 to 100
        /// </summary>
        public int QualityScore { get; set; }

        /// <summary>
        /// When the requirement was created
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// When the requirement was last updated
        /// </summary>
        public DateTime? UpdatedAt { get; set; }

        // Navigation properties
        /// <summary>
        /// Collection of quality issues identified for this requirement
        /// </summary>
        public virtual ICollection<QualityIssue> Issues { get; set; } = new List<QualityIssue>();

        /// <summary>
        /// Collection of enhancement suggestions for this requirement
        /// </summary>
        public virtual ICollection<Enhancement> Enhancements { get; set; } = new List<Enhancement>();
    }

    /// <summary>
    /// Represents a quality issue identified in a requirement
    /// </summary>
    public class QualityIssue
    {
        /// <summary>
        /// Unique identifier for the quality issue
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Foreign key to the associated requirement
        /// </summary>
        public int RequirementId { get; set; }

        /// <summary>
        /// Type of quality issue (ambiguity, completeness, etc.)
        /// </summary>
        [Required]
        public string Type { get; set; } = string.Empty;

        /// <summary>
        /// Severity level of the issue (critical, major, minor)
        /// </summary>
        [Required]
        public string Severity { get; set; } = string.Empty;

        /// <summary>
        /// Description of the quality issue
        /// </summary>
        [Required]
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// The specific text that has the quality issue
        /// </summary>
        public string? ProblematicText { get; set; }

        /// <summary>
        /// Suggested improvement for the issue
        /// </summary>
        public string? Suggestion { get; set; }

        // Navigation property
        /// <summary>
        /// The requirement this issue belongs to
        /// </summary>
        public virtual Requirement? Requirement { get; set; }
    }

    /// <summary>
    /// Represents an enhancement suggestion for a requirement
    /// </summary>
    public class Enhancement
    {
        /// <summary>
        /// Unique identifier for the enhancement
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Foreign key to the associated requirement
        /// </summary>
        public int RequirementId { get; set; }

        /// <summary>
        /// Enhanced text of the requirement
        /// </summary>
        [Required]
        public string Text { get; set; } = string.Empty;

        /// <summary>
        /// List of changes made during enhancement
        /// </summary>
        public string? Changes { get; set; }

        /// <summary>
        /// List of quality improvements achieved
        /// </summary>
        public string? Improvements { get; set; }

        /// <summary>
        /// Quality score of the enhanced version
        /// </summary>
        public int QualityScore { get; set; }

        /// <summary>
        /// When the enhancement was created
        /// </summary>
        public DateTime CreatedAt { get; set; }

        // Navigation property
        /// <summary>
        /// The requirement this enhancement belongs to
        /// </summary>
        public virtual Requirement? Requirement { get; set; }
    }
}

// DTOs for API communication
namespace RequirementsAnalyzer.API.DTOs
{
    /// <summary>
    /// Request model for analyzing a requirement
    /// </summary>
    public class AnalyzeRequest
    {
        /// <summary>
        /// The requirement text to analyze
        /// </summary>
        [Required]
        public string Text { get; set; } = string.Empty;
    }

    /// <summary>
    /// Response model for requirement analysis
    /// </summary>
    public class AnalysisResponse
    {
        /// <summary>
        /// Overall quality score from 0 to 100
        /// </summary>
        public int OverallScore { get; set; }

        /// <summary>
        /// List of quality issues identified
        /// </summary>
        public List<QualityIssueDto> Issues { get; set; } = new();

        /// <summary>
        /// When the analysis was performed
        /// </summary>
        public string AnalyzedAt { get; set; } = string.Empty;
    }

    /// <summary>
    /// DTO for quality issue information
    /// </summary>
    public class QualityIssueDto
    {
        /// <summary>
        /// Type of quality issue
        /// </summary>
        public string Type { get; set; } = string.Empty;

        /// <summary>
        /// Severity level of the issue
        /// </summary>
        public string Severity { get; set; } = string.Empty;

        /// <summary>
        /// Description of the issue
        /// </summary>
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// Problematic text that caused the issue
        /// </summary>
        public string ProblematicText { get; set; } = string.Empty;

        /// <summary>
        /// Suggested fix for the issue
        /// </summary>
        public string Suggestion { get; set; } = string.Empty;
    }

    /// <summary>
    /// Request model for enhancing a requirement
    /// </summary>
    public class EnhanceRequest
    {
        /// <summary>
        /// The requirement text to enhance
        /// </summary>
        [Required]
        public string Text { get; set; } = string.Empty;

        /// <summary>
        /// Optional list of issues to address in the enhancement
        /// </summary>
        public List<QualityIssueDto>? Issues { get; set; }
    }

    /// <summary>
    /// Response model for requirement enhancement
    /// </summary>
    public class EnhancementResponse
    {
        /// <summary>
        /// List of enhanced requirement versions
        /// </summary>
        public List<EnhancementDto> Enhancements { get; set; } = new();

        /// <summary>
        /// Index of the recommended enhancement (0-based)
        /// </summary>
        public int RecommendedIndex { get; set; }
    }

    /// <summary>
    /// DTO for enhancement information
    /// </summary>
    public class EnhancementDto
    {
        /// <summary>
        /// Enhanced requirement text
        /// </summary>
        public string Text { get; set; } = string.Empty;

        /// <summary>
        /// List of changes made during enhancement
        /// </summary>
        public List<string> Changes { get; set; } = new();

        /// <summary>
        /// List of quality improvements achieved
        /// </summary>
        public List<string> Improvements { get; set; } = new();

        /// <summary>
        /// Quality score of the enhanced version
        /// </summary>
        public int QualityScore { get; set; }

        /// <summary>
        /// Explanation of why this enhancement is recommended
        /// </summary>
        public string Rationale { get; set; } = string.Empty;
    }
}