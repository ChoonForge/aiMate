using Fluxor;

namespace AiMate.Web.Store.Notes;

public static class NotesReducers
{
    // ========================================================================
    // LOAD NOTES
    // ========================================================================

    [ReducerMethod]
    public static NotesState LoadNotes(NotesState state, LoadNotesAction action)
        => state with { IsLoading = true, Error = null };

    [ReducerMethod]
    public static NotesState LoadNotesSuccess(NotesState state, LoadNotesSuccessAction action)
    {
        // Extract unique collections and tags
        var collections = action.Notes
            .Where(n => !string.IsNullOrEmpty(n.Collection))
            .Select(n => n.Collection!)
            .Distinct()
            .OrderBy(c => c)
            .ToList();

        var allTags = action.Notes
            .SelectMany(n => n.Tags)
            .Distinct()
            .OrderBy(t => t)
            .ToList();

        return state with
        {
            Notes = action.Notes,
            Collections = collections,
            AllTags = allTags,
            IsLoading = false
        };
    }

    [ReducerMethod]
    public static NotesState LoadNotesFailure(NotesState state, LoadNotesFailureAction action)
        => state with { IsLoading = false, Error = action.Error };

    // ========================================================================
    // CREATE NOTE
    // ========================================================================

    [ReducerMethod]
    public static NotesState CreateNote(NotesState state, CreateNoteAction action)
        => state with { IsSaving = true, Error = null };

    [ReducerMethod]
    public static NotesState CreateNoteSuccess(NotesState state, CreateNoteSuccessAction action)
    {
        var notes = state.Notes.ToList();
        notes.Insert(0, action.Note); // Add to top

        // Update collections if new collection added
        var collections = state.Collections.ToList();
        if (!string.IsNullOrEmpty(action.Note.Collection) && !collections.Contains(action.Note.Collection))
        {
            collections.Add(action.Note.Collection);
            collections = collections.OrderBy(c => c).ToList();
        }

        // Update tags
        var allTags = state.AllTags.ToList();
        foreach (var tag in action.Note.Tags.Where(t => !allTags.Contains(t)))
        {
            allTags.Add(tag);
        }
        allTags = allTags.OrderBy(t => t).ToList();

        return state with
        {
            Notes = notes,
            Collections = collections,
            AllTags = allTags,
            IsSaving = false,
            IsEditDialogOpen = false
        };
    }

    [ReducerMethod]
    public static NotesState CreateNoteFailure(NotesState state, CreateNoteFailureAction action)
        => state with { IsSaving = false, Error = action.Error };

    // ========================================================================
    // UPDATE NOTE
    // ========================================================================

    [ReducerMethod]
    public static NotesState UpdateNote(NotesState state, UpdateNoteAction action)
        => state with { IsSaving = true, Error = null };

    [ReducerMethod]
    public static NotesState UpdateNoteSuccess(NotesState state, UpdateNoteSuccessAction action)
    {
        var notes = state.Notes.ToList();
        var index = notes.FindIndex(n => n.Id == action.UpdatedNote.Id);

        if (index >= 0)
        {
            notes[index] = action.UpdatedNote;
        }

        // Recalculate collections and tags
        var collections = notes
            .Where(n => !string.IsNullOrEmpty(n.Collection))
            .Select(n => n.Collection!)
            .Distinct()
            .OrderBy(c => c)
            .ToList();

        var allTags = notes
            .SelectMany(n => n.Tags)
            .Distinct()
            .OrderBy(t => t)
            .ToList();

        return state with
        {
            Notes = notes,
            Collections = collections,
            AllTags = allTags,
            IsSaving = false,
            IsEditDialogOpen = false
        };
    }

    [ReducerMethod]
    public static NotesState UpdateNoteFailure(NotesState state, UpdateNoteFailureAction action)
        => state with { IsSaving = false, Error = action.Error };

    // ========================================================================
    // DELETE NOTE
    // ========================================================================

    [ReducerMethod]
    public static NotesState DeleteNote(NotesState state, DeleteNoteAction action)
        => state with { IsLoading = true, Error = null };

    [ReducerMethod]
    public static NotesState DeleteNoteSuccess(NotesState state, DeleteNoteSuccessAction action)
    {
        var notes = state.Notes.Where(n => n.Id != action.NoteId).ToList();

        // Recalculate collections and tags
        var collections = notes
            .Where(n => !string.IsNullOrEmpty(n.Collection))
            .Select(n => n.Collection!)
            .Distinct()
            .OrderBy(c => c)
            .ToList();

        var allTags = notes
            .SelectMany(n => n.Tags)
            .Distinct()
            .OrderBy(t => t)
            .ToList();

        return state with
        {
            Notes = notes,
            Collections = collections,
            AllTags = allTags,
            IsLoading = false,
            SelectedNote = state.SelectedNote?.Id == action.NoteId ? null : state.SelectedNote
        };
    }

