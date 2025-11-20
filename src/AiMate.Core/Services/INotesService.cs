using AiMate.Core.Entities;

namespace AiMate.Core.Services;

/// <summary>
/// Notes service - CRUD operations for personal notes
/// </summary>
public interface INotesService
{
    /// <summary>
    /// Get all notes for a user
    /// </summary>
    Task<List<Note>> GetUserNotesAsync(
        Guid userId,
        bool includeArchived = false,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get note by ID
    /// </summary>
    Task<Note?> GetNoteByIdAsync(
        Guid noteId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Create new note
    /// </summary>
    Task<Note> CreateNoteAsync(
        Note note,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Update existing note
    /// </summary>
    Task<Note> UpdateNoteAsync(
        Note note,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Delete note
    /// </summary>
    Task DeleteNoteAsync(
        Guid noteId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Search notes by title or content
    /// </summary>
    Task<List<Note>> SearchNotesAsync(
        Guid userId,
        string searchTerm,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get notes by collection
    /// </summary>
    Task<List<Note>> GetNotesByCollectionAsync(
        Guid userId,
        string collection,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get notes by tag
    /// </summary>
    Task<List<Note>> GetNotesByTagAsync(
        Guid userId,
        string tag,
        CancellationToken cancellationToken = default);
}
