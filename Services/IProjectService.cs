using RequirementsAnalyzer.API.DTOs;
using RequirementsAnalyzer.API.Models;

namespace RequirementsAnalyzer.API.Services
{
    /// <summary>
    /// Service interface for project and requirement management
    /// </summary>
    public interface IProjectService
    {
        /// <summary>
        /// Gets all projects with summary information
        /// </summary>
        /// <returns>List of project DTOs</returns>
        Task<List<ProjectDto>> GetAllProjectsAsync();

        /// <summary>
        /// Gets a specific project by ID with all requirements
        /// </summary>
        /// <param name="id">Project ID</param>
        /// <returns>Project with requirements or null if not found</returns>
        Task<ProjectDto?> GetProjectByIdAsync(int id);

        /// <summary>
        /// Creates a new project
        /// </summary>
        /// <param name="request">Project creation request</param>
        /// <returns>Created project DTO</returns>
        Task<ProjectDto> CreateProjectAsync(CreateProjectRequest request);

        /// <summary>
        /// Updates an existing project
        /// </summary>
        /// <param name="id">Project ID</param>
        /// <param name="request">Project update request</param>
        /// <returns>Updated project DTO or null if not found</returns>
        Task<ProjectDto?> UpdateProjectAsync(int id, UpdateProjectRequest request);

        /// <summary>
        /// Deletes a project and all its requirements
        /// </summary>
        /// <param name="id">Project ID</param>
        /// <returns>True if deleted, false if not found</returns>
        Task<bool> DeleteProjectAsync(int id);

        /// <summary>
        /// Gets project statistics and analysis summary
        /// </summary>
        /// <param name="id">Project ID</param>
        /// <returns>Project statistics or null if not found</returns>
        Task<ProjectStatsDto?> GetProjectStatsAsync(int id);

        /// <summary>
        /// Gets all requirements for a specific project
        /// </summary>
        /// <param name="projectId">Project ID</param>
        /// <returns>List of requirement DTOs</returns>
        Task<List<RequirementDto>> GetProjectRequirementsAsync(int projectId);

        /// <summary>
        /// Gets a specific requirement
        /// </summary>
        /// <param name="projectId">Project ID</param>
        /// <param name="requirementId">Requirement ID</param>
        /// <returns>Requirement DTO or null if not found</returns>
        Task<RequirementDto?> GetRequirementAsync(int projectId, int requirementId);

        /// <summary>
        /// Adds a new requirement to a project
        /// </summary>
        /// <param name="projectId">Project ID</param>
        /// <param name="request">Requirement creation request</param>
        /// <returns>Created requirement DTO or null if project not found</returns>
        Task<RequirementDto?> AddRequirementAsync(int projectId, CreateRequirementRequest request);

        /// <summary>
        /// Updates an existing requirement
        /// </summary>
        /// <param name="projectId">Project ID</param>
        /// <param name="requirementId">Requirement ID</param>
        /// <param name="request">Requirement update request</param>
        /// <returns>Updated requirement DTO or null if not found</returns>
        Task<RequirementDto?> UpdateRequirementAsync(int projectId, int requirementId, UpdateRequirementRequest request);

        /// <summary>
        /// Deletes a requirement from a project
        /// </summary>
        /// <param name="projectId">Project ID</param>
        /// <param name="requirementId">Requirement ID</param>
        /// <returns>True if deleted, false if not found</returns>
        Task<bool> DeleteRequirementAsync(int projectId, int requirementId);

        /// <summary>
        /// Analyzes a requirement for quality issues
        /// </summary>
        /// <param name="projectId">Project ID</param>
        /// <param name="requirementId">Requirement ID</param>
        /// <returns>Analysis results or null if not found</returns>
        Task<AnalysisResponse?> AnalyzeRequirementAsync(int projectId, int requirementId);

        /// <summary>
        /// Generates enhancement suggestions for a requirement
        /// </summary>
        /// <param name="projectId">Project ID</param>
        /// <param name="requirementId">Requirement ID</param>
        /// <returns>Enhancement suggestions or null if not found</returns>
        Task<EnhancementResponse?> EnhanceRequirementAsync(int projectId, int requirementId);

        /// <summary>
        /// Analyzes all requirements in a project
        /// </summary>
        /// <param name="projectId">Project ID</param>
        /// <returns>List of analysis results or null if project not found</returns>
        Task<List<AnalysisResponse>?> AnalyzeAllRequirementsAsync(int projectId);
    }
}