    [ReducerMethod]
    public static NotesState DeleteNoteFailure(NotesState state, DeleteNoteFailureAction action)
        => state with { IsLoading = false, Error = action.Error };

    // ========================================================================
    // TOGGLE ACTIONS
    // ========================================================================

    [ReducerMethod]
    public static NotesState ToggleNotePin(NotesState state, ToggleNotePinAction action)
    {
        var notes = state.Notes.ToList();
        var note = notes.FirstOrDefault(n => n.Id == action.NoteId);

        if (note != null)
        {
            var index = notes.IndexOf(note);
            notes[index] = note with { IsPinned = !note.IsPinned, UpdatedAt = DateTime.UtcNow };
        }

        return state with { Notes = notes };
    }

    [ReducerMethod]
    public static NotesState ToggleNoteFavorite(NotesState state, ToggleNoteFavoriteAction action)
    {
        var notes = state.Notes.ToList();
        var note = notes.FirstOrDefault(n => n.Id == action.NoteId);

        if (note != null)
        {
            var index = notes.IndexOf(note);
            notes[index] = note with { IsFavorite = !note.IsFavorite, UpdatedAt = DateTime.UtcNow };
        }

        return state with { Notes = notes };
    }

    [ReducerMethod]
    public static NotesState ArchiveNote(NotesState state, ArchiveNoteAction action)
    {
        var notes = state.Notes.ToList();
        var note = notes.FirstOrDefault(n => n.Id == action.NoteId);

        if (note != null)
        {
            var index = notes.IndexOf(note);
            notes[index] = note with { IsArchived = action.Archive, UpdatedAt = DateTime.UtcNow };
        }

        return state with { Notes = notes };
    }

    // ========================================================================
    // FILTER & SEARCH
    // ========================================================================

    [ReducerMethod]
    public static NotesState SetSearchQuery(NotesState state, SetSearchQueryAction action)
        => state with { SearchQuery = action.Query };

    [ReducerMethod]
    public static NotesState SetCollectionFilter(NotesState state, SetCollectionFilterAction action)
        => state with { SelectedCollection = action.Collection };

    [ReducerMethod]
    public static NotesState SetTagsFilter(NotesState state, SetTagsFilterAction action)
        => state with { SelectedTags = action.Tags };

    [ReducerMethod]
    public static NotesState ToggleShowArchived(NotesState state, ToggleShowArchivedAction action)
        => state with { ShowArchivedOnly = !state.ShowArchivedOnly };

    [ReducerMethod]
    public static NotesState ToggleShowPinned(NotesState state, ToggleShowPinnedAction action)
        => state with { ShowPinnedOnly = !state.ShowPinnedOnly };

    [ReducerMethod]
    public static NotesState ToggleShowFavorites(NotesState state, ToggleShowFavoritesAction action)
        => state with { ShowFavoritesOnly = !state.ShowFavoritesOnly };

    [ReducerMethod]
    public static NotesState ClearFilters(NotesState state, ClearFiltersAction action)
        => state with
        {
            SearchQuery = string.Empty,
            SelectedCollection = null,
            SelectedTags = new(),
            ShowArchivedOnly = false,
            ShowPinnedOnly = false,
            ShowFavoritesOnly = false
        };

    // ========================================================================
    // SORTING
    // ========================================================================

    [ReducerMethod]
    public static NotesState SetSortBy(NotesState state, SetSortByAction action)
        => state with { SortBy = action.SortBy };

    [ReducerMethod]
    public static NotesState ToggleSortDirection(NotesState state, ToggleSortDirectionAction action)
        => state with { SortDescending = !state.SortDescending };

    // ========================================================================
    // UI ACTIONS
    // ========================================================================

    [ReducerMethod]
    public static NotesState SelectNote(NotesState state, SelectNoteAction action)
        => state with { SelectedNote = action.Note };

    [ReducerMethod]
    public static NotesState OpenEditDialog(NotesState state, OpenEditDialogAction action)
        => state with { IsEditDialogOpen = true, SelectedNote = action.Note };

    [ReducerMethod]
    public static NotesState CloseEditDialog(NotesState state, CloseEditDialogAction action)
        => state with { IsEditDialogOpen = false, SelectedNote = null };

    [ReducerMethod]
    public static NotesState ClearNoteError(NotesState state, ClearNoteErrorAction action)
        => state with { Error = null };
}
