namespace RequirementsAnalyzer.API.DTOs
{
    public class ProjectDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public int RequirementCount { get; set; }
        public int AnalyzedCount { get; set; }
        public double? AverageQualityScore { get; set; }
        public List<RequirementDto>? Requirements { get; set; }
    }
}