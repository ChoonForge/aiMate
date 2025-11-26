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
public class NotesApiController : ControllerBase
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
    /// <param name="userId">User ID (GUID)</param>
    /// <returns>List of user's notes</returns>
    /// <response code="200">Returns the list of notes</response>
    /// <response code="400">Invalid user ID format</response>
    /// <response code="500">Internal server error</response>
    [HttpGet]
    [ProducesResponseType(typeof(List<NoteDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
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
    /// <param name="id">Note ID (GUID)</param>
    /// <param name="userId">User ID (GUID) for permission check</param>
    /// <returns>Note details</returns>
    /// <response code="200">Returns the note</response>
    /// <response code="400">Invalid note ID or user ID format</response>
    /// <response code="403">User doesn't have permission to access this note</response>
    /// <response code="404">Note not found</response>
    /// <response code="500">Internal server error</response>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(NoteDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
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
    /// <param name="request">Note details</param>
    /// <param name="userId">User ID (GUID)</param>
    /// <returns>Created note with ID</returns>
    /// <response code="201">Note created successfully</response>
    /// <response code="400">Invalid request or user ID</response>
    /// <response code="500">Internal server error</response>
    /// <remarks>
    /// Sample request:
    ///
    ///     POST /api/v1/notes?userId=abc123
    ///     {
    ///         "title": "Meeting Notes",
    ///         "content": "Discussion about Q1 goals...",
    ///         "contentType": "Markdown",
    ///         "tags": ["meetings", "q1", "goals"],
    ///         "collection": "Work",
    ///         "category": "Business",
    ///         "color": "#FF5733"
    ///     }
    ///
    /// </remarks>
    [HttpPost]
    [ProducesResponseType(typeof(NoteDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
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
                Tags = request.Tags ?? [],
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
    /// <param name="id">Note ID (GUID)</param>
    /// <param name="request">Updated note details (only include fields to update)</param>
    /// <param name="userId">User ID (GUID) for permission check</param>
    /// <returns>Updated note</returns>
    /// <response code="200">Note updated successfully</response>
    /// <response code="400">Invalid note ID or user ID format</response>
    /// <response code="403">User doesn't have permission to update this note</response>
    /// <response code="404">Note not found</response>
    /// <response code="500">Internal server error</response>
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(NoteDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
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
    /// <param name="id">Note ID (GUID)</param>
    /// <param name="userId">User ID (GUID) for permission check</param>
    /// <returns>No content</returns>
    /// <response code="204">Note deleted successfully</response>
    /// <response code="400">Invalid note ID or user ID format</response>
    /// <response code="403">User doesn't have permission to delete this note</response>
    /// <response code="404">Note not found</response>
    /// <response code="500">Internal server error</response>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
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
    /// Get all collections for a user (distinct collection names from user's notes)
    /// </summary>
    /// <param name="userId">User ID (GUID)</param>
    /// <returns>List of collection names</returns>
    /// <response code="200">Returns the list of collection names</response>
    /// <response code="400">Invalid user ID format</response>
    /// <response code="500">Internal server error</response>
    [HttpGet("collections")]
    [ProducesResponseType(typeof(List<string>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
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
    /// Get all tags used by a user (distinct tags from user's notes)
    /// </summary>
    /// <param name="userId">User ID (GUID)</param>
    /// <returns>List of tag names</returns>
    /// <response code="200">Returns the list of tag names</response>
    /// <response code="400">Invalid user ID format</response>
    /// <response code="500">Internal server error</response>
    [HttpGet("tags")]
    [ProducesResponseType(typeof(List<string>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
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
