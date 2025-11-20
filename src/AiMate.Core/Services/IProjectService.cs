using AiMate.Core.Entities;

namespace AiMate.Core.Services;

/// <summary>
/// Project service - CRUD operations for projects
/// </summary>
public interface IProjectService
{
    /// <summary>
    /// Get all projects for a user
    /// </summary>
    Task<List<Project>> GetUserProjectsAsync(
        Guid userId,
        bool includeArchived = false,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get project by ID
    /// </summary>
    Task<Project?> GetProjectByIdAsync(
        Guid projectId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Create new project
    /// </summary>
    Task<Project> CreateProjectAsync(
        Project project,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Update existing project
    /// </summary>
    Task<Project> UpdateProjectAsync(
        Project project,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Delete project
    /// </summary>
    Task DeleteProjectAsync(
        Guid projectId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Archive/unarchive project
    /// </summary>
    Task<Project> ArchiveProjectAsync(
        Guid projectId,
        bool isArchived,
        CancellationToken cancellationToken = default);
}
