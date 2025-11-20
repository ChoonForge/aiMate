using AiMate.Core.Entities;
using AiMate.Core.Services;
using AiMate.Shared.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AiMate.Web.Controllers;

/// <summary>
/// API controller for note management
/// </summary>
[ApiController]
[Route("api/v1/[controller]")]
[Authorize] // Requires authentication
public class NotesController : ControllerBase
{
    private readonly INotesService _notesService;
    private readonly ILogger<NotesApiController> _logger;

    public NotesApiController(
        INotesService notesService,
        ILogger<NotesApiController> logger)
    {
        _notesService = notesService;
        _logger = logger;
    }

    /// <summary>
    /// Get all notes for a user
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<List<NoteDto>>> GetNotes([FromQuery] string userId)
    {
        try
        {
            if (!Guid.TryParse(userId, out var userGuid))
            {
                return BadRequest("Invalid user ID");
            }

            var notes = await _notesService.GetUserNotesAsync(userGuid);
            var noteDtos = notes.Select(MapToDto).ToList();

            return Ok(noteDtos);
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
    public async Task<ActionResult<NoteDto>> GetNote(string id, [FromQuery] string userId)
    {
        try
        {
            if (!Guid.TryParse(id, out var noteGuid))
            {
                return BadRequest("Invalid note ID");
            }

            if (!Guid.TryParse(userId, out var userGuid))
            {
                return BadRequest("Invalid user ID");
            }

            var note = await _notesService.GetNoteByIdAsync(noteGuid);

            if (note == null)
            {
                return NotFound($"Note with ID {id} not found");
            }

            // Check ownership or shared access
            if (note.UserId != userGuid &&
                !note.SharedWith.Contains(userId) &&
                note.Visibility != "Public")
            {
                return Forbid("You don't have permission to access this note");
            }

            return Ok(MapToDto(note));
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
    public async Task<ActionResult<NoteDto>> CreateNote([FromBody] CreateNoteRequest request, [FromQuery] string userId)
    {
        try
        {
            if (!Guid.TryParse(userId, out var userGuid))
            {
                return BadRequest("Invalid user ID");
            }

            var note = new Note
            {
                Title = request.Title,
                Content = request.Content,
                ContentType = request.ContentType,
                Tags = request.Tags ?? new List<string>(),
                Collection = request.Collection,
                Category = request.Category,
                Color = request.Color,
                UserId = userGuid
            };

            var created = await _notesService.CreateNoteAsync(note);

            _logger.LogInformation("Created note {NoteId} for user {UserId}", created.Id, userId);

            return CreatedAtAction(nameof(GetNote), new { id = created.Id.ToString(), userId }, MapToDto(created));
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
    public async Task<ActionResult<NoteDto>> UpdateNote(string id, [FromBody] UpdateNoteRequest request, [FromQuery] string userId)
    {
        try
        {
            if (!Guid.TryParse(id, out var noteGuid))
            {
                return BadRequest("Invalid note ID");
            }

            if (!Guid.TryParse(userId, out var userGuid))
            {
                return BadRequest("Invalid user ID");
            }

            var note = await _notesService.GetNoteByIdAsync(noteGuid);

            if (note == null)
            {
                return NotFound($"Note with ID {id} not found");
            }

            // Check ownership
            if (note.UserId != userGuid)
            {
                return Forbid("You don't have permission to update this note");
            }

            // Update fields
            note.Title = request.Title ?? note.Title;
            note.Content = request.Content ?? note.Content;
            note.ContentType = request.ContentType ?? note.ContentType;
            note.Tags = request.Tags ?? note.Tags;
            note.Collection = request.Collection ?? note.Collection;
            note.Category = request.Category ?? note.Category;
            note.Color = request.Color ?? note.Color;
            note.IsPinned = request.IsPinned ?? note.IsPinned;
            note.IsFavorite = request.IsFavorite ?? note.IsFavorite;
            note.IsArchived = request.IsArchived ?? note.IsArchived;

            var updated = await _notesService.UpdateNoteAsync(note);

            _logger.LogInformation("Updated note {NoteId}", id);

            return Ok(MapToDto(updated));
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
    public async Task<IActionResult> DeleteNote(string id, [FromQuery] string userId)
    {
        try
        {
            if (!Guid.TryParse(id, out var noteGuid))
            {
                return BadRequest("Invalid note ID");
            }

            if (!Guid.TryParse(userId, out var userGuid))
            {
                return BadRequest("Invalid user ID");
            }

            var note = await _notesService.GetNoteByIdAsync(noteGuid);

            if (note == null)
            {
                return NotFound($"Note with ID {id} not found");
            }

            // Check ownership
            if (note.UserId != userGuid)
            {
                return Forbid("You don't have permission to delete this note");
            }

            await _notesService.DeleteNoteAsync(noteGuid);

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
    public async Task<ActionResult<List<string>>> GetCollections([FromQuery] string userId)
    {
        try
        {
            if (!Guid.TryParse(userId, out var userGuid))
            {
                return BadRequest("Invalid user ID");
            }

            var notes = await _notesService.GetUserNotesAsync(userGuid);
            var collections = notes
                .Where(n => !string.IsNullOrEmpty(n.Collection))
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
    public async Task<ActionResult<List<string>>> GetTags([FromQuery] string userId)
    {
        try
        {
            if (!Guid.TryParse(userId, out var userGuid))
            {
                return BadRequest("Invalid user ID");
            }

            var notes = await _notesService.GetUserNotesAsync(userGuid);
            var tags = notes
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

    // Helper method to map Note entity to NoteDto
    private static NoteDto MapToDto(Note note)
    {
        return new NoteDto
        {
            Id = note.Id.ToString(),
            Title = note.Title,
            Content = note.Content,
            ContentType = note.ContentType,
            Tags = note.Tags,
            Collection = note.Collection,
            Category = note.Category,
            Color = note.Color,
            IsPinned = note.IsPinned,
            IsFavorite = note.IsFavorite,
            IsArchived = note.IsArchived,
            OwnerId = note.UserId.ToString(),
            Visibility = note.Visibility,
            SharedWith = note.SharedWith,
            LinkedConversationId = note.LinkedConversationId?.ToString(),
            LinkedWorkspaceId = note.LinkedWorkspaceId?.ToString(),
            Attachments = note.Attachments,
            CreatedAt = note.CreatedAt,
            UpdatedAt = note.UpdatedAt,
            LastViewedAt = note.LastViewedAt
        };
    }
}
