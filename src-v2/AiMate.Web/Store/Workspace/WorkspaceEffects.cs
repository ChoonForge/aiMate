using AiMate.Core.Services;
using Fluxor;
using Microsoft.Extensions.Logging;

namespace AiMate.Web.Store.Workspace;

public class WorkspaceEffects
{
    private readonly IWorkspaceService _workspaceService;
    private readonly ILogger<WorkspaceEffects> _logger;

    public WorkspaceEffects(
        IWorkspaceService workspaceService,
        ILogger<WorkspaceEffects> logger)
    {
        _workspaceService = workspaceService;
        _logger = logger;
    }

    [EffectMethod]
    public async Task HandleLoadWorkspaces(LoadWorkspacesAction action, IDispatcher dispatcher)
    {
        try
        {
            // TODO: Get actual user ID from auth context
            // For now, use a hardcoded demo user ID
            var userId = Guid.Parse("00000000-0000-0000-0000-000000000001");

            var workspaces = await _workspaceService.GetUserWorkspacesAsync(userId);

            // If no workspaces exist, create a default one
            if (!workspaces.Any())
            {
                _logger.LogInformation("No workspaces found for user {UserId}, creating default", userId);
                var defaultWorkspace = await _workspaceService.GetOrCreateDefaultWorkspaceAsync(userId);
                workspaces = new List<Core.Entities.Workspace> { defaultWorkspace };
            }

            dispatcher.Dispatch(new LoadWorkspacesSuccessAction(workspaces));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load workspaces");
            dispatcher.Dispatch(new LoadWorkspacesFailureAction(ex.Message));
        }
    }

    [EffectMethod]
    public async Task HandleCreateWorkspace(CreateWorkspaceAction action, IDispatcher dispatcher)
    {
        try
        {
            // TODO: Get actual user ID from auth context
            var userId = Guid.Parse("00000000-0000-0000-0000-000000000001");

            var workspace = new Core.Entities.Workspace
            {
                UserId = userId,
                Name = action.Name,
                Type = action.Type,
                DefaultPersonality = action.DefaultPersonality,
                Context = action.Context,
                EnabledTools = new List<string>() // Will be set from UI later
            };

            var created = await _workspaceService.CreateWorkspaceAsync(workspace);

            dispatcher.Dispatch(new CreateWorkspaceSuccessAction(created));

            _logger.LogInformation("Created workspace {WorkspaceId}: {Name}", created.Id, created.Name);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create workspace");
            dispatcher.Dispatch(new SetWorkspaceErrorAction($"Failed to create workspace: {ex.Message}"));
        }
    }

    [EffectMethod]
    public async Task HandleUpdateWorkspace(UpdateWorkspaceAction action, IDispatcher dispatcher)
    {
        try
        {
            var workspace = await _workspaceService.GetWorkspaceByIdAsync(action.WorkspaceId);

            if (workspace == null)
            {
                dispatcher.Dispatch(new SetWorkspaceErrorAction("Workspace not found"));
                return;
            }

            workspace.Name = action.Name;
            workspace.Type = action.Type;
            workspace.DefaultPersonality = action.DefaultPersonality;
            workspace.Context = action.Context;
            workspace.EnabledTools = action.EnabledTools ?? new List<string>();

            var updated = await _workspaceService.UpdateWorkspaceAsync(workspace);

            dispatcher.Dispatch(new UpdateWorkspaceSuccessAction(updated));

            _logger.LogInformation("Updated workspace {WorkspaceId}: {Name}", updated.Id, updated.Name);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update workspace {WorkspaceId}", action.WorkspaceId);
            dispatcher.Dispatch(new SetWorkspaceErrorAction($"Failed to update workspace: {ex.Message}"));
        }
    }

    [EffectMethod]
    public async Task HandleDeleteWorkspace(DeleteWorkspaceAction action, IDispatcher dispatcher)
    {
        try
        {
            await _workspaceService.DeleteWorkspaceAsync(action.WorkspaceId);

            dispatcher.Dispatch(new DeleteWorkspaceSuccessAction(action.WorkspaceId));

            _logger.LogInformation("Deleted workspace {WorkspaceId}", action.WorkspaceId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete workspace {WorkspaceId}", action.WorkspaceId);
            dispatcher.Dispatch(new SetWorkspaceErrorAction($"Failed to delete workspace: {ex.Message}"));
        }
    }
}
