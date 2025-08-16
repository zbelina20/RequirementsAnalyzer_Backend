using System.Text.Json;

namespace RequirementsAnalyzer.API.DTOs
{
    public class RequirementDto
    {
        public int Id { get; set; }
        public int ProjectId { get; set; }
        public string Text { get; set; } = string.Empty;
        public string? Title { get; set; }
        public string Status { get; set; } = string.Empty;
        public int? QualityScore { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public JsonElement? Analysis { get; set; }
        public JsonElement? Enhancements { get; set; }
    }
}