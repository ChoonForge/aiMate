using AiMate.Shared.Models;
using Microsoft.AspNetCore.Mvc;

namespace AiMate.Web.Controllers;

/// <summary>
/// API controller for note management
/// </summary>
[ApiController]
[Route("api/v1/[controller]")]
public class NotesController : ControllerBase
{
    private readonly ILogger<NotesController> _logger;
    // MOCK IMPLEMENTATION: In-memory store for demonstration only
    // IMPLEMENTATION NEEDED: Replace with INotesService that uses database (Notes table)
    // Inject INotesService in constructor and use it for CRUD operations
    private static readonly List<NoteDto> _notes = new();

    public NotesController(ILogger<NotesController> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Get all notes for a user
    /// </summary>
    [HttpGet]
    public ActionResult<List<NoteDto>> GetNotes([FromQuery] string userId)
    {
        try
        {
            var userNotes = _notes
                .Where(n => n.OwnerId == userId)
                .OrderByDescending(n => n.IsPinned)
                .ThenByDescending(n => n.UpdatedAt)
                .ToList();

            return Ok(userNotes);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting notes for user {UserId}", userId);
            return StatusCode(500, "Error retrieving notes");
        }
    }

    /// <summary>
    /// Get a specific note by ID
    /// </summary>
    [HttpGet("{id}")]
    public ActionResult<NoteDto> GetNote(string id, [FromQuery] string userId)
    {
        try
        {
            var note = _notes.FirstOrDefault(n => n.Id == id);

            if (note == null)
            {
                return NotFound($"Note with ID {id} not found");
            }

            // Check ownership or shared access
            if (note.OwnerId != userId && !note.SharedWith.Contains(userId) && note.Visibility != "Public")
            {
                return Forbid("You don't have permission to access this note");
            }

            return Ok(note);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting note {NoteId}", id);
            return StatusCode(500, "Error retrieving note");
        }
    }

    /// <summary>
    /// Create a new note
    /// </summary>
    [HttpPost]
    public ActionResult<NoteDto> CreateNote([FromBody] CreateNoteRequest request, [FromQuery] string userId)
    {
        try
        {
            var note = new NoteDto
            {
                Id = Guid.NewGuid().ToString(),
                Title = request.Title,
                Content = request.Content,
                ContentType = request.ContentType,
                Tags = request.Tags ?? new List<string>(),
                Collection = request.Collection,
                Category = request.Category,
                Color = request.Color,
                OwnerId = userId,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _notes.Add(note);

            _logger.LogInformation("Created note {NoteId} for user {UserId}", note.Id, userId);

            return CreatedAtAction(nameof(GetNote), new { id = note.Id, userId }, note);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating note");
            return StatusCode(500, "Error creating note");
        }
    }

    /// <summary>
    /// Update an existing note
    /// </summary>
    [HttpPut("{id}")]
    public ActionResult<NoteDto> UpdateNote(string id, [FromBody] UpdateNoteRequest request, [FromQuery] string userId)
    {
        try
        {
            var note = _notes.FirstOrDefault(n => n.Id == id);

            if (note == null)
            {
                return NotFound($"Note with ID {id} not found");
            }

            // Check ownership
            if (note.OwnerId != userId)
            {
                return Forbid("You don't have permission to update this note");
            }

            // Update fields
            var updatedNote = note with
            {
                Title = request.Title ?? note.Title,
                Content = request.Content ?? note.Content,
                ContentType = request.ContentType ?? note.ContentType,
                Tags = request.Tags ?? note.Tags,
                Collection = request.Collection ?? note.Collection,
                Category = request.Category ?? note.Category,
                Color = request.Color ?? note.Color,
                IsPinned = request.IsPinned ?? note.IsPinned,
                IsFavorite = request.IsFavorite ?? note.IsFavorite,
                IsArchived = request.IsArchived ?? note.IsArchived,
                UpdatedAt = DateTime.UtcNow
            };

            var index = _notes.IndexOf(note);
            _notes[index] = updatedNote;

            _logger.LogInformation("Updated note {NoteId}", id);

            return Ok(updatedNote);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating note {NoteId}", id);
            return StatusCode(500, "Error updating note");
        }
    }

    /// <summary>
    /// Delete a note
    /// </summary>
    [HttpDelete("{id}")]
    public IActionResult DeleteNote(string id, [FromQuery] string userId)
    {
        try
        {
            var note = _notes.FirstOrDefault(n => n.Id == id);

            if (note == null)
            {
                return NotFound($"Note with ID {id} not found");
            }

            // Check ownership
            if (note.OwnerId != userId)
            {
                return Forbid("You don't have permission to delete this note");
            }

            _notes.Remove(note);

            _logger.LogInformation("Deleted note {NoteId}", id);

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting note {NoteId}", id);
            return StatusCode(500, "Error deleting note");
        }
    }

    /// <summary>
    /// Get all collections for a user
    /// </summary>
    [HttpGet("collections")]
    public ActionResult<List<string>> GetCollections([FromQuery] string userId)
    {
        try
        {
            var collections = _notes
                .Where(n => n.OwnerId == userId && !string.IsNullOrEmpty(n.Collection))
                .Select(n => n.Collection!)
                .Distinct()
                .OrderBy(c => c)
                .ToList();

            return Ok(collections);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting collections");
            return StatusCode(500, "Error retrieving collections");
        }
    }

    /// <summary>
    /// Get all tags used by a user
    /// </summary>
    [HttpGet("tags")]
    public ActionResult<List<string>> GetTags([FromQuery] string userId)
    {
        try
        {
            var tags = _notes
                .Where(n => n.OwnerId == userId)
                .SelectMany(n => n.Tags)
                .Distinct()
                .OrderBy(t => t)
                .ToList();

            return Ok(tags);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting tags");
            return StatusCode(500, "Error retrieving tags");
        }
    }
}
