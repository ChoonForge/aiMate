using System.ComponentModel.DataAnnotations;

namespace AiMate.Core.Entities;

/// <summary>
/// Personal note for knowledge capture and organization
/// </summary>
public class Note
{
    public Guid Id { get; set; } = Guid.NewGuid();

    [MaxLength(200)]
    public required string Title { get; set; }

    [MaxLength(50000)]
    public string Content { get; set; } = string.Empty;

    /// <summary>
    /// Content format: markdown, plain, html
    /// </summary>
    [MaxLength(50)]
    public string ContentType { get; set; } = "markdown";

    /// <summary>
    /// Owner of this note
    /// </summary>
    public Guid UserId { get; set; }
    public User? User { get; set; }

    /// <summary>
    /// Tags for organization
    /// </summary>
    public List<string> Tags { get; set; } = new();

    /// <summary>
    /// Collection name for grouping notes
    /// </summary>
    [MaxLength(200)]
    public string? Collection { get; set; }

    /// <summary>
    /// Category for additional organization
    /// </summary>
    [MaxLength(100)]
    public string? Category { get; set; }

    /// <summary>
    /// Color for visual organization
    /// </summary>
    [MaxLength(50)]
    public string? Color { get; set; }

    /// <summary>
    /// Pin to top of list
    /// </summary>
    public bool IsPinned { get; set; }

    /// <summary>
    /// Mark as favorite
    /// </summary>
    public bool IsFavorite { get; set; }

    /// <summary>
    /// Archive (hide from main view)
    /// </summary>
    public bool IsArchived { get; set; }

    /// <summary>
    /// Visibility: Private, Shared, Public
    /// </summary>
    [MaxLength(50)]
    public string Visibility { get; set; } = "Private";

    /// <summary>
    /// User IDs this note is shared with
    /// </summary>
    public List<string> SharedWith { get; set; } = new();

    /// <summary>
    /// Linked conversation ID (if note was created from a conversation)
    /// </summary>
    public Guid? LinkedConversationId { get; set; }

    /// <summary>
    /// Linked workspace ID
    /// </summary>
    public Guid? LinkedWorkspaceId { get; set; }

    /// <summary>
    /// File attachment IDs
    /// </summary>
    public List<string> Attachments { get; set; } = new();

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? LastViewedAt { get; set; }
}
