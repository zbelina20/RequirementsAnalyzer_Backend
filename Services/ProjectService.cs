using RequirementsAnalyzer.API.DTOs;
using RequirementsAnalyzer.API.Models;
using System.Text.Json;
using System.Linq;

namespace RequirementsAnalyzer.API.Services
{
    /// <summary>
    /// In-memory implementation of project service for development
    /// </summary>
    public class ProjectService : IProjectService
    {
        private readonly ILogger<ProjectService> _logger;
        private readonly IPerplexityService _perplexityService;

        // In-memory storage (replace with database later)
        private static readonly List<Project> _projects = new();
        private static readonly List<ProjectRequirement> _requirements = new();
        private static int _nextProjectId = 1;
        private static int _nextRequirementId = 1;

        /// <summary>
        /// Initializes a new instance of the ProjectService
        /// </summary>
        /// <param name="logger">Logger instance</param>
        /// <param name="perplexityService">Perplexity service for analysis</param>
        public ProjectService(ILogger<ProjectService> logger, IPerplexityService perplexityService)
        {
            _logger = logger;
            _perplexityService = perplexityService;
        }

        /// <summary>
        /// Gets all projects with summary information
        /// </summary>
        /// <returns>List of project DTOs</returns>
        public async Task<List<ProjectDto>> GetAllProjectsAsync()
        {
            await Task.CompletedTask; // Simulate async operation

            return _projects.Select(p => new ProjectDto {
                Id = p.Id,
                Name = p.Name,
                Description = p.Description,
                CreatedAt = p.CreatedAt,
                UpdatedAt = p.UpdatedAt,
                RequirementCount = _requirements.Count(r => r.ProjectId == p.Id),
                AnalyzedCount = _requirements.Count(r => r.ProjectId == p.Id && r.Status != RequirementStatus.Draft),
                AverageQualityScore = _requirements
                    .Where(r => r.ProjectId == p.Id && r.QualityScore.HasValue)
                    .Average(r => (double?)r.QualityScore)
            }).ToList();
        }

        /// <summary>
        /// Gets a specific project by ID with all requirements
        /// </summary>
        /// <param name="id">Project ID</param>
        /// <returns>Project with requirements or null if not found</returns>
        public async Task<ProjectDto?> GetProjectByIdAsync(int id)
        {
            await Task.CompletedTask;

            var project = _projects.FirstOrDefault(p => p.Id == id);
            if (project == null) return null;

            var requirements = _requirements.Where(r => r.ProjectId == id).ToList();

            return new ProjectDto {
                Id = project.Id,
                Name = project.Name,
                Description = project.Description,
                CreatedAt = project.CreatedAt,
                UpdatedAt = project.UpdatedAt,
                RequirementCount = requirements.Count,
                AnalyzedCount = requirements.Count(r => r.Status != RequirementStatus.Draft),
                AverageQualityScore = requirements
                    .Where(r => r.QualityScore.HasValue)
                    .Average(r => (double?)r.QualityScore)
            };
        }

