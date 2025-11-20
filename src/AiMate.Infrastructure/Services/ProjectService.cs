using AiMate.Core.Entities;
using AiMate.Core.Services;
using AiMate.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace AiMate.Infrastructure.Services;

/// <summary>
/// Project service implementation
/// </summary>
public class ProjectService : IProjectService
{
    private readonly AiMateDbContext _context;
    private readonly ILogger<ProjectService> _logger;

    public ProjectService(
        AiMateDbContext context,
        ILogger<ProjectService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<List<Project>> GetUserProjectsAsync(
        Guid userId,
        bool includeArchived = false,
        CancellationToken cancellationToken = default)
    {
        var query = _context.Projects.Where(p => p.UserId == userId);

        if (!includeArchived)
        {
            query = query.Where(p => !p.IsArchived);
        }

        return await query
            .OrderByDescending(p => p.UpdatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<Project?> GetProjectByIdAsync(
        Guid projectId,
        CancellationToken cancellationToken = default)
    {
        return await _context.Projects
            .Include(p => p.Workspaces)
            .FirstOrDefaultAsync(p => p.Id == projectId, cancellationToken);
    }

    public async Task<Project> CreateProjectAsync(
        Project project,
        CancellationToken cancellationToken = default)
    {
        _context.Projects.Add(project);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Created project {ProjectId} for user {UserId}",
            project.Id, project.UserId);

        return project;
    }

    public async Task<Project> UpdateProjectAsync(
        Project project,
        CancellationToken cancellationToken = default)
    {
        var existing = await _context.Projects.FindAsync(
            new object[] { project.Id }, cancellationToken);

        if (existing == null)
        {
            throw new InvalidOperationException($"Project {project.Id} not found");
        }

        existing.Key = project.Key;
        existing.Name = project.Name;
        existing.Description = project.Description;
        existing.Owner = project.Owner;
        existing.OwnerEmail = project.OwnerEmail;
        existing.Status = project.Status;
        existing.Priority = project.Priority;
        existing.Budget = project.Budget;
        existing.StartDate = project.StartDate;
        existing.DueDate = project.DueDate;
        existing.ProgressPercent = project.ProgressPercent;
        existing.Tags = project.Tags;
        existing.TeamMembers = project.TeamMembers;
        existing.Notes = project.Notes;
        existing.IsArchived = project.IsArchived;
        existing.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Updated project {ProjectId}", project.Id);

        return existing;
    }

    public async Task DeleteProjectAsync(
        Guid projectId,
        CancellationToken cancellationToken = default)
    {
        var project = await _context.Projects.FindAsync(
            new object[] { projectId }, cancellationToken);

        if (project != null)
        {
            _context.Projects.Remove(project);
            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Deleted project {ProjectId}", projectId);
        }
    }

    public async Task<Project> ArchiveProjectAsync(
        Guid projectId,
        bool isArchived,
        CancellationToken cancellationToken = default)
    {
        var project = await _context.Projects.FindAsync(
            new object[] { projectId }, cancellationToken);

        if (project == null)
        {
            throw new InvalidOperationException($"Project {projectId} not found");
        }

        project.IsArchived = isArchived;
        project.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("{Action} project {ProjectId}",
            isArchived ? "Archived" : "Unarchived", projectId);

        return project;
    }
}
