using AiMate.Core.Entities;
using AiMate.Core.Services;
using AiMate.Shared.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AiMate.Web.Controllers;

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

    [HttpGet]
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

    [HttpGet("{id}")]
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

    [HttpPost]
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

    [HttpPut("{id}")]
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

    [HttpDelete("{id}")]
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
