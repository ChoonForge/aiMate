using AiMate.Core.Entities;
using AiMate.Core.Services;
using AiMate.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace AiMate.Infrastructure.Services;

/// <summary>
/// Notes service implementation
/// </summary>
public class NotesService : INotesService
{
    private readonly AiMateDbContext _context;
    private readonly ILogger<NotesService> _logger;

    public NotesService(
        AiMateDbContext context,
        ILogger<NotesService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<List<Note>> GetUserNotesAsync(
        Guid userId,
        bool includeArchived = false,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var query = _context.Notes.Where(n => n.UserId == userId);

            if (!includeArchived)
            {
                query = query.Where(n => !n.IsArchived);
            }

            return await query
                .OrderByDescending(n => n.IsPinned)
                .ThenByDescending(n => n.UpdatedAt)
                .ToListAsync(cancellationToken);
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning("Get user notes operation was cancelled for user {UserId}", userId);
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving notes for user {UserId}", userId);
            throw;
        }
    }

    public async Task<Note?> GetNoteByIdAsync(
        Guid noteId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            return await _context.Notes
                .FirstOrDefaultAsync(n => n.Id == noteId, cancellationToken);
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning("Get note by ID operation was cancelled for note {NoteId}", noteId);
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving note {NoteId}", noteId);
            throw;
        }
    }

    public async Task<Note> CreateNoteAsync(
        Note note,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _context.Notes.Add(note);
            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Created note {NoteId} for user {UserId}",
                note.Id, note.UserId);

            return note;
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "Database error creating note {NoteId}", note.Id);
            throw;
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning("Create note operation was cancelled for note {NoteId}", note.Id);
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating note {NoteId}", note.Id);
            throw;
        }
    }

    public async Task<Note> UpdateNoteAsync(
        Note note,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var existing = await _context.Notes.FindAsync(
                new object[] { note.Id }, cancellationToken);

            if (existing == null)
            {
                throw new InvalidOperationException($"Note {note.Id} not found");
            }

            existing.Title = note.Title;
            existing.Content = note.Content;
            existing.ContentType = note.ContentType;
            existing.Tags = note.Tags;
            existing.Collection = note.Collection;
            existing.Category = note.Category;
            existing.Color = note.Color;
            existing.IsPinned = note.IsPinned;
            existing.IsFavorite = note.IsFavorite;
            existing.IsArchived = note.IsArchived;
            existing.Visibility = note.Visibility;
            existing.SharedWith = note.SharedWith;
            existing.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Updated note {NoteId}", note.Id);

            return existing;
        }
        catch (DbUpdateConcurrencyException ex)
        {
            _logger.LogError(ex, "Concurrency error updating note {NoteId}", note.Id);
            throw;
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "Database error updating note {NoteId}", note.Id);
            throw;
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning("Update note operation was cancelled for note {NoteId}", note.Id);
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating note {NoteId}", note.Id);
            throw;
        }
    }

    public async Task DeleteNoteAsync(
        Guid noteId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var note = await _context.Notes.FindAsync(
                new object[] { noteId }, cancellationToken);

            if (note != null)
            {
                _context.Notes.Remove(note);
                await _context.SaveChangesAsync(cancellationToken);

                _logger.LogInformation("Deleted note {NoteId}", noteId);
            }
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "Database error deleting note {NoteId}", noteId);
            throw;
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning("Delete note operation was cancelled for note {NoteId}", noteId);
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting note {NoteId}", noteId);
            throw;
        }
    }

    public async Task<List<Note>> SearchNotesAsync(
        Guid userId,
        string searchTerm,
        CancellationToken cancellationToken = default)
    {
        try
        {
            return await _context.Notes
                .Where(n => n.UserId == userId &&
                           !n.IsArchived &&
                           (n.Title.Contains(searchTerm) || n.Content.Contains(searchTerm)))
                .OrderByDescending(n => n.IsPinned)
                .ThenByDescending(n => n.UpdatedAt)
                .ToListAsync(cancellationToken);
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning("Search notes operation was cancelled for user {UserId}", userId);
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching notes for user {UserId} with term '{SearchTerm}'", userId, searchTerm);
            throw;
        }
    }

    public async Task<List<Note>> GetNotesByCollectionAsync(
        Guid userId,
        string collection,
        CancellationToken cancellationToken = default)
    {
        try
        {
            return await _context.Notes
                .Where(n => n.UserId == userId && n.Collection == collection && !n.IsArchived)
                .OrderByDescending(n => n.IsPinned)
                .ThenByDescending(n => n.UpdatedAt)
                .ToListAsync(cancellationToken);
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning("Get notes by collection operation was cancelled for user {UserId}", userId);
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving notes by collection '{Collection}' for user {UserId}", collection, userId);
            throw;
        }
    }

    public async Task<List<Note>> GetNotesByTagAsync(
        Guid userId,
        string tag,
        CancellationToken cancellationToken = default)
    {
        try
        {
            return await _context.Notes
                .Where(n => n.UserId == userId && n.Tags.Contains(tag) && !n.IsArchived)
                .OrderByDescending(n => n.IsPinned)
                .ThenByDescending(n => n.UpdatedAt)
                .ToListAsync(cancellationToken);
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning("Get notes by tag operation was cancelled for user {UserId}", userId);
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving notes by tag '{Tag}' for user {UserId}", tag, userId);
            throw;
        }
    }
}
