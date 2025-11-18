using AiMate.Core.Entities;
using AiMate.Core.Enums;
using AiMate.Core.Services;
using AiMate.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace AiMate.Infrastructure.Services;

/// <summary>
/// Workspace service implementation
/// </summary>
public class WorkspaceService : IWorkspaceService
{
    private readonly AiMateDbContext _context;
    private readonly ILogger<WorkspaceService> _logger;

    public WorkspaceService(
        AiMateDbContext context,
        ILogger<WorkspaceService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<List<Workspace>> GetUserWorkspacesAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        return await _context.Workspaces
            .Where(w => w.UserId == userId)
            .OrderByDescending(w => w.UpdatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<Workspace?> GetWorkspaceByIdAsync(
        Guid workspaceId,
        CancellationToken cancellationToken = default)
    {
        return await _context.Workspaces
            .Include(w => w.Conversations)
            .Include(w => w.Files)
            .FirstOrDefaultAsync(w => w.Id == workspaceId, cancellationToken);
    }

    public async Task<Workspace> CreateWorkspaceAsync(
        Workspace workspace,
        CancellationToken cancellationToken = default)
    {
        _context.Workspaces.Add(workspace);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Created workspace {WorkspaceId} for user {UserId}",
            workspace.Id, workspace.UserId);

        return workspace;
    }

    public async Task<Workspace> UpdateWorkspaceAsync(
        Workspace workspace,
        CancellationToken cancellationToken = default)
    {
        var existing = await _context.Workspaces.FindAsync(
            new object[] { workspace.Id }, cancellationToken);

        if (existing == null)
        {
            throw new InvalidOperationException($"Workspace {workspace.Id} not found");
        }

        existing.Name = workspace.Name;
        existing.Type = workspace.Type;
        existing.DefaultPersonality = workspace.DefaultPersonality;
        existing.Context = workspace.Context;
        existing.EnabledTools = workspace.EnabledTools;
        existing.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Updated workspace {WorkspaceId}", workspace.Id);

        return existing;
    }

    public async Task DeleteWorkspaceAsync(
        Guid workspaceId,
        CancellationToken cancellationToken = default)
    {
        var workspace = await _context.Workspaces.FindAsync(
            new object[] { workspaceId }, cancellationToken);

        if (workspace != null)
        {
            _context.Workspaces.Remove(workspace);
            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Deleted workspace {WorkspaceId}", workspaceId);
        }
    }

    public async Task<Workspace> GetOrCreateDefaultWorkspaceAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        // Check for existing default workspace
        var defaultWorkspace = await _context.Workspaces
            .FirstOrDefaultAsync(w => w.UserId == userId && w.Type.ToString() == "Default", cancellationToken);

        if (defaultWorkspace != null)
        {
            return defaultWorkspace;
        }

        // Create default workspace
        var workspace = new Workspace
        {
            UserId = userId,
            Name = "My Workspace",
            Type = WorkspaceType.General,
            DefaultPersonality = PersonalityMode.KiwiMate,
            EnabledTools = new List<string> { "web_search", "code_interpreter" }
        };

        return await CreateWorkspaceAsync(workspace, cancellationToken);
    }
}
