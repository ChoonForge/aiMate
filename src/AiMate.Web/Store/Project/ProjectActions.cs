namespace AiMate.Web.Store.Project;

// Load actions
public record LoadProjectsAction;
public record LoadProjectsSuccessAction(Dictionary<Guid, Core.Entities.Project> Projects);
public record LoadProjectsFailureAction(string Error);

// Create actions
public record CreateProjectAction(Core.Entities.Project Project);
public record CreateProjectSuccessAction(Core.Entities.Project Project);
public record CreateProjectFailureAction(string Error);

// Update actions
public record UpdateProjectAction(Core.Entities.Project Project);
public record UpdateProjectSuccessAction(Core.Entities.Project Project);
public record UpdateProjectFailureAction(string Error);

// Delete actions
public record DeleteProjectAction(Guid ProjectId);
public record DeleteProjectSuccessAction(Guid ProjectId);
public record DeleteProjectFailureAction(string Error);

// Archive actions
public record ArchiveProjectAction(Guid ProjectId);
public record ArchiveProjectSuccessAction(Guid ProjectId);

// UI actions
public record OpenProjectEditorAction(Guid? ProjectId = null);
public record CloseProjectEditorAction;
public record ClearProjectErrorAction;

// Table actions
public record SetProjectPageAction(int Page);
public record SetProjectPageSizeAction(int PageSize);
public record SortProjectsAction(string Column);
