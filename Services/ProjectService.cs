using RequirementsAnalyzer.API.DTOs;
using RequirementsAnalyzer.API.Models;
using RequirementsAnalyzer.API.Data;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using Entities = RequirementsAnalyzer.API.Models.Entities; // Alias to resolve conflicts

namespace RequirementsAnalyzer.API.Services
{
    /// <summary>
    /// Database implementation of project service using Entity Framework
    /// </summary>
    public class ProjectService : IProjectService
    {
        private readonly ILogger<ProjectService> _logger;
        private readonly IPerplexityService _perplexityService;
        private readonly ApplicationDbContext _context;

        /// <summary>
        /// Initializes a new instance of the ProjectService
        /// </summary>
        /// <param name="logger">Logger instance</param>
        /// <param name="perplexityService">Perplexity service for analysis</param>
        /// <param name="context">Database context</param>
        public ProjectService(
            ILogger<ProjectService> logger,
            IPerplexityService perplexityService,
            ApplicationDbContext context)
        {
            _logger = logger;
            _perplexityService = perplexityService;
            _context = context;
        }

        /// <summary>
        /// Gets all projects with summary information
        /// </summary>
        /// <returns>List of project DTOs</returns>
        public async Task<List<ProjectDto>> GetAllProjectsAsync()
        {
            var projects = await _context.Projects
                .Include(p => p.Requirements)
                .ToListAsync();

            return projects.Select(p => new ProjectDto {
                Id = p.Id,
                Name = p.Name,
                Description = p.Description,
                CreatedAt = p.CreatedAt,
                UpdatedAt = p.UpdatedAt,
                RequirementCount = p.Requirements.Count,
                AnalyzedCount = p.Requirements.Count(r => r.Status != Entities.RequirementStatus.Draft),
                AverageQualityScore = p.Requirements
                    .Where(r => r.QualityScore.HasValue)
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
            var project = await _context.Projects
                .Include(p => p.Requirements)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (project == null) return null;

            return new ProjectDto {
                Id = project.Id,
                Name = project.Name,
                Description = project.Description,
                CreatedAt = project.CreatedAt,
                UpdatedAt = project.UpdatedAt,
                RequirementCount = project.Requirements.Count,
                AnalyzedCount = project.Requirements.Count(r => r.Status != Entities.RequirementStatus.Draft),
                AverageQualityScore = project.Requirements
                    .Where(r => r.QualityScore.HasValue)
                    .Average(r => (double?)r.QualityScore),
                Requirements = project.Requirements.Select(MapToRequirementDto).ToList()
            };
        }

        /// <summary>
        /// Creates a new project
        /// </summary>
        /// <param name="request">Project creation request</param>
        /// <returns>Created project DTO</returns>
        public async Task<ProjectDto> CreateProjectAsync(CreateProjectRequest request)
        {
            var project = new Entities.Project {
                Name = request.Name,
                Description = request.Description
            };

            _context.Projects.Add(project);
            await _context.SaveChangesAsync();

            return new ProjectDto {
                Id = project.Id,
                Name = project.Name,
                Description = project.Description,
                CreatedAt = project.CreatedAt,
                UpdatedAt = project.UpdatedAt,
                RequirementCount = 0,
                AnalyzedCount = 0,
                AverageQualityScore = null,
                Requirements = new List<RequirementDto>()
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
            var project = await _context.Projects
                .Include(p => p.Requirements)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (project == null) return null;

            project.Name = request.Name;
            project.Description = request.Description;
            // UpdatedAt will be set automatically by the DbContext

            await _context.SaveChangesAsync();

            return new ProjectDto {
                Id = project.Id,
                Name = project.Name,
                Description = project.Description,
                CreatedAt = project.CreatedAt,
                UpdatedAt = project.UpdatedAt,
                RequirementCount = project.Requirements.Count,
                AnalyzedCount = project.Requirements.Count(r => r.Status != Entities.RequirementStatus.Draft),
                AverageQualityScore = project.Requirements
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
            var project = await _context.Projects.FindAsync(id);
            if (project == null) return false;

            _context.Projects.Remove(project);
            await _context.SaveChangesAsync();

            return true;
        }

        /// <summary>
        /// Gets project statistics and analysis summary
        /// </summary>
        /// <param name="id">Project ID</param>
        /// <returns>Project statistics or null if not found</returns>
        public async Task<ProjectStatsDto?> GetProjectStatsAsync(int id)
        {
            var project = await _context.Projects
                .Include(p => p.Requirements)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (project == null) return null;

            var requirements = project.Requirements.ToList();

            return new ProjectStatsDto {
                ProjectId = id,
                TotalRequirements = requirements.Count,
                AnalyzedRequirements = requirements.Count(r => r.Status != Entities.RequirementStatus.Draft),
                AverageQualityScore = requirements
                    .Where(r => r.QualityScore.HasValue)
                    .Average(r => (double?)r.QualityScore),
                StatusBreakdown = requirements
                    .GroupBy(r => r.Status)
                    .ToDictionary(g => g.Key.ToString(), g => g.Count()),
                LastAnalyzed = requirements
                    .Where(r => r.Status != Entities.RequirementStatus.Draft)
                    .Max(r => (DateTime?)r.UpdatedAt)
            };
        }

        /// <summary>
        /// Gets all requirements for a specific project
        /// </summary>
        /// <param name="projectId">Project ID</param>
        /// <returns>List of requirement DTOs</returns>
        public async Task<List<RequirementDto>> GetProjectRequirementsAsync(int projectId)
        {
            var requirements = await _context.Requirements
                .Where(r => r.ProjectId == projectId)
                .OrderBy(r => r.CreatedAt)
                .ToListAsync();

            return requirements.Select(MapToRequirementDto).ToList();
        }

        /// <summary>
        /// Gets a specific requirement
        /// </summary>
        /// <param name="projectId">Project ID</param>
        /// <param name="requirementId">Requirement ID</param>
        /// <returns>Requirement DTO or null if not found</returns>
        public async Task<RequirementDto?> GetRequirementAsync(int projectId, int requirementId)
        {
            var requirement = await _context.Requirements
                .FirstOrDefaultAsync(r => r.ProjectId == projectId && r.Id == requirementId);

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
            var project = await _context.Projects.FindAsync(projectId);
            if (project == null) return null;

            var requirement = new Entities.Requirement {
                ProjectId = projectId,
                Text = request.Text,
                Title = request.Title,
                Status = Entities.RequirementStatus.Draft
            };

            _context.Requirements.Add(requirement);
            await _context.SaveChangesAsync();

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
            var requirement = await _context.Requirements
                .FirstOrDefaultAsync(r => r.ProjectId == projectId && r.Id == requirementId);

            if (requirement == null) return null;

            requirement.Text = request.Text;
            requirement.Title = request.Title;

            // Reset analysis status if text changed significantly
            requirement.Status = Entities.RequirementStatus.Draft;
            requirement.QualityScore = null;
            requirement.AnalysisData = null;
            requirement.EnhancementData = null;
            // UpdatedAt will be set automatically by DbContext

            await _context.SaveChangesAsync();

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
            var requirement = await _context.Requirements
                .FirstOrDefaultAsync(r => r.ProjectId == projectId && r.Id == requirementId);

            if (requirement == null) return false;

            _context.Requirements.Remove(requirement);
            await _context.SaveChangesAsync();

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
            var requirement = await _context.Requirements
                .FirstOrDefaultAsync(r => r.ProjectId == projectId && r.Id == requirementId);

            if (requirement == null) return null;

            try
            {
                // Use the Perplexity service for analysis
                var analysisResponse = await _perplexityService.AnalyzeRequirementAsync(requirement.Text);

                // Update requirement with analysis results
                requirement.Status = Entities.RequirementStatus.Analyzed;
                requirement.QualityScore = (int?)analysisResponse.OverallScore;
                requirement.AnalysisData = JsonSerializer.Serialize(new {
                    overallScore = analysisResponse.OverallScore,
                    issues = analysisResponse.Issues.Select(i => new {
                        type = i.Type,
                        severity = i.Severity,
                        description = i.Description,
                        problematicText = i.ProblematicText,
                        suggestion = i.Suggestion
                    }),
                    analyzedAt = analysisResponse.AnalyzedAt
                });

                await _context.SaveChangesAsync();

                return analysisResponse;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Analysis failed for requirement {RequirementId}", requirementId);

                requirement.Status = Entities.RequirementStatus.Rejected;
                await _context.SaveChangesAsync();

                throw new InvalidOperationException("Analysis failed", ex);
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
            var requirement = await _context.Requirements
                .FirstOrDefaultAsync(r => r.ProjectId == projectId && r.Id == requirementId);

            if (requirement == null || requirement.Status != Entities.RequirementStatus.Analyzed)
                return null;

            try
            {
                var issues = new List<QualityIssueDto>();

                // Use the Perplexity service for enhancement
                var enhancementResponse = await _perplexityService.EnhanceRequirementAsync(
                    requirement.Text, issues);

                requirement.EnhancementData = JsonSerializer.Serialize(new {
                    enhancements = enhancementResponse.Enhancements.Select(e => new {
                        text = e.Text,
                        changes = e.Changes ?? new List<string>(), // Already a List<string>, no need to split
                        improvements = e.Improvements ?? new List<string>(), // Already a List<string>, no need to split
                        qualityScore = e.QualityScore,
                        rationale = e.Rationale
                    }),
                    recommendedIndex = enhancementResponse.RecommendedIndex
                });

                requirement.Status = Entities.RequirementStatus.Enhanced;
                await _context.SaveChangesAsync();

                return enhancementResponse;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Enhancement failed for requirement {RequirementId}", requirementId);
                throw new InvalidOperationException("Enhancement failed", ex);
            }
        }

        /// <summary>
        /// Analyzes all requirements in a project
        /// </summary>
        /// <param name="projectId">Project ID</param>
        /// <returns>List of analysis results or null if project not found</returns>
        public async Task<List<AnalysisResponse>?> AnalyzeAllRequirementsAsync(int projectId)
        {
            var project = await _context.Projects.FindAsync(projectId);
            if (project == null) return null;

            var requirements = await _context.Requirements
                .Where(r => r.ProjectId == projectId && r.Status == Entities.RequirementStatus.Draft)
                .ToListAsync();

            var results = new List<AnalysisResponse>();

            foreach (var requirement in requirements)
            {
                try
                {
                    var analysisResponse = await _perplexityService.AnalyzeRequirementAsync(requirement.Text);

                    // Update requirement
                    requirement.Status = Entities.RequirementStatus.Analyzed;
                    requirement.QualityScore = (int?)analysisResponse.OverallScore;
                    requirement.AnalysisData = JsonSerializer.Serialize(new {
                        overallScore = analysisResponse.OverallScore,
                        issues = analysisResponse.Issues.Select(i => new {
                            type = i.Type,
                            severity = i.Severity,
                            description = i.Description,
                            problematicText = i.ProblematicText,
                            suggestion = i.Suggestion
                        }),
                        analyzedAt = analysisResponse.AnalyzedAt
                    });

                    results.Add(analysisResponse);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Analysis failed for requirement {RequirementId}", requirement.Id);
                    requirement.Status = Entities.RequirementStatus.Rejected;
                }
            }

            await _context.SaveChangesAsync();
            return results;
        }

        /// <summary>
        /// Maps a requirement entity to a DTO
        /// </summary>
        private static RequirementDto MapToRequirementDto(Entities.Requirement requirement)
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

            // Parse and include analysis data if available
            if (!string.IsNullOrEmpty(requirement.AnalysisData))
            {
                try
                {
                    var analysisData = JsonSerializer.Deserialize<JsonElement>(requirement.AnalysisData);
                    dto.Analysis = analysisData;
                }
                catch (Exception)
                {
                    // Log but don't fail if JSON parsing fails
                }
            }

            // Parse and include enhancement data if available
            if (!string.IsNullOrEmpty(requirement.EnhancementData))
            {
                try
                {
                    var enhancementData = JsonSerializer.Deserialize<JsonElement>(requirement.EnhancementData);
                    dto.Enhancements = enhancementData;
                }
                catch (Exception)
                {
                    // Log but don't fail if JSON parsing fails
                }
            }

            return dto;
        }
    }
}