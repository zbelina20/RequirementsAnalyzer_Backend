using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RequirementsAnalyzer.API.Models.Entities
{
    [Table("projects")]
    public class Project
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Required]
        [MaxLength(255)]
        [Column("name")]
        public string Name { get; set; } = string.Empty;

        [MaxLength(1000)]
        [Column("description")]
        public string? Description { get; set; }

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Column("updated_at")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Navigation property
        public virtual ICollection<Requirement> Requirements { get; set; } = new List<Requirement>();

        // Computed properties (not stored in DB)
        [NotMapped]
        public int RequirementCount => Requirements.Count;

        [NotMapped]
        public int AnalyzedCount => Requirements.Count(r => r.Status != RequirementStatus.Draft);

        [NotMapped]
        public double? AverageQualityScore => Requirements
            .Where(r => r.QualityScore.HasValue)
            .Average(r => (double?)r.QualityScore);
    }
}