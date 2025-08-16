// Update your ProjectsController.cs to add the missing CRUD endpoints

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
        /// <returns>Created project</returns>
        [HttpPost]
        public async Task<IActionResult> CreateProject([FromBody] CreateProjectRequest request)
        {
            try
            {
                _logger.LogInformation("Creating project with name: {Name}", request.Name);

                var project = await _projectService.CreateProjectAsync(request);

                _logger.LogInformation("Created project {ProjectId}", project.Id);
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
        /// <returns>Updated project</returns>
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateProject(int id, [FromBody] UpdateProjectRequest request)
        {
            try
            {
                _logger.LogInformation("Updating project {ProjectId}", id);

                var project = await _projectService.UpdateProjectAsync(id, request);
                if (project == null)
                {
                    return NotFound(new { error = "Project not found" });
                }

                _logger.LogInformation("Updated project {ProjectId}", id);
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
                _logger.LogInformation("Deleting project {ProjectId}", id);

                var deleted = await _projectService.DeleteProjectAsync(id);
                if (!deleted)
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
        /// Gets project statistics
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
                _logger.LogError(ex, "Error retrieving project stats for {ProjectId}", id);
                return StatusCode(500, new { error = "Failed to retrieve project statistics" });
            }
        }

        /// <summary>
        /// Gets all requirements for a project
        /// </summary>
        /// <param name="id">Project ID</param>
        /// <returns>List of requirements</returns>
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
        /// Gets a specific requirement
        /// </summary>
        /// <param name="id">Project ID</param>
        /// <param name="requirementId">Requirement ID</param>
        /// <returns>Requirement details</returns>
        [HttpGet("{id}/requirements/{requirementId}")]
        public async Task<IActionResult> GetRequirement(int id, int requirementId)
        {
            try
            {
                var requirement = await _projectService.GetRequirementAsync(id, requirementId);
                if (requirement == null)
                {
                    return NotFound(new { error = "Requirement not found" });
                }

                return Ok(requirement);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving requirement {RequirementId} in project {ProjectId}", requirementId, id);
                return StatusCode(500, new { error = "Failed to retrieve requirement" });
            }
        }

        /// <summary>
        /// Adds a new requirement to a project
        /// </summary>
        /// <param name="id">Project ID</param>
        /// <param name="request">Requirement creation request</param>
        /// <returns>Created requirement</returns>
        [HttpPost("{id}/requirements")]
        public async Task<IActionResult> CreateRequirement(int id, [FromBody] CreateRequirementRequest request)
        {
            try
            {
                _logger.LogInformation("Creating requirement for project {ProjectId} with data: {Request}", id, request.GetType().Name);

                var requirement = await _projectService.AddRequirementAsync(id, request);
                if (requirement == null)
                {
                    return NotFound(new { error = "Project not found" });
                }

                _logger.LogInformation("Created requirement {RequirementId} for project {ProjectId}", requirement.Id, id);
                return CreatedAtAction(nameof(GetRequirement), new { id, requirementId = requirement.Id }, requirement);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating requirement for project {ProjectId}", id);
                return StatusCode(500, new { error = "Failed to create requirement" });
            }
        }

        /// <summary>
        /// Updates an existing requirement
        /// </summary>
        /// <param name="id">Project ID</param>
        /// <param name="requirementId">Requirement ID</param>
        /// <param name="request">Requirement update request</param>
        /// <returns>Updated requirement</returns>
        [HttpPut("{id}/requirements/{requirementId}")]
        public async Task<IActionResult> UpdateRequirement(int id, int requirementId, [FromBody] UpdateRequirementRequest request)
        {
            try
            {
                _logger.LogInformation("Updating requirement {RequirementId} in project {ProjectId}", requirementId, id);

                var requirement = await _projectService.UpdateRequirementAsync(id, requirementId, request);
                if (requirement == null)
                {
                    return NotFound(new { error = "Requirement not found" });
                }

                _logger.LogInformation("Updated requirement {RequirementId} in project {ProjectId}", requirementId, id);
                return Ok(requirement);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating requirement {RequirementId} in project {ProjectId}", requirementId, id);
                return StatusCode(500, new { error = "Failed to update requirement" });
            }
        }

        /// <summary>
        /// Deletes a requirement from a project
        /// </summary>
        /// <param name="id">Project ID</param>
        /// <param name="requirementId">Requirement ID</param>
        /// <returns>Success confirmation</returns>
        [HttpDelete("{id}/requirements/{requirementId}")]
        public async Task<IActionResult> DeleteRequirement(int id, int requirementId)
        {
            try
            {
                _logger.LogInformation("Deleting requirement {RequirementId} from project {ProjectId}", requirementId, id);

                var deleted = await _projectService.DeleteRequirementAsync(id, requirementId);
                if (!deleted)
                {
                    return NotFound(new { error = "Requirement not found" });
                }

                _logger.LogInformation("Deleted requirement {RequirementId} from project {ProjectId}", requirementId, id);
                return Ok(new { message = "Requirement deleted successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting requirement {RequirementId} from project {ProjectId}", requirementId, id);
                return StatusCode(500, new { error = "Failed to delete requirement" });
            }
        }

        /// <summary>
        /// Analyzes a requirement for quality issues
        /// </summary>
        /// <param name="id">Project ID</param>
        /// <param name="requirementId">Requirement ID</param>
        /// <returns>Analysis results</returns>
        [HttpPost("{id}/requirements/{requirementId}/analyze")]
        public async Task<IActionResult> AnalyzeRequirement(int id, int requirementId)
        {
            try
            {
                _logger.LogInformation("Analyzing requirement {RequirementId} in project {ProjectId}", requirementId, id);

                var analysis = await _projectService.AnalyzeRequirementAsync(id, requirementId);
                if (analysis == null)
                {
                    return NotFound(new { error = "Requirement not found" });
                }

                _logger.LogInformation("Analyzed requirement {RequirementId} in project {ProjectId}", requirementId, id);
                return Ok(analysis);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error analyzing requirement {RequirementId} in project {ProjectId}", requirementId, id);
                return StatusCode(500, new { error = "Failed to analyze requirement" });
            }
        }

        /// <summary>
        /// Generates enhancement suggestions for a requirement
        /// </summary>
        /// <param name="id">Project ID</param>
        /// <param name="requirementId">Requirement ID</param>
        /// <returns>Enhancement suggestions</returns>
        [HttpPost("{id}/requirements/{requirementId}/enhance")]
        public async Task<IActionResult> EnhanceRequirement(int id, int requirementId)
        {
            try
            {
                _logger.LogInformation("Enhancing requirement {RequirementId} in project {ProjectId}", requirementId, id);

                var enhancement = await _projectService.EnhanceRequirementAsync(id, requirementId);
                if (enhancement == null)
                {
                    return NotFound(new { error = "Requirement not found or not analyzed" });
                }

                _logger.LogInformation("Enhanced requirement {RequirementId} in project {ProjectId}", requirementId, id);
                return Ok(enhancement);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error enhancing requirement {RequirementId} in project {ProjectId}", requirementId, id);
                return StatusCode(500, new { error = "Failed to enhance requirement" });
            }
        }

        /// <summary>
        /// Analyzes all requirements in a project
        /// </summary>
        /// <param name="id">Project ID</param>
        /// <returns>Batch analysis results</returns>
        [HttpPost("{id}/requirements/analyze-all")]
        public async Task<IActionResult> AnalyzeAllRequirements(int id)
        {
            try
            {
                _logger.LogInformation("Analyzing all requirements in project {ProjectId}", id);

                var results = await _projectService.AnalyzeAllRequirementsAsync(id);
                if (results == null)
                {
                    return NotFound(new { error = "Project not found" });
                }

                _logger.LogInformation("Analyzed {Count} requirements in project {ProjectId}", results.Count, id);
                return Ok(results);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error analyzing all requirements in project {ProjectId}", id);
                return StatusCode(500, new { error = "Failed to analyze requirements" });
            }
        }
    }
}