using AiMate.Shared.Models;
using Microsoft.AspNetCore.Mvc;

namespace AiMate.Web.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class ProjectsController : ControllerBase
{
    private static readonly List<ProjectDto> _projects = new();

    public ProjectsController()
    {
        if (!_projects.Any()) InitializeSampleData();
    }

    [HttpGet]
    public ActionResult<List<ProjectDto>> GetProjects([FromQuery] string userId)
    {
        var projects = _projects.Where(p => p.OwnerId == userId).OrderByDescending(p => p.UpdatedAt).ToList();
        return Ok(projects);
    }

    [HttpGet("{id}")]
    public ActionResult<ProjectDto> GetProject(string id)
    {
        var project = _projects.FirstOrDefault(p => p.Id == id);
        return project == null ? NotFound() : Ok(project);
    }

    [HttpPost]
    public ActionResult<ProjectDto> CreateProject([FromBody] CreateProjectRequest request, [FromQuery] string userId)
    {
        var project = new ProjectDto
        {
            Key = request.Key,
            Name = request.Name,
            Description = request.Description,
            OwnerId = userId,
            OwnerName = request.OwnerName,
            OwnerEmail = request.OwnerEmail,
            Status = request.Status,
            Priority = request.Priority,
            Budget = request.Budget,
            StartDate = request.StartDate,
            DueDate = request.DueDate
        };
        _projects.Add(project);
        return CreatedAtAction(nameof(GetProject), new { id = project.Id }, project);
    }

    [HttpPut("{id}")]
    public ActionResult<ProjectDto> UpdateProject(string id, [FromBody] UpdateProjectRequest request)
    {
        var project = _projects.FirstOrDefault(p => p.Id == id);
        if (project == null) return NotFound();

        var updated = project with
        {
            Name = request.Name ?? project.Name,
            Description = request.Description ?? project.Description,
            Status = request.Status ?? project.Status,
            Priority = request.Priority ?? project.Priority,
            Budget = request.Budget ?? project.Budget,
            ProgressPercent = request.ProgressPercent ?? project.ProgressPercent,
            UpdatedAt = DateTime.UtcNow
        };

        var index = _projects.IndexOf(project);
        _projects[index] = updated;
        return Ok(updated);
    }

    [HttpDelete("{id}")]
    public IActionResult DeleteProject(string id)
    {
        var project = _projects.FirstOrDefault(p => p.Id == id);
        if (project == null) return NotFound();
        _projects.Remove(project);
        return NoContent();
    }

    private void InitializeSampleData()
    {
        _projects.AddRange(new[]
        {
            new ProjectDto
            {
                Key = "AIMATE-001",
                Name = "AI Mate Platform v2",
                Description = "Complete rewrite with Blazor and modern architecture",
                OwnerId = "user-1",
                OwnerName = "John Doe",
                OwnerEmail = "john@example.com",
                Status = "Active",
                Priority = "Critical",
                Budget = 50000,
                ProgressPercent = 75,
                StartDate = DateTime.UtcNow.AddDays(-90),
                DueDate = DateTime.UtcNow.AddDays(30),
                Tags = new List<string> { "blazor", "ai", "platform" }
            },
            new ProjectDto
            {
                Key = "BYOK-002",
                Name = "BYOK Implementation",
                Description = "Bring Your Own Key feature with tier-based access control",
                OwnerId = "user-1",
                OwnerName = "John Doe",
                Status = "Completed",
                Priority = "High",
                Budget = 15000,
                ProgressPercent = 100,
                CompletedDate = DateTime.UtcNow.AddDays(-5),
                Tags = new List<string> { "security", "api", "byok" }
            }
        });
    }
}
