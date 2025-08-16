namespace RequirementsAnalyzer.API.DTOs
{
    public class ProjectStatsDto
    {
        public int ProjectId { get; set; }
        public int TotalRequirements { get; set; }
        public int AnalyzedRequirements { get; set; }
        public double? AverageQualityScore { get; set; }
        public Dictionary<string, int> StatusBreakdown { get; set; } = new();
        public DateTime? LastAnalyzed { get; set; }
    }
}