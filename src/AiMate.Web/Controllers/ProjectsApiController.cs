using AiMate.Core.Entities;
using AiMate.Core.Services;
using AiMate.Shared.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AiMate.Web.Controllers;

/// <summary>
/// API for managing software projects and initiatives
/// </summary>
[ApiController]
[Route("api/v1/projects")]
[Authorize] // Requires authentication
public class ProjectsApiController : ControllerBase
{
    private readonly IProjectService _projectService;
    private readonly ILogger<ProjectsApiController> _logger;

    public ProjectsApiController(
        IProjectService projectService,
        ILogger<ProjectsApiController> logger)
    {
        _projectService = projectService;
        _logger = logger;
    }

    /// <summary>
    /// Get all projects for a user
    /// </summary>
    /// <param name="userId">User ID (GUID)</param>
    /// <returns>List of projects ordered by last updated</returns>
    /// <response code="200">Returns the list of projects</response>
    /// <response code="400">Invalid user ID format</response>
    /// <response code="500">Internal server error</response>
    [HttpGet]
    [ProducesResponseType(typeof(List<ProjectDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<List<ProjectDto>>> GetProjects([FromQuery] string userId)
    {
        try
        {
            if (!Guid.TryParse(userId, out var userGuid))
            {
                return BadRequest("Invalid user ID");
            }

            var projects = await _projectService.GetUserProjectsAsync(userGuid);
            var projectDtos = projects.Select(MapToDto).OrderByDescending(p => p.UpdatedAt).ToList();

            return Ok(projectDtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting projects for user {UserId}", userId);
            return StatusCode(500, "Error retrieving projects");
        }
    }

    /// <summary>
    /// Get a specific project by ID
    /// </summary>
    /// <param name="id">Project ID (GUID)</param>
    /// <returns>Project details</returns>
    /// <response code="200">Returns the project</response>
    /// <response code="400">Invalid project ID format</response>
    /// <response code="404">Project not found</response>
    /// <response code="500">Internal server error</response>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ProjectDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ProjectDto>> GetProject(string id)
    {
        try
        {
            if (!Guid.TryParse(id, out var projectGuid))
            {
                return BadRequest("Invalid project ID");
            }

            var project = await _projectService.GetProjectByIdAsync(projectGuid);

            if (project == null)
            {
                return NotFound();
            }

            return Ok(MapToDto(project));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting project {ProjectId}", id);
            return StatusCode(500, "Error retrieving project");
        }
    }

    /// <summary>
    /// Create a new project
    /// </summary>
    /// <param name="request">Project details</param>
    /// <param name="userId">User ID (GUID)</param>
    /// <returns>Created project with ID</returns>
    /// <response code="201">Project created successfully</response>
    /// <response code="400">Invalid request or user ID</response>
    /// <response code="500">Internal server error</response>
    /// <remarks>
    /// Sample request:
    ///
    ///     POST /api/v1/projects?userId=abc123
    ///     {
    ///         "key": "PROJ-001",
    ///         "name": "New Website",
    ///         "description": "Build a new company website",
    ///         "status": "In Progress",
    ///         "priority": "High",
    ///         "budget": 50000,
    ///         "startDate": "2024-01-01T00:00:00Z",
    ///         "dueDate": "2024-06-30T00:00:00Z",
    ///         "owner": "John Doe",
    ///         "ownerEmail": "john@example.com",
    ///         "teamMembers": ["alice@example.com", "bob@example.com"],
    ///         "tags": ["web", "frontend", "priority"]
    ///     }
    ///
    /// </remarks>
    [HttpPost]
    [ProducesResponseType(typeof(ProjectDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ProjectDto>> CreateProject([FromBody] CreateProjectRequest request, [FromQuery] string userId)
    {
        try
        {
            if (!Guid.TryParse(userId, out var userGuid))
            {
                return BadRequest("Invalid user ID");
            }

            var project = new Project
            {
                Key = request.Key,
                Name = request.Name,
                Description = request.Description,
                UserId = userGuid,
                Owner = request.Owner ?? request.OwnerName,
                OwnerEmail = request.OwnerEmail,
                Status = request.Status,
                Priority = request.Priority,
                Budget = request.Budget,
                StartDate = request.StartDate,
                DueDate = request.DueDate,
                ProgressPercent = request.ProgressPercent,
                Tags = request.Tags ?? new List<string>(),
                TeamMembers = request.TeamMembers ?? new List<string>(),
                Notes = request.Notes
            };

            var created = await _projectService.CreateProjectAsync(project);

            _logger.LogInformation("Created project {ProjectId} for user {UserId}", created.Id, userId);

            return CreatedAtAction(nameof(GetProject), new { id = created.Id.ToString() }, MapToDto(created));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating project");
            return StatusCode(500, "Error creating project");
        }
    }

    /// <summary>
    /// Update an existing project
    /// </summary>
    /// <param name="id">Project ID (GUID)</param>
    /// <param name="request">Updated project details (only include fields to update)</param>
    /// <returns>Updated project</returns>
    /// <response code="200">Project updated successfully</response>
    /// <response code="400">Invalid project ID format</response>
    /// <response code="404">Project not found</response>
    /// <response code="500">Internal server error</response>
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(ProjectDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ProjectDto>> UpdateProject(string id, [FromBody] UpdateProjectRequest request)
    {
        try
        {
            if (!Guid.TryParse(id, out var projectGuid))
            {
                return BadRequest("Invalid project ID");
            }

            var project = await _projectService.GetProjectByIdAsync(projectGuid);

            if (project == null)
            {
                return NotFound();
            }

            // Update fields
            project.Name = request.Name ?? project.Name;
            project.Description = request.Description ?? project.Description;
            project.Status = request.Status ?? project.Status;
            project.Priority = request.Priority ?? project.Priority;
            project.Budget = request.Budget ?? project.Budget;
            project.ProgressPercent = request.ProgressPercent ?? project.ProgressPercent;
            project.StartDate = request.StartDate ?? project.StartDate;
            project.DueDate = request.DueDate ?? project.DueDate;
            project.Owner = request.Owner ?? project.Owner;
            project.TeamMembers = request.TeamMembers ?? project.TeamMembers;
            project.Tags = request.Tags ?? project.Tags;
            project.Notes = request.Notes ?? project.Notes;

            var updated = await _projectService.UpdateProjectAsync(project);

            _logger.LogInformation("Updated project {ProjectId}", id);

            return Ok(MapToDto(updated));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating project {ProjectId}", id);
            return StatusCode(500, "Error updating project");
        }
    }

    /// <summary>
    /// Delete a project
    /// </summary>
    /// <param name="id">Project ID (GUID)</param>
    /// <returns>No content</returns>
    /// <response code="204">Project deleted successfully</response>
    /// <response code="400">Invalid project ID format</response>
    /// <response code="500">Internal server error</response>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> DeleteProject(string id)
    {
        try
        {
            if (!Guid.TryParse(id, out var projectGuid))
            {
                return BadRequest("Invalid project ID");
            }

            await _projectService.DeleteProjectAsync(projectGuid);

            _logger.LogInformation("Deleted project {ProjectId}", id);

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting project {ProjectId}", id);
            return StatusCode(500, "Error deleting project");
        }
    }

    // Helper method to map Project entity to ProjectDto
    private static ProjectDto MapToDto(Project project)
    {
        return new ProjectDto
        {
            Id = project.Id.ToString(),
            Key = project.Key ?? "",
            Name = project.Name,
            Description = project.Description ?? "",
            OwnerId = project.UserId.ToString(),
            Owner = project.Owner,
            OwnerEmail = project.OwnerEmail,
            Status = project.Status,
            Priority = project.Priority,
            Budget = project.Budget,
            StartDate = project.StartDate,
            DueDate = project.DueDate,
            ProgressPercent = project.ProgressPercent,
            Tags = project.Tags,
            TeamMembers = project.TeamMembers,
            Notes = project.Notes,
            CreatedAt = project.CreatedAt,
            UpdatedAt = project.UpdatedAt
        };
    }
}
