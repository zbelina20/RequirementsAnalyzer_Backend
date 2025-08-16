using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RequirementsAnalyzer.API.Models.Entities
{
    [Table("requirements")]
    public class Requirement
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Required]
        [Column("project_id")]
        public int ProjectId { get; set; }

        [Required]
        [MaxLength(2000)]
        [Column("text")]
        public string Text { get; set; } = string.Empty;

        [MaxLength(255)]
        [Column("title")]
        public string? Title { get; set; }

        [Required]
        [Column("status")]
        public RequirementStatus Status { get; set; } = RequirementStatus.Draft;

        [Column("quality_score")]
        public int? QualityScore { get; set; }

        [Column("analysis_data", TypeName = "jsonb")]
        public string? AnalysisData { get; set; }

        [Column("enhancement_data", TypeName = "jsonb")]
        public string? EnhancementData { get; set; }

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Column("updated_at")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Navigation property
        [ForeignKey("ProjectId")]
        public virtual Project Project { get; set; } = null!;
    }
}