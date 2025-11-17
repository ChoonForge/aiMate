using AiMate.Core.Entities;
using Fluxor;

namespace AiMate.Web.Store.Workspace;

/// <summary>
/// Workspace state - all workspace-related state
/// </summary>
[FeatureState]
public record WorkspaceState
{
    public Guid? ActiveWorkspaceId { get; init; }
    public Dictionary<Guid, Core.Entities.Workspace> Workspaces { get; init; } = new();
    public bool IsLoading { get; init; }
    public bool IsSaving { get; init; }
    public string? Error { get; init; }
    public bool ShowWorkspaceEditor { get; init; }
    public Guid? EditingWorkspaceId { get; init; }
}
