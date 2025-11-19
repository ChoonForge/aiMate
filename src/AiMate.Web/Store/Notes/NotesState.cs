using AiMate.Shared.Models;
using Fluxor;

namespace AiMate.Web.Store.Notes;

/// <summary>
/// Notes state - personal note-taking and knowledge management
/// </summary>
[FeatureState]
public record NotesState
{
    public List<NoteDto> Notes { get; init; } = new();
    public List<string> Collections { get; init; } = new();
    public List<string> AllTags { get; init; } = new();

    // Filters
    public string SearchQuery { get; init; } = string.Empty;
    public string? SelectedCollection { get; init; }
    public List<string> SelectedTags { get; init; } = new();
    public bool ShowArchivedOnly { get; init; } = false;
    public bool ShowPinnedOnly { get; init; } = false;
    public bool ShowFavoritesOnly { get; init; } = false;

    // Sorting
    public string SortBy { get; init; } = "UpdatedAt"; // UpdatedAt, CreatedAt, Title
    public bool SortDescending { get; init; } = true;

    // UI State
    public bool IsLoading { get; init; }
    public bool IsSaving { get; init; }
    public string? Error { get; init; }
    public NoteDto? SelectedNote { get; init; }
    public bool IsEditDialogOpen { get; init; }
}
