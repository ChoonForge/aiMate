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

    public async Task<Note?> GetNoteByIdAsync(
        Guid noteId,
        CancellationToken cancellationToken = default)
    {
        return await _context.Notes
            .FirstOrDefaultAsync(n => n.Id == noteId, cancellationToken);
    }

    public async Task<Note> CreateNoteAsync(
        Note note,
        CancellationToken cancellationToken = default)
    {
        _context.Notes.Add(note);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Created note {NoteId} for user {UserId}",
            note.Id, note.UserId);

        return note;
    }

    public async Task<Note> UpdateNoteAsync(
        Note note,
        CancellationToken cancellationToken = default)
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

    public async Task DeleteNoteAsync(
        Guid noteId,
        CancellationToken cancellationToken = default)
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

    public async Task<List<Note>> SearchNotesAsync(
        Guid userId,
        string searchTerm,
        CancellationToken cancellationToken = default)
    {
        return await _context.Notes
            .Where(n => n.UserId == userId &&
                       !n.IsArchived &&
                       (n.Title.Contains(searchTerm) || n.Content.Contains(searchTerm)))
            .OrderByDescending(n => n.IsPinned)
            .ThenByDescending(n => n.UpdatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<Note>> GetNotesByCollectionAsync(
        Guid userId,
        string collection,
        CancellationToken cancellationToken = default)
    {
        return await _context.Notes
            .Where(n => n.UserId == userId && n.Collection == collection && !n.IsArchived)
            .OrderByDescending(n => n.IsPinned)
            .ThenByDescending(n => n.UpdatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<Note>> GetNotesByTagAsync(
        Guid userId,
        string tag,
        CancellationToken cancellationToken = default)
    {
        return await _context.Notes
            .Where(n => n.UserId == userId && n.Tags.Contains(tag) && !n.IsArchived)
            .OrderByDescending(n => n.IsPinned)
            .ThenByDescending(n => n.UpdatedAt)
            .ToListAsync(cancellationToken);
    }
}