        /// <summary>
        /// Creates a new project
        /// </summary>
        /// <param name="request">Project creation request</param>
        /// <returns>Created project DTO</returns>
        public async Task<ProjectDto> CreateProjectAsync(CreateProjectRequest request)
        {
            await Task.CompletedTask;

            var project = new Project {
                Id = _nextProjectId++,
                Name = request.Name,
                Description = request.Description,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _projects.Add(project);
            _logger.LogInformation("Created project {ProjectId}: {ProjectName}", project.Id, project.Name);

            return new ProjectDto {
                Id = project.Id,
                Name = project.Name,
                Description = project.Description,
                CreatedAt = project.CreatedAt,
                UpdatedAt = project.UpdatedAt,
                RequirementCount = 0,
                AnalyzedCount = 0,
                AverageQualityScore = null
            };
        }

        /// <summary>
        /// Updates an existing project
        /// </summary>
        /// <param name="id">Project ID</param>
        /// <param name="request">Project update request</param>
        /// <returns>Updated project DTO or null if not found</returns>
        public async Task<ProjectDto?> UpdateProjectAsync(int id, UpdateProjectRequest request)
        {
            await Task.CompletedTask;

            var project = _projects.FirstOrDefault(p => p.Id == id);
            if (project == null) return null;

            project.Name = request.Name;
            project.Description = request.Description;
            project.UpdatedAt = DateTime.UtcNow;

            var requirements = _requirements.Where(r => r.ProjectId == id).ToList();

            return new ProjectDto {
                Id = project.Id,
                Name = project.Name,
                Description = project.Description,
                CreatedAt = project.CreatedAt,
                UpdatedAt = project.UpdatedAt,
                RequirementCount = requirements.Count,
                AnalyzedCount = requirements.Count(r => r.Status != RequirementStatus.Draft),
                AverageQualityScore = requirements
                    .Where(r => r.QualityScore.HasValue)
                    .Average(r => (double?)r.QualityScore)
            };
        }

        /// <summary>
        /// Deletes a project and all its requirements
        /// </summary>
        /// <param name="id">Project ID</param>
        /// <returns>True if deleted, false if not found</returns>
        public async Task<bool> DeleteProjectAsync(int id)
        {
            await Task.CompletedTask;

            var project = _projects.FirstOrDefault(p => p.Id == id);
            if (project == null) return false;

            // Remove all requirements for this project
            _requirements.RemoveAll(r => r.ProjectId == id);
            _projects.Remove(project);

            return true;
        }

        /// <summary>
        /// Gets project statistics and analysis summary
        /// </summary>
        /// <param name="id">Project ID</param>
        /// <returns>Project statistics or null if not found</returns>
        public async Task<ProjectStatsDto?> GetProjectStatsAsync(int id)
        {
            await Task.CompletedTask;

            var project = _projects.FirstOrDefault(p => p.Id == id);
            if (project == null) return null;

            var requirements = _requirements.Where(r => r.ProjectId == id).ToList();
            var analyzed = requirements.Where(r => r.Status != RequirementStatus.Draft).ToList();

            var stats = new ProjectStatsDto {
                TotalRequirements = requirements.Count,
                AnalyzedRequirements = analyzed.Count,
                EnhancedRequirements = requirements.Count(r => r.Status == RequirementStatus.Enhanced),
                AverageQualityScore = analyzed
                    .Where(r => r.QualityScore.HasValue)
                    .Average(r => (double?)r.QualityScore)
            };

            // Quality distribution
            foreach (var req in analyzed.Where(r => r.QualityScore.HasValue))
            {
                var category = req.QualityScore >= 80 ? "Excellent" :
                              req.QualityScore >= 60 ? "Good" :
                              req.QualityScore >= 40 ? "Fair" : "Poor";
                stats.QualityDistribution[category] = stats.QualityDistribution.GetValueOrDefault(category, 0) + 1;
            }

            // Common issues (simplified for in-memory implementation)
            foreach (var req in analyzed.Where(r => !string.IsNullOrEmpty(r.AnalysisData)))
            {
                try
                {
                    var analysis = JsonSerializer.Deserialize<AnalysisResponse>(req.AnalysisData!);
                    if (analysis?.Issues != null)
                    {
                        foreach (var issue in analysis.Issues)
                        {
                            if (!string.IsNullOrEmpty(issue.Type))
                            {
                                stats.CommonIssues[issue.Type] = stats.CommonIssues.GetValueOrDefault(issue.Type, 0) + 1;
                            }
                        }
                    }
                }
                catch (JsonException ex)
                {
                    _logger.LogWarning(ex, "Failed to parse analysis data for requirement {RequirementId}", req.Id);
                }
            }

            return stats;
        }

        /// <summary>
        /// Gets all requirements for a specific project
        /// </summary>
        /// <param name="projectId">Project ID</param>
        /// <returns>List of requirement DTOs</returns>
        public async Task<List<RequirementDto>> GetProjectRequirementsAsync(int projectId)
        {
            await Task.CompletedTask;

            return _requirements
                .Where(r => r.ProjectId == projectId)
                .Select(MapToRequirementDto)
                .ToList();
        }

        /// <summary>
        /// Gets a specific requirement
        /// </summary>
        /// <param name="projectId">Project ID</param>
        /// <param name="requirementId">Requirement ID</param>
        /// <returns>Requirement DTO or null if not found</returns>
        public async Task<RequirementDto?> GetRequirementAsync(int projectId, int requirementId)
        {
            await Task.CompletedTask;

            var requirement = _requirements.FirstOrDefault(r => r.ProjectId == projectId && r.Id == requirementId);
            return requirement != null ? MapToRequirementDto(requirement) : null;
        }

        /// <summary>
        /// Adds a new requirement to a project
        /// </summary>
        /// <param name="projectId">Project ID</param>
        /// <param name="request">Requirement creation request</param>
        /// <returns>Created requirement DTO or null if project not found</returns>
        public async Task<RequirementDto?> AddRequirementAsync(int projectId, CreateRequirementRequest request)
        {
            await Task.CompletedTask;

            var project = _projects.FirstOrDefault(p => p.Id == projectId);
            if (project == null) return null;

            var requirement = new ProjectRequirement {
                Id = _nextRequirementId++,
                ProjectId = projectId,
                Text = request.Text,
                Title = request.Title,
                Status = RequirementStatus.Draft,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _requirements.Add(requirement);
            project.UpdatedAt = DateTime.UtcNow;

            return MapToRequirementDto(requirement);
        }

        /// <summary>
        /// Updates an existing requirement
        /// </summary>
        /// <param name="projectId">Project ID</param>
        /// <param name="requirementId">Requirement ID</param>
        /// <param name="request">Requirement update request</param>
        /// <returns>Updated requirement DTO or null if not found</returns>
        public async Task<RequirementDto?> UpdateRequirementAsync(int projectId, int requirementId, UpdateRequirementRequest request)
        {
            await Task.CompletedTask;

            var requirement = _requirements.FirstOrDefault(r => r.ProjectId == projectId && r.Id == requirementId);
            if (requirement == null) return null;

            requirement.Text = request.Text;
            requirement.Title = request.Title;
            requirement.UpdatedAt = DateTime.UtcNow;

            // Reset analysis status if text changed significantly
            requirement.Status = RequirementStatus.Draft;
            requirement.QualityScore = null;
            requirement.AnalysisData = null;
            requirement.EnhancementData = null;

            var project = _projects.FirstOrDefault(p => p.Id == projectId);
            if (project != null) project.UpdatedAt = DateTime.UtcNow;

            return MapToRequirementDto(requirement);
        }

        /// <summary>
        /// Deletes a requirement from a project
        /// </summary>
        /// <param name="projectId">Project ID</param>
        /// <param name="requirementId">Requirement ID</param>
        /// <returns>True if deleted, false if not found</returns>
        public async Task<bool> DeleteRequirementAsync(int projectId, int requirementId)
        {
            await Task.CompletedTask;

            var requirement = _requirements.FirstOrDefault(r => r.ProjectId == projectId && r.Id == requirementId);
            if (requirement == null) return false;

            _requirements.Remove(requirement);

            var project = _projects.FirstOrDefault(p => p.Id == projectId);
            if (project != null) project.UpdatedAt = DateTime.UtcNow;

            return true;
        }

        /// <summary>
        /// Analyzes a requirement for quality issues
        /// </summary>
        /// <param name="projectId">Project ID</param>
        /// <param name="requirementId">Requirement ID</param>
        /// <returns>Analysis results or null if not found</returns>
        public async Task<AnalysisResponse?> AnalyzeRequirementAsync(int projectId, int requirementId)
        {
            var requirement = _requirements.FirstOrDefault(r => r.ProjectId == projectId && r.Id == requirementId);
            if (requirement == null) return null;

            try
            {
                var analysis = await _perplexityService.AnalyzeRequirementAsync(requirement.Text);

                // Store analysis results
                requirement.AnalysisData = JsonSerializer.Serialize(analysis);
                requirement.QualityScore = (int?)analysis.OverallScore;
                requirement.Status = RequirementStatus.Analyzed;
                requirement.UpdatedAt = DateTime.UtcNow;

                var project = _projects.FirstOrDefault(p => p.Id == projectId);
                if (project != null) project.UpdatedAt = DateTime.UtcNow;

                return analysis;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to analyze requirement {RequirementId}", requirementId);
                requirement.Status = RequirementStatus.Failed;
                requirement.UpdatedAt = DateTime.UtcNow;
                throw;
            }
        }

        /// <summary>
        /// Generates enhancement suggestions for a requirement
        /// </summary>
        /// <param name="projectId">Project ID</param>
        /// <param name="requirementId">Requirement ID</param>
        /// <returns>Enhancement suggestions or null if not found</returns>
        public async Task<EnhancementResponse?> EnhanceRequirementAsync(int projectId, int requirementId)
        {
            var requirement = _requirements.FirstOrDefault(r => r.ProjectId == projectId && r.Id == requirementId);
            if (requirement == null || string.IsNullOrEmpty(requirement.AnalysisData)) return null;

            try
            {
                var analysis = JsonSerializer.Deserialize<AnalysisResponse>(requirement.AnalysisData);
                if (analysis == null) return null;

                var enhancement = await _perplexityService.EnhanceRequirementAsync(requirement.Text, analysis.Issues);

                // Store enhancement results
                requirement.EnhancementData = JsonSerializer.Serialize(enhancement);
                requirement.Status = RequirementStatus.Enhanced;
                requirement.UpdatedAt = DateTime.UtcNow;

                var project = _projects.FirstOrDefault(p => p.Id == projectId);
                if (project != null) project.UpdatedAt = DateTime.UtcNow;

                return enhancement;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to enhance requirement {RequirementId}", requirementId);
                throw;
            }
        }

        /// <summary>
        /// Analyzes all requirements in a project
        /// </summary>
        /// <param name="projectId">Project ID</param>
        /// <returns>List of analysis results or null if project not found</returns>
        public async Task<List<AnalysisResponse>?> AnalyzeAllRequirementsAsync(int projectId)
        {
            await Task.CompletedTask; // Simulate async operation

            var project = _projects.FirstOrDefault(p => p.Id == projectId);
            if (project == null) return null;

            var requirements = _requirements.Where(r => r.ProjectId == projectId).ToList();
            var analysisResults = new List<AnalysisResponse>();

            foreach (var requirement in requirements)
            {
                // Analyze each requirement
                var analysis = await _perplexityService.AnalyzeRequirementAsync(requirement.Text);

                // Update requirement with analysis data
                requirement.AnalysisData = JsonSerializer.Serialize(analysis);
                requirement.QualityScore = (int)analysis.OverallScore;
                requirement.Status = RequirementStatus.Analyzed;
                requirement.UpdatedAt = DateTime.UtcNow;

                analysisResults.Add(analysis);
            }

            return analysisResults;
        }

        private RequirementDto MapToRequirementDto(ProjectRequirement requirement)
        {
            var dto = new RequirementDto {
                Id = requirement.Id,
                ProjectId = requirement.ProjectId,
                Text = requirement.Text,
                Title = requirement.Title,
                Status = requirement.Status.ToString(),
                QualityScore = requirement.QualityScore,
                CreatedAt = requirement.CreatedAt,
                UpdatedAt = requirement.UpdatedAt
            };

            // Parse analysis data if available
            if (!string.IsNullOrEmpty(requirement.AnalysisData))
            {
                try
                {
                    dto.Analysis = JsonSerializer.Deserialize<AnalysisResponse>(requirement.AnalysisData);
                }
                catch (JsonException ex)
                {
                    _logger.LogWarning(ex, "Failed to parse analysis data for requirement {RequirementId}", requirement.Id);
                }
            }

            // Parse enhancement data if available
            if (!string.IsNullOrEmpty(requirement.EnhancementData))
            {
                try
                {
                    dto.Enhancements = JsonSerializer.Deserialize<EnhancementResponse>(requirement.EnhancementData);
                }
                catch (JsonException ex)
                {
                    _logger.LogWarning(ex, "Failed to parse enhancement data for requirement {RequirementId}", requirement.Id);
                }
            }

            return dto;
        }
    }
}