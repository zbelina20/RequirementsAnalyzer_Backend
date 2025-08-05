// Models/Requirement.cs
using System.ComponentModel.DataAnnotations;

namespace RequirementsAnalyzer.API.Models
{
    public class Requirement
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public string OriginalText { get; set; } = string.Empty;

        public string? EnhancedText { get; set; }

        public int QualityScore { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public virtual ICollection<QualityIssue> Issues { get; set; } = new List<QualityIssue>();
        public virtual ICollection<Enhancement> Enhancements { get; set; } = new List<Enhancement>();
    }

    public class QualityIssue
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public Guid RequirementId { get; set; }

        [Required]
        public string Type { get; set; } = string.Empty; // ambiguity, completeness, etc.

        [Required]
        public string Severity { get; set; } = string.Empty; // critical, major, minor

        [Required]
        public string Description { get; set; } = string.Empty;

        public string? ProblematicText { get; set; }

        public string? Suggestion { get; set; }

        // Navigation property
        public virtual Requirement Requirement { get; set; } = null!;
    }

    public class Enhancement
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public Guid RequirementId { get; set; }

        [Required]
        public string Text { get; set; } = string.Empty;

        public string? Changes { get; set; } // JSON serialized array

        public string? Improvements { get; set; } // JSON serialized array

        public int QualityScore { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation property
        public virtual Requirement Requirement { get; set; } = null!;
    }
}

// DTOs for API responses
namespace RequirementsAnalyzer.API.DTOs
{
    public class AnalyzeRequest
    {
        public string Text { get; set; } = string.Empty;
    }

    public class AnalysisResponse
    {
        public int OverallScore { get; set; }
        public List<QualityIssueDto> Issues { get; set; } = new();
        public string AnalyzedAt { get; set; } = DateTime.UtcNow.ToString("O");
    }

    public class QualityIssueDto
    {
        public string Type { get; set; } = string.Empty;
        public string Severity { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string ProblematicText { get; set; } = string.Empty;
        public string Suggestion { get; set; } = string.Empty;
    }

    public class EnhanceRequest
    {
        public string Text { get; set; } = string.Empty;
        public List<QualityIssueDto>? Issues { get; set; }
    }

    public class EnhancementResponse
    {
        public List<EnhancementDto> Enhancements { get; set; } = new();
        public int RecommendedIndex { get; set; } = 0;
    }

    public class EnhancementDto
    {
        public string Text { get; set; } = string.Empty;
        public List<string> Changes { get; set; } = new();
        public List<string> Improvements { get; set; } = new();
        public int QualityScore { get; set; }
        public string? Rationale { get; set; }
    }
}