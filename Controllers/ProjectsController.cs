// Controllers/ProjectsController.cs - Project management endpoints
using Microsoft.AspNetCore.Mvc;
using RequirementsAnalyzer.API.DTOs;
using RequirementsAnalyzer.API.Services;
using System.Text.Json;

namespace RequirementsAnalyzer.API.Controllers
{
    /// <summary>
    /// Controller for managing projects and their requirements
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class ProjectsController : ControllerBase
    {
        private readonly ILogger<ProjectsController> _logger;
        private readonly IProjectService _projectService;

        /// <summary>
        /// Initializes a new instance of the ProjectsController
        /// </summary>
        /// <param name="logger">Logger instance</param>
        /// <param name="projectService">Project service for business logic</param>
        public ProjectsController(
            ILogger<ProjectsController> logger,
            IProjectService projectService)
        {
            _logger = logger;
            _projectService = projectService;
        }

        /// <summary>
        /// Gets all projects with basic information
        /// </summary>
        /// <returns>List of projects</returns>
        [HttpGet]
        public async Task<IActionResult> GetProjects()
        {
            try
            {
                var projects = await _projectService.GetAllProjectsAsync();
                return Ok(projects);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving projects");
                return StatusCode(500, new { error = "Failed to retrieve projects" });
            }
        }

        /// <summary>
        /// Gets a specific project by ID with all requirements
        /// </summary>
        /// <param name="id">Project ID</param>
        /// <returns>Project details with requirements</returns>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetProject(int id)
        {
            try
            {
                var project = await _projectService.GetProjectByIdAsync(id);
                if (project == null)
                {
                    return NotFound(new { error = "Project not found" });
                }

                return Ok(project);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving project {ProjectId}", id);
                return StatusCode(500, new { error = "Failed to retrieve project" });
            }
        }

        /// <summary>
        /// Creates a new project
        /// </summary>
        /// <param name="request">Project creation request</param>
        /// <returns>Created project information</returns>
        [HttpPost]
        public async Task<IActionResult> CreateProject([FromBody] CreateProjectRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                _logger.LogInformation("Creating new project: {ProjectName}", request.Name);

                var project = await _projectService.CreateProjectAsync(request);
                return CreatedAtAction(nameof(GetProject), new { id = project.Id }, project);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating project");
                return StatusCode(500, new { error = "Failed to create project" });
            }
        }

