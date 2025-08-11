// Models/Project.cs - Project-based requirement management
using RequirementsAnalyzer.API.Models;
using System.ComponentModel.DataAnnotations;

namespace RequirementsAnalyzer.API.Models
{
    /// <summary>
    /// Represents a project containing multiple requirements
    /// </summary>
    public class Project
    {
        /// <summary>
        /// Unique identifier for the project
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Name of the project
        /// </summary>
        [Required]
        [MaxLength(200)]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Optional description of the project
        /// </summary>
        [MaxLength(1000)]
        public string? Description { get; set; }

        /// <summary>
        /// When the project was created
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// When the project was last updated
        /// </summary>
        public DateTime UpdatedAt { get; set; }

        /// <summary>
        /// Collection of requirements belonging to this project
        /// </summary>
        public virtual ICollection<ProjectRequirement> Requirements { get; set; } = new List<ProjectRequirement>();
    }

    /// <summary>
    /// Represents a requirement within a project
    /// </summary>
    public class ProjectRequirement
    {
        /// <summary>
        /// Unique identifier for the requirement
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Foreign key to the associated project
        /// </summary>
        public int ProjectId { get; set; }

        /// <summary>
        /// The original text of the requirement
        /// </summary>
        [Required]
        [MaxLength(5000)]
        public string Text { get; set; } = string.Empty;

        /// <summary>
        /// Optional title or label for the requirement
        /// </summary>
        [MaxLength(200)]
        public string? Title { get; set; }

        /// <summary>
        /// Status of the requirement analysis
        /// </summary>
        [Required]
        public RequirementStatus Status { get; set; } = RequirementStatus.Draft;

        /// <summary>
        /// Quality score from last analysis (0-100)
        /// </summary>
        public int? QualityScore { get; set; }

        /// <summary>
        /// JSON data of the last analysis result
        /// </summary>
        public string? AnalysisData { get; set; }

        /// <summary>
        /// JSON data of enhancement suggestions
        /// </summary>
        public string? EnhancementData { get; set; }

        /// <summary>
        /// When the requirement was created
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// When the requirement was last updated
        /// </summary>
        public DateTime UpdatedAt { get; set; }

        /// <summary>
        /// Navigation property to the parent project
        /// </summary>
        public virtual Project? Project { get; set; }
    }

    /// <summary>
    /// Status of requirement analysis
    /// </summary>
    public enum RequirementStatus
    {
        /// <summary>
        /// Requirement created but not analyzed
        /// </summary>
        Draft,
        /// <summary>
        /// Requirement has been analyzed
        /// </summary>
        Analyzed,
        /// <summary>
        /// Requirement has enhancements generated
        /// </summary>
        Enhanced,
        /// <summary>
        /// Analysis failed
        /// </summary>
        Failed
    }
}

// DTOs for API communication
namespace RequirementsAnalyzer.API.DTOs
{
    /// <summary>
    /// DTO for creating a new project
    /// </summary>
    public class CreateProjectRequest
    {
        /// <summary>
        /// Name of the project
        /// </summary>
        [Required]
        [MaxLength(200)]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Optional description of the project
        /// </summary>
        [MaxLength(1000)]
        public string? Description { get; set; }
    }

    /// <summary>
    /// DTO for updating a project
    /// </summary>
    public class UpdateProjectRequest
    {
        /// <summary>
        /// Updated name of the project
        /// </summary>
        [Required]
        [MaxLength(200)]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Updated description of the project
        /// </summary>
        [MaxLength(1000)]
        public string? Description { get; set; }
    }

    /// <summary>
    /// DTO for project information
    /// </summary>
    public class ProjectDto
    {
        /// <summary>
        /// Project identifier
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Project name
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Project description
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// Creation timestamp
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// Last update timestamp
        /// </summary>
        public DateTime UpdatedAt { get; set; }

        /// <summary>
        /// Number of requirements in the project
        /// </summary>
        public int RequirementCount { get; set; }

        /// <summary>
        /// Number of analyzed requirements
        /// </summary>
        public int AnalyzedCount { get; set; }

        /// <summary>
        /// Average quality score of analyzed requirements
        /// </summary>
        public double? AverageQualityScore { get; set; }
    }

    /// <summary>
    /// DTO for creating a new requirement
    /// </summary>
    public class CreateRequirementRequest
    {
        /// <summary>
        /// The requirement text
        /// </summary>
        [Required]
        [MaxLength(5000)]
        public string Text { get; set; } = string.Empty;

        /// <summary>
        /// Optional title for the requirement
        /// </summary>
        [MaxLength(200)]
        public string? Title { get; set; }
    }

    /// <summary>
    /// DTO for updating a requirement
    /// </summary>
    public class UpdateRequirementRequest
    {
        /// <summary>
        /// Updated requirement text
        /// </summary>
        [Required]
        [MaxLength(5000)]
        public string Text { get; set; } = string.Empty;

        /// <summary>
        /// Updated title for the requirement
        /// </summary>
        [MaxLength(200)]
        public string? Title { get; set; }
    }

    /// <summary>
    /// DTO for requirement information
    /// </summary>
    public class RequirementDto
    {
        /// <summary>
        /// Requirement identifier
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Parent project identifier
        /// </summary>
        public int ProjectId { get; set; }

        /// <summary>
        /// Requirement text
        /// </summary>
        public string Text { get; set; } = string.Empty;

        /// <summary>
        /// Requirement title
        /// </summary>
        public string? Title { get; set; }

        /// <summary>
        /// Analysis status
        /// </summary>
        public string Status { get; set; } = string.Empty;

        /// <summary>
        /// Quality score if analyzed
        /// </summary>
        public int? QualityScore { get; set; }

        /// <summary>
        /// Creation timestamp
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// Last update timestamp
        /// </summary>
        public DateTime UpdatedAt { get; set; }

        /// <summary>
        /// Analysis result data
        /// </summary>
        public AnalysisResponse? Analysis { get; set; }

        /// <summary>
        /// Enhancement suggestions
        /// </summary>
        public EnhancementResponse? Enhancements { get; set; }
    }

    /// <summary>
    /// DTO for project statistics
    /// </summary>
    public class ProjectStatsDto
    {
        /// <summary>
        /// Total number of requirements
        /// </summary>
        public int TotalRequirements { get; set; }

        /// <summary>
        /// Number of analyzed requirements
        /// </summary>
        public int AnalyzedRequirements { get; set; }

        /// <summary>
        /// Number of enhanced requirements
        /// </summary>
        public int EnhancedRequirements { get; set; }

        /// <summary>
        /// Average quality score
        /// </summary>
        public double? AverageQualityScore { get; set; }

        /// <summary>
        /// Quality score distribution
        /// </summary>
        public Dictionary<string, int> QualityDistribution { get; set; } = new();

        /// <summary>
        /// Most common issue types
        /// </summary>
        public Dictionary<string, int> CommonIssues { get; set; } = new();
    }
}