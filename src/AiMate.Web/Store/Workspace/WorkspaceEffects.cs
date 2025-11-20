using AiMate.Core.Services;
using AiMate.Web.Store.Auth;
using Fluxor;
using Microsoft.Extensions.Logging;

namespace AiMate.Web.Store.Workspace;

public class WorkspaceEffects
{
    private readonly IWorkspaceService _workspaceService;
    private readonly IState<AuthState> _authState;
    private readonly ILogger<WorkspaceEffects> _logger;

    public WorkspaceEffects(
        IWorkspaceService workspaceService,
        IState<AuthState> authState,
        ILogger<WorkspaceEffects> logger)
    {
        _workspaceService = workspaceService;
        _authState = authState;
        _logger = logger;
    }

    [EffectMethod]
    public async Task HandleLoadWorkspaces(LoadWorkspacesAction action, IDispatcher dispatcher)
    {
        try
        {
            // Get current user ID from auth state
            var userId = _authState.Value.CurrentUser?.Id
                ?? throw new UnauthorizedAccessException("User must be authenticated to load workspaces");

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
            // Get current user ID from auth state
            var userId = _authState.Value.CurrentUser?.Id
                ?? throw new UnauthorizedAccessException("User must be authenticated to create a workspace");

            // Parse enum types from string
            var workspaceType = Enum.TryParse<Core.Enums.WorkspaceType>(action.Type, out var parsedType)
                ? parsedType
                : Core.Enums.WorkspaceType.General;

            var personality = Enum.TryParse<Core.Enums.PersonalityMode>(action.DefaultPersonality, out var parsedPersonality)
                ? parsedPersonality
                : Core.Enums.PersonalityMode.KiwiMate;

            var workspace = new Core.Entities.Workspace
            {
                UserId = userId,
                Name = action.Name,
                Type = workspaceType,
                DefaultPersonality = personality,
                Context = string.IsNullOrEmpty(action.Context) ? new Dictionary<string, string>() : new Dictionary<string, string> { { "description", action.Context } },
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

            // Parse enum types from string
            if (Enum.TryParse<Core.Enums.WorkspaceType>(action.Type, out var parsedType))
            {
                workspace.Type = parsedType;
            }

            if (!string.IsNullOrEmpty(action.DefaultPersonality) &&
                Enum.TryParse<Core.Enums.PersonalityMode>(action.DefaultPersonality, out var parsedPersonality))
            {
                workspace.DefaultPersonality = parsedPersonality;
            }

            // Update context
            if (!string.IsNullOrEmpty(action.Context))
            {
                workspace.Context = new Dictionary<string, string> { { "description", action.Context } };
            }

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
