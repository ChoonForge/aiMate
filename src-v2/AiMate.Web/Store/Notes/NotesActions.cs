using AiMate.Shared.Models;

namespace AiMate.Web.Store.Notes;

// ============================================================================
// LOAD NOTES
// ============================================================================

public record LoadNotesAction();
public record LoadNotesSuccessAction(List<NoteDto> Notes);
public record LoadNotesFailureAction(string Error);

// ============================================================================
// CREATE NOTE
// ============================================================================

public record CreateNoteAction(NoteDto Note);
public record CreateNoteSuccessAction(NoteDto Note);
public record CreateNoteFailureAction(string Error);

// ============================================================================
// UPDATE NOTE
// ============================================================================

public record UpdateNoteAction(string NoteId, NoteDto UpdatedNote);
public record UpdateNoteSuccessAction(NoteDto UpdatedNote);
public record UpdateNoteFailureAction(string Error);

// ============================================================================
// DELETE NOTE
// ============================================================================

public record DeleteNoteAction(string NoteId);
public record DeleteNoteSuccessAction(string NoteId);
public record DeleteNoteFailureAction(string Error);

// ============================================================================
// TOGGLE ACTIONS
// ============================================================================

public record ToggleNotePinAction(string NoteId);
public record ToggleNoteFavoriteAction(string NoteId);
public record ArchiveNoteAction(string NoteId, bool Archive);

// ============================================================================
// FILTER & SEARCH ACTIONS
// ============================================================================

public record SetSearchQueryAction(string Query);
public record SetCollectionFilterAction(string? Collection);
public record SetTagsFilterAction(List<string> Tags);
public record ToggleShowArchivedAction();
public record ToggleShowPinnedAction();
public record ToggleShowFavoritesAction();
public record ClearFiltersAction();

// ============================================================================
// SORTING ACTIONS
// ============================================================================

public record SetSortByAction(string SortBy);
public record ToggleSortDirectionAction();

// ============================================================================
// UI ACTIONS
// ============================================================================

public record SelectNoteAction(NoteDto? Note);
public record OpenEditDialogAction(NoteDto? Note = null);
public record CloseEditDialogAction();
public record ClearNoteErrorAction();
