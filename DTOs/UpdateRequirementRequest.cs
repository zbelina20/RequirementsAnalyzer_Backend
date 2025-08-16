using System.ComponentModel.DataAnnotations;

namespace RequirementsAnalyzer.API.DTOs
{
    public class UpdateRequirementRequest
    {
        [Required]
        [MaxLength(2000)]
        public string Text { get; set; } = string.Empty;

        [MaxLength(255)]
        public string? Title { get; set; }
    }
}