using AiMate.Core.Entities;

namespace AiMate.Core.Services;

/// <summary>
/// Workspace service - CRUD operations for workspaces
/// </summary>
public interface IWorkspaceService
{
    /// <summary>
    /// Get all workspaces for a user
    /// </summary>
    Task<List<Workspace>> GetUserWorkspacesAsync(
        Guid userId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get workspace by ID
    /// </summary>
    Task<Workspace?> GetWorkspaceByIdAsync(
        Guid workspaceId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Create new workspace
    /// </summary>
    Task<Workspace> CreateWorkspaceAsync(
        Workspace workspace,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Update existing workspace
    /// </summary>
    Task<Workspace> UpdateWorkspaceAsync(
        Workspace workspace,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Delete workspace
    /// </summary>
    Task DeleteWorkspaceAsync(
        Guid workspaceId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get or create default workspace for user
    /// </summary>
    Task<Workspace> GetOrCreateDefaultWorkspaceAsync(
        Guid userId,
        CancellationToken cancellationToken = default);
}
