using Fluxor;

namespace AiMate.Web.Store.Project;

public static class ProjectReducers
{
    [ReducerMethod]
    public static ProjectState ReduceLoadProjectsAction(ProjectState state, LoadProjectsAction action)
        => state with { IsLoading = true, Error = null };

    [ReducerMethod]
    public static ProjectState ReduceLoadProjectsSuccessAction(ProjectState state, LoadProjectsSuccessAction action)
        => state with { IsLoading = false, Projects = action.Projects };

    [ReducerMethod]
    public static ProjectState ReduceLoadProjectsFailureAction(ProjectState state, LoadProjectsFailureAction action)
        => state with { IsLoading = false, Error = action.Error };

    [ReducerMethod]
    public static ProjectState ReduceCreateProjectAction(ProjectState state, CreateProjectAction action)
        => state with { IsSaving = true, Error = null };

    [ReducerMethod]
    public static ProjectState ReduceCreateProjectSuccessAction(ProjectState state, CreateProjectSuccessAction action)
    {
        var projects = new Dictionary<Guid, Core.Entities.Project>(state.Projects)
        {
            [action.Project.Id] = action.Project
        };
        return state with { IsSaving = false, Projects = projects, ShowProjectEditor = false };
    }

    [ReducerMethod]
    public static ProjectState ReduceCreateProjectFailureAction(ProjectState state, CreateProjectFailureAction action)
        => state with { IsSaving = false, Error = action.Error };

    [ReducerMethod]
    public static ProjectState ReduceUpdateProjectAction(ProjectState state, UpdateProjectAction action)
        => state with { IsSaving = true, Error = null };

    [ReducerMethod]
    public static ProjectState ReduceUpdateProjectSuccessAction(ProjectState state, UpdateProjectSuccessAction action)
    {
        var projects = new Dictionary<Guid, Core.Entities.Project>(state.Projects)
        {
            [action.Project.Id] = action.Project
        };
        return state with { IsSaving = false, Projects = projects, ShowProjectEditor = false };
    }

    [ReducerMethod]
    public static ProjectState ReduceUpdateProjectFailureAction(ProjectState state, UpdateProjectFailureAction action)
        => state with { IsSaving = false, Error = action.Error };

    [ReducerMethod]
    public static ProjectState ReduceDeleteProjectAction(ProjectState state, DeleteProjectAction action)
        => state with { IsLoading = true, Error = null };

    [ReducerMethod]
    public static ProjectState ReduceDeleteProjectSuccessAction(ProjectState state, DeleteProjectSuccessAction action)
    {
        var projects = new Dictionary<Guid, Core.Entities.Project>(state.Projects);
        projects.Remove(action.ProjectId);
        return state with { IsLoading = false, Projects = projects };
    }

    [ReducerMethod]
    public static ProjectState ReduceDeleteProjectFailureAction(ProjectState state, DeleteProjectFailureAction action)
        => state with { IsLoading = false, Error = action.Error };

    [ReducerMethod]
    public static ProjectState ReduceArchiveProjectSuccessAction(ProjectState state, ArchiveProjectSuccessAction action)
    {
        if (state.Projects.TryGetValue(action.ProjectId, out var project))
        {
            // Update property manually (Project is a class, not a record)
            project.IsArchived = true;
            var projects = new Dictionary<Guid, Core.Entities.Project>(state.Projects);
            return state with { Projects = projects };
        }
        return state;
    }

    [ReducerMethod]
    public static ProjectState ReduceOpenProjectEditorAction(ProjectState state, OpenProjectEditorAction action)
        => state with { ShowProjectEditor = true, EditingProjectId = action.ProjectId };

    [ReducerMethod]
    public static ProjectState ReduceCloseProjectEditorAction(ProjectState state, CloseProjectEditorAction action)
        => state with { ShowProjectEditor = false, EditingProjectId = null };

    [ReducerMethod]
    public static ProjectState ReduceClearProjectErrorAction(ProjectState state, ClearProjectErrorAction action)
        => state with { Error = null };

    [ReducerMethod]
    public static ProjectState ReduceSetProjectPageAction(ProjectState state, SetProjectPageAction action)
        => state with { CurrentPage = action.Page };

    [ReducerMethod]
    public static ProjectState ReduceSetProjectPageSizeAction(ProjectState state, SetProjectPageSizeAction action)
        => state with { PageSize = action.PageSize, CurrentPage = 1 };

    [ReducerMethod]
    public static ProjectState ReduceSortProjectsAction(ProjectState state, SortProjectsAction action)
    {
        var sortAscending = state.SortColumn == action.Column ? !state.SortAscending : true;
        return state with { SortColumn = action.Column, SortAscending = sortAscending };
    }
}