        /// <summary>
        /// Updates an existing project
        /// </summary>
        /// <param name="id">Project ID</param>
        /// <param name="request">Project update request</param>
        /// <returns>Updated project information</returns>
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateProject(int id, [FromBody] UpdateProjectRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var project = await _projectService.UpdateProjectAsync(id, request);
                if (project == null)
                {
                    return NotFound(new { error = "Project not found" });
                }

                _logger.LogInformation("Updated project {ProjectId}: {ProjectName}", id, request.Name);
                return Ok(project);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating project {ProjectId}", id);
                return StatusCode(500, new { error = "Failed to update project" });
            }
        }

        /// <summary>
        /// Deletes a project and all its requirements
        /// </summary>
        /// <param name="id">Project ID</param>
        /// <returns>Success confirmation</returns>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProject(int id)
        {
            try
            {
                var success = await _projectService.DeleteProjectAsync(id);
                if (!success)
                {
                    return NotFound(new { error = "Project not found" });
                }

                _logger.LogInformation("Deleted project {ProjectId}", id);
                return Ok(new { message = "Project deleted successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting project {ProjectId}", id);
                return StatusCode(500, new { error = "Failed to delete project" });
            }
        }

        /// <summary>
        /// Gets project statistics and analysis summary
        /// </summary>
        /// <param name="id">Project ID</param>
        /// <returns>Project statistics</returns>
        [HttpGet("{id}/stats")]
        public async Task<IActionResult> GetProjectStats(int id)
        {
            try
            {
                var stats = await _projectService.GetProjectStatsAsync(id);
                if (stats == null)
                {
                    return NotFound(new { error = "Project not found" });
                }

                return Ok(stats);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving stats for project {ProjectId}", id);
                return StatusCode(500, new { error = "Failed to retrieve project statistics" });
            }
        }

        /// <summary>
        /// Gets all requirements for a specific project
        /// </summary>
        /// <param name="id">Project ID</param>
        /// <returns>List of project requirements</returns>
        [HttpGet("{id}/requirements")]
        public async Task<IActionResult> GetProjectRequirements(int id)
        {
            try
            {
                var requirements = await _projectService.GetProjectRequirementsAsync(id);
                return Ok(requirements);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving requirements for project {ProjectId}", id);
                return StatusCode(500, new { error = "Failed to retrieve requirements" });
            }
        }

        /// <summary>
        /// Adds a new requirement to a project
        /// </summary>
        /// <param name="id">Project ID</param>
        /// <param name="request">Requirement creation request</param>
        /// <returns>Created requirement information</returns>
        [HttpPost("{id}/requirements")]
        public async Task<IActionResult> AddRequirement(int id, [FromBody] CreateRequirementRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var requirement = await _projectService.AddRequirementAsync(id, request);
                if (requirement == null)
                {
                    return NotFound(new { error = "Project not found" });
                }

                _logger.LogInformation("Added requirement to project {ProjectId}", id);
                return CreatedAtAction(nameof(GetRequirement),
                    new { projectId = id, requirementId = requirement.Id }, requirement);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding requirement to project {ProjectId}", id);
                return StatusCode(500, new { error = "Failed to add requirement" });
            }
        }

        /// <summary>
        /// Gets a specific requirement
        /// </summary>
        /// <param name="projectId">Project ID</param>
        /// <param name="requirementId">Requirement ID</param>
        /// <returns>Requirement details</returns>
        [HttpGet("{projectId}/requirements/{requirementId}")]
        public async Task<IActionResult> GetRequirement(int projectId, int requirementId)
        {
            try
            {
                var requirement = await _projectService.GetRequirementAsync(projectId, requirementId);
                if (requirement == null)
                {
                    return NotFound(new { error = "Requirement not found" });
                }

                return Ok(requirement);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving requirement {RequirementId} from project {ProjectId}",
                    requirementId, projectId);
                return StatusCode(500, new { error = "Failed to retrieve requirement" });
            }
        }

        /// <summary>
        /// Updates a specific requirement
        /// </summary>
        /// <param name="projectId">Project ID</param>
        /// <param name="requirementId">Requirement ID</param>
        /// <param name="request">Requirement update request</param>
        /// <returns>Updated requirement information</returns>
        [HttpPut("{projectId}/requirements/{requirementId}")]
        public async Task<IActionResult> UpdateRequirement(int projectId, int requirementId,
            [FromBody] UpdateRequirementRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var requirement = await _projectService.UpdateRequirementAsync(projectId, requirementId, request);
                if (requirement == null)
                {
                    return NotFound(new { error = "Requirement not found" });
                }

                _logger.LogInformation("Updated requirement {RequirementId} in project {ProjectId}",
                    requirementId, projectId);
                return Ok(requirement);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating requirement {RequirementId} in project {ProjectId}",
                    requirementId, projectId);
                return StatusCode(500, new { error = "Failed to update requirement" });
            }
        }

        /// <summary>
        /// Deletes a specific requirement
        /// </summary>
        /// <param name="projectId">Project ID</param>
        /// <param name="requirementId">Requirement ID</param>
        /// <returns>Success confirmation</returns>
        [HttpDelete("{projectId}/requirements/{requirementId}")]
        public async Task<IActionResult> DeleteRequirement(int projectId, int requirementId)
        {
            try
            {
                var success = await _projectService.DeleteRequirementAsync(projectId, requirementId);
                if (!success)
                {
                    return NotFound(new { error = "Requirement not found" });
                }

                _logger.LogInformation("Deleted requirement {RequirementId} from project {ProjectId}",
                    requirementId, projectId);
                return Ok(new { message = "Requirement deleted successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting requirement {RequirementId} from project {ProjectId}",
                    requirementId, projectId);
                return StatusCode(500, new { error = "Failed to delete requirement" });
            }
        }

        /// <summary>
        /// Analyzes a specific requirement for quality issues
        /// </summary>
        /// <param name="projectId">Project ID</param>
        /// <param name="requirementId">Requirement ID</param>
        /// <returns>Analysis results</returns>
        [HttpPost("{projectId}/requirements/{requirementId}/analyze")]
        public async Task<IActionResult> AnalyzeRequirement(int projectId, int requirementId)
        {
            try
            {
                var result = await _projectService.AnalyzeRequirementAsync(projectId, requirementId);
                if (result is null)
                {
                    return NotFound(new { error = "Requirement not found" });
                }

                _logger.LogInformation("Analyzed requirement {RequirementId} in project {ProjectId}",
                    requirementId, projectId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error analyzing requirement {RequirementId} in project {ProjectId}",
                    requirementId, projectId);
                return StatusCode(500, new { error = "Failed to analyze requirement" });
            }
        }

        /// <summary>
        /// Generates enhancements for a specific requirement
        /// </summary>
        /// <param name="projectId">Project ID</param>
        /// <param name="requirementId">Requirement ID</param>
        /// <returns>Enhancement suggestions</returns>
        [HttpPost("{projectId}/requirements/{requirementId}/enhance")]
        public async Task<IActionResult> EnhanceRequirement(int projectId, int requirementId)
        {
            try
            {
                var result = await _projectService.EnhanceRequirementAsync(projectId, requirementId);
                if (result is null)
                {
                    return NotFound(new { error = "Requirement not found or not analyzed" });
                }

                _logger.LogInformation("Generated enhancements for requirement {RequirementId} in project {ProjectId}",
                    requirementId, projectId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error enhancing requirement {RequirementId} in project {ProjectId}",
                    requirementId, projectId);
                return StatusCode(500, new { error = "Failed to enhance requirement" });
            }
        }

        /// <summary>
        /// Analyzes all requirements in a project
        /// </summary>
        /// <param name="id">Project ID</param>
        /// <returns>Batch analysis results</returns>
        [HttpPost("{id}/analyze-all")]
        public async Task<IActionResult> AnalyzeAllRequirements(int id)
        {
            try
            {
                var results = await _projectService.AnalyzeAllRequirementsAsync(id);
                if (results == null)
                {
                    return NotFound(new { error = "Project not found" });
                }

                _logger.LogInformation("Analyzed all requirements in project {ProjectId}", id);
                return Ok(new {
                    message = "Batch analysis completed",
                    analyzedCount = results.Count,
                    results = results
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error analyzing all requirements in project {ProjectId}", id);
                return StatusCode(500, new { error = "Failed to analyze project requirements" });
            }
        }
    }
}