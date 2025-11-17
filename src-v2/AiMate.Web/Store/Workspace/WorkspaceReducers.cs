using Fluxor;

namespace AiMate.Web.Store.Workspace;

public static class WorkspaceReducers
{
    // Load workspaces
    [ReducerMethod]
    public static WorkspaceState OnLoadWorkspaces(WorkspaceState state, LoadWorkspacesAction action)
    {
        return state with { IsLoading = true, Error = null };
    }

    [ReducerMethod]
    public static WorkspaceState OnLoadWorkspacesSuccess(WorkspaceState state, LoadWorkspacesSuccessAction action)
    {
        var workspaces = action.Workspaces.ToDictionary(w => w.Id);
        var activeWorkspaceId = state.ActiveWorkspaceId ?? action.Workspaces.FirstOrDefault()?.Id;

        return state with
        {
            Workspaces = workspaces,
            ActiveWorkspaceId = activeWorkspaceId,
            IsLoading = false,
            Error = null
        };
    }

    [ReducerMethod]
    public static WorkspaceState OnLoadWorkspacesFailure(WorkspaceState state, LoadWorkspacesFailureAction action)
    {
        return state with { IsLoading = false, Error = action.Error };
    }

    // Create workspace
    [ReducerMethod]
    public static WorkspaceState OnCreateWorkspace(WorkspaceState state, CreateWorkspaceAction action)
    {
        return state with { IsSaving = true, Error = null };
    }

    [ReducerMethod]
    public static WorkspaceState OnCreateWorkspaceSuccess(WorkspaceState state, CreateWorkspaceSuccessAction action)
    {
        var newWorkspaces = new Dictionary<Guid, Core.Entities.Workspace>(state.Workspaces)
        {
            [action.Workspace.Id] = action.Workspace
        };

        return state with
        {
            Workspaces = newWorkspaces,
            ActiveWorkspaceId = action.Workspace.Id,
            IsSaving = false,
            ShowWorkspaceEditor = false,
            EditingWorkspaceId = null,
            Error = null
        };
    }

    // Update workspace
    [ReducerMethod]
    public static WorkspaceState OnUpdateWorkspace(WorkspaceState state, UpdateWorkspaceAction action)
    {
        return state with { IsSaving = true, Error = null };
    }

    [ReducerMethod]
    public static WorkspaceState OnUpdateWorkspaceSuccess(WorkspaceState state, UpdateWorkspaceSuccessAction action)
    {
        var newWorkspaces = new Dictionary<Guid, Core.Entities.Workspace>(state.Workspaces)
        {
            [action.Workspace.Id] = action.Workspace
        };

        return state with
        {
            Workspaces = newWorkspaces,
            IsSaving = false,
            ShowWorkspaceEditor = false,
            EditingWorkspaceId = null,
            Error = null
        };
    }

    // Delete workspace
    [ReducerMethod]
    public static WorkspaceState OnDeleteWorkspace(WorkspaceState state, DeleteWorkspaceAction action)
    {
        return state with { IsLoading = true, Error = null };
    }

    [ReducerMethod]
    public static WorkspaceState OnDeleteWorkspaceSuccess(WorkspaceState state, DeleteWorkspaceSuccessAction action)
    {
        var newWorkspaces = new Dictionary<Guid, Core.Entities.Workspace>(state.Workspaces);
        newWorkspaces.Remove(action.WorkspaceId);

        // If we deleted the active workspace, switch to first available
        var newActiveWorkspaceId = state.ActiveWorkspaceId == action.WorkspaceId
            ? newWorkspaces.Keys.FirstOrDefault()
            : state.ActiveWorkspaceId;

        return state with
        {
            Workspaces = newWorkspaces,
            ActiveWorkspaceId = newActiveWorkspaceId,
            IsLoading = false,
            Error = null
        };
    }

    // Switch workspace
    [ReducerMethod]
    public static WorkspaceState OnSwitchWorkspace(WorkspaceState state, SwitchWorkspaceAction action)
    {
        return state with { ActiveWorkspaceId = action.WorkspaceId };
    }

    // UI actions
    [ReducerMethod]
    public static WorkspaceState OnOpenWorkspaceEditor(WorkspaceState state, OpenWorkspaceEditorAction action)
    {
        return state with
        {
            ShowWorkspaceEditor = true,
            EditingWorkspaceId = action.WorkspaceId,
            Error = null
        };
    }

    [ReducerMethod]
    public static WorkspaceState OnCloseWorkspaceEditor(WorkspaceState state, CloseWorkspaceEditorAction action)
    {
        return state with
        {
            ShowWorkspaceEditor = false,
            EditingWorkspaceId = null,
            Error = null
        };
    }

    // Error handling
    [ReducerMethod]
    public static WorkspaceState OnSetWorkspaceError(WorkspaceState state, SetWorkspaceErrorAction action)
    {
        return state with { Error = action.Error, IsLoading = false, IsSaving = false };
    }

    [ReducerMethod]
    public static WorkspaceState OnClearWorkspaceError(WorkspaceState state, ClearWorkspaceErrorAction action)
    {
        return state with { Error = null };
    }
}
