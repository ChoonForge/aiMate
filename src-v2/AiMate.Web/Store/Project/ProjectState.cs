using AiMate.Core.Entities;
using Fluxor;

namespace AiMate.Web.Store.Project;

/// <summary>
/// Project state - all project-related state
/// </summary>
[FeatureState]
public record ProjectState
{
    public Dictionary<Guid, Core.Entities.Project> Projects { get; init; } = new();
    public bool IsLoading { get; init; }
    public bool IsSaving { get; init; }
    public string? Error { get; init; }
    public bool ShowProjectEditor { get; init; }
    public Guid? EditingProjectId { get; init; }

    // Table state
    public int CurrentPage { get; init; } = 1;
    public int PageSize { get; init; } = 10;
    public string SortColumn { get; init; } = "UpdatedAt";
    public bool SortAscending { get; init; } = false;
}
