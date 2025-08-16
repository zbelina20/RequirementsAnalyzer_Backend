using System.ComponentModel.DataAnnotations;

namespace RequirementsAnalyzer.API.DTOs
{
    public class UpdateProjectRequest
    {
        [Required]
        [MaxLength(255)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(1000)]
        public string? Description { get; set; }
    }
}