using AiMate.Shared.Models;
using AiMate.Web.Store.Auth;
using Fluxor;
using System.Net.Http.Json;

namespace AiMate.Web.Store.Notes;

public class NotesEffects
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IState<AuthState> _authState;

    public NotesEffects(
        IHttpClientFactory httpClientFactory,
        IState<AuthState> authState)
    {
        _httpClientFactory = httpClientFactory;
        _authState = authState;
    }

    // ========================================================================
    // LOAD NOTES
    // ========================================================================

    [EffectMethod]
    public async Task HandleLoadNotes(LoadNotesAction action, IDispatcher dispatcher)
    {
        try
        {
            var httpClient = _httpClientFactory.CreateClient("ApiClient");

            // Get current user ID from auth state
            var userId = _authState.Value.CurrentUser?.Id.ToString()
                ?? throw new UnauthorizedAccessException("User must be authenticated to load notes");

            var notes = await httpClient.GetFromJsonAsync<List<NoteDto>>($"/api/v1/notes?userId={userId}");

            if (notes != null)
            {
                dispatcher.Dispatch(new LoadNotesSuccessAction(notes));
            }
            else
            {
                dispatcher.Dispatch(new LoadNotesFailureAction("No notes found"));
            }
        }
        catch (Exception ex)
        {
            dispatcher.Dispatch(new LoadNotesFailureAction(ex.Message));
        }
    }

    // ========================================================================
    // CREATE NOTE
    // ========================================================================

    [EffectMethod]
    public async Task HandleCreateNote(CreateNoteAction action, IDispatcher dispatcher)
    {
        try
        {
            var httpClient = _httpClientFactory.CreateClient("ApiClient");

            var request = new CreateNoteRequest
            {
                Title = action.Note.Title,
                Content = action.Note.Content,
                ContentType = action.Note.ContentType,
                Tags = action.Note.Tags.Count > 0 ? action.Note.Tags : null,
                Collection = action.Note.Collection,
                Category = action.Note.Category,
                Color = action.Note.Color
            };

            var response = await httpClient.PostAsJsonAsync("/api/v1/notes", request);

            if (response.IsSuccessStatusCode)
            {
                var createdNote = await response.Content.ReadFromJsonAsync<NoteDto>();
                if (createdNote != null)
                {
                    dispatcher.Dispatch(new CreateNoteSuccessAction(createdNote));
                }
                else
                {
                    dispatcher.Dispatch(new CreateNoteFailureAction("Failed to create note"));
                }
            }
            else
            {
                var error = await response.Content.ReadAsStringAsync();
                dispatcher.Dispatch(new CreateNoteFailureAction(error));
            }
        }
        catch (Exception ex)
        {
            dispatcher.Dispatch(new CreateNoteFailureAction(ex.Message));
        }
    }

    // ========================================================================
    // UPDATE NOTE
    // ========================================================================

    [EffectMethod]
    public async Task HandleUpdateNote(UpdateNoteAction action, IDispatcher dispatcher)
    {
        try
        {
            var httpClient = _httpClientFactory.CreateClient("ApiClient");

            var request = new UpdateNoteRequest
            {
                Title = action.UpdatedNote.Title,
                Content = action.UpdatedNote.Content,
                ContentType = action.UpdatedNote.ContentType,
                Tags = action.UpdatedNote.Tags.Count > 0 ? action.UpdatedNote.Tags : null,
                Collection = action.UpdatedNote.Collection,
                Category = action.UpdatedNote.Category,
                Color = action.UpdatedNote.Color,
                IsPinned = action.UpdatedNote.IsPinned,
                IsFavorite = action.UpdatedNote.IsFavorite,
                IsArchived = action.UpdatedNote.IsArchived
            };

            var response = await httpClient.PutAsJsonAsync($"/api/v1/notes/{action.NoteId}", request);

            if (response.IsSuccessStatusCode)
            {
                var updatedNote = await response.Content.ReadFromJsonAsync<NoteDto>();
                if (updatedNote != null)
                {
                    dispatcher.Dispatch(new UpdateNoteSuccessAction(updatedNote));
                }
                else
                {
                    dispatcher.Dispatch(new UpdateNoteFailureAction("Failed to update note"));
                }
            }
            else
            {
                var error = await response.Content.ReadAsStringAsync();
                dispatcher.Dispatch(new UpdateNoteFailureAction(error));
            }
        }
        catch (Exception ex)
        {
            dispatcher.Dispatch(new UpdateNoteFailureAction(ex.Message));
        }
    }

    // ========================================================================
    // DELETE NOTE
    // ========================================================================

    [EffectMethod]
    public async Task HandleDeleteNote(DeleteNoteAction action, IDispatcher dispatcher)
    {
        try
        {
            var httpClient = _httpClientFactory.CreateClient("ApiClient");

            var response = await httpClient.DeleteAsync($"/api/v1/notes/{action.NoteId}");

            if (response.IsSuccessStatusCode)
            {
                dispatcher.Dispatch(new DeleteNoteSuccessAction(action.NoteId));
            }
            else
            {
                var error = await response.Content.ReadAsStringAsync();
                dispatcher.Dispatch(new DeleteNoteFailureAction(error));
            }
        }
        catch (Exception ex)
        {
            dispatcher.Dispatch(new DeleteNoteFailureAction(ex.Message));
        }
    }
}
