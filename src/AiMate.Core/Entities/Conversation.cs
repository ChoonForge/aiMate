using AiMate.Core.Enums;
using System.ComponentModel.DataAnnotations;

namespace AiMate.Core.Entities;

/// <summary>
/// A conversation thread within a workspace
/// </summary>
public class Conversation
{
    public Guid Id { get; set; } = Guid.NewGuid();

    [MaxLength(200)]
    public required string Title { get; set; }

    /// <summary>
    /// Workspace this conversation belongs to
    /// </summary>
    public Guid WorkspaceId { get; set; }
    public Workspace? Workspace { get; set; }

    /// <summary>
    /// Messages in this conversation
    /// </summary>
    public List<Message> Messages { get; set; } = new();

    /// <summary>
    /// Model being used for this conversation
    /// </summary>
    [MaxLength(100)]
    public string? ModelId { get; set; }

    /// <summary>
    /// Personality mode for this conversation (can override workspace default)
    /// </summary>
    public PersonalityMode? PersonalityOverride { get; set; }

    /// <summary>
    /// Parent conversation if this is a branch/fork
    /// </summary>
    public Guid? ParentConversationId { get; set; }
    public Conversation? ParentConversation { get; set; }

    /// <summary>
    /// Branched conversations from this one
    /// </summary>
    public List<Conversation> BranchedConversations { get; set; } = new();

    public bool IsPinned { get; set; }

    public bool IsArchived { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
