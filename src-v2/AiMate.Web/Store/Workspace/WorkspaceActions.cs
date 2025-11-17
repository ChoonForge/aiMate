namespace AiMate.Web.Store.Workspace;

// Load workspaces
public record LoadWorkspacesAction;
public record LoadWorkspacesSuccessAction(List<Core.Entities.Workspace> Workspaces);
public record LoadWorkspacesFailureAction(string Error);

// Create workspace
public record CreateWorkspaceAction(
    string Name,
    string Type,
    string? DefaultPersonality = null,
    string? Context = null);
public record CreateWorkspaceSuccessAction(Core.Entities.Workspace Workspace);

// Update workspace
public record UpdateWorkspaceAction(
    Guid WorkspaceId,
    string Name,
    string Type,
    string? DefaultPersonality,
    string? Context,
    List<string>? EnabledTools);
public record UpdateWorkspaceSuccessAction(Core.Entities.Workspace Workspace);

// Delete workspace
public record DeleteWorkspaceAction(Guid WorkspaceId);
public record DeleteWorkspaceSuccessAction(Guid WorkspaceId);

// Switch workspace
public record SwitchWorkspaceAction(Guid WorkspaceId);

// UI actions
public record OpenWorkspaceEditorAction(Guid? WorkspaceId = null); // null = create new
public record CloseWorkspaceEditorAction;

// Error handling
public record SetWorkspaceErrorAction(string Error);
public record ClearWorkspaceErrorAction;
